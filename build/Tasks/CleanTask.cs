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
/// Task to clean up all the artifacts left by builds.
/// </summary>
[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // Delete the artifacts folder
        context.EnsureDirectoryDoesNotExist(context.Settings.ArtifactsFolder);

        // Delete the libusb build folders
        context.EnsureDirectoryDoesNotExist("../contrib/libusb/x64");
        context.EnsureDirectoryDoesNotExist("../contrib/libusb/arm64");

        // Delete the rtl-sdr build folder
        context.EnsureDirectoryDoesNotExist("../contrib/rtl-sdr/build");
    }
}
