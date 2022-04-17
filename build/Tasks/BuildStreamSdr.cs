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

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;

namespace StreamSDR.Build.Tasks;

/// <summary>
/// Task to build the StreamSDR application.
/// </summary>
[TaskName("BuildStreamSDR")]
[IsDependentOn(typeof(BuildRtlSdrTask))]
[IsDependentOn(typeof(CopySdrPlayApiTask))]
public sealed class BuildStreamSdrTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // Generate the .NET runtime for the build
        string runtime = context.Platform switch
        {
            Configuration.Platform.Windows => $"win-{context.Settings.Architecture}",
            Configuration.Platform.MacOS => $"osx-{context.Settings.Architecture}",
            Configuration.Platform.Linux => $"linux-{context.Settings.Architecture}",
            _ => throw new Exception("Unable to set runtime")
        };

        // Ensure the artifacts directory exists
        context.EnsureDirectoryExists(context.Settings.ArtifactsFolder!.Combine(context.BuildIdentifier));

        // Build StreamSDR to the artifacts folder
        context.DotNetPublish("../src/StreamSDR.csproj", new DotNetPublishSettings
        {
            Configuration = context.Settings.BuildConfiguration,
            OutputDirectory = context.Settings.ArtifactsFolder.Combine(context.BuildIdentifier),
            Runtime = runtime,
            SelfContained = true
        });
    }
}
