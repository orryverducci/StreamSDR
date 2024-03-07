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

    public override bool ShouldRun(BuildContext context) =>
        context.Platform == Configuration.Platform.Windows ||
        context.Platform == Configuration.Platform.MacOS;

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
            case Configuration.Platform.Windows:
                CreateWindowsInstaller(context, version);
                break;
            case Configuration.Platform.MacOS:
                CreateMacPackage(context, version);
                break;
        }
    }

    private void CreateWindowsInstaller(BuildContext context, MinVerVersion version)
    {
        // Check MSBuild is available
        if (context.MsBuildPath == null)
        {
            throw new Exception("Unable to locate MSBuild");
        }

        // Add the extensions to the WiX
        context.StartProcess(context.Tools.Resolve("wix.exe"), new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                    .Append("extension")
                    .Append("add")
                    .Append("-g")
                    .Append("WixToolset.UI.winext")
        });

        context.StartProcess(context.Tools.Resolve("wix.exe"), new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                    .Append("extension")
                    .Append("add")
                    .Append("-g")
                    .Append("WixToolset.Firewall.winext")
        });

        // Build the installer
        int wixExitCode = context.StartProcess(context.Tools.Resolve("wix.exe"), new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                    .Append("build")
                    .Append("-arch")
                    .Append("x64")
                    .Append("-define")
                    .Append($"Platform={context.Settings.Architecture}")
                    .Append("-define")
                    .Append($"Version={version.FileVersion}")
                    .Append("-ext")
                    .Append("WixToolset.UI.wixext")
                    .Append("-ext")
                    .Append("WixToolset.Firewall.wixext")
                    .Append("-b")
                    .Append(context.Settings.ArtifactsFolder!.Combine(context.BuildIdentifier).FullPath)
                    .Append("-b")
                    .Append("../assets")
                    .Append("-b")
                    .Append("../installers/windows")
                    .Append("-pdbtype")
                    .Append("none")
                    .Append("-out")
                    .Append(outputPath!.CombineWithFilePath("streamsdr.msi").FullPath)
                    .Append("../installers/windows/Product.wxs")
        });

        // Check the exit code indicates it completed successfully
        if (wixExitCode != 0)
        {
            throw new Exception("Unable to create MSI package");
        }
    }

    private void CreateMacPackage(BuildContext context, MinVerVersion version)
    {
        // Create a temporary folder
        DirectoryPath tempDir = context.Directory(System.IO.Path.GetTempPath());
        tempDir = tempDir.Combine(Guid.NewGuid().ToString());
        context.EnsureDirectoryExists(tempDir);

        try
        {
            // Run pkgbuild
            int pkgBuildExitCode = context.StartProcess("pkgbuild", new ProcessSettings
            {
                Arguments = new ProcessArgumentBuilder()
                    .Append("--root")
                    .Append(context.Settings.ArtifactsFolder!.Combine("macos-universal").FullPath)
                    .Append("--identifier")
                    .Append("io.streamsdr.app.pkg")
                    .Append("--version")
                    .Append(version.FileVersion)
                    .Append("--install-location")
                    .Append("/usr/local/bin")
                    .Append(tempDir.CombineWithFilePath(context.File("streamsdr.pkg")).FullPath)
            });

            // Check the exit code indicates it completed successfully
            if (pkgBuildExitCode != 0)
            {
                throw new Exception("Unable to create app component package");
            }

            // Build the arguments for productbuild
            ProcessArgumentBuilder productBuildArguments = new ProcessArgumentBuilder()
                .Append("--distribution")
                .Append(context.File("../installers/macos/distribution.xml"))
                .Append("--package-path")
                .Append(tempDir.FullPath)
                .Append("--resources")
                .Append(context.Directory("../installers/macos"))
                .Append("--identifier")
                .Append("io.streamsdr.installer")
                .Append("--version")
                .Append(version.FileVersion);

            if (context.Settings.InstallerSigningCertificate != null)
            {
                productBuildArguments = productBuildArguments
                    .Append("--sign")
                    .AppendSecret('"' + context.Settings.InstallerSigningCertificate + '"');
            }

            productBuildArguments = productBuildArguments.Append(outputPath!.CombineWithFilePath(context.File("streamsdr.pkg")).FullPath);

            // Run productbuild
            int productBuildExitCode = context.StartProcess("productbuild", new ProcessSettings
            {
                Arguments = productBuildArguments
            });

            // Check the exit code indicates it completed successfully
            if (productBuildExitCode != 0)
            {
                throw new Exception("Unable to create installer");
            }
        }
        finally
        {
            // Delete the temporary folder
            context.DeleteDirectory(tempDir, new DeleteDirectorySettings
            {
                Recursive = true
            });
        }
    }
}
