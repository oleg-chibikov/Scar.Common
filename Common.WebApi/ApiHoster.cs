using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scar.Common.WebApi
{
    public abstract class ApiHoster : IApiHoster
    {
        public abstract Uri BaseUrl { get; }

        protected abstract Assembly WebApiAssembly { get; }

        public async Task RegisterWebApiHostAsync()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHost(
                    x =>
                    {
                        x.UseKestrel().UseUrls(BaseUrl.ToString()).UseContentRoot(Directory.GetCurrentDirectory()).UseIISIntegration().UseStartup<Startup>();
                    })
                .ConfigureServices(
                    x =>
                    {
                        x.AddOptions().AddRouting().AddControllers().AddApplicationPart(WebApiAssembly);
                        RegisterDependencies(x);
                    })
                .Build();
            await host.RunAsync();
        }

        protected abstract void RegisterDependencies(IServiceCollection x);
    }
}
