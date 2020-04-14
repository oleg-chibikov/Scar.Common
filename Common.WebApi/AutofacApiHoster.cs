using System;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Common.Logging;
using Microsoft.Owin.Hosting;
using Owin;

namespace Scar.Common.WebApi
{
    public abstract class AutofacApiHoster : IDisposable
    {
        readonly IDisposable _apiHost;
        readonly ILifetimeScope _innerScope;
        readonly ILog _logger;
        bool _disposedValue;

        protected AutofacApiHoster(ILog logger, ILifetimeScope lifetimeScope)
        {
            _ = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            logger.Trace("Starting WebApi...");
            _innerScope = lifetimeScope.BeginLifetimeScope(innerBuilder => innerBuilder.RegisterApiControllers(ControllersAssembly).InstancePerDependency());
            _apiHost = WebApp.Start(
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

        protected abstract string BaseAddress { get; }

        protected abstract Assembly ControllersAssembly { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void RegisterRoutes(HttpRouteCollection routes);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _apiHost.Dispose();
                    _innerScope.Dispose();
                    _logger.Trace("WebApi has been stopped...");
                }

                _disposedValue = true;
            }
        }
    }
}
