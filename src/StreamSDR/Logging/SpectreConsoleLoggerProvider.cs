using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace StreamSDR.Logging
{
    /// <summary>
    /// Provides loggers that outputs to the console using the <see cref="N:Spectre.Console"/> library.
    /// </summary>
    public class SpectreConsoleLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// The directory of loggers for each category.
        /// </summary>
        private readonly ConcurrentDictionary<string, SpectreConsoleLogger> _loggers = new();

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, new SpectreConsoleLogger(categoryName));

        /// <summary>
        /// Releases all resources used by the <see cref="SpectreConsoleLoggerProvider"/> object.
        /// </summary>
        public void Dispose() => _loggers.Clear();
    }
}
