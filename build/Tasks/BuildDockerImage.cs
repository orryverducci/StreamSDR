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

using Cake.Docker;
using Cake.MinVer;

namespace StreamSDR.Build.Tasks;

/// <summary>
/// Task to build the StreamSDR application.
/// </summary>
[TaskName("BuildDockerImage")]
public sealed class BuildDockerImage : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        MinVerVersion version = context.MinVer(new MinVerSettings
        {
            DefaultPreReleasePhase = "preview",
            TagPrefix = "v"
        });

        string[] tags;
        if (version.IsPreRelease)
        {
            tags = new string[]
            {
                $"{context.ContainerRegistry}orryverducci/streamsdr:latest",
                $"{context.ContainerRegistry}orryverducci/streamsdr:{version.Version}"
            };
        }
        else
        {
            tags = new string[]
            {
                $"{context.ContainerRegistry}orryverducci/streamsdr:latest",
                $"{context.ContainerRegistry}orryverducci/streamsdr:{version.Major}.{version.Minor}.{version.Patch}",
                $"{context.ContainerRegistry}orryverducci/streamsdr:{version.Major}.{version.Minor}",
                $"{context.ContainerRegistry}orryverducci/streamsdr:{version.Major}"
            };
        }

        context.DockerBuild(new DockerImageBuildSettings
        {
            BuildArg = new string[] { $"version={version.Version}" },
            Pull = true,
            Tag = tags,
        }, "../");
    }
}
