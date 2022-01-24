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

namespace StreamSDR.DSP;

/// <summary>
/// Converts buffers of samples from their original bit depth, contained within a <see cref="short"/>, to a different bit depth, contained within a <see cref="byte"/>.
/// </summary>
internal class ShortToByteBitDepthConversion
{
    /// <summary>
    /// How many bits each sample is shifted by to move its most significant bit to the 16th bit.
    /// </summary>
    private readonly int _shiftFactor;

    /// <summary>
    /// Initialises a new instance of the <see cref="ShortToByteBitDepthConversion"/> class.
    /// </summary>
    /// <param name="originalDepth">The bit depth being converted from.</param>
    /// <param name="newDepth">The bit depth being converted to.</param>
    public ShortToByteBitDepthConversion(int originalDepth, int newDepth)
    {
        // Check the original bit depth fits in a short and is above 0
        if (originalDepth > 16 || originalDepth < 0)
        {
            throw new ArgumentException("The original bit depth must be between 1 and 16", nameof(originalDepth));
        }

        // Check the new bit depth fits in a byte and is above 0
        if (newDepth > 8 || originalDepth < 0)
        {
            throw new ArgumentException("The new bit depth must be between 1 and 8", nameof(newDepth));
        }

        // Check the new bit depth is less than or the same as the original
        if (newDepth > originalDepth)
        {
            throw new ArgumentException("The new bit depth must be less than or the same as the original bit depth", nameof(newDepth));
        }

        // Calculate how many bits each sample needs to be shifted by
        _shiftFactor = 16 - originalDepth;
    }

    /// <summary>
    /// Converts a <see cref="Span{T}"/> of samples to the desired bit depth.
    /// </summary>
    /// <param name="samples">The samples to be converted.</param>
    /// <returns>The converted samples.</returns>
    public Span<byte> ConvertSamples(Span<short> samples)
    {
        // Get a second reference to the span of samples cast to the new type
        Span<byte> convertedSamples = MemoryMarshal.Cast<short, byte>(samples);

        // Convert each sample to the lower bit depth, and then write back over the original buffer
        for (int i = 0; i < samples.Length; i++)
        {
            convertedSamples[i] = (byte)(((samples[i] << _shiftFactor) + 32768) >> 8);
        }

        // Trim the span of converted samples to number of samples and return it
        return convertedSamples.Slice(0, samples.Length);
    }
}
