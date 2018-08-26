using System;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Common.Logging;
using JetBrains.Annotations;
using Microsoft.Owin.Hosting;
using Owin;

namespace Scar.Common.WebApi
{
    [UsedImplicitly]
    public abstract class AutofacApiHoster : IDisposable
    {
        [NotNull]
        private readonly IDisposable _apiHost;

        [NotNull]
        private readonly ILifetimeScope _innerScope;

        [NotNull]
        private readonly ILog _logger;

        protected AutofacApiHoster([NotNull] ILog logger, [NotNull] ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null)
            {
                throw new ArgumentNullException(nameof(lifetimeScope));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            logger.Trace("Starting WebApi...");
            _innerScope = lifetimeScope.BeginLifetimeScope(innerBuilder => innerBuilder.RegisterApiControllers(ControllersAssembly).InstancePerDependency());
            _apiHost = WebApp.Start(
                // ReSharper disable once VirtualMemberCallInConstructor
                new StartOptions(BaseAddress),
                app =>
                {
                    var config = new HttpConfiguration();
                    config.Services.Replace(typeof(IExceptionHandler), new LocalizableExceptionHandler());
                    config.Services.Replace(typeof(IExceptionLogger), new LocalizableExceptionLogger());
                    config.MessageHandlers.Add(new MessageLoggingHandler());
                    RegisterRoutes(config.Routes);
                    config.DependencyResolver = new AutofacWebApiDependencyResolver(_innerScope);
                    app.UseAutofacMiddleware(_innerScope);
                    app.UseAutofacWebApi(config);
                    app.DisposeScopeOnAppDisposing(_innerScope);
                    app.UseWebApi(config);
                });
            logger.Trace("WebApi is started");
        }

        [NotNull]
        protected abstract string BaseAddress { get; }

        [NotNull]
        protected abstract Assembly ControllersAssembly { get; }

        public void Dispose()
        {
            _apiHost.Dispose();
            _innerScope.Dispose();
            _logger.Trace("WebApi has been stopped...");
        }

        protected abstract void RegisterRoutes([NotNull] HttpRouteCollection routes);
    }
}