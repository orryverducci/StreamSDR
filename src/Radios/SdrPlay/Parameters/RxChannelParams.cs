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

namespace StreamSDR.Radios.SdrPlay.Parameters;

/// <summary>
/// The parameters for a SDRplay tuner.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RxChannelParams
{
    /// <summary>
    /// The parameters for the device tuner that are available on all SDRplay devices.
    /// </summary>
    public Tuner.TunerParams TunerParams;

    /// <summary>
    /// The tuner control parameters.
    /// </summary>
    public Control.ControlParams CtrlParams;

    /// <summary>
    /// The parameters for a RSP1A device tuner.
    /// </summary>
    public Rsp1a.TunerParams Rsp1aTunerParams;

    /// <summary>
    /// The parameters for a RSP2 device tuner.
    /// </summary>
    public Rsp2.TunerParams Rsp2TunerParams;

    /// <summary>
    /// The parameters for a RSPduo device tuner.
    /// </summary>
    public RspDuo.TunerParams RspDuoTunerParams;

    /// <summary>
    /// The parameters for a RSPdx device tuner.
    /// </summary>
    public RspDx.TunerParams RspDxTunerParams;
}
