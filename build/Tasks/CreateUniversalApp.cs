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
/// Task to create a universal Mac app from x64 and arm64 builds.
/// </summary>
[TaskName("CreateUniversalApp")]
public sealed class CreateUniversalAppTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.Platform == Configuration.Platform.MacOS;

    public override void Run(BuildContext context)
    {
        // Set the paths to the x64 and arm64 binaries, and the folder to output the universal binary to
        FilePath x64Path = context.Settings.ArtifactsFolder!.Combine("macos-x64").CombineWithFilePath("streamsdr");
        FilePath arm64Path = context.Settings.ArtifactsFolder!.Combine("macos-arm64").CombineWithFilePath("streamsdr");
        DirectoryPath outputPath = context.Settings.ArtifactsFolder!.Combine("macos-universal");

        // Check for a x64 build
        if (!context.FileExists(x64Path))
        {
            throw new Exception("Unable to find a macOS x64 build of the application");
        }

        // Check for a ARM64 build
        if (!context.FileExists(arm64Path))
        {
            throw new Exception("Unable to find a macOS ARM64 build of the application");
        }

        // Create the output directory for the universal binary if it doesn't already
        context.EnsureDirectoryExists(outputPath);

        // Run lipo to create the universal binary
        int exitCode = context.StartProcess("lipo", new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("-create")
                .Append("-output")
                .Append(outputPath.CombineWithFilePath("streamsdr").FullPath)
                .Append(x64Path.FullPath)
                .Append(arm64Path.FullPath)
        });

        // Check the exit code indicates it completed successfully
        if (exitCode != 0)
        {
            throw new Exception("Unable to create universal Mac app");
        }
    }
}
