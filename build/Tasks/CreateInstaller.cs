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
public sealed class CreateInstallerTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.Platform == "osx";

    public override void Run(BuildContext context)
    {
        MinVerVersion version = context.MinVer(new MinVerSettings
        {
            DefaultPreReleasePhase = "preview",
            TagPrefix = "v"
        });

        switch (context.Platform)
        {
            case "osx":
                CreateMacPackage(context, version);
                break;
        }
    }

    private void CreateMacPackage(BuildContext context, MinVerVersion version)
    {
        // Create the output directory for the artifacts
        context.EnsureDirectoryExists(context.InstallerOutputFolder);

        // Build the arguments for pkgbuild
        ProcessArgumentBuilder arguments = new ProcessArgumentBuilder()
            .Append("--root")
            .Append(context.OutputFolder.FullPath)
            .Append("--identifier")
            .Append("io.streamsdr.app")
            .Append("--version")
            .Append(version.FileVersion)
            .Append("--install-location")
            .Append("/usr/local/bin");

        if (context.InstallerSigningCertificate != null)
        {
            arguments = arguments
                .Append("--sign")
                .AppendSecret('"' + context.InstallerSigningCertificate + '"');
        }

        arguments = arguments
            .Append(context.InstallerOutputFolder.CombineWithFilePath(context.File("streamsdr.pkg")).FullPath);

        context.StartProcess("pkgbuild", new ProcessSettings
        {
            Arguments = arguments
        });
    }
}
