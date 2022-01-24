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
/// Provides the lookup tables to translate a gain level to the LNA and IF gain settings for each device and band.
/// </summary>
public static class GainTables
{
    #region Tables for the RSP1
    public static readonly byte[] Rsp1AmLnaStates = { 3, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1AmIfGains = { 59, 56, 53, 50, 47, 44, 41, 58, 55, 52, 49, 46, 43, 45, 42, 58, 55, 52, 49, 46, 43, 41, 38, 35, 32, 29, 26, 23, 20 };
    public static readonly byte[] Rsp1VhfLnaStates = { 3, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1VhfIfGains = { 59, 56, 53, 50, 47, 44, 41, 58, 55, 52, 49, 46, 43, 45, 42, 58, 55, 52, 49, 46, 43, 41, 38, 35, 32, 29, 26, 23, 20 };
    public static readonly byte[] Rsp1Band3LnaStates = { 3, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1Band3IfGains = { 59, 56, 53, 50, 47, 44, 41, 58, 55, 52, 49, 46, 43, 45, 42, 58, 55, 52, 49, 46, 43, 41, 38, 35, 32, 29, 26, 23, 20 };
    public static readonly byte[] Rsp1UhfLowerLnaStates = { 3, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1UhfLowerIfGains = { 59, 56, 53, 50, 47, 44, 41, 58, 55, 52, 49, 46, 43, 45, 42, 58, 55, 52, 49, 46, 43, 41, 38, 35, 32, 29, 26, 23, 20 };
    public static readonly byte[] Rsp1UhfUpperLnaStates = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 1, 1, 1, 1, 1, 0, 0, 0, 0 };
    public static readonly int[] Rsp1UhfUpperIfGains = { 59, 57, 54, 52, 50, 47, 45, 43, 40, 38, 36, 33, 31, 29, 27, 24, 22, 27, 24, 22, 32, 29, 27, 25, 22, 27, 25, 22, 20 };
    public static readonly byte[] Rsp1LBandLnaStates = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1LBandIfGains = { 59, 57, 55, 52, 50, 48, 46, 43, 41, 44, 42, 53, 51, 49, 47, 44, 42, 45, 43, 40, 38, 36, 34, 31, 29, 27, 25, 22, 20 };
    #endregion

    #region Tables for the RSP1A
    public static readonly byte[] Rsp1aAmLnaStates = { 6, 6, 6, 6, 6, 6, 5, 5, 5, 5, 5, 4, 4, 3, 3, 3, 3, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1aAmIfGains = { 59, 55, 52, 48, 45, 41, 57, 53, 49, 46, 42, 44, 40, 56, 52, 48, 45, 41, 44, 40, 43, 45, 41, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] Rsp1aVhfLnaStates = { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1aVhfIfGains = { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] Rsp1aBand3LnaStates = { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1aBand3IfGains = { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] Rsp1aUhfLowerLnaStates = { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1aUhfLowerIfGains = { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] Rsp1aUhfUpperLnaStates = { 9, 9, 9, 9, 9, 9, 8, 8, 8, 8, 8, 7, 6, 6, 5, 5, 4, 4, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1aUhfUpperIfGains = { 59, 55, 52, 48, 44, 41, 56, 52, 49, 45, 41, 44, 46, 42, 45, 41, 44, 40, 44, 40, 42, 46, 42, 38, 35, 31, 27, 24, 20 };
    public static readonly byte[] Rsp1aLBandLnaStates = { 8, 8, 8, 8, 8, 8, 7, 7, 7, 7, 7, 6, 5, 5, 4, 4, 3, 2, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp1aLBandIfGains = { 59, 55, 52, 48, 45, 41, 56, 53, 49, 46, 42, 43, 46, 42, 44, 41, 43, 48, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 };
    #endregion

    #region Tables for the RSP2
    public static readonly byte[] Rsp2AmLnaStates = { 8, 8, 8, 8, 8, 8, 7, 7, 7, 7, 7, 6, 5, 5, 4, 4, 4, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp2AmIfGains = { 59, 55, 52, 48, 44, 41, 56, 52, 49, 45, 41, 44, 45, 41, 48, 44, 40, 45, 42, 43, 49, 46, 42, 38, 35, 31, 27, 24, 20 };
    public static readonly byte[] Rsp2VhfLnaStates = { 8, 8, 8, 8, 8, 8, 7, 7, 7, 7, 7, 6, 5, 5, 4, 4, 4, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp2VhfIfGains = { 59, 55, 52, 48, 44, 41, 56, 52, 49, 45, 41, 44, 45, 41, 48, 44, 40, 45, 42, 43, 49, 46, 42, 38, 35, 31, 27, 24, 20 };
    public static readonly byte[] Rsp2Band3LnaStates = { 8, 8, 8, 8, 8, 8, 7, 7, 7, 7, 7, 6, 5, 5, 4, 4, 4, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp2Band3IfGains = { 59, 55, 52, 48, 44, 41, 56, 52, 49, 45, 41, 44, 45, 41, 48, 44, 40, 45, 42, 43, 49, 46, 42, 38, 35, 31, 27, 24, 20 };
    public static readonly byte[] Rsp2UhfLowerLnaStates = { 8, 8, 8, 8, 8, 8, 7, 7, 7, 7, 7, 6, 5, 5, 4, 4, 4, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp2UhfLowerIfGains = { 59, 55, 52, 48, 44, 41, 56, 52, 49, 45, 41, 44, 45, 41, 48, 44, 40, 45, 42, 43, 49, 46, 42, 38, 35, 31, 27, 24, 20 };
    public static readonly byte[] Rsp2UhfUpperLnaStates = { 5, 5, 5, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 3, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp2UhfUpperIfGains = { 59, 56, 53, 50, 48, 45, 42, 58, 55, 52, 49, 47, 44, 41, 43, 40, 44, 41, 42, 46, 43, 40, 37, 34, 31, 29, 26, 23, 20 };
    public static readonly byte[] Rsp2LBandLnaStates = { 4, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 2, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] Rsp2LBandIfGains = { 59, 56, 54, 51, 48, 45, 43, 40, 56, 54, 51, 48, 45, 43, 40, 43, 41, 44, 41, 44, 42, 39, 36, 34, 31, 28, 25, 23, 20 };
    #endregion

    #region Tables for the RSPduo
    public static readonly byte[] RspDuoAmLnaStates = { 6, 6, 6, 6, 6, 6, 5, 5, 5, 5, 5, 4, 4, 3, 3, 3, 3, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDuoAmIfGains = { 59, 55, 52, 48, 45, 41, 57, 53, 49, 46, 42, 44, 40, 56, 52, 48, 45, 41, 44, 40, 43, 45, 41, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] RspDuoVhfLnaStates = { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDuoVhfIfGains = { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] RspDuoBand3LnaStates = { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDuoBand3IfGains = { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] RspDuoUhfLowerLnaStates = { 9, 9, 9, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 5, 5, 4, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDuoUhfLowerIfGains = { 59, 55, 52, 48, 45, 41, 42, 58, 54, 51, 47, 43, 46, 42, 44, 41, 43, 42, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] RspDuoUhfUpperLnaStates = { 9, 9, 9, 9, 9, 9, 8, 8, 8, 8, 8, 7, 6, 6, 5, 5, 4, 4, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDuoUhfUpperIfGains = { 59, 55, 52, 48, 44, 41, 56, 52, 49, 45, 41, 44, 46, 42, 45, 41, 44, 40, 44, 40, 42, 46, 42, 38, 35, 31, 27, 24, 20 };
    public static readonly byte[] RspDuoLBandLnaStates = { 8, 8, 8, 8, 8, 8, 7, 7, 7, 7, 7, 6, 5, 5, 4, 4, 3, 2, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDuoLBandIfGains = { 59, 55, 52, 48, 45, 41, 56, 53, 49, 46, 42, 43, 46, 42, 44, 41, 43, 48, 44, 40, 43, 45, 42, 38, 34, 31, 27, 24, 20 };
    #endregion

    #region Tables for the RSPdx
    public static readonly byte[] RspDxAmLnaStates = { 18, 18, 18, 18, 18, 18, 17, 16, 14, 13, 12, 11, 10, 9, 7, 6, 5, 5, 5, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDxAmIfGains = { 59, 55, 52, 48, 45, 41, 41, 40, 43, 42, 42, 41, 41, 40, 42, 42, 47, 44, 40, 43, 42, 42, 41, 38, 34, 31, 27, 24, 20 };
    public static readonly byte[] RspDxVhfLnaStates = { 26, 26, 26, 26, 26, 25, 23, 22, 20, 19, 17, 16, 14, 13, 11, 10, 8, 7, 5, 5, 5, 3, 2, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDxVhfIfGains = { 59, 55, 50, 46, 41, 40, 42, 40, 42, 40, 42, 41, 42, 41, 43, 41, 43, 41, 49, 45, 40, 42, 40, 42, 38, 33, 29, 24, 20 };
    public static readonly byte[] RspDxBand3LnaStates = { 26, 26, 26, 26, 26, 25, 23, 22, 20, 19, 17, 16, 14, 13, 11, 10, 8, 7, 5, 5, 5, 3, 2, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDxBand3IfGains = { 59, 55, 50, 46, 41, 40, 42, 40, 42, 40, 42, 41, 42, 41, 43, 41, 43, 41, 49, 45, 40, 42, 40, 42, 38, 33, 29, 24, 20 };
    public static readonly byte[] RspDxUhfLowerLnaStates = { 27, 27, 27, 27, 27, 26, 24, 23, 21, 20, 18, 17, 15, 14, 12, 11, 9, 8, 6, 6, 5, 3, 2, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDxUhfLowerIfGains = { 59, 55, 50, 46, 41, 40, 42, 40, 42, 40, 42, 41, 42, 41, 43, 41, 43, 41, 46, 42, 40, 42, 40, 42, 38, 33, 29, 24, 20 };
    public static readonly byte[] RspDxUhfUpperLnaStates = { 20, 20, 20, 20, 20, 20, 18, 17, 16, 14, 13, 12, 11, 9, 8, 7, 7, 5, 4, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDxUhfUpperIfGains = { 59, 55, 51, 48, 44, 40, 42, 42, 41, 43, 42, 41, 41, 43, 42, 44, 40, 43, 42, 41, 40, 46, 43, 39, 35, 31, 28, 24, 20 };
    public static readonly byte[] RspDxLBandLnaStates = { 18, 18, 18, 18, 18, 18, 16, 15, 14, 13, 11, 10, 9, 8, 7, 6, 6, 6, 5, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly int[] RspDxLBandIfGains = { 59, 55, 52, 48, 44, 40, 43, 42, 41, 41, 43, 42, 41, 41, 40, 48, 45, 41, 40, 42, 42, 41, 42, 39, 35, 31, 27, 24, 20 };
    #endregion
}
