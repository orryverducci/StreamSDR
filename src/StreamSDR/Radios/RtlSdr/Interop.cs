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

using System;
using System.Runtime.InteropServices;

namespace StreamSDR.Radios.RtlSdr
{
    /// <summary>
    /// Provides access to the native methods provided by the rtl-sdr shared library.
    /// </summary>
    internal class Interop
    {
        /// <summary>
        /// The name of the librtlsdr library.
        /// </summary>
        private const string LibRtlSdr = "rtlsdr";

        /// <summary>
        /// Delegate for the callback function called by the <see cref="ReadAsync"/> method.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="len"></param>
        /// <param name="ctx"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void ReadDelegate(byte* buf, uint len, IntPtr ctx);

        /// <summary>
        /// Gets the number of rtl-sdr devices available for use.
        /// </summary>
        /// <returns>The number of rtl-sdr devices available for use</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_device_count")]
        public static extern uint GetDeviceCount();

        /// <summary>
        /// Gets the name of the specified rtl-sdr device.
        /// </summary>
        /// <param name="index">The index of the device to get the name of.</param>
        /// <returns>The name of the rtl-sdr device.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_device_name")]
        //[return: MarshalAs(UnmanagedType.LPStr)]
        public static extern IntPtr GetDeviceNameNative(uint index);

        /// <summary>
        /// Gets the name of the specified rtl-sdr device.
        /// </summary>
        /// <param name="index">The index of the device to get the name of.</param>
        /// <returns>The name of the rtl-sdr device.</returns>
        public static string GetDeviceName(uint index)
        {
            string? name = Marshal.PtrToStringAnsi(GetDeviceNameNative(index));
            return name != null ? name : string.Empty;
        }

        /// <summary>
        /// Opens a rtl-sdr dongle.
        /// </summary>
        /// <param name="dev"><see cref="IntPtr"/> to output the device handle to.</param>
        /// <param name="index">The index of the device to open.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_open")]
        public static extern int Open(out IntPtr dev, uint index);

        /// <summary>
        /// Closes a rtl-sdr device.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_close")]
        public static extern int Close(IntPtr dev);

        /// <summary>
        /// Read samples from the device asynchronously. This function will block until it is canceled using <see cref="CancelAsync"/>.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="cb">The callback function to be called when samples have been received.</param>
        /// <param name="ctx">A user specific context to pass to the callback function.</param>
        /// <param name="bufNum">The number of buffers to return in the callback function. Set to 0 for default buffer count (15).</param>
        /// <param name="bufLen">The buffer length. Must be a multiple of 512 and should be a multiple of 16384 (URB size). Set to 0 for default buffer count.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_read_async")]
        public static extern int ReadAsync(IntPtr dev, ReadDelegate cb, IntPtr ctx, uint bufNum, uint bufLen);

        /// <summary>
        /// Cancel all pending asynchronous operations on the device.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_cancel_async")]
        public static extern int CancelAsync(IntPtr dev);

        /// <summary>
        /// Resets the device sample buffer.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_reset_buffer")]
        public static extern int ResetBuffer(IntPtr dev);

        /// <summary>
        /// Set the sample rate for the device. Also selects the baseband filters according to the requested sample rate
        /// on tuners that provide this functionality.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="rate">The desired sample rate in Hertz.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_sample_rate")]
        public static extern int SetSampleRate(IntPtr dev, uint rate);

        /// <summary>
        /// Gets the sample rate the device is operating at.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>The sample rate in Hertz.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_sample_rate")]
        public static extern uint GetSampleRate(IntPtr dev);

        /// <summary>
        /// Sets the centre frequency that the device is tuned in to.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="freq">The desired frequency in Hertz.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_center_freq")]
        public static extern int SetCenterFreq(IntPtr dev, uint freq);

        /// <summary>
        /// Gets the centre frequency that the device is tuned in to.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>The frequency in Hertz.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_center_freq")]
        public static extern uint GetCenterFreq(IntPtr dev);

        /// <summary>
        /// Sets the tuner frequency correction.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="ppm">The desired frequency correction in parts per million (PPM).</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_freq_correction")]
        public static extern int SetFreqCorrection(IntPtr dev, int ppm);

        /// <summary>
        /// Gets the the tuner frequency correction.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>The frequency correction in parts per million (PPM).</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_freq_correction")]
        public static extern int GetFreqCorrection(IntPtr dev);

        /// <summary>
        /// Sets if offset tuning for zero IF devices has been enabled.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="on">An integer representing if offset tuning should be enabled. 0 to disable or 1 to enable.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_offset_tuning")]
        public static extern int SetOffsetTuning(IntPtr dev, int on);

        /// <summary>
        /// Gets if offset tuning for zero IF devices has been enabled.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>An integer representing if offset has been enabled. Returns 0 if disabled, 1 if enabled, and -1 if an error occurs.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_offset_tuning")]
        public static extern int GetOffsetTuning(IntPtr dev);

        /// <summary>
        /// Sets the direct sampling mode.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="on">An integer representing the direct sampling mode. 0 to disable, 1 for I branch and 2 for Q branch.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_direct_sampling")]
        public static extern int SetDirectSampling(IntPtr dev, int on);

        /// <summary>
        /// Gets the direct sampling mode.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>An integer representing the direct sampling mode. Returns 0 if disabled, 1 for I branch, 2 for Q branch and -1 if an error occurs.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_direct_sampling")]
        public static extern int GetDirectSampling(IntPtr dev);

        /// <summary>
        /// Sets the gain of the tuner. Manual gain mode must be enabled for this to work.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="gain">The desired gain as a tenth of a dB (e.g. 115 for 11.5dB).</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_tuner_gain")]
        public static extern int SetTunerGain(IntPtr dev, int gain);

        /// <summary>
        /// Gets the gain of the tuner.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>The gain as a tenth of a dB (e.g. 115 for 11.5dB).</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_tuner_gain")]
        public static extern int GetTunerGain(IntPtr dev);

        /// <summary>
        /// Sets the mode in which the radio's gain is operating. Manual gain must be enabled for <see cref="SetTunerGain"/> to work.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="manual">An integer representing the desired game mode. 0 for automatic or 1 for manual.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_tuner_gain_mode")]
        public static extern int SetTunerGainMode(IntPtr dev, int manual);

        /// <summary>
        /// Gets the list of gains supported by the tuner.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="gains">An array to be populated with the list of gains, or <see langword="null"/> to just get the number of supported gains.</param>
        /// <returns>The number of gains supported, or 0 if an error occurred.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_get_tuner_gains")]
        public static extern int GetTunerGains(IntPtr dev, [In, Out] int[]? gains);

        /// <summary>
        /// Sets if the digital automatic gain correction of the RTL2832 is enabled.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="on">An integer representing if the AGC should be enabled. 0 to disable or 1 to enable.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_agc_mode")]
        public static extern int SetAGCMode(IntPtr dev, int on);

        /// <summary>
        /// Sets if the bias tee (on GPIO pin 0) is enabled.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="on">An integer representing if the bias tee should be enabled. 0 to disable or 1 to enable.</param>
        /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
        [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl, EntryPoint = "rtlsdr_set_bias_tee")]
        public static extern int SetBiasTee(IntPtr dev, int on);
    }
}
