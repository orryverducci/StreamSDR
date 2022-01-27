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

using System;
using System.Runtime.InteropServices;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Core.IO;
using Cake.Frosting;

namespace StreamSDR.Build
{
    /// <summary>
    /// Task to build the libusb library.
    /// </summary>
    [TaskName("BuildLibusb")]
    public sealed class BuildLibUsbTask : FrostingTask<BuildContext>
    {
        public override bool ShouldRun(BuildContext context) => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public override void Run(BuildContext context)
        {
            if (context.MsBuildPath == null)
            {
                throw new Exception("Unable to locate MSBuild or the Visual Studio 2019 C++ build tools");
            }

            context.MSBuild("../contrib/libusb/msvc/libusb_dll_2019.vcxproj", new MSBuildSettings
            {
                Configuration = context.BuildConfiguration,
                MSBuildPlatform = MSBuildPlatform.x64,
                ToolPath = context.MsBuildPath
            });

            if (context.FileExists("../artifacts/libusb-1.0.dll"))
            {
                context.DeleteFile("../artifacts/libusb-1.0.dll");
            }

            context.CopyFile("../contrib/libusb/Win32/Release/dll/libusb-1.0.dll", "../artifacts/libusb-1.0.dll");
        }
    }
}
