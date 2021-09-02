using System;
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
            AnsiConsole.Render(new FigletText("StreamSDR").LeftAligned().Color(Color.DeepSkyBlue1));
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
                    services.AddHostedService<Server.RtlTcpServer>();
                });
    }
}
