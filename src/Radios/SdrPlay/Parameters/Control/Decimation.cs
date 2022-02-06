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
/// The parameters for decimation on a SDRPlay tuner.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Decimation
{
    /// <summary>
    /// The state of decimation. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
    /// </summary>
    [MarshalAs(UnmanagedType.U8)]
    public bool Enable;

    /// <summary>
    /// The decimation factor. Defaults to 1.
    /// </summary>
    public byte DecimationFactor;

    public byte WideBandSignal;
}
