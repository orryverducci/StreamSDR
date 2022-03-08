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
    /// The configuration to used while building the application and libraries.
    /// </summary>
    public string BuildConfiguration { get; private set; }

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
    /// The architecture the application is being built for.
    /// </summary>
    public string Architecture { get; private set; }

    /// <summary>
    /// The directory to output the build artifacts to.
    /// </summary>
    public DirectoryPath OutputFolder { get; private set; }

    /// <summary>
    /// The domain for the Docker container registry to be used.
    /// </summary>
    public string ContainerRegistry { get; private set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="BuildContext"/> class.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    public BuildContext(ICakeContext context) : base(context)
    {
        // Set build properties from passed in arguments
        BuildConfiguration = context.Argument("configuration", "Release");

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

        string? architecture = context.Argument<string?>("architecture", null);
        if (Architecture == null)
        {
            Architecture = RuntimeInformation.OSArchitecture.ToString().ToLower();
        }
        if (Architecture != "x64" && Architecture != "arm" && Architecture != "arm64")
        {
            throw new PlatformNotSupportedException("This architecture is not supported");
        }

        // Set the folder for the build output
        OutputFolder = context.Directory($"../artifacts/{Platform}-{Architecture}");

        // Set the Docker container registry, if one is specified
        if (context.HasArgument("registry"))
        {
            string registryDomain = context.Argument<string>("registry");

            // Add a trailing forward slash if there isn't one already
            if (!registryDomain.EndsWith('/'))
            {
                registryDomain += '/';
            }

            ContainerRegistry = registryDomain;
        }
        else
        {
            ContainerRegistry = string.Empty;
        }
    }
}
