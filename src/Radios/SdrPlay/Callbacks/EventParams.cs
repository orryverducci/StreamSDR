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

namespace StreamSDR.Radios.SdrPlay.Callbacks;

/// <summary>
/// The parameters for an event that has occurred.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal struct EventParams
{
    /// <summary>
    /// The gain change event parameters.
    /// </summary>
    [FieldOffset(0)]
    public GainCbParam GainParams;

    /// <summary>
    /// The power overload event parameters.
    /// </summary>
    [FieldOffset(0)]
    public PowerOverloadCbParam PowerOverloadParams;

    /// <summary>
    /// The RSPduo mode change parameters.
    /// </summary>
    [FieldOffset(0)]
    public RspDuo.ModeCbParam RspDuoModeParams;
}
