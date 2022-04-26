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

using System.Collections.Generic;
using Cake.CMake;
using Cake.Common.Tools.MSBuild;

namespace StreamSDR.Build.Tasks;

/// <summary>
/// Task to build the rtl-sdr library.
/// </summary>
[TaskName("BuildRtlSdr")]
[IsDependentOn(typeof(BuildLibUsbTask))]
public sealed class BuildRtlSdrTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.Platform == Configuration.Platform.Windows;

    public override void Run(BuildContext context)
    {
        // Check MSBuild is available
        if (context.MsBuildPath == null)
        {
            throw new Exception("Unable to locate MSBuild or the Visual Studio 2019 C++ build tools");
        }

        // Check CMake is available
        if (context.CMakePath == null)
        {
            throw new Exception("Unable to locate CMake");
        }

        // Ensure the artifacts directory exists
        context.EnsureDirectoryExists(context.Settings.ArtifactsFolder!.Combine(context.BuildIdentifier));

        // Set the path for the rtl-sdr library to be output to
        FilePath outputPath = context.Settings.ArtifactsFolder.Combine(context.BuildIdentifier).CombineWithFilePath("rtlsdr.dll");

        // Create a build directory for the output from CMake
        context.EnsureDirectoryExists("../contrib/rtl-sdr/build");

        // Set libusb library path
        FilePath libUsbPath = context.Settings.Architecture switch
        {
            "arm64" => "../contrib/libusb/arm64/Release/dll/libusb-1.0.lib",
            _ => "../contrib/libusb/x64/Release/dll/libusb-1.0.lib"
        };

        // Set the CMake target architecture
        string cMakeArchitecture = context.Settings.Architecture switch
        {
            "arm64" => "ARM64",
            _ => "x64"
        };

        // Generate a Visual C++ project for rtl-sdr
        context.CMake("../contrib/rtl-sdr", new CMakeSettings
        {
            Generator = "Visual Studio 17 2022",
            Options = new List<string>()
            {
                { $"-A {cMakeArchitecture}" },
                { $"-DLIBUSB_LIBRARIES={context.MakeAbsolute(libUsbPath).FullPath}" },
                { $"-DLIBUSB_INCLUDE_DIRS={context.MakeAbsolute(context.Directory("../contrib/libusb/libusb")).FullPath}" },
                { "-DPKG_CONFIG_EXECUTABLE=C:\\non\\existent\\app.exe" } // A valid pkg-config install breaks the build, so we point to a non-existent executable
            },
            OutputPath = "../contrib/rtl-sdr/build",
            Toolset = "v143",
            ToolPath = context.CMakePath
        });

        // Set the MSBuild target architecture
        PlatformTarget msBuildArchitecture = context.Settings.Architecture switch
        {
            "arm64" => PlatformTarget.ARM64,
            _ => PlatformTarget.x64
        };

        // Build rtl-sdr using the VS 2022 build tools
        context.MSBuild("../contrib/rtl-sdr/build/src/rtl_sdr.vcxproj", new MSBuildSettings
        {
            Configuration = context.Settings.BuildConfiguration,
            MSBuildPlatform = MSBuildPlatform.x64,
            PlatformTarget = msBuildArchitecture,
            ToolPath = context.MsBuildPath
        });

        // Remove the rtl-sdr library from the artifacts folder if it already exists
        if (context.FileExists(outputPath))
        {
            context.DeleteFile(outputPath);
        }

        // Copy the built library to the artifacts folder
        context.CopyFile("../contrib/rtl-sdr/build/src/Release/rtlsdr.dll", outputPath);
    }
}
