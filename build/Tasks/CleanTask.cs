using System;
using Cake.Common.IO;
using Cake.Frosting;

namespace StreamSDR.Build
{
    /// <summary>
    /// Task to clean up all the artifacts left by builds.
    /// </summary>
    [TaskName("Clean")]
    public sealed class CleanTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.CleanDirectory($"../artifacts");
            context.CleanDirectory($"../src/StreamSDR/bin");
            context.CleanDirectory($"../src/StreamSDR/obj");
        }
    }
}
