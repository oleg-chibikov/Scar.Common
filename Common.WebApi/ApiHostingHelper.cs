using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scar.Common.WebApi
{
    public static class ApiHostingHelper
    {
        public static IHostBuilder RegisterWebApiHost(IHostBuilder hostBuilder, Uri baseUrl) =>
            hostBuilder.ConfigureWebHost(
                x =>
                {
                    x.UseKestrel().UseUrls(baseUrl.ToString()).UseContentRoot(Directory.GetCurrentDirectory()).UseIISIntegration().UseStartup<Startup>();
                });

        public static void RegisterServices(IServiceCollection servicesCollection, Assembly webApiAssembly)
        {
            servicesCollection.AddOptions().AddRouting().AddControllers().AddApplicationPart(webApiAssembly);
        }
    }
}
