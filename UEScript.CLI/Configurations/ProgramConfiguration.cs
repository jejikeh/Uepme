using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UEScript.CLI.Commands;
using UEScript.CLI.Services;
using UEScript.CLI.Services.Impl;
using UEScript.Logging;

namespace UEScript.CLI.Configurations;

public static class ProgramConfiguration
{
    public static CliConfiguration Configure(this CliConfiguration cliConfiguration, string[] args)
    {
        cliConfiguration.UseHost(
            _ => Host.CreateDefaultBuilder().UseContentRoot(AppDomain.CurrentDomain.BaseDirectory),
            host => host.ConfigureHostBuilder(cliConfiguration, args));

        return cliConfiguration;
    }
    
    public static CliConfiguration BuildCommandLine()
    {
        return new CliConfiguration(ConfigureRootCommand.AddRootCommand());
    }

    private static IHostBuilder ConfigureHostBuilder(this IHostBuilder host, CliConfiguration cliConfiguration, string[] args)
    {
        ConfigureAppConfiguration(host);

        host.ConfigureLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddUEScriptLogger(ConfigureLogger);
            
            var parseResult = cliConfiguration.Parse(args);
            var debugLog = parseResult.GetValue<bool>("--debug-log");
            if (debugLog)
            {
                builder.AddFilter("Microsoft.Hosting", LogLevel.Information);
            }
        });

        ConfigureServices(host);
        
        return host;
    }

    private static void ConfigureServices(IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {
            services.AddHttpClient();
            
            services.AddSingleton<IUnrealEngineAssociationRepository, UnrealEngineEngineAssociationRepository>();
            services.AddSingleton<IUnrealBuildToolService, UnrealBuildToolService>();
            services.AddSingleton<IUnrealEngineEditorService, UnrealEngineEditorService>();
            services.AddSingleton<IUnrealArgsBuilder, UnrealArgsBuilder>();

            services.AddSingleton<IFileDownloaderService, FileDownloaderService>();
            services.AddSingleton<IFileExtractor, FileExtractor>();
            services.AddSingleton<IArchiveExtractor, ArchiveExtractor>();
            services.AddSingleton<IArchiveService, ArchiveService>(); 
        });
    }

    private static void ConfigureLogger(UEScriptLoggerConfiguration configure)
    {
        // @Cleanup: Check how to do this stuff automatically
        var configuration = GetConfiguration();
        var logLevelColors = configuration.GetSection("Logging:UEScript:LogLevelColors").Get<Dictionary<LogLevel, ConsoleColor>>();

        if (logLevelColors is not null)
        {
            return;
        }
        
        // Fallback to default values
        // @Cleanup: Move it just to UEScriptLoggerConfiguration default value
        logLevelColors = new Dictionary<LogLevel, ConsoleColor>()
        {
            { LogLevel.Trace, ConsoleColor.Gray },
            { LogLevel.Debug, ConsoleColor.Gray },
            { LogLevel.Information, ConsoleColor.DarkGreen },
            { LogLevel.Warning, ConsoleColor.Cyan },
            { LogLevel.Error, ConsoleColor.Red },
            { LogLevel.Critical, ConsoleColor.Red }
        };

        configure.LogLevelColors = logLevelColors;
    }

    private static void ConfigureAppConfiguration(IHostBuilder host)
    {
        host.ConfigureAppConfiguration(config =>
        {
            config
                .AddJsonFile(AppDomain.CurrentDomain.BaseDirectory + "appsettings.json")
                .AddJsonFile(
                    AppDomain.CurrentDomain.BaseDirectory +
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables();
        });
    }

    private static IConfiguration GetConfiguration()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(AppDomain.CurrentDomain.BaseDirectory + "appsettings.json")
            .AddJsonFile(AppDomain.CurrentDomain.BaseDirectory + $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        return config;
    }
}