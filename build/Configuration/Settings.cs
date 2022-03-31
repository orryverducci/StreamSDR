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

namespace StreamSDR.Build.Configuration;

public record Settings
{
    /// <summary>
    /// The configuration to used while building the application and libraries.
    /// </summary>
    [Argument("configuration")]
    public string BuildConfiguration { get; private set; } = "Release";

    /// <summary>
    /// The architecture the application is being built for.
    /// </summary>
    [Argument("architecture")]
    public string Architecture { get; private set; } = RuntimeInformation.OSArchitecture.ToString().ToLower();

    /// <summary>
    /// The domain for the Docker container registry to be used.
    /// </summary>
    [Argument("registry")]
    public string? ContainerRegistry { get; private set; }

    /// <summary>
    /// The certificate to be used when signing the application.
    /// </summary>
    [Argument("appcert")]
    public string? SigningCertificate { get; private set; }

    /// <summary>
    /// The certificate to be used when signing the installer.
    /// </summary>
    [Argument("installcert")]
    public string? InstallerSigningCertificate { get; private set; }

    /// <summary>
    /// The Apple ID account to be used when notarizing the application.
    /// </summary>
    [Argument("appleid")]
    public string? AppleID { get; private set; }

    /// <summary>
    /// The Apple ID password to be used when notarizing the application.
    /// </summary>
    [Argument("applepassword")]
    public string? AppleIDPassword { get; private set; }

    /// <summary>
    /// The Apple developer team ID to be used when notarizing the application.
    /// </summary>
    [Argument("teamid")]
    public string? AppleDeveloperTeam { get; private set; }
}
