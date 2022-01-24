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

namespace StreamSDR.Radios.RtlSdr;

/// <summary>
/// Represents the type of tuner in a rtl-sdr device.
/// </summary>
internal enum Tuner
{
    Unknown = 0,
    E4000 = 1,
    FC0012 = 2,
    FC0013 = 3,
    FC2580 = 4,
    R820T = 5,
    R828D = 6
}
