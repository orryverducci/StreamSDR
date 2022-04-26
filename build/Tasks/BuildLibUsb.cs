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

using Cake.Common.Tools.MSBuild;

namespace StreamSDR.Build.Tasks;

/// <summary>
/// Task to build the libusb library.
/// </summary>
[TaskName("BuildLibusb")]
public sealed class BuildLibUsbTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.Platform == Configuration.Platform.Windows;

    public override void Run(BuildContext context)
    {
        // Check MSBuild is available
        if (context.MsBuildPath == null)
        {
            throw new Exception("Unable to locate MSBuild or the Visual Studio 2022 C++ build tools");
        }

        // Ensure the artifacts directory exists
        context.EnsureDirectoryExists(context.Settings.ArtifactsFolder!.Combine(context.BuildIdentifier));

        // Set the path for the libusb library to be output to
        FilePath outputPath = context.Settings.ArtifactsFolder.Combine(context.BuildIdentifier).CombineWithFilePath("libusb-1.0.dll");

        // Set the MSBuild target architecture
        PlatformTarget architecture = context.Settings.Architecture switch
        {
            "arm64" => PlatformTarget.ARM64,
            _ => PlatformTarget.x64
        };

        // Build libusb using the VS 2022 build tools
        context.MSBuild("../contrib/libusb/msvc/libusb_dll_2019.vcxproj", new MSBuildSettings
        {
            ArgumentCustomization = args => args.Append("/p:PlatformToolset=v143"),
            Configuration = context.Settings.BuildConfiguration,
            MSBuildPlatform = MSBuildPlatform.x64,
            PlatformTarget = architecture,
            ToolPath = context.MsBuildPath,
        });

        // Remove libusb from the artifacts folder if it already exists
        if (context.FileExists(outputPath))
        {
            context.DeleteFile(outputPath);
        }

        // Copy the built library to the artifacts folder
        FilePath builtLibraryPath = context.Settings.Architecture switch
        {
            "arm64" => "../contrib/libusb/arm64/Release/dll/libusb-1.0.dll",
            _ => "../contrib/libusb/x64/Release/dll/libusb-1.0.dll"
        };
        context.CopyFile(builtLibraryPath, outputPath);
    }
}
