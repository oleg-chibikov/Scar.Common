using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Easy.MessageHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scar.Common.ApplicationLifetime.Contracts;
using Scar.Common.Exceptions;
using Scar.Common.Localization;
using Scar.Common.Messages;

namespace Scar.Common.ApplicationLifetime.Core;

public class ApplicationStartupBootstrapper : IApplicationStartupBootstrapper
{
    readonly string _alreadyRunningMessage;
    readonly IApplicationTerminator _applicationTerminator;
    readonly IAssemblyInfoProvider _assemblyInfoProvider;
    readonly Mutex? _mutex;
    readonly NewInstanceHandling _newInstanceHandling;
    readonly List<Guid> _subscriptionTokens = new();
    readonly int _waitAfterOldInstanceKillMilliseconds;
    readonly IHost _host;
    readonly ILogger _logger;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Mutex is lifetime object")]
    public ApplicationStartupBootstrapper(
        ICultureManager cultureManager,
        IApplicationTerminator applicationTerminator,
        Action<Message> showMessage,
        Func<Mutex>? createMutex,
        Action<ContainerBuilder> registerDependencies,
        IAssemblyInfoProvider assemblyInfoProvider,
        Func<IHostBuilder, IHostBuilder>? configureHost = null,
        Action<IServiceCollection>? configureServices = null,
        Action<HostBuilderContext, ILoggingBuilder>? configureLogging = null,
        string? alreadyRunningMessage = null,
        int waitAfterOldInstanceKillMilliseconds = 0,
        NewInstanceHandling newInstanceHandling = NewInstanceHandling.Restart,
        CultureInfo? startupCulture = null,
        string? baseDirectory = null,
        Func<ILifetimeScope, Task>? afterBuild = null)
    {
        // This is used for logs - every log will have a CorrelationId;
        Trace.CorrelationManager.ActivityId = Guid.NewGuid();
        _ = registerDependencies ?? throw new ArgumentNullException(nameof(registerDependencies));
        _assemblyInfoProvider = assemblyInfoProvider ?? throw new ArgumentNullException(nameof(assemblyInfoProvider));
        _alreadyRunningMessage = alreadyRunningMessage ?? $"{assemblyInfoProvider.Product} is already launched";
        _waitAfterOldInstanceKillMilliseconds = waitAfterOldInstanceKillMilliseconds;
        _newInstanceHandling = newInstanceHandling;
        ShowMessage = showMessage ?? throw new ArgumentNullException(nameof(showMessage));
        CultureManager = cultureManager ?? throw new ArgumentNullException(nameof(cultureManager));
        _applicationTerminator =
            applicationTerminator ?? throw new ArgumentNullException(nameof(applicationTerminator));
        baseDirectory ??= AppContext.BaseDirectory ??
                          throw new InvalidOperationException("Cannot get base directory");

        Console.WriteLine("Base directory of Bootstrapper: " + baseDirectory);

        if (_newInstanceHandling != NewInstanceHandling.AllowMultiple)
        {
            _mutex = createMutex == null ? CreateCommonMutex() : createMutex();
        }

        var hostBuilder = Host.CreateDefaultBuilder().ConfigureServices(
            serviceCollection =>
            {
                configureServices?.Invoke(serviceCollection);

                serviceCollection.AddLogging();
            }).ConfigureAppConfiguration(
            (hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                var mainAppSettingsFilePath = Path.Combine(
                    baseDirectory,
                    "appsettings.json");
                var environmentSpecificAppSettingsFilePath = Path.Combine(
                    baseDirectory,
                    $"appsettings.{env.EnvironmentName}.json");
                if (File.Exists(mainAppSettingsFilePath))
                {
                    config.AddJsonFile(
                        mainAppSettingsFilePath,
                        true,
                        true);
                }

                if (File.Exists(environmentSpecificAppSettingsFilePath))
                {
                    config.AddJsonFile(
                        environmentSpecificAppSettingsFilePath,
                        true,
                        true);
                }
            }).ConfigureLogging(
            (hostBuilderContext, loggingBuilder) =>
            {
                configureLogging?.Invoke(
                    hostBuilderContext,
                    loggingBuilder);
            }).UseServiceProviderFactory(
            new AutofacServiceProviderFactory(
                containerBuilder =>
                {
                    containerBuilder.
                        Register(
                            _ => SynchronizationContext ?? throw new InvalidOperationException(
                                "SyncContext should not be null at the moment of registration")).AsSelf().
                        SingleInstance();
                    containerBuilder.RegisterInstance(new MessageHub()).AsImplementedInterfaces().SingleInstance();
                    containerBuilder.RegisterInstance(_assemblyInfoProvider).AsImplementedInterfaces().SingleInstance();
                    registerDependencies(containerBuilder);
                }));

        if (configureHost != null)
        {
            hostBuilder = configureHost(hostBuilder);
        }

        _host = hostBuilder.Build();
        Container = _host.Services.GetAutofacRoot();
        CultureManager.ChangeCulture(startupCulture ?? Thread.CurrentThread.CurrentUICulture);
        Messenger = Container.Resolve<IMessageHub>();
        _subscriptionTokens.Add(Messenger.Subscribe<Message>(LogAndShowMessage));
        _subscriptionTokens.Add(Messenger.Subscribe<CultureInfo>(CultureManager.ChangeCulture));
        _logger = Container.Resolve<ILogger<ApplicationStartupBootstrapper>>();
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        afterBuild?.Invoke(Container);
    }

    public SynchronizationContext? SynchronizationContext { get; private set; }

    public Action<Message> ShowMessage { get; }

    public ILifetimeScope Container { get; }

    public IMessageHub Messenger { get; }

    public ICultureManager CultureManager { get; }

    public string AppGuid => _assemblyInfoProvider.AppGuid;

    public async Task OnExitAsync()
    {
        foreach (var cancellationToken in _subscriptionTokens)
        {
            Messenger.Unsubscribe(cancellationToken);
        }

        await Container.DisposeAsync().ConfigureAwait(false);
        _mutex?.Dispose();
    }

    public void BeforeStart()
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

    public async Task OnStartAsync()
    {
        await _host.RunAsync().ConfigureAwait(false);
    }

    public void HandleException(Exception e)
    {
        if (e is OperationCanceledException)
        {
            _logger.LogTrace(
                e,
                "Operation canceled");
        }
        else if (e is ObjectDisposedException { Source: nameof(Autofac) })
        {
            _logger.LogTrace(
                e,
                "Autofac LifeTimeScope has already been disposed");
        }
        else
        {
            _logger.LogError(
                e,
                "Unhandled exception");
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
            alreadyRunning = !_mutex.WaitOne(
                0,
                false);
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
        var anotherInstance = Process.GetProcesses().SingleOrDefault(
            proc => proc.ProcessName.Equals(
                Process.GetCurrentProcess().ProcessName,
                StringComparison.Ordinal) && proc.Id != Environment.ProcessId);
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

    Mutex CreateCommonMutex()
    {
        return new Mutex(
            false,
            $"Global\\{AppGuid}",
            out _);
    }

    void LogAndShowMessage(Message message)
    {
        if (message.Exception is OperationCanceledException or ObjectDisposedException { Source: nameof(Autofac) })
        {
            return;
        }

        switch (message.Type)
        {
            case MessageType.Warning:
                _logger.LogWarning(
                    message.Exception,
                    "{Message}",
                    message.Text);
                break;
            case MessageType.Error:
                _logger.LogWarning(
                    message.Exception,
                    "{Message}",
                    message.Text);
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

    void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleException(e.Exception.InnerException ?? e.Exception);
        e.SetObserved();
        e.Exception.Handle(_ => true);
    }
}
