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

using System.Reflection;
using Cake.Common.Diagnostics;
using Cake.Common.Tools.VSWhere;
using Cake.Common.Tools.VSWhere.Latest;

namespace StreamSDR.Build;

/// <summary>
/// Controls the lifetime of the Cake build.
/// </summary>
public sealed class BuildLifetime : FrostingLifetime<BuildContext>
{
    /// <summary>
    /// Setup method executed before the build. Sets the build configuration and locates the required tools.
    /// </summary>
    /// <param name="context">The build context.</param>
    public override void Setup(BuildContext context)
    {
        // Sets the build settings from the arguments that have been provided
        foreach (PropertyInfo setting in context.Settings.GetType().GetProperties())
        {
            Configuration.ArgumentAttribute? argument = setting.GetCustomAttribute<Configuration.ArgumentAttribute>();
            if (argument == null)
            {
                continue;
            }

            if (context.HasArgument(argument.Name))
            {
                setting.SetValue(context.Settings, context.Argument<string>(argument.Name));
            }
        }

        // Set the artifacts folder to the default path if not already set
        if (context.Settings.ArtifactsFolder == null)
        {
            context.Settings.ArtifactsFolder = "../artifacts";
        }

        // Log platform, architecture, configuration and artifacts folder
        context.Information($"Platform: {context.Platform}");
        context.Information($"Architecture: {context.Settings.Architecture}");
        context.Information($"Configuration: {context.Settings.BuildConfiguration}");
        context.Information($"Output path: {context.MakeAbsolute(context.Settings.ArtifactsFolder)}");

        // Check if the architecture being built is supported
        if (context.Settings.Architecture != "x64" && context.Settings.Architecture != "arm64" && !(context.Settings.Architecture == "arm" && context.Platform == Configuration.Platform.Linux))
        {
            throw new PlatformNotSupportedException("This architecture is not supported");
        }

        // Set the build and installer identifiers
        switch (context.Platform)
        {
            case Configuration.Platform.Windows:
                context.BuildIdentifier = $"win-{context.Settings.Architecture}";
                context.InstallerIdentifier = "win-installer";
                break;
            case Configuration.Platform.MacOS:
                context.BuildIdentifier = $"macos-{context.Settings.Architecture}";
                context.InstallerIdentifier = "macos-installer";
                break;
            case Configuration.Platform.Linux:
                context.BuildIdentifier = $"linux-{context.Settings.Architecture}";
                context.InstallerIdentifier = "linux-installer";
                break;
        }

        // Run platform specific setup
        switch (context.Platform)
        {
            case Configuration.Platform.Windows:
                WindowsSetup(context);
                break;
        }
    }

    /// <summary>
    /// Platform specific setup for the Windows platform. Locates the build tools required to build the native libraries.
    /// </summary>
    /// <param name="context">The build context.</param>
    private void WindowsSetup(BuildContext context)
    {
        // Find Visual Studio
        DirectoryPath? installationPath = context.VSWhereLatest(new VSWhereLatestSettings
        {
            IncludePrerelease = true,
            Requires = "Microsoft.Component.MSBuild Microsoft.VisualStudio.Component.VC.CMake.Project",
            Version = "17.0"
        });

        // Find MSBuild and check it is installed
        FilePath? msBuildPath = installationPath?.CombineWithFilePath("./MsBuild/Current/Bin/amd64/MSBuild.exe");
        if (msBuildPath != null && context.FileExists(msBuildPath))
        {
            context.MsBuildPath = msBuildPath;
            context.Information($"MSBuild path: {msBuildPath}");
        }
        else
        {
            context.Information("MSBuild path: Not found");
        }

        // Find CMake and check it is installed
        FilePath? cMakePath = installationPath?.CombineWithFilePath("./Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe");
        if (cMakePath != null && context.FileExists(cMakePath))
        {
            context.CMakePath = cMakePath;
            context.Information($"CMake path: {cMakePath}");
        }
        else
        {
            context.Information("CMake path: Not found");
        }
    }

    /// <summary>
    /// Teardown method executed after the build. This is an empty method required to inherit from <see cref="FrostingLifetime{TContext}"/>.
    /// </summary>
    /// <param name="context">The build context.</param>
    /// <param name="info">Teardown information.</param>
    public override void Teardown(BuildContext context, ITeardownContext info) { }
}
