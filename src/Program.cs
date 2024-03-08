/*
 * This file is part of StreamSDR.
 *
 * StreamSDR is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * StreamSDR is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with StreamSDR. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Spectre.Console;
using SpectreConsoleLogger;

namespace StreamSDR;

/// <summary>
/// Provides the main functionality for the application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// If the application is in debug mode.
    /// </summary>
    public static bool DebugMode { get; private set; }

    /// <summary>
    /// The entry point for the application.
    /// </summary>
    /// <param name="args">The command line arguments the application is launched with.</param>
    public static void Main(string[] args)
    {
        Console.Title = "StreamSDR";

        AnsiConsole.Write(new FigletText("StreamSDR").LeftJustified().Color(Color.DeepSkyBlue1));
        string version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unavailable";
        AnsiConsole.Write(new Rule($"[DeepSkyBlue1]Version {version}[/]").LeftJustified());
        AnsiConsole.WriteLine();

        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), LibraryLoader.ImportResolver);

        CreateHostBuilder(args).Build().Run();
    }

    /// <summary>
    /// Creates the builder used to set up and launch application host.
    /// </summary>
    /// <param name="args">The command line arguments the application is launched with.</param>
    /// <returns>The host builder.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostContext, builder) =>
            {
                // Determine if debug mode should be enabled
                DebugMode = hostContext.HostingEnvironment.IsDevelopment() || hostContext.Configuration.GetValue<bool>("debug");

                // Build the logger
                builder.ClearProviders()
                        .AddSpectreConsole(DebugMode ? SpectreConsoleLogger.Style.Standard : SpectreConsoleLogger.Style.Extended)
                        .SetMinimumLevel(DebugMode ? LogLevel.Debug : LogLevel.Information);

                // Add the debug logger if running in debug mode
                if (DebugMode)
                {
                    builder.AddProvider(new DebugLoggerProvider());
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Suppress the status messages logged by the console lifetime if not in debug mode
                if (!DebugMode)
                {
                    services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
                }

                // Create a logger
                ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddSpectreConsole(DebugMode ? SpectreConsoleLogger.Style.Standard : SpectreConsoleLogger.Style.Extended);
                });
                ILogger logger = loggerFactory.CreateLogger(typeof(Program));

                // Get the configured radio type
                string? radioType = hostContext.Configuration.GetValue<string>("radio");

                // Add the service for the desired type of radio
                switch (radioType)
                {
                    case "rtlsdr":
                        services.AddSingleton<Radios.RadioBase, Radios.RtlSdr.Radio>();
                        break;
                    case "sdrplay":
                        services.AddSingleton<Radios.RadioBase, Radios.SdrPlay.Radio>();
                        break;
                    case "dummy":
                        services.AddSingleton<Radios.RadioBase, Radios.Dummy.Radio>();
                        break;
                    default:
                        logger.LogWarning("The type of radio has not been specified, assuming rtl-sdr");
                        goto case "rtlsdr";
                }

                // Add the server service
                services.AddHostedService<Server.RtlTcpServer>();
            });
}
