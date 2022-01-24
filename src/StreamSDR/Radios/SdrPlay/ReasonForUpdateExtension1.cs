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
/// The reason for a device parameters update.
/// </summary>
internal enum ReasonForUpdateExtension1 : uint
{
    Ext1_None = 0x00000000,
    RspDx_HdrEnable = 0x00000001,
    RspDx_BiasTControl = 0x00000002,
    RspDx_AntennaControl = 0x00000004,
    RspDx_RfNotchControl = 0x00000008,
    RspDx_RfDabNotchControl = 0x00000010,
    RspDx_HdrBw = 0x00000020
}
