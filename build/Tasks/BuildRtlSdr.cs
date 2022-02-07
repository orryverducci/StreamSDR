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
using Cake.Common.Tools.MSBuild;
using Cake.CMake;

namespace StreamSDR.Build;

/// <summary>
/// Task to build the rtl-sdr library.
/// </summary>
[TaskName("BuildRtlSdr")]
[IsDependentOn(typeof(BuildLibUsbTask))]
public sealed class BuildRtlSdrTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.Platform == "win";

    public override void Run(BuildContext context)
    {
        if (context.MsBuildPath == null)
        {
            throw new Exception("Unable to locate MSBuild or the Visual Studio 2019 C++ build tools");
        }

        if (context.CMakePath == null)
        {
            throw new Exception("Unable to locate CMake");
        }

        context.CreateDirectory("../contrib/rtl-sdr/build");

        context.CMake("../contrib/rtl-sdr", new CMakeSettings
        {
            Generator = "Visual Studio 17 2022",
            Options = new List<string>()
            {
                { $"-DLIBUSB_LIBRARIES={context.MakeAbsolute(context.Directory("../contrib/libusb/x64/Release/dll/libusb-1.0.lib")).FullPath}" },
                { $"-DLIBUSB_INCLUDE_DIRS={context.MakeAbsolute(context.Directory("../contrib/libusb/libusb")).FullPath}" }
            },
            OutputPath = "../contrib/rtl-sdr/build",
            Toolset = "v142",
            ToolPath = context.CMakePath
        });

        context.MSBuild("../contrib/rtl-sdr/build/src/rtl_sdr.vcxproj", new MSBuildSettings
        {
            Configuration = context.BuildConfiguration,
            MSBuildPlatform = MSBuildPlatform.x64,
            PlatformTarget = PlatformTarget.x64,
            ToolPath = context.MsBuildPath
        });

        if (context.FileExists(context.OutputFolder.CombineWithFilePath(context.File("rtlsdr.dll"))))
        {
            context.DeleteFile(context.OutputFolder.CombineWithFilePath(context.File("rtlsdr.dll")));
        }

        context.CopyFile("../contrib/rtl-sdr/build/src/Release/rtlsdr.dll", context.OutputFolder.CombineWithFilePath(context.File("rtlsdr.dll")));
    }
}
