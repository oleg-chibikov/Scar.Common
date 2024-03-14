using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Autofac;
using Easy.MessageHub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scar.Common.ApplicationLifetime.Core;
using Scar.Common.Messages;
using Scar.Common.WPF.Localization;

namespace Scar.Common.WPF.Startup;

public abstract class BaseApplication : Application
{
    readonly ApplicationStartupBootstrapper _applicationBootstrapper;

    [SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "Intended")]
    protected BaseApplication(
        Func<IHostBuilder, IHostBuilder>? configureHost = null,
        Action<IServiceCollection>? configureServices = null,
        Action<HostBuilderContext, ILoggingBuilder>? configureLogging = null,
        string? alreadyRunningMessage = null,
        int waitAfterOldInstanceKillMilliseconds = 0,
        NewInstanceHandling newInstanceHandling = NewInstanceHandling.Restart,
        CultureInfo? startupCulture = null)
    {
        var cultureManager = new CultureManager();
        var applicationTerminator = new ApplicationTerminator();
        var assemblyInfoProvider = new AssemblyInfoProvider(new EntryAssemblyProvider(), new SpecialPathsProvider());

        _applicationBootstrapper = new ApplicationStartupBootstrapper(
            cultureManager,
            applicationTerminator,
            ShowMessage,
            CreateMutex,
            RegisterDependencies,
            assemblyInfoProvider,
            configureHost,
            configureServices,
            configureLogging,
            alreadyRunningMessage,
            waitAfterOldInstanceKillMilliseconds,
            newInstanceHandling,
            startupCulture);

        DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    protected ILifetimeScope Container => _applicationBootstrapper.Container;

    protected IMessageHub Messenger => _applicationBootstrapper.Messenger;

    protected SynchronizationContext? SynchronizationContext => _applicationBootstrapper.SynchronizationContext;

    protected override async void OnExit(ExitEventArgs e)
    {
        await _applicationBootstrapper.OnExitAsync().ConfigureAwait(true);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        _applicationBootstrapper.BeforeStart();

        // Prevent WPF tooltips from expiration
        ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

        await Task.WhenAll(OnStartupAsync(), _applicationBootstrapper.OnStartAsync()).ConfigureAwait(true);
    }

    protected abstract Task OnStartupAsync();

    protected virtual void RegisterDependencies(ContainerBuilder builder)
    {
    }

    protected virtual void ShowMessage(Message message)
    {
        _ = message ?? throw new ArgumentNullException(nameof(message));

        var image = message.Type switch
        {
            MessageType.Message => MessageBoxImage.Information,
            MessageType.Warning => MessageBoxImage.Warning,
            MessageType.Error => MessageBoxImage.Error,
            MessageType.Success => MessageBoxImage.Information,
            _ => throw new ArgumentOutOfRangeException(nameof(message))
        };
        MessageBox.Show(message.Text, GetAppProduct(), MessageBoxButton.OK, image);
    }

    static string GetAppGuid()
    {
        var entryAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null");
        var attribute = Attribute.GetCustomAttribute(entryAssembly, typeof(GuidAttribute), inherit: false) as GuidAttribute ?? throw new InvalidOperationException("Guid attribute is null");
        return attribute.Value;
    }

    static string GetAppProduct()
    {
        var entryAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null");
        var attribute = Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyProductAttribute), inherit: false) as AssemblyProductAttribute ??
                        throw new InvalidOperationException("Guid attribute is null");
        return attribute.Product;
    }

    void App_DispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _applicationBootstrapper.HandleException(e.Exception);

        // Prevent default unhandled exception processing
        e.Handled = true;
    }

    Mutex CreateMutex()
    {
        return new Mutex(false, $"Global\\{GetAppGuid()}", out _);
    }
}
