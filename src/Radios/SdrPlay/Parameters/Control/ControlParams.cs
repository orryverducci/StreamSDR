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

namespace StreamSDR.Radios.SdrPlay.Parameters.Control;

/// <summary>
/// The parameters for the control of a SDRplay tuner.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ControlParams
{
    /// <summary>
    /// The DC offset options.
    /// </summary>
    public DcOffset DcOffset;

    /// <summary>
    /// The tuner decimation options.
    /// </summary>
    public Decimation Decimation;

    /// <summary>
    /// The tuner automatic gain control options.
    /// </summary>
    public Agc Agc;

    /// <summary>
    /// The mode used for ADS-B reception. Defaults to <see cref="AdsbMode.AdsbDecimation"/>
    /// </summary>
    public AdsbMode AdsbMode;
}
