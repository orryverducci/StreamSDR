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

namespace StreamSDR.Radios.SdrPlay;

/// <summary>
/// Represents an SDRPlay device.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Device
{
    /// <summary>
    /// The serial number of the device.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string SerNo;

    /// <summary>
    /// The hardware version of the device
    /// </summary>
    public HardwareVersion HwVer;

    /// <summary>
    /// The selected tuner, or the tuners available if the device has not yet been selected.
    /// </summary>
    public TunerSelect Tuner;

    /// <summary>
    /// The modes available on RSPduo devices.
    /// </summary>
    public RspDuoMode RspDuoMode;

    /// <summary>
    /// Indicator representing if the device is available for use.
    /// </summary>
    //public char Valid;

    /// <summary>
    /// The sample rate of the RSPduo slave.
    /// </summary>
    public double RspDuoSampleFreq;

    /// <summary>
    /// The device handle.
    /// </summary>
    public IntPtr Dev;
}
