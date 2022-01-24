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
/// The parameters for a gain change event.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct GainCbParam
{
    /// <summary>
    /// The current RF gain reduction.
    /// </summary>
    public uint GRDb;

    /// <summary>
    /// The current LNA gain reduction.
    /// </summary>
    public uint LnaGRDb;

    /// <summary>
    /// The current system gain.
    /// </summary>
    public double CurrGain;
}
