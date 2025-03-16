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

using System.Reflection;
using Microsoft.Win32;

namespace StreamSDR;

/// <summary>
/// Provides functionality to resolve the native libraries used by StreamSDR.
/// </summary>
internal static class LibraryLoader
{
    /// <summary>
    /// Callback to resolve the native libraries used by StreamSDR.
    /// </summary>
    /// <param name="libraryName">The native library to resolve.</param>
    /// <param name="assembly">The assembly requesting the resolution.</param>
    /// <param name="searchPath">The <see cref="DefaultDllImportSearchPathsAttribute"/> provided by the invoke or null.</param>
    /// <returns>Handle for the loaded library, or zero if the default import resolver should be used.</returns>
    public static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) =>
        libraryName switch
        {
            "rtlsdr" => ResolveRtlSdrLibrary(),
            "sdrplay_api" => ResolveSdrPlayLibrary(),
            _ => IntPtr.Zero
        };

    /// <summary>
    /// Resolves the rtl-sdr library. Uses the default import resolver on Windows and macOS,
    /// and attempts to load the versioned library on Linux. Required as some distributions
    /// don't create a symbolic link from 'librtlsdr' to the versioned library.
    /// </summary>
    /// <returns>Handle for the loaded library, or zero if the default import resolver should be used.</returns>
    private static IntPtr ResolveRtlSdrLibrary()
    {
        IntPtr libHandle = IntPtr.Zero;

        // Try to load the versioned libraries on linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            bool v2Loaded = NativeLibrary.TryLoad("librtlsdr.so.2", out libHandle);

            if (!v2Loaded)
            {
                NativeLibrary.TryLoad("librtlsdr.so.0", out libHandle);
            }
        }

        return libHandle;
    }

    /// <summary>
    /// Resolves the SDRplay API library. Retrieves the library location from the
    /// </summary>
    /// <returns>Handle for the loaded library, or zero if the default import resolver should be used.</returns>
    private static IntPtr ResolveSdrPlayLibrary()
    {
        IntPtr libHandle = IntPtr.Zero;

        // Get the API library location from the registry on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string? location = (string?)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\SDRplay\\Service\\API", "Install_Dir", null);

            if (location == null)
            {
                location = (string?)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\SDRplay\\Service\\API", "Install_Dir", null);
            }

            if (location != null)
            {
                NativeLibrary.TryLoad($"{location}\\x64\\sdrplay_api.dll", out libHandle);
            }
        }

        // Use the Linux library extension (.so) on macOS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            NativeLibrary.TryLoad("/usr/local/lib/libsdrplay_api.so", out libHandle);
        }

        return libHandle;
    }
}
