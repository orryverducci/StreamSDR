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
/// The tuner bandwidths that are available.
/// </summary>
internal enum BwMhz
{
    Undefined = 0,
    Bw200 = 200,
    Bw300 = 300,
    Bw600 = 600,
    Bw1536 = 1536,
    Bw5000 = 5000,
    Bw6000 = 6000,
    Bw7000 = 7000,
    Bw8000 = 8000
}
