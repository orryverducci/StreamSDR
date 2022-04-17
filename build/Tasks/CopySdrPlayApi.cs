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

using Cake.Common.Diagnostics;

namespace StreamSDR.Build.Tasks;

/// <summary>
/// Task to copy the SDRplay API library.
/// </summary>
[TaskName("CopySdrPlayApi")]
[ContinueOnError]
public sealed class CopySdrPlayApiTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.Platform == Configuration.Platform.Windows;

    public override void Run(BuildContext context)
    {
        // Get the path to the Program Files directory
        DirectoryPath programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        // Set the path to the SDRplay API library
        FilePath libraryPath = programFilesPath.CombineWithFilePath("SDRplay/API/x64/sdrplay_api.dll");

        // Check the SDRplay API library has been installed
        if (!context.FileExists(libraryPath))
        {
            context.Error($"Unable to find the SDRplay API at {libraryPath}");
            throw new Exception("Unable to find the SDRplay API");
        }

        // Set the path for the SDRplay API library to be output to
        FilePath outputPath = context.Settings.ArtifactsFolder!.Combine(context.BuildIdentifier).CombineWithFilePath("sdrplay_api.dll");

        // Remove the SDRplay API library from the artifacts folder if it already exists
        if (context.FileExists(outputPath))
        {
            context.DeleteFile(outputPath);
        }

        // Copy the SDRplay API library to the artifacts folder
        context.CopyFile(libraryPath, outputPath);
    }
}
