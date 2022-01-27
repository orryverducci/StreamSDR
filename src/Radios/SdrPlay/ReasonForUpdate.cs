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
internal enum ReasonForUpdate : uint
{
    None = 0x00000000,
    Dev_Fs = 0x00000001,
    Dev_Ppm = 0x00000002,
    Dev_SyncUpdate = 0x00000004,
    Dev_ResetFlags = 0x00000008,
    Rsp1a_BiasTControl = 0x00000010,
    Rsp1a_RfNotchControl = 0x00000020,
    Rsp1a_RfDabNotchControl = 0x00000040,
    Rsp2_BiasTControl = 0x00000080,
    Rsp2_AmPortSelect = 0x00000100,
    Rsp2_AntennaControl = 0x00000200,
    Rsp2_RfNotchControl = 0x00000400,
    Rsp2_ExtRefControl = 0x00000800,
    RspDuo_ExtRefControl = 0x00001000,
    Master_Spare_1 = 0x00002000,
    Master_Spare_2 = 0x00004000,
    Tuner_Gr = 0x00008000,
    Tuner_GrLimits = 0x00010000,
    Tuner_Frf = 0x00020000,
    Tuner_BwType = 0x00040000,
    Tuner_IfType = 0x00080000,
    Tuner_DcOffset = 0x00100000,
    Tuner_LoMode = 0x00200000,
    Ctrl_DCoffsetIQimbalance = 0x00400000,
    Ctrl_Decimation = 0x00800000,
    Ctrl_Agc = 0x01000000,
    Ctrl_AdsbMode = 0x02000000,
    Ctrl_OverloadMsgAck = 0x04000000,
    RspDuo_BiasTControl = 0x08000000,
    RspDuo_AmPortSelect = 0x10000000,
    RspDuo_Tuner1AmNotchControl = 0x20000000,
    RspDuo_RfNotchControl = 0x40000000,
    RspDuo_RfDabNotchControl = 0x80000000
}
