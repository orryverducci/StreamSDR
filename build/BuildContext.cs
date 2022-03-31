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

using System.Runtime.InteropServices;

namespace StreamSDR.Build;

/// <summary>
/// Provides the build context for the Cake build.
/// </summary>
public sealed class BuildContext : FrostingContext
{
    /// <summary>
    /// The build settings.
    /// </summary>
    public Configuration.Settings Settings { get; private set; } = new();

    /// <summary>
    /// The path to the installation of MSBuild (only used on the Windows platform).
    /// </summary>
    public FilePath? MsBuildPath { get; set; }

    /// <summary>
    /// The path to the installation of CMake (only used on the Windows platform).
    /// </summary>
    public FilePath? CMakePath { get; set; }

    /// <summary>
    /// The platform the application is being built for.
    /// </summary>
    public string Platform { get; private set; }

    /// <summary>
    /// The directory to output the build artifacts to.
    /// </summary>
    public DirectoryPath OutputFolder { get; set; }

    /// <summary>
    /// The directory to output the installer build artifacts to.
    /// </summary>
    public DirectoryPath InstallerOutputFolder { get; set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="BuildContext"/> class.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    public BuildContext(ICakeContext context) : base(context)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Platform = "win";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Platform = "osx";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Platform = "linux";
        }
        else
        {
            throw new PlatformNotSupportedException("This platform is not supported");
        }

        OutputFolder = Environment.WorkingDirectory;
        InstallerOutputFolder = Environment.WorkingDirectory;
    }
}
