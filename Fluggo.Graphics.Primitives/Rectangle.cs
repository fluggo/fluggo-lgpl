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
	/// Represents a two-dimensional integer rectangle.
	/// </summary>
	public struct Rectangle {
		public int Left, Top, Right, Bottom;
		
		public Rectangle( int left, int top, int right, int bottom ) {
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		/// <summary>
		/// Gets the width of the rectangle.
		/// </summary>
		/// <value>The width of the rectangle, which can be positive, zero, or negative.</value>
		public int Width {
			get {
				return Right - Left;
			}
		}
		
		/// <summary>
		/// Gets the height of the rectangle.
		/// </summary>
		/// <value>The height of the rectangle, which can be positive, zero, or negative.</value>
		public int Height {
			get {
				return Bottom - Top;
			}
		}

		/// <summary>
		/// Gets a value that represents whether the rectangle is empty.
		/// </summary>
		/// <value>True if the rectangle has zero area, false otherwise.</value>
		public bool IsEmpty {
			get {
				return (Right == Left) || (Top == Bottom);
			}
		}
		
		public static implicit operator System.Drawing.Rectangle( Rectangle rect ) {
			return new System.Drawing.Rectangle( rect.Left, rect.Top, rect.Width, rect.Height );
		}
	}
}