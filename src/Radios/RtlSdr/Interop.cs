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

using System.Runtime.CompilerServices;

namespace StreamSDR.Radios.RtlSdr;

/// <summary>
/// Provides access to the native methods provided by the rtl-sdr shared library.
/// </summary>
internal sealed partial class Interop
{
    /// <summary>
    /// The name of the librtlsdr library.
    /// </summary>
    private const string LibRtlSdr = "rtlsdr";

    /// <summary>
    /// Delegate for the callback function called by the <see cref="ReadAsync"/> method.
    /// </summary>
    /// <param name="buf">The buffer containing the read data.</param>
    /// <param name="len">The length of the buffer.</param>
    /// <param name="ctx">The user specific context passed to the callback.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void ReadDelegate(byte* buf, uint len, IntPtr ctx);

    /// <summary>
    /// Gets the number of rtl-sdr devices available for use.
    /// </summary>
    /// <returns>The number of rtl-sdr devices available for use</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_device_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint GetDeviceCount();

    /// <summary>
    /// Gets the name of the specified rtl-sdr device.
    /// </summary>
    /// <param name="index">The index of the device to get the name of.</param>
    /// <returns>The name of the rtl-sdr device.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_device_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr GetDeviceNameNative(uint index);

    /// <summary>
    /// Gets the name of the specified rtl-sdr device.
    /// </summary>
    /// <param name="index">The index of the device to get the name of.</param>
    /// <returns>The name of the rtl-sdr device.</returns>
    public static string GetDeviceName(uint index)
    {
        string? name = Marshal.PtrToStringUTF8(GetDeviceNameNative(index));
        return name != null ? name : string.Empty;
    }

    /// <summary>
    /// Gets the index of the device with the specified serial.
    /// </summary>
    /// <param name="serial">The serial of the device to get the index of.</param>
    /// <returns>The index of the rtl-sdr device, or -2 if no devices are connected, or -3 if a device with the specified serial can't be found.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_index_by_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int GetDeviceIndexBySerialNative(IntPtr serial);

    /// <summary>
    /// Gets the index of the device with the specified serial.
    /// </summary>
    /// <param name="serial">The serial of the device to get the index of.</param>
    /// <returns>The index of the rtl-sdr device, or -2 if no devices are connected, or -3 if a device with the specified serial can't be found.</returns>
    public static int GetDeviceIndexBySerial(string serial) => GetDeviceIndexBySerialNative(Marshal.StringToHGlobalAnsi(serial));

    /// <summary>
    /// Opens a rtl-sdr dongle.
    /// </summary>
    /// <param name="dev"><see cref="IntPtr"/> to output the device handle to.</param>
    /// <param name="index">The index of the device to open.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_open")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int Open(out IntPtr dev, uint index);

    /// <summary>
    /// Closes a rtl-sdr device.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int Close(IntPtr dev);

    /// <summary>
    /// Read samples from the device asynchronously. This function will block until it is canceled using <see cref="CancelAsync"/>.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="cb">The callback function to be called when samples have been received.</param>
    /// <param name="ctx">A user specific context to pass to the callback function.</param>
    /// <param name="bufNum">The number of buffers to return in the callback function. Set to 0 for default buffer count (15).</param>
    /// <param name="bufLen">The buffer length. Must be a multiple of 512 and should be a multiple of 16384 (URB size). Set to 0 for default buffer count.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_read_async")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int ReadAsync(IntPtr dev, ReadDelegate cb, IntPtr ctx, uint bufNum, uint bufLen);

    /// <summary>
    /// Cancel all pending asynchronous operations on the device.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_cancel_async")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int CancelAsync(IntPtr dev);

    /// <summary>
    /// Resets the device sample buffer.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_reset_buffer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int ResetBuffer(IntPtr dev);

    /// <summary>
    /// Set the sample rate for the device. Also selects the baseband filters according to the requested sample rate
    /// on tuners that provide this functionality.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="rate">The desired sample rate in Hertz.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_sample_rate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetSampleRate(IntPtr dev, uint rate);

    /// <summary>
    /// Gets the sample rate the device is operating at.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>The sample rate in Hertz.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_sample_rate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint GetSampleRate(IntPtr dev);

    /// <summary>
    /// Sets the centre frequency that the device is tuned in to.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="freq">The desired frequency in Hertz.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_center_freq")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetCenterFreq(IntPtr dev, uint freq);

    /// <summary>
    /// Gets the centre frequency that the device is tuned in to.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>The frequency in Hertz.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_center_freq")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint GetCenterFreq(IntPtr dev);

    /// <summary>
    /// Sets the tuner frequency correction.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="ppm">The desired frequency correction in parts per million (PPM).</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_freq_correction")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetFreqCorrection(IntPtr dev, int ppm);

    /// <summary>
    /// Gets the the tuner frequency correction.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>The frequency correction in parts per million (PPM).</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_freq_correction")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int GetFreqCorrection(IntPtr dev);

    /// <summary>
    /// Sets if offset tuning for zero IF devices has been enabled.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="on">An integer representing if offset tuning should be enabled. 0 to disable or 1 to enable.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_offset_tuning")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetOffsetTuning(IntPtr dev, int on);

    /// <summary>
    /// Gets if offset tuning for zero IF devices has been enabled.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>An integer representing if offset has been enabled. Returns 0 if disabled, 1 if enabled, and -1 if an error occurs.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_offset_tuning")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int GetOffsetTuning(IntPtr dev);

    /// <summary>
    /// Sets the direct sampling mode.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="on">An integer representing the direct sampling mode. 0 to disable, 1 for I branch and 2 for Q branch.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_direct_sampling")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetDirectSampling(IntPtr dev, int on);

    /// <summary>
    /// Gets the direct sampling mode.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>An integer representing the direct sampling mode. Returns 0 if disabled, 1 for I branch, 2 for Q branch and -1 if an error occurs.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_direct_sampling")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int GetDirectSampling(IntPtr dev);

    /// <summary>
    /// Gets the type of tuner in the device.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>A <see cref="Tuner"/> struct indicating the type of tuner in the device. Returns <see cref="Tuner.Unknown"/> if an error occurs.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_tuner_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial Tuner GetTunerType(IntPtr dev);

    /// <summary>
    /// Sets the gain of the tuner. Manual gain mode must be enabled for this to work.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="gain">The desired gain as a tenth of a dB (e.g. 115 for 11.5dB).</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_tuner_gain")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetTunerGain(IntPtr dev, int gain);

    /// <summary>
    /// Gets the gain of the tuner.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <returns>The gain as a tenth of a dB (e.g. 115 for 11.5dB).</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_tuner_gain")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int GetTunerGain(IntPtr dev);

    /// <summary>
    /// Sets the mode in which the radio's gain is operating. Manual gain must be enabled for <see cref="SetTunerGain"/> to work.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="manual">An integer representing the desired game mode. 0 for automatic or 1 for manual.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_tuner_gain_mode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetTunerGainMode(IntPtr dev, int manual);

    /// <summary>
    /// Gets the list of gains supported by the tuner.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="gains">Point to an array to be populated with the list of gains, or <see cref="IntPtr.Zero"/> to just get the number of supported gains.</param>
    /// <returns>The number of gains supported, or 0 if an error occurred.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_get_tuner_gains")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public unsafe static partial int GetTunerGainsNative(IntPtr dev, int* gains);

    /// <summary>
    /// Gets the list of gains supported by the tuner.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="gains">An array to be populated with the list of gains, or <see langword="null"/> to just get the number of supported gains.</param>
    /// <returns>The number of gains supported, or 0 if an error occurred.</returns>
    public unsafe static int GetTunerGains(IntPtr dev, int[]? gains)
    {
        fixed (int* gainsPtr = gains)
        {
            return GetTunerGainsNative(dev, gainsPtr);
        }
    }

    /// <summary>
    /// Sets if the digital automatic gain correction of the RTL2832 is enabled.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="on">An integer representing if the AGC should be enabled. 0 to disable or 1 to enable.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_agc_mode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetAGCMode(IntPtr dev, int on);

    /// <summary>
    /// Sets if the bias tee (on GPIO pin 0) is enabled.
    /// </summary>
    /// <param name="dev">The device handle.</param>
    /// <param name="on">An integer representing if the bias tee should be enabled. 0 to disable or 1 to enable.</param>
    /// <returns>An integer indicating an error if one occurred. Returns 0 if successful.</returns>
    [LibraryImport(LibRtlSdr, EntryPoint = "rtlsdr_set_bias_tee")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SetBiasTee(IntPtr dev, int on);
}
