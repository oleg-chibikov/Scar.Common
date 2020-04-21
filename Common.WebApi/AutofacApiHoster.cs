using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;

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


            var myStartup = new Startup(lifetimeScope);

            // copy existing config into memory
            var existingConfig = new MemoryConfigurationSource
            {
                InitialData = myStartup.Configuration
            };

            // create new configuration from existing config
            // and override whatever needed
            var testConfigBuilder = new ConfigurationBuilder()
                .Add(existingConfig)


            myStartup.Configuration = testConfigBuilder.Build();

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(myStartup);
                });

            var server = new TestServer(builder);

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

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

        class Startup : IStartup
        {
            readonly ILifetimeScope _lifetimeScope;
            public IConfigurationRoot Configuration { get; set; }

            public Startup(ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
            }

            // This method gets called by the runtime. Use this method to add services to the container.
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddMvc();

                // Create the container builder.
                var builder = new ContainerBuilder();
                builder.Populate(services);
                _applicationContainer = builder.Build();

                // Create the IServiceProvider based on the container.
                return new AutofacServiceProvider(_applicationContainer);
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app)
            {
                app.UseMvc();
            }
        }
    }
}
