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
    /// The parameters for the sampling rate of a SDRPlay device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FsFreq
    {
        /// <summary>
        /// The sample rate the device is operating at in Hertz. Defaults to 2 MHz.
        /// </summary>
        public double FsHz;

        [MarshalAs(UnmanagedType.U8)]
        public bool SyncUpdate;

        [MarshalAs(UnmanagedType.U8)]
        public bool ReCal;
    }
}
