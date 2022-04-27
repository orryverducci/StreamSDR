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

using Cake.MinVer;

namespace StreamSDR.Build.Tasks;

/// <summary>
/// Task to create the installer for the app.
/// </summary>
[TaskName("CreateInstaller")]
[IsDependentOn(typeof(SignAppTask))]
public sealed class CreateInstallerTask : FrostingTask<BuildContext>
{
    /// <summary>
    /// The path to the installer output directory.
    /// </summary>
    private DirectoryPath? outputPath;

    public override bool ShouldRun(BuildContext context) => context.Platform == Configuration.Platform.MacOS;

    public override void Run(BuildContext context)
    {
        // Get the app version from MinVer
        MinVerVersion version = context.MinVer(new MinVerSettings
        {
            DefaultPreReleasePhase = "preview",
            TagPrefix = "v"
        });

        // Set the path for the installer to be output to
        outputPath = context.Settings.ArtifactsFolder!.Combine(context.InstallerIdentifier);

        // Create the output directory for the installer if it doesn't already
        context.EnsureDirectoryExists(outputPath);

        // Create the installer for the specified platform
        switch (context.Platform)
        {
            case Configuration.Platform.MacOS:
                CreateMacPackage(context, version);
                break;
        }
    }

    private void CreateMacPackage(BuildContext context, MinVerVersion version)
    {
        // Build the arguments for pkgbuild
        ProcessArgumentBuilder arguments = new ProcessArgumentBuilder()
            .Append("--root")
            .Append(context.Settings.ArtifactsFolder!.Combine("macos-universal").FullPath)
            .Append("--identifier")
            .Append("io.streamsdr.app")
            .Append("--version")
            .Append(version.FileVersion)
            .Append("--install-location")
            .Append("/usr/local/bin");

        if (context.Settings.InstallerSigningCertificate != null)
        {
            arguments = arguments
                .Append("--sign")
                .AppendSecret('"' + context.Settings.InstallerSigningCertificate + '"');
        }

        arguments = arguments.Append(outputPath!.CombineWithFilePath(context.File("streamsdr.pkg")).FullPath);

        // Run pkgbuild
        int exitCode = context.StartProcess("pkgbuild", new ProcessSettings
        {
            Arguments = arguments
        });

        // Check the exit code indicates it completed successfully
        if (exitCode != 0)
        {
            throw new Exception("Unable to create installer");
        }
    }
}
