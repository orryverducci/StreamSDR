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

namespace StreamSDR.Radios.SdrPlay.RspDuo;

/// <summary>
/// The parameters for a SDRplay RSP2 tuner.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct TunerParams
{
    /// <summary>
    /// The state of the Bias T. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
    /// </summary>
    [MarshalAs(UnmanagedType.U8)]
    public bool BiasTEnable;

    /// <summary>
    /// The port that should be used for AM reception. Defaults to <see cref="AmPortSelect.AmPort2"/>.
    /// </summary>
    public AmPortSelect AmPortSel;

    /// <summary>
    /// The state of the MW broadcast band notch filter on the high impedance port. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
    /// </summary>
    [MarshalAs(UnmanagedType.U8)]
    public bool Tuner1AmNotchEnable;

    /// <summary>
    /// The state of the AM/FM broadcast band notch. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
    /// </summary>
    [MarshalAs(UnmanagedType.U8)]
    public bool RfNotchEnable;

    /// <summary>
    /// The state of the DAB band notch. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
    /// </summary>
    [MarshalAs(UnmanagedType.U8)]
    public bool RfDabNotchEnable;
}
