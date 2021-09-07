using System;
using Cake.Core;
using Cake.Common;
using Cake.Frosting;

namespace StreamSDR.Build
{
    /// <summary>
    /// Provides the build context for the Cake build.
    /// </summary>
    public class BuildContext : FrostingContext
    {
        /// <summary>
        /// The configuration to be passed to MsBuild.
        /// </summary>
        public string MsBuildConfiguration { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BuildContext"/> class.
        /// </summary>
        /// <param name="context">The Cake context.</param>
        public BuildContext(ICakeContext context) : base(context)
        {
            // Set build properties from passed in arguments
            MsBuildConfiguration = context.Argument("configuration", "Release");
        }
    }
}
