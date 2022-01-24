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
    /// The parameters for a SDRPlay tuner.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RxChannelParams
    {
        /// <summary>
        /// The parameters for the device tuner that are available on all SDRPlay devices.
        /// </summary>
        public TunerParams TunerParams;

        /// <summary>
        /// The tuner control parameters.
        /// </summary>
        public ControlParams CtrlParams;

        /// <summary>
        /// The parameters for a RSP1A device tuner.
        /// </summary>
        public Rsp1aTunerParams Rsp1aTunerParams;

        /// <summary>
        /// The parameters for a RSP2 device tuner.
        /// </summary>
        public Rsp2TunerParams Rsp2TunerParams;

        /// <summary>
        /// The parameters for a RSPduo device tuner.
        /// </summary>
        public RspDuoTunerParams RspDuoTunerParams;

        /// <summary>
        /// The parameters for a RSPdx device tuner.
        /// </summary>
        public RspDxTunerParams RspDxTunerParams;
    }
}
