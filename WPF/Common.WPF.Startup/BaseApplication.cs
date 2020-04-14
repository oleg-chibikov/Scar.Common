using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Autofac;
using Common.Logging;
using Easy.MessageHub;
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

        protected ILog Logger => _applicationBootstrapper.Logger;

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
                ((AssemblyProductAttribute)Attribute.GetCustomAttribute(
                    Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null"),
                    typeof(AssemblyProductAttribute),
                    false)).Product,
                MessageBoxButton.OK,
                image);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _applicationBootstrapper.HandleException(e.Exception);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

        Mutex CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));
            var appGuid = ((GuidAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null"), typeof(GuidAttribute), false))
                .Value;
            return new Mutex(false, $"Global\\{appGuid}", out _, mutexSecurity);
        }
    }
}
