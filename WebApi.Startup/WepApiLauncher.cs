using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Scar.Common.ApplicationLifetime.Core;
using Serilog;
using Serilog.Events;

namespace Scar.Common.WebApi.Startup;

public class WepApiLauncher : IWepApiLauncher
{
    readonly ApplicationStartupBootstrapper _applicationBootstrapper;
    readonly AssemblyInfoProvider _assemblyInfoProvider;

    public WepApiLauncher(
        Action<ContainerBuilder, IConfiguration>? registerDependencies = null,
        Func<IHostBuilder, IHostBuilder>? configureHost = null,
        Action<IServiceCollection>? configureServices = null,
        Action<ILifetimeScope>? afterBuild = null,
        Action<HostBuilderContext, ILoggingBuilder>? configureLogging = null,
        string? alreadyRunningMessage = null,
        int waitAfterOldInstanceKillMilliseconds = 0,
        NewInstanceHandling newInstanceHandling = NewInstanceHandling.AllowMultiple,
        CultureInfo? startupCulture = null,
        Assembly? webApiAssembly = null,
        IReadOnlyCollection<Assembly>? additionalXmlCommentsAssemblies = null,
        Uri? baseUrl = null)
    {
        IConfiguration? configuration = null;
        var cultureManager = new ConsoleCultureManager();
        var applicationTerminator = new ConsoleApplicationTerminator();
        _assemblyInfoProvider = new AssemblyInfoProvider(new EntryAssemblyProvider(), new SpecialPathsProvider());
        webApiAssembly ??= Assembly.GetCallingAssembly();
        var baseDirectory = AppContext.BaseDirectory ?? throw new InvalidOperationException("Cannot get base directory");
        Directory.SetCurrentDirectory(baseDirectory);
        _applicationBootstrapper = new ApplicationStartupBootstrapper(
            cultureManager,
            applicationTerminator,
            _ => { },
            CreateMutex,
            containerBuilder => registerDependencies?.Invoke(containerBuilder, configuration ?? throw new InvalidOperationException("Configuration was not initialized")),
            _assemblyInfoProvider,
            hostBuilder =>
            {
                configureHost?.Invoke(hostBuilder);
                hostBuilder.UseConsoleLifetime();
                return ApiHostingHelper.RegisterWebApiHost(hostBuilder, baseUrl: baseUrl, baseDirectory: baseDirectory, applicationKey: webApiAssembly.GetName().Name);
            },
            services =>
            {
                var sp = services.BuildServiceProvider();
                configuration = sp.GetRequiredService<IConfiguration>();
                var appSettings = configuration.GetSection("AppSettings");
                var appName = appSettings[AppSettingsConstants.AppNameKey];
                var appVersion = appSettings[AppSettingsConstants.AppVersionKey];
                var environment = appSettings[AppSettingsConstants.EnvironmentKey];

                if (environment == "Production")
                {
                    services.AddHttpsRedirection(
                        options =>
                        {
                            options.HttpsPort = 8443;
                        });
                }

                ApiHostingHelper.RegisterServices(
                        services,
                        webApiAssembly,
                        configureMvc: mvcBuilder => mvcBuilder.AddJsonOptions(
                            options =>
                            {
                                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                            }),
                        configureSwagger: swaggerGenOptions =>
                        {
                            swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo { Title = appName, Version = appVersion });

                            var xmlDocAssemblies = new List<Assembly>
                            {
                                webApiAssembly
                            };
                            if (additionalXmlCommentsAssemblies != null)
                            {
                                xmlDocAssemblies.AddRange(additionalXmlCommentsAssemblies);
                            }

                            foreach (var xmlFilePath in xmlDocAssemblies.Select(assembly => assembly.GetName().Name + ".xml")
                                         .Select(xmlFileName => Path.Combine(baseDirectory, xmlFileName))
                                         .Where(File.Exists))
                            {
                                swaggerGenOptions.IncludeXmlComments(xmlFilePath);
                            }

                            // If there are two similar routes - this will fix the collision by choosing the first one
                            swaggerGenOptions.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                        })
                    .Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true)
                    .AddHttpContextAccessor();
                configureServices?.Invoke(services);
            },
            (hostBuilderContext, loggingBuilder) =>
            {
                SetupLogging(baseDirectory, hostBuilderContext.HostingEnvironment.EnvironmentName, LogEventLevel.Debug);
                loggingBuilder.AddSerilog();
                configureLogging?.Invoke(hostBuilderContext, loggingBuilder);
            },
            alreadyRunningMessage,
            waitAfterOldInstanceKillMilliseconds,
            newInstanceHandling,
            startupCulture,
            baseDirectory,
            afterBuild);
        _applicationBootstrapper.BeforeStart();
        Configuration = configuration ?? throw new InvalidOperationException("Configuration was not initialized");
    }

    public IConfiguration Configuration { get; }

    public async Task BuildAndRunHostAsync(string[] args)
    {
        await _applicationBootstrapper.OnStartAsync().ConfigureAwait(false);
    }

    static void SetupLogging(string baseDirectory, string environment, LogEventLevel logLevel) =>
        Log.Logger = new LoggerConfiguration().MinimumLevel.Is(logLevel)
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(Path.Combine(baseDirectory, "logs", "log.log"), rollingInterval: RollingInterval.Hour, shared: true, rollOnFileSizeLimit: true, retainedFileCountLimit: 10, formatProvider: CultureInfo.InvariantCulture)
            .Enrich.WithProperty(AppSettingsConstants.EnvironmentKey, environment)
            .CreateLogger();

    Mutex CreateMutex()
    {
        return new Mutex(false, $"Global\\{_assemblyInfoProvider.AppGuid}", out _);
    }
}
