using System;
using Cake.Frosting;

namespace StreamSDR.Build
{
    /// <summary>
    /// Task to build the application.
    /// </summary>
    [TaskName("Default")]
    [IsDependentOn(typeof(BuildStreamSdrTask))]
    public class DefaultTask : FrostingTask
    {
    }
}