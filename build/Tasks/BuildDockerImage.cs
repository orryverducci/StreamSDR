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
using System.Linq;
using Cake.Docker;
using Cake.MinVer;

namespace StreamSDR.Build.Tasks;

/// <summary>
/// Task to build the StreamSDR Docker image.
/// </summary>
[TaskName("BuildDockerImage")]
public sealed class BuildDockerImageTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // Set the container registry domain if one is specified
        string registryDomain = string.Empty;

        if (context.Settings.ContainerRegistry != null)
        {
            registryDomain = context.Settings.ContainerRegistry;

            // Add a trailing forward slash if there isn't one already
            if (!registryDomain.EndsWith('/'))
            {
                registryDomain += '/';
            }
        }

        // Get the app version from MinVer
        MinVerVersion version = context.MinVer(new MinVerSettings
        {
            DefaultPreReleasePhase = "preview",
            TagPrefix = "v"
        });

        // Get the latest Git commit SHA
        int exitCode = context.StartProcess("git", new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("rev-parse")
                .Append("HEAD"),
            RedirectStandardOutput = true
        }, out IEnumerable<string> gitOutput);

        // Check the exit code indicates it completed successfully
        if (exitCode != 0)
        {
            throw new Exception("Unable to get the Git commit SHA");
        }

        // Set the tags for the Docker image
        string[] tags;
        if (version.IsPreRelease)
        {
            tags = new string[]
            {
                $"{registryDomain}orryverducci/streamsdr:latest",
                $"{registryDomain}orryverducci/streamsdr:{version.Version}"
            };
        }
        else
        {
            tags = new string[]
            {
                $"{registryDomain}orryverducci/streamsdr:latest",
                $"{registryDomain}orryverducci/streamsdr:{version.Major}.{version.Minor}.{version.Patch}",
                $"{registryDomain}orryverducci/streamsdr:{version.Major}.{version.Minor}",
                $"{registryDomain}orryverducci/streamsdr:{version.Major}"
            };
        }

        // Set the labels for the Docker image
        string[] labels = new string[]
        {
            $"org.opencontainers.image.created=\"{DateTime.UtcNow.ToString("o")}\"",
            $"org.opencontainers.image.version=\"{version.Version}\"",
            $"org.opencontainers.image.revision=\"{string.Concat(gitOutput)}\""
        };

        // Build the Docker image
        if (context.Settings.MultiArchitecture == "true")
        {
            // Set the annotations for the multi-arch image index
            string[] annotations = new string[]
            {
                "org.opencontainers.image.title=\"StreamSDR\"",
                "org.opencontainers.image.description=\"Server for software defined radios\"",
                "org.opencontainers.image.url=\"https://streamsdr.io/\"",
                "org.opencontainers.image.source=\"https://github.com/orryverducci/StreamSDR\"",
                "org.opencontainers.image.vendor=\"Orry Verducci\"",
                "org.opencontainers.image.licenses=\"GPL-3.0-only\""
            };
            annotations = annotations.Concat(labels).ToArray();

            context.DockerBuildXBuild(new DockerBuildXBuildSettings
            {
                ArgumentCustomization = args =>
                {
                    foreach (string annotation in annotations)
                    {
                        args.Append($"--annotation {annotation}");
                    }

                    // Disable attestations to prevent unknown/unknown platform appearing on registries
                    args.Append("--provenance=false");
                    args.Append("--sbom false");

                    return args;
                },
                BuildArg = new string[] { $"version={version.Version}" },
                File = "../docker/Dockerfile",
                Label = labels,
                Platform = new string[] { "linux/amd64", "linux/arm64/v8" },
                Progress = "plain",
                Pull = true,
                Push = true,
                Tag = tags
            }, "../");
        }
        else
        {
            context.DockerBuild(new DockerImageBuildSettings
            {
                BuildArg = new string[] { $"version={version.Version}" },
                File = "../docker/Dockerfile",
                Label = labels,
                Progress = "plain",
                Pull = true,
                Tag = tags
            }, "../");
        }
    }
}
