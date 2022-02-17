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
[TaskName("CopySdrPlay")]
[ContinueOnError]
public sealed class CopySdrPlayApi : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.Platform == "win";

    public override void Run(BuildContext context)
    {
        DirectoryPath programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        FilePath libraryPath = programFilesPath.CombineWithFilePath(context.File("SDRplay/API/x64/sdrplay_api.dll"));

        if (!context.FileExists(libraryPath))
        {
            context.Error($"Unable to find the SDRplay API at {libraryPath.FullPath}");
            throw new Exception("Unable to find the SDRplay API");
        }

        if (context.FileExists(context.OutputFolder.CombineWithFilePath(context.File("sdrplay_api.dll"))))
        {
            context.DeleteFile(context.OutputFolder.CombineWithFilePath(context.File("sdrplay_api.dll")));
        }

        context.CopyFile(libraryPath, context.OutputFolder.CombineWithFilePath(context.File("sdrplay_api.dll")));
    }
}
