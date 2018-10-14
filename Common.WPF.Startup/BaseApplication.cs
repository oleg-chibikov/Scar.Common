using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Autofac;
using Common.Logging;
using Easy.MessageHub;
using JetBrains.Annotations;
using Scar.Common.Exceptions;
using Scar.Common.Logging;
using Scar.Common.Messages;
using Scar.Common.WPF.Localization;

namespace Scar.Common.WPF.Startup
{
    public abstract class BaseApplication : Application
    {
        [CanBeNull]
        private readonly Mutex _mutex;

        [NotNull]
        private readonly IList<Guid> _subscriptionTokens = new List<Guid>();

        [NotNull]
        protected readonly ILifetimeScope Container;

        [NotNull]
        protected readonly ILog Logger;

        [NotNull]
        protected readonly IMessageHub Messenger;

        protected BaseApplication()
        {
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            Container = BuildContainer();

            // ReSharper disable once VirtualMemberCallInConstructor
            CultureUtilities.ChangeCulture(GetStartupCulture());

            Messenger = Container.Resolve<IMessageHub>();
            _subscriptionTokens.Add(Messenger.Subscribe<Message>(LogAndShowMessage));
            _subscriptionTokens.Add(Messenger.Subscribe<CultureInfo>(CultureUtilities.ChangeCulture));

            Logger = Container.Resolve<ILog>();
            // ReSharper disable once VirtualMemberCallInConstructor
            if (NewInstanceHandling != NewInstanceHandling.AllowMultiple)
            {
                _mutex = CreateMutex();
            }

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        protected virtual NewInstanceHandling NewInstanceHandling => NewInstanceHandling.Restart;

        protected virtual int WaitAfterOldInstanceKillMilliseconds => 0;

        [NotNull]
        protected virtual string AlreadyRunningCaption => "Cannot launch";

        [NotNull]
        protected virtual string AlreadyRunningMessage =>
            ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyProductAttribute), false)).Product + " is already launched";

        [CanBeNull]
        protected SynchronizationContext SynchronizationContext { get; private set; }

        [NotNull]
        protected virtual CultureInfo GetStartupCulture()
        {
            return CultureUtilities.GetCurrentCulture();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            foreach (var token in _subscriptionTokens)
            {
                Messenger.Unsubscribe(token);
            }

            Container.Dispose();
            _mutex?.Dispose();
        }

        protected abstract void OnStartup();

        protected override void OnStartup(StartupEventArgs e)
        {
            SynchronizationContext = SynchronizationContext.Current;
            if (NewInstanceHandling != NewInstanceHandling.AllowMultiple)
            {
                if (_mutex == null)
                {
                    throw new InvalidOperationException("Mutex should be initialized");
                }

                bool alreadyRunning;
                try
                {
                    alreadyRunning = !_mutex.WaitOne(0, false);
                }
                catch (AbandonedMutexException)
                {
                    // No action required
                    alreadyRunning = false;
                }

                if (alreadyRunning)
                {
                    switch (NewInstanceHandling)
                    {
                        case NewInstanceHandling.Throw:
                            MessageBox.Show(AlreadyRunningMessage, AlreadyRunningCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                            Current.Shutdown();
                            return;
                        case NewInstanceHandling.Restart:
                        {
                            KillAnotherInstance();
                            break;
                        }
                    }
                }
            }

            //Prevent WPF tooltips from expiration
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

            OnStartup();
        }

        private void KillAnotherInstance()
        {
            var anotherInstance = Process.GetProcesses()
                .SingleOrDefault(proc => proc.ProcessName.Equals(Process.GetCurrentProcess().ProcessName) && proc.Id != Process.GetCurrentProcess().Id);
            if (anotherInstance != null)
            {
                anotherInstance.Kill();
                if (WaitAfterOldInstanceKillMilliseconds > 0)
                {
                    // Wait for process to close
                    Thread.Sleep(WaitAfterOldInstanceKillMilliseconds);
                }
            }
        }

        protected virtual void RegisterDependencies(ContainerBuilder builder)
        {
        }

        protected virtual void ShowMessage([NotNull] Message message)
        {
            MessageBoxImage image;
            switch (message.Type)
            {
                case MessageType.Message:
                    image = MessageBoxImage.Information;
                    break;
                case MessageType.Warning:
                    image = MessageBoxImage.Warning;
                    break;
                case MessageType.Error:
                    image = MessageBoxImage.Error;
                    break;
                case MessageType.Success:
                    image = MessageBoxImage.Information;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            MessageBox.Show(
                message.Text,
                ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyProductAttribute), false)).Product,
                MessageBoxButton.OK,
                image);
        }

        private void App_DispatcherUnhandledException(object sender, [NotNull] DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

        [NotNull]
        private ILifetimeScope BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(MessageHub.Instance).AsImplementedInterfaces().SingleInstance();
            builder.RegisterModule<LoggingModule>();
            builder.Register(x => SynchronizationContext).AsSelf().SingleInstance();

            RegisterDependencies(builder);

            return builder.Build();
        }

        [NotNull]
        private Mutex CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));
            var appGuid = ((GuidAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(GuidAttribute), false)).Value;
            return new Mutex(false, $"Global\\{appGuid}", out _, mutexSecurity);
        }

        private void HandleException(Exception e)
        {
            if (e is OperationCanceledException)
            {
                Logger.Trace("Operation canceled", e);
            }
            else if (e is ObjectDisposedException objectDisposedException && objectDisposedException.Source == nameof(Autofac))
            {
                Logger.Trace("Autofac lifetimescope has already been disposed", e);
            }
            else
            {
                Logger.Fatal("Unhandled exception", e);
                NotifyError(e);
            }
        }

        private void LogAndShowMessage([NotNull] Message message)
        {
            if (message.Exception is OperationCanceledException
                || message.Exception is ObjectDisposedException objectDisposedException && objectDisposedException.Source == nameof(Autofac))
            {
                return;
            }

            switch (message.Type)
            {
                case MessageType.Warning:
                    Logger.Warn(message.Text, message.Exception);
                    break;
                case MessageType.Error:
                    Logger.Error(message.Text, message.Exception);
                    break;
            }

            ShowMessage(message);
        }

        private void NotifyError(Exception e)
        {
            var localizable = e as LocalizableException;
            var message = localizable?.ToMessage() ?? e.ToMessage();
            Messenger.Publish(message);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, [NotNull] UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception.InnerException);
            e.SetObserved();
            e.Exception.Handle(ex => true);
        }
    }
}