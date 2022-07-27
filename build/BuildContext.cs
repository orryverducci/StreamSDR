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
using StreamSDR.Build.Configuration;

namespace StreamSDR.Build;

/// <summary>
/// Provides the build context for the Cake build.
/// </summary>
public sealed class BuildContext : FrostingContext
{
    /// <summary>
    /// The build settings.
    /// </summary>
    public Settings Settings { get; private set; } = new();

    /// <summary>
    /// The platform the application is being built for.
    /// </summary>
    public Platform Platform { get; private set; }

    // The identifier for the build, used to name the app artifact output folder
    public string BuildIdentifier { get; set; } = string.Empty;

    // The identifier for the installer, used to name the installer artifact output folder
    public string InstallerIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The path to the installation of MSBuild (only used on the Windows platform).
    /// </summary>
    public FilePath? MsBuildPath { get; set; }

    /// <summary>
    /// The path to the installation of CMake (only used on the Windows platform).
    /// </summary>
    public FilePath? CMakePath { get; set; }

    /// <summary>
    /// The path to the installation of WiX (only used on the Windows platform).
    /// </summary>
    public DirectoryPath? WixPath { get; set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="BuildContext"/> class.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    public BuildContext(ICakeContext context) : base(context)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Platform = Platform.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Platform = Platform.MacOS;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Platform = Platform.Linux;
        }
        else
        {
            throw new PlatformNotSupportedException("This platform is not supported");
        }
    }
}
