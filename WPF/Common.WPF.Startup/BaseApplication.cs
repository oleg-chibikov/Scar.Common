using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Autofac;
using Easy.MessageHub;
using Microsoft.Extensions.Logging;
using Scar.Common.ApplicationLifetime;
using Scar.Common.Messages;
using Scar.Common.WPF.Localization;

namespace Scar.Common.WPF.Startup
{
    public abstract class BaseApplication : Application
    {
        readonly IApplicationStartupBootstrapper _applicationBootstrapper;

        [SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "Intended")]
        protected BaseApplication()
        {
            var cultureManager = new CultureManager();
            var applicationTerminator = new ApplicationTerminator();
            var assemblyInfoProvider = new AssemblyInfoProvider(new EntryAssemblyProvider(), new SpecialPathsProvider());

            // ReSharper disable VirtualMemberCallInConstructor
            _applicationBootstrapper = new ApplicationStartupBootstrapper(
                cultureManager,
                applicationTerminator,
                ShowMessage,
                CreateMutex,
                RegisterDependencies,
                assemblyInfoProvider,
                AlreadyRunningMessage,
                WaitAfterOldInstanceKillMilliseconds,
                NewInstanceHandling,
                GetStartupCulture());

            // ReSharper restore VirtualMemberCallInConstructor
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        protected ILifetimeScope Container => _applicationBootstrapper.Container;

        protected ILogger Logger => _applicationBootstrapper.Logger;

        protected IMessageHub Messenger => _applicationBootstrapper.Messenger;

        protected virtual NewInstanceHandling NewInstanceHandling => NewInstanceHandling.Restart;

        protected virtual int WaitAfterOldInstanceKillMilliseconds => 0;

        protected virtual string? AlreadyRunningMessage => null;

        protected SynchronizationContext? SynchronizationContext => _applicationBootstrapper.SynchronizationContext;

        protected override void OnExit(ExitEventArgs e)
        {
            _applicationBootstrapper.OnExit();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _applicationBootstrapper.OnStart();

            // Prevent WPF tooltips from expiration
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

            OnStartup();
        }

        protected virtual CultureInfo? GetStartupCulture() => null;

        protected abstract void OnStartup();

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
            MessageBox.Show(
                message.Text,
                GetAppProduct(),
                MessageBoxButton.OK,
                image);
        }

        static string GetAppGuid()
        {
            var entryAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null");
            var attribute = Attribute.GetCustomAttribute(entryAssembly, typeof(GuidAttribute), false) as GuidAttribute ?? throw new InvalidOperationException("Guid attribute is null");
            return attribute.Value;
        }

        static string GetAppProduct()
        {
            var entryAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null");
            var attribute = Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyProductAttribute), false) as AssemblyProductAttribute ?? throw new InvalidOperationException("Guid attribute is null");
            return attribute.Product;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _applicationBootstrapper.HandleException(e.Exception);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

        Mutex CreateMutex() => new Mutex(false, $"Global\\{GetAppGuid()}", out _);
    }
}
