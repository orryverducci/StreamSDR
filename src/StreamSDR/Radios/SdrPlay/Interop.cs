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

namespace StreamSDR.Radios.SdrPlay
{
    /// <summary>
    /// Provides access to the native methods provided by the SDRPlay API shared library.
    /// </summary>
    internal class Interop
    {
        /// <summary>
        /// The name of the libsdrplay_api library.
        /// </summary>
#if MACOS
        private const string LibSdrPlayApi = "libsdrplay_api.so";
#else
        private const string LibSdrPlayApi = "sdrplay_api";
#endif

        /// <summary>
        /// Delegate for the data read callback function called by the API after calling the <see cref="Init"/> method.
        /// </summary>
        /// <param name="xi">The buffer containing the real samples read from the device.</param>
        /// <param name="xq">The buffer containing the imaginary samples read from the device.</param>
        /// <param name="parameters">Pointer to the stream callback parameters struct.</param>
        /// <param name="numSamples">The number of samples in the buffers.</param>
        /// <param name="reset">Indicates if local buffers should be dropped due to an API has re-initialisation.</param>
        /// <param name="cbContext">The user specific context passed to the callback.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void ReadDelegate(short* xi, short* xq, StreamCbParams* parameters, uint numSamples, uint reset, IntPtr cbContext);

        /// <summary>
        /// Delegate for the event callback function called by the API after calling the <see cref="Init"/> method.
        /// </summary>
        /// <param name="eventId">Indicates the type of event that has occurred.</param>
        /// <param name="tuner">Indicates which tuner the event relates to.</param>
        /// <param name="parameters">Pointer to the event callback parameters struct.</param>
        /// <param name="cbContext">The user specific context passed to the callback.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void EventDelegate(Event eventId, TunerSelect tuner, EventParams* parameters, IntPtr cbContext);

        /// <summary>
        /// Opens the SDRPlay API for use
        /// </summary>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_Open")]
        public static extern ApiError Open();

        /// <summary>
        /// Closes the SDRPlay API.
        /// </summary>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_Close")]
        public static extern ApiError Close();

        /// <summary>
        /// Checks the API version used by the application matches the API version installed on the system.
        /// </summary>
        /// <param name="apiVer">The version of the API desired.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_ApiVersion")]
        public static extern ApiError ApiVersion(ref float apiVer);

        /// <summary>
        /// Enables debug output from the API.
        /// </summary>
        /// <param name="dev">Pointer to the current SDRPlay device, if one has been selected.</param>
        /// <param name="dbgLvl">The desired debug level.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_DebugEnable")]
        public static extern ApiError DebugEnable(IntPtr dev, DebugLevel dbgLvl);

        /// <summary>
        /// Debug method which disables the API heartbeat. Allows the debugger to step through the code without the API theads timing out.
        /// Must be called before <see cref="SelectDevice"/>.
        /// </summary>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_DisableHeartbeat")]
        public static extern ApiError DisableHeartbeat();

        /// <summary>
        /// Locks the SDRPlay API so that no other application can use it.
        /// </summary>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_LockDeviceApi")]
        public static extern ApiError LockDeviceApi();

        /// <summary>
        /// Unlocks the SDRPlay API, allowing other applications to use it.
        /// </summary>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_UnlockDeviceApi")]
        public static extern ApiError UnlockDeviceApi();

        /// <summary>
        /// Gets a list of the available devices, up to the maximum number defined by the <paramref name="maxDevs"/> parameter.
        /// </summary>
        /// <param name="devices">A pointer to memory in which an array of <see cref="Device"/> structures listing the available devices can be stored.</param>
        /// <param name="numDevs">Returns the number of devices available.</param>
        /// <param name="maxDevs">The maximum number of devices that can be returned.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_GetDevices")]
        public static extern ApiError GetDevicesNative(IntPtr devices, out uint numDevs, uint maxDevs);

        /// <summary>
        /// Gets a list of the available devices, up to the maximum number defined by the <paramref name="maxDevices"/> parameter.
        /// </summary>
        /// <param name="devices">Returns an array of <see cref="Device"/> structures listing the available devices.</param>
        /// <param name="numberOfDevices">Returns the number of devices available.</param>
        /// <param name="maxDevices">The maximum number of devices that can be returned.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        public static ApiError GetDevices(out Device[]? devices, out uint numberOfDevices, int maxDevices)
        {
            // Throw error if maxDevices is 0 or negative
            if (maxDevices < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDevices), "The maximum number of devices must be 1 or above");
            }

            // Allocate memory for the SDRPlay API to store devices in
            int deviceSize = Marshal.SizeOf(typeof(Device));
            IntPtr devicesPtr = Marshal.AllocHGlobal(deviceSize * maxDevices);

            // Get the available devices
            ApiError result = GetDevicesNative(devicesPtr, out numberOfDevices, (uint)maxDevices);

            // If the API returns devices, bring them in to managed memory
            if (result == ApiError.Success && numberOfDevices > 0)
            {
                devices = new Device[numberOfDevices];

                for (int i = 0; i < numberOfDevices; i++)
                {
                    devices[i] = Marshal.PtrToStructure<Device>(devicesPtr + (i * deviceSize));
                }
            }
            else
            {
                devices = null;
            }

            // Free the memory used by the API
            Marshal.FreeHGlobal(devicesPtr);

            // Return the API result
            return result;
        }

        /// <summary>
        /// Selects a device for use by the application.
        /// </summary>
        /// <param name="device">A pointer to the <see cref="Device"/> to be used, as returned by <see cref="GetDevices"/>.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_SelectDevice")]
        public static extern ApiError SelectDeviceNative(IntPtr device);

        /// <summary>
        /// Selects a device for use by the application.
        /// </summary>
        /// <param name="device">A <see cref="Device"/> to be used as returned by <see cref="GetDevices"/>.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        public static ApiError SelectDevice(ref Device device)
        {
            // Allocate memory to store the device structure
            IntPtr devicePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Device)));

            // Copy the device structure
            Marshal.StructureToPtr<Device>(device, devicePtr, false);

            // Select the device
            ApiError result = SelectDeviceNative(devicePtr);

            // If the API selects the device, pull back in to managed memory the updated structure with the device handle
            device = Marshal.PtrToStructure<Device>(devicePtr);

            // Free the memory used by the unmanaged structure
            Marshal.FreeHGlobal(devicePtr);

            // Return the API result
            return result;
        }

        /// <summary>
        /// Releases a device that was selected using <see cref="SelectDeviceNative"/> so that it can be used by other application.
        /// </summary>
        /// <param name="device">A pointer to the <see cref="Device"/> to be released, as returned by <see cref="GetDevices"/>.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_ReleaseDevice")]
        public static extern ApiError ReleaseDeviceNative(IntPtr device);

        /// <summary>
        /// Releases a device that was selected using <see cref="SelectDevice"/> so that it can be used by other application.
        /// </summary>
        /// <param name="device">A <see cref="Device"/> to be released as returned by <see cref="GetDevices"/>.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        public static ApiError ReleaseDevice(Device device)
        {
            // Allocate memory to store the device structure
            IntPtr devicePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Device)));

            // Copy the device structure
            Marshal.StructureToPtr<Device>(device, devicePtr, false);

            // Release the device
            ApiError result = ReleaseDeviceNative(devicePtr);

            // Free the memory used by the unmanaged structure
            Marshal.FreeHGlobal(devicePtr);

            // Return the API result
            return result;
        }

        /// <summary>
        /// Gets the device parameters.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="deviceParams">A pointer to the device parameters.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_GetDeviceParams")]
        public static unsafe extern ApiError GetDeviceParams(IntPtr dev, out DeviceParams* deviceParams);

        /// <summary>
        /// Initialises the device and starts reading samples from the device.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="callbackFns">A struct of callback functions to be called when samples have been received or events have occurred.</param>
        /// <param name="cbContext">A user specific context to pass to the callback function.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_Init")]
        public static extern ApiError InitNative(IntPtr dev, IntPtr callbackFns, IntPtr cbContext);

        /// <summary>
        /// Initialises the device and starts reading samples from the device.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="callbackFns">A pointer to the struct of callback functions to be called when samples have been received or events have occurred.</param>
        /// <param name="cbContext">A user specific context to pass to the callback function.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        public static ApiError Init(IntPtr dev, CallbackFunctions callbackFns, IntPtr cbContext)
        {
            // Allocate memory to store the callback functions
            IntPtr callbacksPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CallbackFunctions)));

            // Copy the callback functions structure
            Marshal.StructureToPtr<CallbackFunctions>(callbackFns, callbacksPtr, false);

            // Initialise the device
            ApiError result = InitNative(dev, callbacksPtr, cbContext);

            // Free the memory used by the unmanaged structure
            Marshal.FreeHGlobal(callbacksPtr);

            // Return the API result
            return result;
        }

        /// <summary>
        /// Stops reading samples from the device and uninitialises it.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_Uninit")]
        public static extern ApiError Uninit(IntPtr dev);

        /// <summary>
        /// Indicates to the API that parameters have changed and should be applied.
        /// </summary>
        /// <param name="dev">The device handle.</param>
        /// <param name="tuner">The tuner the update applies to.</param>
        /// <param name="reasonForUpdate">The reason for the update as specified in the <see cref="ReasonForUpdate"/> enum.</param>
        /// <param name="reasonForUpdateExt1">The reason for the update as specified in the <see cref="ReasonForUpdateExtension1"/> enum.</param>
        /// <returns>The <see cref="ApiError"/> returned by the API. Returns <see cref="ApiError.Success"/> if successful.</returns>
        [DllImport(LibSdrPlayApi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sdrplay_api_Update")]
        public static extern ApiError Update(IntPtr dev, TunerSelect tuner, ReasonForUpdate reasonForUpdate, ReasonForUpdateExtension1 reasonForUpdateExt1);
    }
}
