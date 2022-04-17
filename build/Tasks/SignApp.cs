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
/// Task to sign the application.
/// </summary>
[TaskName("SignApp")]
[IsDependentOn(typeof(BuildStreamSdrTask))]
public sealed class SignAppTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.Platform == Configuration.Platform.MacOS && context.Settings.SigningCertificate != null;

    public override void Run(BuildContext context)
    {
        // Run codesign on the StreamSDR app binary
        int exitCode = context.StartProcess("codesign", new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("--timestamp")
                .Append("--force")
                .Append("--options=runtime")
                .Append("--entitlements")
                .Append("../src/Entitlements.plist")
                .Append("--sign")
                .AppendSecret('"' + context.Settings.SigningCertificate + '"')
                .Append(context.Settings.ArtifactsFolder!.CombineWithFilePath("streamsdr").FullPath)
        });

        // Check the exit code indicates it completed successfully
        if (exitCode != 0)
        {
            throw new Exception("Unable to sign app");
        }
    }
}
