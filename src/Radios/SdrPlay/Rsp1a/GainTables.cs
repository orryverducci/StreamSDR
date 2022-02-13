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

namespace StreamSDR.Radios.SdrPlay.Rsp1a;

/// <summary>
/// Provides the lookup tables to translate a gain level to the LNA and IF gain settings for each band on a RSP1A.
/// </summary>
internal sealed record GainTables : IGainTables
{
    /// <inheritdoc/>
    public IDictionary<RadioBand, byte[]> LnaStates { get; set; } = new Dictionary<RadioBand, byte[]>()
    {
        { RadioBand.AM, new byte[] { 6, 6, 6, 6, 6, 6, 5, 5, 5, 5, 5, 4, 4, 3, 3, 3, 3, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 } },
        { RadioBand.VHF, new byte[] { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 } },
        { RadioBand.III, new byte[] { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 } },
        { RadioBand.UHFLower, new byte[] { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 } },
        { RadioBand.UHFUpper, new byte[] { 9, 9, 9, 9, 9, 9, 8, 8, 8, 8, 8, 7, 6, 6, 5, 5, 4, 4, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 } },
        { RadioBand.L, new byte[] { 8, 8, 8, 8, 8, 8, 7, 7, 7, 7, 7, 6, 5, 5, 4, 4, 3, 2, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 } },
    };

    /// <inheritdoc/>
    public IDictionary<RadioBand, int[]> IfGains { get; set; } = new Dictionary<RadioBand, int[]>()
    {
        { RadioBand.AM, new int[] { 59, 55, 52, 48, 45, 41, 57, 53, 49, 46, 42, 44, 40, 56, 52, 48, 45, 41, 44, 40, 43, 45, 41, 38, 34, 31, 27, 24, 20 } },
        { RadioBand.VHF, new int[] { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 } },
        { RadioBand.III, new int[] { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 } },
        { RadioBand.UHFLower, new int[] { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 } },
        { RadioBand.UHFUpper, new int[] { 59, 55, 52, 48, 44, 41, 56, 52, 49, 45, 41, 44, 46, 42, 45, 41, 44, 40, 44, 40, 42, 46, 42, 38, 35, 31, 27, 24, 20 } },
        { RadioBand.L, new int[] { 59, 55, 52, 48, 45, 41, 56, 53, 49, 46, 42, 43, 46, 42, 44, 41, 43, 48, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 } },
    };
}
