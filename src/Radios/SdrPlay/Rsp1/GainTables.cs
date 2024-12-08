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

namespace StreamSDR.Radios.SdrPlay.Rsp1;

/// <summary>
/// Provides the lookup tables to translate a gain level to the LNA and IF gain settings for each band on a RSP1.
/// </summary>
internal sealed record GainTables : IGainTables
{
    /// <inheritdoc/>
    public IDictionary<RadioBand, byte[]> LnaStates { get; set; } = new Dictionary<RadioBand, byte[]>()
    {
        { RadioBand.AM, [3, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
        { RadioBand.VHF, [3, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
        { RadioBand.III, [3, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
        { RadioBand.UHFLower, [3, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
        { RadioBand.UHFUpper, [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 1, 1, 1, 1, 1, 0, 0, 0, 0] },
        { RadioBand.L, [3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] }
    };

    /// <inheritdoc/>
    public IDictionary<RadioBand, int[]> IfGains { get; set; } = new Dictionary<RadioBand, int[]>()
    {
        { RadioBand.AM, [59, 56, 53, 50, 47, 44, 41, 58, 55, 52, 49, 46, 43, 45, 42, 58, 55, 52, 49, 46, 43, 41, 38, 35, 32, 29, 26, 23, 20] },
        { RadioBand.VHF, [59, 56, 53, 50, 47, 44, 41, 58, 55, 52, 49, 46, 43, 45, 42, 58, 55, 52, 49, 46, 43, 41, 38, 35, 32, 29, 26, 23, 20] },
        { RadioBand.III, [59, 56, 53, 50, 47, 44, 41, 58, 55, 52, 49, 46, 43, 45, 42, 58, 55, 52, 49, 46, 43, 41, 38, 35, 32, 29, 26, 23, 20] },
        { RadioBand.UHFLower, [59, 56, 53, 50, 47, 44, 41, 58, 55, 52, 49, 46, 43, 45, 42, 58, 55, 52, 49, 46, 43, 41, 38, 35, 32, 29, 26, 23, 20] },
        { RadioBand.UHFUpper, [59, 57, 54, 52, 50, 47, 45, 43, 40, 38, 36, 33, 31, 29, 27, 24, 22, 27, 24, 22, 32, 29, 27, 25, 22, 27, 25, 22, 20] },
        { RadioBand.L, [59, 57, 55, 52, 50, 48, 46, 43, 41, 44, 42, 53, 51, 49, 47, 44, 42, 45, 43, 40, 38, 36, 34, 31, 29, 27, 25, 22, 20] }
    };
}
