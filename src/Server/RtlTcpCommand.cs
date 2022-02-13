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

namespace StreamSDR.Server;

/// <summary>
/// Represents a command received from an rtl_tcp client.
/// </summary>
internal sealed record RtlTcpCommand
{
    /// <summary>
    /// The type of command.
    /// </summary>
    public RtlTcpCommandType Type { get; set; }

    /// <summary>
    /// The value of the command.
    /// </summary>
    public uint Value { get; set; }
}
