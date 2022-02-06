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
/// The parameters for the automatic gain control on a SDRPlay tuner.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Agc
{
    /// <summary>
    /// The state of the AGC and the control scheme it is using.
    /// </summary>
    public AgcControl Enable; // default: sdrplay_api_AGC_50HZ

    /// <summary>
    /// The AGC setpoint in dBFS. Defaults to -60 dBFS.
    /// </summary>
    public int SetPointDbfs;

    /// <summary>
    /// The AGC attack period in milliseconds. Defaults to 0.
    /// </summary>
    public ushort AttackMs;

    /// <summary>
    /// The AGC decay period in milliseconds. Defaults to 0.
    /// </summary>
    public ushort DecayMs;

    /// <summary>
    /// The delay before the AGC decay in milliseconds. Defaults to 0.
    /// </summary>
    public ushort DecayDelayMs;

    /// <summary>
    /// The AGC decay threshold in dB. Defaults to 0 dB.
    /// </summary>
    public ushort DecayThresholdDb;

    [MarshalAs(UnmanagedType.I8)]
    public int SyncUpdate;
}
