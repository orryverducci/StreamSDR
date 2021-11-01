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
        /// If debug mode is enabled.
        /// </summary>
        private readonly bool _debug;

        /// <summary>
        /// The directory of loggers for each category.
        /// </summary>
        private readonly ConcurrentDictionary<string, SpectreConsoleLogger> _loggers = new();

        /// <summary>
        /// Initialises a new instance of the <see cref="SpectreConsoleLoggerProvider"/> class.
        /// </summary>
        /// <param name="debug">If debug mode is enabled.</param>
        public SpectreConsoleLoggerProvider(bool debug) => _debug = debug;

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, new SpectreConsoleLogger(categoryName, _debug));

        /// <summary>
        /// Releases all resources used by the <see cref="SpectreConsoleLoggerProvider"/> object.
        /// </summary>
        public void Dispose() => _loggers.Clear();
    }
}
