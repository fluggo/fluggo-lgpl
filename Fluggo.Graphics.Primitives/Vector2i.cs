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
	public struct Vector2i {
		public int X, Y;

		public Vector2i( int x, int y ) {
			X = x;
			Y = y;
		}

		/// <summary>
		/// Represents a zero vector.
		/// </summary>
		/// <value>A vector with the value [0 0].</value>
		public static readonly Vector2i Zero = new Vector2i( 0, 0 );

		/// <summary>
		/// Represents a unit vector for the positive X axis.
		/// </summary>
		/// <value>A vector with the value [1 0].</value>
		public static readonly Vector2i UnitX = new Vector2i( 1, 0 );

		/// <summary>
		/// Represents a unit vector for the positive Y axis.
		/// </summary>
		/// <value>A vector with the value [0 1].</value>
		public static readonly Vector2i UnitY = new Vector2i( 0, 1 );

		/// <summary>
		/// Represents a unit vector for the negative X axis.
		/// </summary>
		/// <value>A vector with the value [-1 0].</value>
		public static readonly Vector2i NegativeUnitX = new Vector2i( -1, 0 );

		/// <summary>
		/// Represents a unit vector for the negative Y axis.
		/// </summary>
		/// <value>A vector with the value [0 -1].</value>
		public static readonly Vector2i NegativeUnitY = new Vector2i( 0, -1 );

		/// <summary>
		/// Represents an identity (unit) scale factor.
		/// </summary>
		/// <value>A vector with the value [1 1].</value>
		public static readonly Vector2i IdentityScale = new Vector2i( 1, 1 );

		#region Arithmetic operators
		public static Vector2i operator +( Vector2i v1, Vector2i v2 ) {
			return new Vector2i( v1.X + v2.X, v1.Y + v2.Y );
		}

		public static Vector2i operator -( Vector2i v1, Vector2i v2 ) {
			return new Vector2i( v1.X - v2.X, v1.Y - v2.Y );
		}

		public static Vector2i operator *( Vector2i v, int scalar ) {
			return new Vector2i( v.X * scalar, v.Y * scalar );
		}
		
		public static Vector2f operator *( Vector2i v, float scalar ) {
			return new Vector2f( v.X * scalar, v.Y * scalar );
		}

		public static Vector2i operator /( Vector2i v, int scalar ) {
			return new Vector2i( v.X / scalar, v.Y / scalar );
		}

		public static Vector2i operator *( int scalar, Vector2i v ) {
			return v * scalar;
		}

		public static Vector2i operator /( int scalar, Vector2i v ) {
			return new Vector2i( scalar / v.X, scalar / v.Y );
		}

		public static Vector2i operator -( Vector2i v ) {
			return new Vector2i( -v.X, -v.Y );
		}
		#endregion

		#region Vector operations
		/// <summary>
		/// Computes the dot product of two vectors.
		/// </summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>The dot product of the given vectors.</returns>
		public static int Dot( Vector2i v1, Vector2i v2 ) {
			return v1.X * v2.X + v1.Y * v2.Y;
		}

		/// <summary>
		/// Computes the cross product of two vectors.
		/// </summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>The cross product of the given vectors.</returns>
		public static int Cross( Vector2i v1, Vector2i v2 ) {
			return v1.X * v2.Y - v1.Y * v2.X;
		}

		public float Dot( Vector2i v ) {
			return Dot( this, v );
		}

		public float Cross( Vector2i v ) {
			return Cross( this, v );
		}

		///	<summary>
		///	Creates a vector that contains the minimum components of each given vector.
		///	</summary>
		///	<param name="v1">First <see cref="Vector2i"/> value to compare.</param>
		///	<param name="v2">Second <see cref="Vector2i"/> value to compare.</param>
		///	<returns>Returns a <see cref="Vector2i"/> with the minimum value of each component.</returns>
		public static Vector2i Min( Vector2i v1, Vector2i v2 ) {
			return new Vector2i(
				Math.Min( v1.X, v2.X ),
				Math.Min( v1.Y, v2.Y )
			);
		}

		///	<summary>
		///	Creates a vector that contains the maximum components of each given vector.
		///	</summary>
		///	<param name="v1">First <see cref="Vector2i"/> value to compare.</param>
		///	<param name="v2">Second <see cref="Vector2i"/> value to compare.</param>
		///	<returns>Returns a <see cref="Vector2i"/> with the maximum value of each component.</returns>
		public static Vector2i Max( Vector2i v1, Vector2i v2 ) {
			return new Vector2i(
				Math.Max( v1.X, v2.X ),
				Math.Max( v1.Y, v2.Y )
			);
		}

		public static float DistanceSquared( Vector2i v1, Vector2i v2 ) {
			return (v1 - v2).SquaredLength;
		}

		public static float Distance( Vector2i v1, Vector2i v2 ) {
			return (float) Math.Sqrt( DistanceSquared( v1, v2 ) );
		}

		public static Vector2i Clamp( Vector2i v, Vector2i minValue, Vector2i maxValue ) {
			return Max( Min( v, maxValue ), minValue );
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the square of the vector's length.
		/// </summary>
		/// <value>The square of the vector's length.</value>
		/// <remarks>This property is useful when comparing two vectors by length. You can compare the
		///	  <see cref="SquaredLength"/> property to determine the longer vector without incurring the
		///	  penalty of a lengthy square-root.</remarks>
		public int SquaredLength {
			get {
				return Dot( this, this );
			}
		}

		/// <summary>
		/// Gets the vector's length, or normal.
		/// </summary>
		/// <value>The vector's length, or normal.</value>
		public float Length {
			get {
				return (float) Math.Sqrt( SquaredLength );
			}
		}
		#endregion

		public override string ToString() {
			return string.Format( "[{0} {1}]", X, Y );
		}
	}
}