using System;
using Autofac;
using Common.Logging;
using Easy.MessageHub;
using Scar.Common.Localization;
using Scar.Common.Messages;
using System.Globalization;
using System.Threading;
using Scar.Common.ApplicationLifetime;
using Scar.Common.ApplicationLifetime.Contracts;
using Xamarin.Forms;

namespace Scar.Common.Xamarin.Startup
{
    public class CultureManager : ICultureManager
    {
        public void ChangeCulture(CultureInfo cultureInfo)
        {
            //TODO:
        }
    }

    public class ApplicationTerminator : IApplicationTerminator
    {
        public void Terminate()
        {
            // TODO: Platform specific https://stackoverflow.com/questions/29257929/how-to-terminate-a-xamarin-application
        }
    }

    public abstract class BaseApplication : Application
    {
        private readonly IApplicationStartupBootstrapper _applicationBootstrapper;
        protected ILifetimeScope Container => _applicationBootstrapper.Container;
        protected ILog Logger => _applicationBootstrapper.Logger;
        protected IMessageHub Messenger => _applicationBootstrapper.Messenger;

        protected BaseApplication(IAssemblyInfoProvider assemblyInfoProvider)
        {
            _ = assemblyInfoProvider ?? throw new ArgumentNullException(nameof(assemblyInfoProvider));
            var cultureManager = new CultureManager();
            var applicationTerminator = new ApplicationTerminator();
            // ReSharper disable VirtualMemberCallInConstructor
            _applicationBootstrapper = new ApplicationStartupBootstrapper(cultureManager, applicationTerminator, ShowMessage, null, RegisterDependencies, assemblyInfoProvider, AlreadyRunningMessage, WaitAfterOldInstanceKillMilliseconds, NewInstanceHandling.AllowMultiple, GetStartupCulture());
            // ReSharper restore VirtualMemberCallInConstructor
        }

        protected virtual int WaitAfterOldInstanceKillMilliseconds => 0;
        protected virtual string? AlreadyRunningMessage => null;
        protected SynchronizationContext? SynchronizationContext => _applicationBootstrapper.SynchronizationContext;
        protected virtual CultureInfo? GetStartupCulture() => null;

        protected abstract void OnStartup();

        protected virtual void RegisterDependencies(ContainerBuilder builder)
        {
        }

        protected virtual void ShowMessage(Message message)
        {
            //TODO: Include Page into message.
        }

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
    }
}
