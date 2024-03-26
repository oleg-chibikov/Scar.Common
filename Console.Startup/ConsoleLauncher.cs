using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scar.Common.ApplicationLifetime.Core;
using Scar.Common.Localization;
using Serilog;
using Serilog.Events;

namespace Scar.Common.Console.Startup;

public class ConsoleLauncher
{
    static bool _exitSystem;

    ApplicationStartupBootstrapper? _bootstrapper;

    public async Task SetupAsync(
        Action<ContainerBuilder> registerDependencies,
        Func<ILifetimeScope, Task> launch,
        Func<IConfigurationSection, object>? readConfig = null)
    {
        System.Console.OutputEncoding = Encoding.UTF8;

        try
        {
            var platform = Environment.OSVersion.Platform;
            System.Console.WriteLine("Registering exit signal handler for {0}...", platform);
            IExitSignal exitSignalHandler =
                // Some boilerplate to react to close window event, CTRL-C, kill, etc
                platform is PlatformID.Win32NT or PlatformID.Win32S or PlatformID.Win32Windows
                ? new WinExitSignal()
                : new UnixExitSignal();

            exitSignalHandler.Exit += AppExitHandler;
        }
        catch (Exception ex)
        {
            System.Console.Write("Cannot set app exit handler : {0}", ex);
        }

        var appDirectory = AppContext.BaseDirectory ?? throw new InvalidOperationException("Cannot get base directory");
        Directory.SetCurrentDirectory(appDirectory);
        object? config = null;

        Log.Information(
            "Application Directory is {ApplicationDirectory}",
            appDirectory);

        var assemblyInfoProvider = new AssemblyInfoProvider(
            new EntryAssemblyProvider(),
            new SpecialPathsProvider());
        var cultureManager = new ConsoleCultureManager();
        var applicationTerminator = new ConsoleApplicationTerminator();
        _bootstrapper = new ApplicationStartupBootstrapper(
            cultureManager,
            applicationTerminator,
            System.Console.WriteLine,
            () => new Mutex(
                false,
                $"Global\\{assemblyInfoProvider.AppGuid}",
                out _),
            (builder) =>
            {
                Log.Debug("Registering dependencies...");
                builder.RegisterType<SynchronizationContext>().AsSelf().SingleInstance();
                if (config != null)
                {
                    builder.RegisterInstance(config).AsSelf().AsImplementedInterfaces().SingleInstance();
                }

                registerDependencies(builder);
                Log.Debug("Registered dependencies");
            },
            assemblyInfoProvider,
            hostBuilder => hostBuilder.UseConsoleLifetime(),
            _ =>
            {
            },
            (hostBuilderContext, loggingBuilder) =>
            {
                var configuration = hostBuilderContext.Configuration;
                var appSettings = configuration.GetSection("AppSettings");
                if (readConfig != null)
                {
                    config = readConfig(appSettings);
                    System.Console.WriteLine(
                        "Config {0}",
                        config);
                }

                SetupSerilogLogger(
                    appDirectory,
                    appSettings["Environment"] ?? "Development");
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            },
            afterBuild: launch);
        _bootstrapper.BeforeStart();
        await _bootstrapper.OnStartAsync().ConfigureAwait(false);
        //hold the console so it doesn’t run off the end
        while (!_exitSystem)
        {
            await Task.Delay(500).ConfigureAwait(false);
        }
    }

    static void SetupSerilogLogger(string appDirectory, string environment)
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Is(LogEventLevel.Debug).WriteTo.
            Console(formatProvider: CultureInfo.CurrentCulture).WriteTo.File(
                Path.Combine(
                    appDirectory,
                    "logs",
                    "log.log"),
                rollingInterval: RollingInterval.Hour,
                shared: true,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 10,
                formatProvider: CultureInfo.CurrentCulture).Enrich.WithProperty(
                "Environment",
                environment).CreateLogger();
    }

    void AppExitHandler(object? sender, EventArgs args)
    {
        System.Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

        //do your cleanup here
        _bootstrapper?.OnExitAsync().Wait();

        System.Console.WriteLine("Cleanup complete");

        //allow main to run off
        _exitSystem = true;

        //shutdown right away so there are no lingering threads
        Environment.Exit(-1);
    }
}
