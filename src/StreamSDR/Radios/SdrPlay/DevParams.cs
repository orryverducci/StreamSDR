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
    /// The parameters for a SDRPlay device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DevParams
    {
        /// <summary>
        /// The tuner frequency correction in parts per million (PPM). Defaults to 0.
        /// </summary>
        public double Ppm;

        /// <summary>
        /// The device sampling rate parameters.
        /// </summary>
        public FsFreq FsFreq;

        /// <summary>
        /// The device synchronous update parameters.
        /// </summary>
        public SyncUpdate SyncUpdate;

        /// <summary>
        /// The device reset flags.
        /// </summary>
        public ResetFlags ResetFlags;

        /// <summary>
        /// The device transfer mode. Defaults to isochronous.
        /// </summary>
        public TransferMode Mode;

        /// <summary>
        /// The number of samples per packet. Defaults to 0, indicating it should be set automatically by the device.
        /// </summary>
        public uint SamplesPerPkt;

        /// <summary>
        /// The parameters for a RSP1A device.
        /// </summary>
        public Rsp1aParams Rsp1aParams;

        /// <summary>
        /// The parameters for a RSP2 device.
        /// </summary>
        public Rsp2Params Rsp2Params;

        /// <summary>
        /// The parameters for a RSPduo device.
        /// </summary>
        public RspDuoParams RspDuoParams;

        /// <summary>
        /// The parameters for a RSPdx device.
        /// </summary>
        public RspDxParams RspDxParams;
    }
}
