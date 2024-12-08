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

using StreamSDR.Radios.SdrPlay.Hardware;

namespace StreamSDR.Radios.SdrPlay.RspDx;

/// <summary>
/// Provides the lookup tables to translate a gain level to the LNA and IF gain settings for each band on a RSPdx.
/// </summary>
internal sealed record GainTables : IGainTables
{
    /// <inheritdoc/>
    public IDictionary<RadioBand, byte[]> LnaStates { get; set; } = new Dictionary<RadioBand, byte[]>()
    {
        { RadioBand.AM, [18, 18, 18, 18, 18, 18, 17, 16, 14, 13, 12, 11, 10, 9, 7, 6, 5, 5, 5, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0] },
        { RadioBand.VHF, [26, 26, 26, 26, 26, 25, 23, 22, 20, 19, 17, 16, 14, 13, 11, 10, 8, 7, 5, 5, 5, 3, 2, 0, 0, 0, 0, 0, 0] },
        { RadioBand.III, [26, 26, 26, 26, 26, 25, 23, 22, 20, 19, 17, 16, 14, 13, 11, 10, 8, 7, 5, 5, 5, 3, 2, 0, 0, 0, 0, 0, 0] },
        { RadioBand.UHFLower, [27, 27, 27, 27, 27, 26, 24, 23, 21, 20, 18, 17, 15, 14, 12, 11, 9, 8, 6, 6, 5, 3, 2, 0, 0, 0, 0, 0, 0] },
        { RadioBand.UHFUpper, [20, 20, 20, 20, 20, 20, 18, 17, 16, 14, 13, 12, 11, 9, 8, 7, 7, 5, 4, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0] },
        { RadioBand.L, [18, 18, 18, 18, 18, 18, 16, 15, 14, 13, 11, 10, 9, 8, 7, 6, 6, 6, 5, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0] }
    };

    /// <inheritdoc/>
    public IDictionary<RadioBand, int[]> IfGains { get; set; } = new Dictionary<RadioBand, int[]>()
    {
        { RadioBand.AM, [59, 55, 52, 48, 45, 41, 41, 40, 43, 42, 42, 41, 41, 40, 42, 42, 47, 44, 40, 43, 42, 42, 41, 38, 34, 31, 27, 24, 20] },
        { RadioBand.VHF, [59, 55, 52, 48, 45, 41, 41, 40, 43, 42, 42, 41, 41, 40, 42, 42, 47, 44, 40, 43, 42, 42, 41, 38, 34, 31, 27, 24, 20] },
        { RadioBand.III, [59, 55, 50, 46, 41, 40, 42, 40, 42, 40, 42, 41, 42, 41, 43, 41, 43, 41, 49, 45, 40, 42, 40, 42, 38, 33, 29, 24, 20] },
        { RadioBand.UHFLower, [59, 55, 50, 46, 41, 40, 42, 40, 42, 40, 42, 41, 42, 41, 43, 41, 43, 41, 46, 42, 40, 42, 40, 42, 38, 33, 29, 24, 20] },
        { RadioBand.UHFUpper, [59, 55, 51, 48, 44, 40, 42, 42, 41, 43, 42, 41, 41, 43, 42, 44, 40, 43, 42, 41, 40, 46, 43, 39, 35, 31, 28, 24, 20] },
        { RadioBand.L, [59, 55, 52, 48, 44, 40, 43, 42, 41, 41, 43, 42, 41, 41, 40, 48, 45, 41, 40, 42, 42, 41, 42, 39, 35, 31, 27, 24, 20] }
    };
}
