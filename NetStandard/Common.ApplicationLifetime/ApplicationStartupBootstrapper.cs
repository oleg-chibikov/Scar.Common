using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Easy.MessageHub;
using Microsoft.Extensions.Logging;
using Scar.Common.ApplicationLifetime.Contracts;
using Scar.Common.Exceptions;
using Scar.Common.Localization;
using Scar.Common.Messages;

namespace Scar.Common.ApplicationLifetime
{
    public class ApplicationStartupBootstrapper : IApplicationStartupBootstrapper
    {
        readonly string _alreadyRunningMessage;
        readonly IApplicationTerminator _applicationTerminator;
        readonly IAssemblyInfoProvider _assemblyInfoProvider;
        readonly Mutex? _mutex;
        readonly NewInstanceHandling _newInstanceHandling;
        readonly Action<ContainerBuilder> _registerDependencies;
        readonly IList<Guid> _subscriptionTokens = new List<Guid>();
        readonly int _waitAfterOldInstanceKillMilliseconds;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Mutex is lifetime object")]
        public ApplicationStartupBootstrapper(
            ICultureManager cultureManager,
            IApplicationTerminator applicationTerminator,
            Action<Message> showMessage,
            Func<Mutex>? createMutex,
            Action<ContainerBuilder> registerDependencies,
            IAssemblyInfoProvider assemblyInfoProvider,
            string? alreadyRunningMessage = null,
            int waitAfterOldInstanceKillMilliseconds = 0,
            NewInstanceHandling newInstanceHandling = NewInstanceHandling.Restart,
            CultureInfo? startupCulture = null)
        {
            // This is used for logs - every log will have a CorrelationId;
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            _registerDependencies = registerDependencies ?? throw new ArgumentNullException(nameof(registerDependencies));
            _assemblyInfoProvider = assemblyInfoProvider ?? throw new ArgumentNullException(nameof(assemblyInfoProvider));
            _alreadyRunningMessage = alreadyRunningMessage ?? $"{assemblyInfoProvider.Product} is already launched";
            _waitAfterOldInstanceKillMilliseconds = waitAfterOldInstanceKillMilliseconds;
            _newInstanceHandling = newInstanceHandling;
            ShowMessage = showMessage ?? throw new ArgumentNullException(nameof(cultureManager));
            CultureManager = cultureManager ?? throw new ArgumentNullException(nameof(cultureManager));
            _applicationTerminator = applicationTerminator ?? throw new ArgumentNullException(nameof(applicationTerminator));

            Container = BuildContainer();

            // ReSharper disable once VirtualMemberCallInConstructor
            CultureManager.ChangeCulture(startupCulture ?? Thread.CurrentThread.CurrentUICulture);
            Messenger = Container.Resolve<IMessageHub>();
            _subscriptionTokens.Add(Messenger.Subscribe<Message>(LogAndShowMessage));
            _subscriptionTokens.Add(Messenger.Subscribe<CultureInfo>(CultureManager.ChangeCulture));
            Logger = Container.Resolve<ILogger>();

            // ReSharper disable once VirtualMemberCallInConstructor
            if (_newInstanceHandling != NewInstanceHandling.AllowMultiple)
            {
                _mutex = createMutex == null ? CreateCommonMutex() : createMutex();
            }

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        public SynchronizationContext? SynchronizationContext { get; private set; }

        public Action<Message> ShowMessage { get; }

        public ILifetimeScope Container { get; }

        public ILogger Logger { get; }

        public IMessageHub Messenger { get; }

        public ICultureManager CultureManager { get; }

        public string AppGuid => _assemblyInfoProvider.AppGuid;

        public void OnExit()
        {
            foreach (var token in _subscriptionTokens)
            {
                Messenger.Unsubscribe(token);
            }

            Container.Dispose();
            _mutex?.Dispose();
        }

        public void OnStart()
        {
            SynchronizationContext = SynchronizationContext.Current;
            if (_newInstanceHandling == NewInstanceHandling.AllowMultiple)
            {
                return;
            }

            switch (_newInstanceHandling)
            {
                case NewInstanceHandling.Throw:
                    if (CheckAlreadyRunning())
                    {
                        return;
                    }

                    ShowMessage(_alreadyRunningMessage.ToWarning());
                    _applicationTerminator.Terminate();

                    return;
                case NewInstanceHandling.Restart:
                    {
                        KillAnotherInstanceIfExists();
                        break;
                    }
            }
        }

        public void HandleException(Exception e)
        {
            if (e is OperationCanceledException)
            {
                Logger.LogTrace(e, "Operation canceled");
            }
            else if (e is ObjectDisposedException objectDisposedException && (objectDisposedException.Source == nameof(Autofac)))
            {
                Logger.LogTrace(e, "Autofac LifeTimeScope has already been disposed");
            }
            else
            {
                Logger.LogError(e, "Unhandled exception");
                NotifyError(e);
            }
        }

        bool CheckAlreadyRunning()
        {
            bool alreadyRunning;
            if (_mutex == null)
            {
                throw new InvalidOperationException("Mutex should be initialized");
            }

            try
            {
                alreadyRunning = !_mutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException)
            {
                // No action required
                alreadyRunning = false;
            }

            return !alreadyRunning;
        }

        void KillAnotherInstanceIfExists()
        {
            var anotherInstance = Process.GetProcesses().SingleOrDefault(proc => proc.ProcessName.Equals(Process.GetCurrentProcess().ProcessName, StringComparison.Ordinal) && (proc.Id != Process.GetCurrentProcess().Id));
            if (anotherInstance != null)
            {
                anotherInstance.Kill();
                if (_waitAfterOldInstanceKillMilliseconds > 0)
                {
                    // Wait for process to close
                    Thread.Sleep(_waitAfterOldInstanceKillMilliseconds);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Lifetime object")]
        ILifetimeScope BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.Register(x => SynchronizationContext ?? throw new InvalidOperationException("SyncContext should not be null at the moment of registration")).AsSelf().SingleInstance();
            builder.RegisterInstance(new MessageHub()).AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(_assemblyInfoProvider).AsImplementedInterfaces().SingleInstance();

            // builder.RegisterModule<LoggingModule>();
            _registerDependencies(builder);

            return builder.Build();
        }

        Mutex CreateCommonMutex()
        {
            return new Mutex(false, $"Global\\{AppGuid}", out _);
        }

        void LogAndShowMessage(Message message)
        {
            if (message.Exception is OperationCanceledException || (message.Exception is ObjectDisposedException objectDisposedException && (objectDisposedException.Source == nameof(Autofac))))
            {
                return;
            }

            switch (message.Type)
            {
                case MessageType.Warning:
                    Logger.LogWarning(message.Exception, message.Text);
                    break;
                case MessageType.Error:
                    Logger.LogWarning(message.Exception, message.Text);
                    break;
            }

            ShowMessage(message);
        }

        void NotifyError(Exception e)
        {
            var localizable = e as LocalizableException;
            var message = localizable?.ToMessage() ?? e.ToMessage();
            Messenger.Publish(message);
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception.InnerException);
            e.SetObserved();
            e.Exception.Handle(ex => true);
        }
    }
}
