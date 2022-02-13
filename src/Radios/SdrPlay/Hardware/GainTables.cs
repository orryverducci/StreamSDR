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

namespace StreamSDR.Radios.SdrPlay.Hardware;

/// <summary>
/// Provides the lookup tables to translate a gain level to the LNA and IF gain settings for each band.
/// </summary>
internal interface IGainTables
{
    /// <summary>
    /// Dictionary of LNA gain levels for the available radio bands.
    /// </summary>
    public IDictionary<RadioBand, byte[]> LnaStates { get; protected set; }

    /// <summary>
    /// Dictionary of IF gain levels for the available radio bands.
    /// </summary>
    public IDictionary<RadioBand, int[]> IfGains { get; protected set; }
}
