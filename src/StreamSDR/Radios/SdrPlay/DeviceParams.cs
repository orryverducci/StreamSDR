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
using System.Runtime.InteropServices;

namespace StreamSDR.Radios.SdrPlay
{
    /// <summary>
    /// The parameters for a SDRPlay device and its tuners.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DeviceParams
    {
        /// <summary>
        /// The parameters for the device.
        /// </summary>
        public DevParams* DevParams;

        /// <summary>
        /// The parameters for the first tuner.
        /// </summary>
        public RxChannelParams* RxChannelA;

        /// <summary>
        /// The parameters for the second tuner on RSPduo devices.
        /// </summary>
        public RxChannelParams* RxChannelB;
    }
}
