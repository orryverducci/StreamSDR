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

namespace StreamSDR.Build;

/// <summary>
/// Provides the main functionality for the application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The entry point for the application.
    /// </summary>
    /// <param name="args">The command line arguments the application is launched with.</param>
    public static int Main(string[] args) =>
        new CakeHost().UseContext<BuildContext>()
                        .UseLifetime<BuildLifetime>()
                        .InstallTool(new Uri("dotnet:?package=minver-cli&version=6.0.0"))
                        .Run(args);
}
