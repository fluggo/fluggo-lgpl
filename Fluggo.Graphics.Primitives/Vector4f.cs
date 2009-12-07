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
using System.Collections.Generic;
using System.Text;

namespace Fluggo.Graphics {
	/// <summary>
	/// Specifies a homogeneous three-dimensional floating-point vector.
	/// </summary>
	/// <remarks>For the purposes of matrix multiplication, all vertices are considered row-vertices.</remarks>
	public struct Vector4f {
		public float X, Y, Z, W;
		
		public Vector4f( float x, float y, float z, float w ) {
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Represents a zero vector.
		/// </summary>
		/// <value>A vector with the value [0 0 0 0].</value>
		/// <remarks>When using homogeneous vectors, a nonzero W value represents a point, and a zero W value
		///   represents a directional vector.</remarks>
		public static readonly Vector4f Zero = new Vector4f();

		/// <summary>
		/// Represents a zero point.
		/// </summary>
		/// <value>A vector with the value [0 0 0 1].</value>
		/// <remarks>When using homogeneous vectors, a nonzero W value represents a point, and a zero W value
		///   represents a directional vector.</remarks>
		public static readonly Vector4f ZeroPoint = new Vector4f( 0.0f, 0.0f, 0.0f, 1.0f );

		/// <summary>
		/// Represents a unit vector for the positive X axis.
		/// </summary>
		/// <value>A vector with the value [1 0 0 0].</value>
		public static readonly Vector4f UnitX = new Vector4f( 1.0f, 0.0f, 0.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the positive Y axis.
		/// </summary>
		/// <value>A vector with the value [0 1 0 0].</value>
		public static readonly Vector4f UnitY = new Vector4f( 0.0f, 1.0f, 0.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the positive Z axis.
		/// </summary>
		/// <value>A vector with the value [0 0 1 0].</value>
		public static readonly Vector4f UnitZ = new Vector4f( 0.0f, 0.0f, 1.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the negative X axis.
		/// </summary>
		/// <value>A vector with the value [-1 0 0 0].</value>
		public static readonly Vector4f NegativeUnitX = new Vector4f( -1.0f, 0.0f, 0.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the negative Y axis.
		/// </summary>
		/// <value>A vector with the value [0 -1 0 0].</value>
		public static readonly Vector4f NegativeUnitY = new Vector4f( 0.0f, -1.0f, 0.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the negative Z axis.
		/// </summary>
		/// <value>A vector with the value [0 0 -1 0].</value>
		public static readonly Vector4f NegativeUnitZ = new Vector4f( 0.0f, 0.0f, -1.0f, 0.0f );

		/// <summary>
		/// Represents an identity (unit) scale factor.
		/// </summary>
		/// <value>A vector with the value [1 1 1 1].</value>
		public static readonly Vector4f IdentityScale = new Vector4f( 1.0f, 1.0f, 1.0f, 1.0f );

	#region Arithmetic operators
		public static Vector4f Add( Vector4f v1, Vector4f v2 ) {
			return new Vector4f( v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W );
		}
		
		public static Vector4f operator +( Vector4f v1, Vector4f v2 ) {
			return Add( v1, v2 );
		}
		
		public Vector4f Offset( Vector4f v ) {
			return Add( this, v );
		}
		
		public Vector4f Offset( float x, float y, float z, float w ) {	
			return Add( this, new Vector4f( x, y, z, w ) );
		}
		
		public static Vector4f Subtract( Vector4f v1, Vector4f v2 ) {
			return new Vector4f( v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W );
		}

		public static Vector4f operator -( Vector4f v1, Vector4f v2 ) {
			return Subtract( v1, v2 );
		}

		public Vector4f Scale( float scalar ) {
			return new Vector4f( X * scalar, Y * scalar, Z * scalar, W * scalar );
		}
		
		public static Vector4f operator *( Vector4f v, float scalar ) {
			return v.Scale( scalar );
		}

		public static Vector4f operator /( Vector4f v, float scalar ) {
			return v.Scale( 1.0f / scalar );
		}

		public static Vector4f operator *( float scalar, Vector4f v ) {
			return v.Scale( scalar );
		}

		public static Vector4f operator /( float scalar, Vector4f v ) {
			return new Vector4f( scalar / v.X, scalar / v.Y, scalar / v.Z, scalar / v.W );
		}

		public Vector4f Negate() {
			return new Vector4f( -X, -Y, -Z, -W );
		}
		
		public static Vector4f operator -( Vector4f v ) {
			return v.Negate();
		}
	#endregion

	#region Vector operations
		/// <summary>
		/// Computes the dot product of two vectors.
		/// </summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>The dot product of the given vectors.</returns>
		public static float Dot( Vector4f v1, Vector4f v2 ) {
			return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
		}

		/// <summary>
		/// Computes the unit vector of the given vector.
		/// </summary>
		/// <param name="v">Vector to normalize.</param>
		/// <returns>Vector to normalize. If <paramref name="v"/> has a length of zero, a zero vector is returned.</returns>
		public static Vector4f Normalize( Vector4f v ) {
			float squareLength = v.SquaredLength;

			if( squareLength == 0.0f )
				return v;

			return v / (float) Math.Sqrt( squareLength );
		}

		/// <summary>
		/// Computes the homogeneous vector with unit W of the given vector.
		/// </summary>
		/// <param name="v">Vector to normalize.</param>
		/// <returns>Vector to normalize. If <paramref name="v"/> has a W of zero, a zero vector is returned.</returns>
		public static Vector4f NormalizeW( Vector4f v ) {
			if( v.W == 0.0f )
				return v;

			return v / v.W;
		}
		
		/*		/// <summary>
				/// Computes the cross product of two vectors.
				/// </summary>
				/// <param name="v1">First vector.</param>
				/// <param name="v2">Second vector.</param>
				/// <returns>The cross product of the given vectors.</returns>
				public static Vector4f Cross( Vector4f v1, Vector4f v2 ) {
					return new Vector4f(
						v1.Y * v2.Z - v1.Z * v2.Y,
						v1.Z * v2.X - v1.X * v2.Z,
						v1.X * v2.Y - v1.Y * v2.X
					);
				}*/

		public float Dot( Vector4f v ) {
			return Dot( this, v );
		}

		public Vector4f Normalize() {
			return Normalize( this );
		}
		
		public Vector4f NormalizeW() {
			return NormalizeW( this );
		}
		
		/// <summary>
		/// Multiplies the elements of two vectors to form a third.
		/// </summary>
		/// <param name="v1">First vector to multiply.</param>
		/// <param name="v2">Second vector to multiply.</param>
		/// <returns>The result of the multiplication.</returns>
		/// <remarks>This method multiplies the elements individually. For example, the returned X value is
		///   the product of v1.X and v2.X.</remarks>
		public static Vector4f MultiplyPiecewise( Vector4f v1, Vector4f v2 ) {
			return new Vector4f( v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W );
		}

/*		public Vector4f Cross( Vector4f v ) {
			return Cross( this, v );
		}*/

		///	<summary>
		///	Creates a vector that contains the minimum components of each given vector.
		///	</summary>
		///	<param name="v1">First <see cref="Vector4f"/> value to compare.</param>
		///	<param name="v2">Second <see cref="Vector4f"/> value to compare.</param>
		///	<returns>Returns a <see cref="Vector4f"/> with the minimum value of each component.</returns>
		public static Vector4f Min( Vector4f v1, Vector4f v2 ) {
			return new Vector4f(
				Math.Min( v1.X, v2.X ),
				Math.Min( v1.Y, v2.Y ),
				Math.Min( v1.Z, v2.Z ),
				Math.Min( v1.W, v2.W )
			);
		}

		///	<summary>
		///	Creates a vector that contains the maximum components of each given vector.
		///	</summary>
		///	<param name="v1">First <see cref="Vector4f"/> value to compare.</param>
		///	<param name="v2">Second <see cref="Vector4f"/> value to compare.</param>
		///	<returns>Returns a <see cref="Vector4f"/> with the maximum value of each component.</returns>
		public static Vector4f Max( Vector4f v1, Vector4f v2 ) {
			return new Vector4f(
				Math.Max( v1.X, v2.X ),
				Math.Max( v1.Y, v2.Y ),
				Math.Max( v1.Z, v2.Z ),
				Math.Max( v1.W, v2.W )
			);
		}

		public static float DistanceSquared( Vector4f v1, Vector4f v2 ) {
			return (v1 - v2).SquaredLength;
		}

		public static float Distance( Vector4f v1, Vector4f v2 ) {
			return (float) Math.Sqrt( DistanceSquared( v1, v2 ) );
		}

		public static Vector4f Clamp( Vector4f v, Vector4f minValue, Vector4f maxValue ) {
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

		public static explicit operator Vector4f( Vector3f v ) {
			return new Vector4f( v.X, v.Y, v.Z, 0.0f );
		}
		
		public override string ToString() {
			return string.Format( "[{0:0.###} {1:0.###} {2:0.###} {3:0.###}]", X, Y, Z, W );
		}
	}
}
