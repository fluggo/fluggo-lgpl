/*
	Fluggo Graphics Primitives Library
	Copyright (C) 2005-7  Brian J. Crowell <brian@fluggo.com>

	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;

namespace Fluggo.Graphics {
	/// <summary>
	/// Represents a packed 32-bit color with eight bits each for red, green, blue, and alpha, with alpha in the least significant position.
	/// </summary>
	public struct ColorRGBA {
		uint _color;
		const int __redOffset = 24, __greenOffset = 16, __blueOffset = 8, __alphaOffset = 0;

		/// <summary>
		/// Creates a new instance of the <see cref='ColorRGBA'/> structure.
		/// </summary>
		/// <param name="color">A packed RGBA color.</param>
		public ColorRGBA( int color ) {
			_color = unchecked( (uint) color );
		}

		/// <summary>
		/// Creates a new instance of the <see cref='ColorRGBA'/> structure.
		/// </summary>
		/// <param name="red">The value of red for the color, from 0 (black) to 255 (red).</param>
		/// <param name="green">The value of green for the color, from 0 (black) to 255 (green).</param>
		/// <param name="blue">The value of blue for the color, from 0 (black) to 255 (blue).</param>
		/// <param name="alpha">The value of alpha for the color, from 0 (transparent) to 255 (opaque).</param>
		public ColorRGBA( byte red, byte green, byte blue, byte alpha ) {
			_color = unchecked( (uint) ((red << __redOffset) | (green << __greenOffset) | (blue << __blueOffset) | (alpha << __alphaOffset)) );
		}

		/// <summary>
		/// Creates a new instance of the <see cref='ColorRGBA'/> structure.
		/// </summary>
		/// <param name="red">The value of red for the color, from 0 (black) to 255 (red).</param>
		/// <param name="green">The value of green for the color, from 0 (black) to 255 (green).</param>
		/// <param name="blue">The value of blue for the color, from 0 (black) to 255 (blue).</param>
		/// <remarks>The alpha value is set to 255, although this value may be ignored.</remarks>
		public ColorRGBA( byte red, byte green, byte blue )
			: this( red, green, blue, 0xFF ) {
		}

		public int PackedColor {
			get {
				return unchecked( (int) _color );
			}
			set {
				_color = unchecked( (uint) value );
			}
		}

		/// <summary>
		/// Gets or sets the value of red for the color.
		/// </summary>
		/// <value>The value of red for the color, from 0 (black) to 255 (red).</value>
		public byte Red {
			get {
				return unchecked( (byte) (_color >> __redOffset) );
			}
			set {
				unchecked {
					_color = (_color & (uint) (0xFF << __redOffset)) | (uint) (value << __redOffset);
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of green for the color.
		/// </summary>
		/// <value>The value of green for the color, from 0 (black) to 255 (green).</value>
		public byte Green {
			get {
				return unchecked( (byte) (_color >> __greenOffset) );
			}
			set {
				unchecked {
					_color = (_color & (uint) (0xFF << __greenOffset)) | (uint) (value << __greenOffset);
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of blue for the color.
		/// </summary>
		/// <value>The value of blue for the color, from 0 (black) to 255 (blue).</value>
		public byte Blue {
			get {
				return unchecked( (byte) (_color >> __blueOffset) );
			}
			set {
				unchecked {
					_color = (_color & (uint) (0xFF << __blueOffset)) | (uint) (value << __blueOffset);
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of alpha (transparency) for the color.
		/// </summary>
		/// <value>The value of alpha for the color, from 0 (transparent) to 255 (opaque).</value>
		public byte Alpha {
			get {
				return unchecked( (byte) (_color >> __alphaOffset) );
			}
			set {
				unchecked {
					_color = (_color & (uint) (0xFF << __alphaOffset)) | (uint) (value << __alphaOffset);
				}
			}
		}

		/// <summary>
		/// Converts a packed RGB color to a packed ARGB color.
		/// </summary>
		/// <param name="color"><see cref="ColorRGBA"/> value to convert.</param>
		/// <returns>The given color as a <see cref="ColorARGB"/> value.</returns>
		public static implicit operator ColorARGB( ColorRGBA color ) {
			return new ColorARGB( color.Red, color.Green, color.Blue, color.Alpha );
		}
	}
}