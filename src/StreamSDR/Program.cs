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

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace StreamSDR
{
    /// <summary>
    /// Provides the main functionality for the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments the application is launched with.</param>
        public static void Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("StreamSDR").LeftAligned().Color(Color.DeepSkyBlue1));
            AnsiConsole.WriteLine();

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the builder used to set up and launch application host.
        /// </summary>
        /// <param name="args">The command line arguments the application is launched with.</param>
        /// <returns>The host builder.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder =>
                {
                    builder.ClearProviders()
                           .AddProvider(new Logging.SpectreConsoleLoggerProvider());
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

                    // Create a logger
                    ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                    {
                        builder.AddProvider(new Logging.SpectreConsoleLoggerProvider());
                    });
                    ILogger logger = loggerFactory.CreateLogger(typeof(Program));

                    // Get the configured radio type
                    string? radioType = hostContext.Configuration.GetValue<string>("radio");

                    // Add the service for the desired type of radio
                    switch (radioType)
                    {
                        case "rtlsdr":
                            services.AddSingleton<Radios.IRadio, Radios.RtlSdr.Radio>();
                            break;
                        default:
                            logger.LogWarning("The type of radio has not been specified, assuming rtl-sdr");
                            goto case "rtlsdr";
                    }

                    // Add the server service
                    services.AddHostedService<Server.RtlTcpServer>();
                });
    }
}
