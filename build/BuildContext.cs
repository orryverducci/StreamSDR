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

using System;
using Cake.Common;
using Cake.Core;
using Cake.Frosting;

namespace StreamSDR.Build
{
    /// <summary>
    /// Provides the build context for the Cake build.
    /// </summary>
    public class BuildContext : FrostingContext
    {
        /// <summary>
        /// The configuration to used while building the application and libraries.
        /// </summary>
        public string BuildConfiguration { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BuildContext"/> class.
        /// </summary>
        /// <param name="context">The Cake context.</param>
        public BuildContext(ICakeContext context) : base(context)
        {
            // Set build properties from passed in arguments
            BuildConfiguration = context.Argument("configuration", "Release");
        }
    }
}
