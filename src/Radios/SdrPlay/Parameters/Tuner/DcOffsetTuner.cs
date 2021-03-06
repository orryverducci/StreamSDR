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

namespace StreamSDR.Radios.SdrPlay.Parameters.Tuner;
/// <summary>
/// The parameters for a SDRplay tuner DC calibration.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct DcOffsetTuner
{
    /// <summary>
    /// The DC calibration mode. Defaults to 3 (periodic mode).
    /// </summary>
    public byte DcCal;

    [MarshalAs(UnmanagedType.U8)]
    public bool SpeedUp;

    public int TrackTime;

    public int RefreshRateTime;
}
