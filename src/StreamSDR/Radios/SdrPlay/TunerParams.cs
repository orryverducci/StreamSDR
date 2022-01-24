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
/// The parameters for a SDRPlay tuner.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TunerParams
{
    /// <summary>
    /// The tuner bandwidth. Defaults to <see cref="BwMhz.Bw200"/> (200 kHz).
    /// </summary>
    public BwMhz BwType;

    /// <summary>
    /// The tuner intermediate frequency. Defaults to <see cref="IfKhz.IfZero"/> (zero IF mode).
    /// </summary>
    public IfKhz IfType;

    /// <summary>
    /// The first local oscillator frequency of the tuner. Defaults to <see cref="LoMode.LoAuto"/>.
    /// </summary>
    public LoMode LoMode;

    /// <summary>
    /// The tuner gain setting parameters.
    /// </summary>
    public Gain Gain;

    /// <summary>
    /// The tuner RF frequency parameters.
    /// </summary>
    public RfFreq RfFreq;

    /// <summary>
    /// The tuner DC calibration parameters.
    /// </summary>
    public DcOffsetTuner DcOffsetTuner;
}
