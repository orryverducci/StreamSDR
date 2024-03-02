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

namespace StreamSDR.Build.Tasks;

/// <summary>
/// Task to notarize the Mac installer package.
/// </summary>
[TaskName("NotarizeInstaller")]
[IsDependentOn(typeof(CreateInstallerTask))]
public sealed class NotarizeInstallerTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) =>
        context.Platform == Configuration.Platform.MacOS &&
        context.Settings.InstallerSigningCertificate != null &&
        context.Settings.AppleID != null &&
        context.Settings.AppleIDPassword != null &&
        context.Settings.AppleDeveloperTeam != null;

    public override void Run(BuildContext context)
    {
        // Set the path for the installer
        FilePath installerPath = context.Settings.ArtifactsFolder!.Combine(context.InstallerIdentifier).CombineWithFilePath("streamsdr.pkg");

        // Create a temporary folder
        DirectoryPath tempDir = context.Directory(System.IO.Path.GetTempPath());
        tempDir = tempDir.Combine(Guid.NewGuid().ToString());
        context.EnsureDirectoryExists(tempDir);

        try
        {
            // Copy the installer package to the temporary directory and zip it
            DirectoryPath appDir = tempDir.Combine("upload");
            context.EnsureDirectoryExists(appDir);
            context.CopyFile(installerPath, appDir.CombineWithFilePath("streamsdr.pkg"));
            context.Zip(appDir, tempDir.CombineWithFilePath("streamsdr.zip"));

            // Send the app for notarization
            int exitCode = context.StartProcess("xcrun", new ProcessSettings
            {
                Arguments = new ProcessArgumentBuilder()
                    .Append("notarytool")
                    .Append("submit")
                    .Append(tempDir.CombineWithFilePath("streamsdr.zip").FullPath)
                    .Append("--apple-id")
                    .AppendSecret('"' + context.Settings.AppleID + '"')
                    .Append("--team-id")
                    .AppendSecret('"' + context.Settings.AppleDeveloperTeam + '"')
                    .Append("--password")
                    .AppendSecret('"' + context.Settings.AppleIDPassword + '"')
                    .Append("--wait")
            });

            // Check the exit code to ensure the installer was notarized
            if (exitCode != 0)
            {
                throw new Exception("Unable to notarize the installer");
            }

            // Staple the notarization to the installer
            exitCode = context.StartProcess("xcrun", new ProcessSettings
            {
                Arguments = new ProcessArgumentBuilder()
                    .Append("stapler")
                    .Append("staple")
                    .Append(installerPath.FullPath)
            });

            // Check the exit code to ensure the notarization has been stapled to the installer
            if (exitCode != 0)
            {
                throw new Exception("Unable to staple the notarization to the installer");
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
