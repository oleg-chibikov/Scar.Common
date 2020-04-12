using System;
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
        private readonly IApplicationStartupBootstrapper _applicationBootstrapper;

        protected ILifetimeScope Container => _applicationBootstrapper.Container;

        protected ILog Logger => _applicationBootstrapper.Logger;

        protected IMessageHub Messenger => _applicationBootstrapper.Messenger;

        protected BaseApplication()
        {
            var cultureManager = new CultureManager();
            var applicationTerminator = new ApplicationTerminator();
            var assemblyInfoProvider = new AssemblyInfoProvider(new EntryAssemblyProvider(), new SpecialPathsProvider());
            // ReSharper disable VirtualMemberCallInConstructor
            _applicationBootstrapper = new ApplicationStartupBootstrapper(cultureManager, applicationTerminator, ShowMessage, CreateMutex, RegisterDependencies, assemblyInfoProvider, AlreadyRunningMessage, WaitAfterOldInstanceKillMilliseconds, NewInstanceHandling, GetStartupCulture());
            // ReSharper restore VirtualMemberCallInConstructor

            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        protected virtual NewInstanceHandling NewInstanceHandling => NewInstanceHandling.Restart;

        protected virtual int WaitAfterOldInstanceKillMilliseconds => 0;

        protected virtual string? AlreadyRunningMessage => null;

        protected SynchronizationContext? SynchronizationContext => _applicationBootstrapper.SynchronizationContext;

        protected virtual CultureInfo? GetStartupCulture() => null;

        protected override void OnExit(ExitEventArgs e)
        {
            _applicationBootstrapper.OnExit();
        }

        protected abstract void OnStartup();

        protected override void OnStartup(StartupEventArgs e)
        {
            _applicationBootstrapper.OnStart();

            //Prevent WPF tooltips from expiration
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

            OnStartup();
        }

        protected virtual void RegisterDependencies(ContainerBuilder builder)
        {
        }

        protected virtual void ShowMessage(Message message)
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
                ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null"), typeof(AssemblyProductAttribute), false)).Product,
                MessageBoxButton.OK,
                image);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _applicationBootstrapper.HandleException(e.Exception);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

        private Mutex CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));
            var appGuid = ((GuidAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null"), typeof(GuidAttribute), false)).Value;
            return new Mutex(false, $"Global\\{appGuid}", out _, mutexSecurity);
        }
    }
}
