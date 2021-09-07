using System;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Publish;
using Cake.Frosting;

namespace StreamSDR.Build
{
    /// <summary>
    /// Task to build the StreamSDR application.
    /// </summary>
    [TaskName("BuildStreamSDR")]
    public sealed class BuildStreamSdrTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.DotNetCorePublish("../src/StreamSDR/StreamSDR.csproj", new DotNetCorePublishSettings
            {
                Configuration = context.MsBuildConfiguration,
                OutputDirectory = "../artifacts"
            });
        }
    }
}