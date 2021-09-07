using System;
using Cake.Frosting;

namespace StreamSDR.Build
{
    /// <summary>
    /// Provides the main functionality for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments the application is launched with.</param>
        public static int Main(string[] args) =>
            new CakeHost().UseContext<BuildContext>()
                          .Run(args);
    }
}
