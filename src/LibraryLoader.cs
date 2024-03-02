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
}
