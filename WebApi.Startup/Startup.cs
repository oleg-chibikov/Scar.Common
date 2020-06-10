using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scar.Common.WebApiStartup
{
    class Startup
    {
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = env ?? throw new ArgumentNullException(nameof(env));
            _ = app ?? throw new ArgumentNullException(nameof(app));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(corsPolicyBuilder => corsPolicyBuilder.SetIsOriginAllowed(x => _ = true).AllowAnyMethod().AllowAnyHeader().AllowCredentials())
                .UseHttpsRedirection()
                .UseRouting()
                .UseEndpoints(
                    endpoints =>
                    {
                        endpoints.MapControllers();
                    })
                .UseSwagger()
                .UseSwaggerUI(
                    swaggerUiOptions =>
                    {
                        swaggerUiOptions.RoutePrefix = string.Empty;

                        swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", env.ApplicationName);
                    });
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}
