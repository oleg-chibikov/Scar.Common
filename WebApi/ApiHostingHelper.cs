using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scar.Common.WebApi.ActionFilters;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Scar.Common.WebApi
{
    public static class ApiHostingHelper
    {
        public static IHostBuilder RegisterWebApiHost(IHostBuilder hostBuilder, Uri? baseUrl = null, string? baseDirectory = null, string? applicationKey = null)
        {
            return hostBuilder.ConfigureWebHost(
                x =>
                {
                    x.UseKestrel().UseContentRoot(baseDirectory ?? Directory.GetCurrentDirectory()).UseIISIntegration().UseStartup<Startup>();
                    if (!string.IsNullOrEmpty(applicationKey))
                    {
                        x.UseSetting(WebHostDefaults.ApplicationKey, applicationKey);
                    }

                    if (baseUrl != null)
                    {
                        x.UseUrls(baseUrl.ToString());
                    }
                });
        }

        public static IServiceCollection RegisterServices(
            IServiceCollection servicesCollection,
            Assembly webApiAssembly,
            Action<MvcOptions>? configureControllers = null,
            Action<IMvcBuilder>? configureMvc = null,
            Action<SwaggerGenOptions>? configureSwagger = null)
        {
            var mvcBuilder = servicesCollection.AddOptions()
                .AddRouting()
                .AddControllers(
                    mvcOptions =>
                    {
                        mvcOptions.Filters.Add(new CheckModelForNullAttribute());
                        mvcOptions.Filters.Add(new ValidateModelStateAttribute());
                        configureControllers?.Invoke(mvcOptions);
                    })
                .AddApplicationPart(webApiAssembly);
            configureMvc?.Invoke(mvcBuilder);
            return mvcBuilder.Services.AddSwaggerExamplesFromAssemblies().AddSwaggerGen(configureSwagger);
        }
    }
}
