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

namespace StreamSDR.Radios.SdrPlay
{
    /// <summary>
    /// Provides extension methods to the <see cref="HardwareVersion"/> enum.
    /// </summary>
    public static class HardwareVersionExtension
    {
        /// <summary>
        /// Converts the hardware version to the device model as a string.
        /// </summary>
        /// <param name="hardwareVersion">The <see cref="HardwareVersion"/> provided by the SDRPlay API.</param>
        /// <returns>The device model name.</returns>
        public static string ToDeviceModel(this HardwareVersion hardwareVersion) => hardwareVersion switch
        {
            HardwareVersion.Rsp1 => "RSP1",
            HardwareVersion.Rsp1A => "RSP1A",
            HardwareVersion.Rsp2 => "RSP2",
            HardwareVersion.RspDuo => "RSPduo",
            HardwareVersion.RspDx => "RSPdx",
            _ => "Unknown"
        };
    }
}

