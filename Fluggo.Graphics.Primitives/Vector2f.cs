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
	public struct Vector2f {
		public float X, Y;

		public Vector2f( float x, float y ) {
			X = x;
			Y = y;
		}

		/// <summary>
		/// Represents a zero vector.
		/// </summary>
		/// <value>A vector with the value [0 0].</value>
		public static readonly Vector2f Zero = new Vector2f( 0.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the positive X axis.
		/// </summary>
		/// <value>A vector with the value [1 0].</value>
		public static readonly Vector2f UnitX = new Vector2f( 1.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the positive Y axis.
		/// </summary>
		/// <value>A vector with the value [0 1].</value>
		public static readonly Vector2f UnitY = new Vector2f( 0.0f, 1.0f );

		/// <summary>
		/// Represents a unit vector for the negative X axis.
		/// </summary>
		/// <value>A vector with the value [-1 0].</value>
		public static readonly Vector2f NegativeUnitX = new Vector2f( -1.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the negative Y axis.
		/// </summary>
		/// <value>A vector with the value [0 -1].</value>
		public static readonly Vector2f NegativeUnitY = new Vector2f( 0.0f, -1.0f );

		/// <summary>
		/// Represents an identity (unit) scale factor.
		/// </summary>
		/// <value>A vector with the value [1 1].</value>
		public static readonly Vector2f IdentityScale = new Vector2f( 1.0f, 1.0f );

		#region Arithmetic operators
		public static Vector2f operator +( Vector2f v1, Vector2f v2 ) {
			return new Vector2f( v1.X + v2.X, v1.Y + v2.Y );
		}

		public static Vector2f operator -( Vector2f v1, Vector2f v2 ) {
			return new Vector2f( v1.X - v2.X, v1.Y - v2.Y );
		}

		public static Vector2f operator *( Vector2f v, float scalar ) {
			return new Vector2f( v.X * scalar, v.Y * scalar );
		}

		public static Vector2f operator /( Vector2f v, float scalar ) {
			return v * (1.0f / scalar);
		}

		public static Vector2f operator *( float scalar, Vector2f v ) {
			return v * scalar;
		}

		public static Vector2f operator /( float scalar, Vector2f v ) {
			return new Vector2f( scalar / v.X, scalar / v.Y );
		}

		public static Vector2f operator -( Vector2f v ) {
			return new Vector2f( -v.X, -v.Y );
		}
		#endregion

		#region Vector operations
		/// <summary>
		/// Computes the dot product of two vectors.
		/// </summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>The dot product of the given vectors.</returns>
		public static float Dot( Vector2f v1, Vector2f v2 ) {
			return v1.X * v2.X + v1.Y * v2.Y;
		}

		/// <summary>
		/// Computes the unit vector of the given vector.
		/// </summary>
		/// <param name="v">Vector to normalize.</param>
		/// <returns>Vector to normalize. If <paramref name="v"/> has a length of zero, a zero vector is returned.</returns>
		public static Vector2f Normalize( Vector2f v ) {
			float squareLength = v.SquaredLength;

			if( squareLength == 0.0f )
				return v;

			return v / (float) Math.Sqrt( squareLength );
		}

		/// <summary>
		/// Computes the cross product of two vectors.
		/// </summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>The cross product of the given vectors.</returns>
		public static float Cross( Vector2f v1, Vector2f v2 ) {
			return v1.X * v2.Y - v1.Y * v2.X;
		}

		public float Dot( Vector2f v ) {
			return Dot( this, v );
		}

		public Vector2f Normalize() {
			return Normalize( this );
		}

		public float Cross( Vector2f v ) {
			return Cross( this, v );
		}

		///	<summary>
		///	Creates a vector that contains the minimum components of each given vector.
		///	</summary>
		///	<param name="v1">First <see cref="Vector2f"/> value to compare.</param>
		///	<param name="v2">Second <see cref="Vector2f"/> value to compare.</param>
		///	<returns>Returns a <see cref="Vector2f"/> with the minimum value of each component.</returns>
		public static Vector2f Min( Vector2f v1, Vector2f v2 ) {
			return new Vector2f(
				Math.Min( v1.X, v2.X ),
				Math.Min( v1.Y, v2.Y )
			);
		}

		///	<summary>
		///	Creates a vector that contains the maximum components of each given vector.
		///	</summary>
		///	<param name="v1">First <see cref="Vector2f"/> value to compare.</param>
		///	<param name="v2">Second <see cref="Vector2f"/> value to compare.</param>
		///	<returns>Returns a <see cref="Vector2f"/> with the maximum value of each component.</returns>
		public static Vector2f Max( Vector2f v1, Vector2f v2 ) {
			return new Vector2f(
				Math.Max( v1.X, v2.X ),
				Math.Max( v1.Y, v2.Y )
			);
		}

		public static float DistanceSquared( Vector2f v1, Vector2f v2 ) {
			return (v1 - v2).SquaredLength;
		}

		public static float Distance( Vector2f v1, Vector2f v2 ) {
			return (float) Math.Sqrt( DistanceSquared( v1, v2 ) );
		}

		public static Vector2f Clamp( Vector2f v, Vector2f minValue, Vector2f maxValue ) {
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
		public float SquaredLength {
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

		public Vector4f ToPointVector4f() {
			return new Vector4f( X, Y, 0.0f, 1.0f );
		}
		
		public Vector4f ToDirectionVector4f() {
			return new Vector4f( X, Y, 0.0f, 0.0f );
		}
		
		public override string ToString() {
			return string.Format( "[{0:0.##} {1:0.##}]", X, Y );
		}
	}
}