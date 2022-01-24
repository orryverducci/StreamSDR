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
    /// The parameters for a SDRPlay tuner gain setting.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Gain
    {
        /// <summary>
        /// The gain reduction in dB. Defaults to 50 dB.
        /// </summary>
        public int GrDb;

        /// <summary>
        /// The state of the LNA.
        /// </summary>
        public byte LnaState;

        [MarshalAs(UnmanagedType.U8)]
        public bool SyncUpdate;

        /// <summary>
        /// The minimum gain reduction mode. Defaults to <see cref="MinGainReduction.NormalMinGr"/>.
        /// </summary>
        public MinGainReduction MinGr;

        /// <summary>
        /// The LNA gain values.
        /// </summary>
        public GainValues GainVals;
    }
}
