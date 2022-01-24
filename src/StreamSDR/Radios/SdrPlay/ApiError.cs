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
    /// The error codes returned by the SDRPlay API.
    /// </summary>
    internal enum ApiError
    {
        Success = 0,
        Fail = 1,
        InvalidParam = 2,
        OutOfRange = 3,
        GainUpdateError = 4,
        RfUpdateError = 5,
        FsUpdateError = 6,
        HwError = 7,
        AliasingError = 8,
        AlreadyInitialised = 9,
        NotInitialised = 10,
        NotEnabled = 11,
        HwVerError = 12,
        OutOfMemError = 13,
        ServiceNotResponding = 14,
        StartPending = 15,
        StopPending = 16,
        InvalidMode = 17,
        FailedVerification1 = 18,
        FailedVerification2 = 19,
        FailedVerification3 = 20,
        FailedVerification4 = 21,
        FailedVerification5 = 22,
        FailedVerification6 = 23,
        InvalidServiceVersion = 24
    }
}
