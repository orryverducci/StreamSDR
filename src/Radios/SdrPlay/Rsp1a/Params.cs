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

namespace StreamSDR.Radios.SdrPlay.Rsp1a;

/// <summary>
/// The parameters for a RSP1A device.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Params
{
    /// <summary>
    /// The state of the broadcast band (AM/FM) notch filter. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
    /// </summary>
    [MarshalAs(UnmanagedType.U8)]
    public bool RfNotchEnable;

    /// <summary>
    /// The state of the DAB notch filter. <see langword="true"/> if enabled, <see langword="false"/> otherwise.
    /// </summary>
    [MarshalAs(UnmanagedType.U8)]
    public bool RfDabNotchEnable;
}
