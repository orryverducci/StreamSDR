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

namespace StreamSDR.Radios.SdrPlay
{
    /// <summary>
    /// The parameters for a RSPdx device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RspDxParams
    {
        /// <summary>
        /// The state of HDR mode. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
        /// </summary>
        [MarshalAs(UnmanagedType.U8)]
        public bool HdrEnable;

        /// <summary>
        /// The state of the bias T. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
        /// </summary>
        [MarshalAs(UnmanagedType.U8)]
        public bool BiasTEnable;

        /// <summary>
        /// The antenna that has been selected.
        /// </summary>
        public RspDxAntennaSelect AntennaSel;

        /// <summary>
        /// The state of the broadcast band (AM/FM) notch filter. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
        /// </summary>
        [MarshalAs(UnmanagedType.U8)]
        public bool RfNotchEnable;

        /// <summary>
        /// The state of the DAB notch filter. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
        /// </summary>
        [MarshalAs(UnmanagedType.U8)]
        public bool RfDabNotchEnable;
    }
}
