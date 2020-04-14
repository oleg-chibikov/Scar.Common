using System;
using System.Globalization;
using System.Threading;
using Autofac;
using Common.Logging;
using Easy.MessageHub;
using Scar.Common.ApplicationLifetime;
using Scar.Common.ApplicationLifetime.Contracts;
using Scar.Common.Messages;
using Xamarin.Forms;

namespace Scar.Common.Xamarin.Startup
{
    public abstract class BaseApplication : Application
    {
        readonly IApplicationStartupBootstrapper _applicationBootstrapper;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "Calling GetStartupCulture here is OK")]
        protected BaseApplication(IAssemblyInfoProvider assemblyInfoProvider)
        {
            _ = assemblyInfoProvider ?? throw new ArgumentNullException(nameof(assemblyInfoProvider));
            var cultureManager = new CultureManager();
            var applicationTerminator = new ApplicationTerminator();

            // ReSharper disable VirtualMemberCallInConstructor
            _applicationBootstrapper = new ApplicationStartupBootstrapper(
                cultureManager,
                applicationTerminator,
                ShowMessage,
                null,
                RegisterDependencies,
                assemblyInfoProvider,
                AlreadyRunningMessage,
                WaitAfterOldInstanceKillMilliseconds,
                NewInstanceHandling.AllowMultiple,
                GetStartupCulture());

            // ReSharper restore VirtualMemberCallInConstructor
        }

        protected ILifetimeScope Container => _applicationBootstrapper.Container;

        protected ILog Logger => _applicationBootstrapper.Logger;

        protected IMessageHub Messenger => _applicationBootstrapper.Messenger;

        protected virtual int WaitAfterOldInstanceKillMilliseconds => 0;

        protected virtual string? AlreadyRunningMessage => null;

        protected SynchronizationContext? SynchronizationContext => _applicationBootstrapper.SynchronizationContext;

        protected override void OnStart()
        {
            _applicationBootstrapper.OnStart();
            OnStartup();
        }

        protected override void OnSleep()
        {
            // TODO: WHEN to dispose?
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        protected virtual CultureInfo? GetStartupCulture() => null;

        protected abstract void OnStartup();

        protected virtual void RegisterDependencies(ContainerBuilder builder)
        {
        }

        protected virtual void ShowMessage(Message message)
        {
            // TODO: Include Page into message.
        }
    }
}
