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

namespace StreamSDR.Build;

/// <summary>
/// Task to clean up all the artifacts left by builds.
/// </summary>
[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.CleanDirectory($"../artifacts");

        if (context.DirectoryExists("../src/bin"))
        {
            context.DeleteDirectory(context.Directory("../src/bin").Path, new DeleteDirectorySettings
            {
                Recursive = true
            });
        }

        if (context.DirectoryExists("../src/obj"))
        {
            context.DeleteDirectory(context.Directory("../src/obj").Path, new DeleteDirectorySettings
            {
                Recursive = true
            });
        }

        if (context.DirectoryExists("../contrib/libusb/x64"))
        {
            context.DeleteDirectory(context.Directory("../contrib/libusb/x64").Path, new DeleteDirectorySettings
            {
                Recursive = true
            });
        }

        if (context.DirectoryExists("../contrib/rtl-sdr/build"))
        {
            context.DeleteDirectory(context.Directory("../contrib/rtl-sdr/build").Path, new DeleteDirectorySettings
            {
                Recursive = true
            });
        }
    }
}
