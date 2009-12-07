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
	/// Specifies a three-dimensional floating-point vector.
	/// </summary>
	/// <remarks>For the purposes of matrix multiplication, all vertices are considered row-vertices.</remarks>
	public struct Vector3f : IEquatable<Vector3f> {
		public float X, Y, Z;
		
		public Vector3f( float x, float y, float z ) {
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Represents a zero vector.
		/// </summary>
		/// <value>A vector with the value [0 0 0].</value>
		public static readonly Vector3f Zero = new Vector3f( 0.0f, 0.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the positive X axis.
		/// </summary>
		/// <value>A vector with the value [1 0 0].</value>
		public static readonly Vector3f UnitX = new Vector3f( 1.0f, 0.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the positive Y axis.
		/// </summary>
		/// <value>A vector with the value [0 1 0].</value>
		public static readonly Vector3f UnitY = new Vector3f( 0.0f, 1.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the positive Z axis.
		/// </summary>
		/// <value>A vector with the value [0 0 1].</value>
		public static readonly Vector3f UnitZ = new Vector3f( 0.0f, 0.0f, 1.0f );

		/// <summary>
		/// Represents a unit vector for the negative X axis.
		/// </summary>
		/// <value>A vector with the value [-1 0 0].</value>
		public static readonly Vector3f NegativeUnitX = new Vector3f( -1.0f, 0.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the negative Y axis.
		/// </summary>
		/// <value>A vector with the value [0 -1 0].</value>
		public static readonly Vector3f NegativeUnitY = new Vector3f( 0.0f, -1.0f, 0.0f );

		/// <summary>
		/// Represents a unit vector for the negative Z axis.
		/// </summary>
		/// <value>A vector with the value [0 0 -1].</value>
		public static readonly Vector3f NegativeUnitZ = new Vector3f( 0.0f, 0.0f, -1.0f );

		/// <summary>
		/// Represents an identity (unit) scale factor.
		/// </summary>
		/// <value>A vector with the value [1 1 1].</value>
		public static readonly Vector3f IdentityScale = new Vector3f( 1.0f, 1.0f, 1.0f );
		
	#region Arithmetic operators
		public static Vector3f operator +( Vector3f v1, Vector3f v2 ) {
			return new Vector3f( v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z );
		}
		
		public static Vector3f operator -( Vector3f v1, Vector3f v2 ) {
			return new Vector3f( v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z );
		}
		
		public static Vector3f operator *( Vector3f v, float scalar ) {
			return new Vector3f( v.X * scalar, v.Y * scalar, v.Z * scalar );
		}
		
		public static Vector3f operator /( Vector3f v, float scalar ) {
			return v * (1.0f / scalar);
		}
		
		public static Vector3f operator *( float scalar, Vector3f v ) {
			return v * scalar;
		}
		
		public static Vector3f operator /( float scalar, Vector3f v ) {
			return new Vector3f( scalar / v.X, scalar / v.Y, scalar / v.Z );
		}
		
		public static Vector3f operator -( Vector3f v ) {
			return new Vector3f( -v.X, -v.Y, -v.Z );
		}
	#endregion

	#region Vector operations
		/// <summary>
		/// Computes the dot product of two vectors.
		/// </summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>The dot product of the given vectors.</returns>
		public static float Dot( Vector3f v1, Vector3f v2 ) {
			return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
		}
		
		/// <summary>
		/// Computes the unit vector of the given vector.
		/// </summary>
		/// <param name="v">Vector to normalize.</param>
		/// <returns>Vector to normalize. If <paramref name="v"/> has a length of zero, a zero vector is returned.</returns>
		public static Vector3f Normalize( Vector3f v ) {
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
		public static Vector3f Cross( Vector3f v1, Vector3f v2 ) {
			return new Vector3f(
				v1.Y * v2.Z - v1.Z * v2.Y,
				v1.Z * v2.X - v1.X * v2.Z,
				v1.X * v2.Y - v1.Y * v2.X
			);
		}
		
		public float Dot( Vector3f v ) {
			return Dot( this, v );
		}
		
		public Vector3f Normalize() {
			return Normalize( this );
		}
		
		public Vector3f Cross( Vector3f v ) {
			return Cross( this, v );
		}

		///	<summary>
		///	Creates a vector that contains the minimum components of each given vector.
		///	</summary>
		///	<param name="v1">First <see cref="Vector3f"/> value to compare.</param>
		///	<param name="v2">Second <see cref="Vector3f"/> value to compare.</param>
		///	<returns>Returns a <see cref="Vector3f"/> with the minimum value of each component.</returns>
		public static Vector3f Min( Vector3f v1, Vector3f v2 ) {
			return new Vector3f(
				Math.Min( v1.X, v2.X ),
				Math.Min( v1.Y, v2.Y ),
				Math.Min( v1.Z, v2.Z )
			);
		}

		///	<summary>
		///	Creates a vector that contains the maximum components of each given vector.
		///	</summary>
		///	<param name="v1">First <see cref="Vector3f"/> value to compare.</param>
		///	<param name="v2">Second <see cref="Vector3f"/> value to compare.</param>
		///	<returns>Returns a <see cref="Vector3f"/> with the maximum value of each component.</returns>
		public static Vector3f Max( Vector3f v1, Vector3f v2 ) {
			return new Vector3f(
				Math.Max( v1.X, v2.X ),
				Math.Max( v1.Y, v2.Y ),
				Math.Max( v1.Z, v2.Z )
			);
		}
		
		public static float DistanceSquared( Vector3f v1, Vector3f v2 ) {
			return (v1 - v2).SquaredLength;
		}
		
		public static float Distance( Vector3f v1, Vector3f v2 ) {
			return (float) Math.Sqrt( DistanceSquared( v1, v2 ) );
		}
		
		public static Vector3f Clamp( Vector3f v, Vector3f minValue, Vector3f maxValue ) {
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

		/// <summary>
		/// Converts a <see cref="Vector4f"/> to a <see cref="Vector3f"/> by discarding <see cref="Vector4f.W"/>.
		/// </summary>
		/// <param name="v"><see cref="Vector4f"/> to convert.</param>
		/// <returns>A <see cref="Vector3f"/> with the X, Y, and Z coordinates of the given vector.</returns>
		public static explicit operator Vector3f( Vector4f v ) {
			return new Vector3f( v.X, v.Y, v.Z );
		}
		
		public override string ToString() {
			return string.Format( "[{0:0.###} {1:0.###} {2:0.###}]", X, Y, Z );
		}
		
		public Vector4f ToPointVector4f() {
			return new Vector4f( X, Y, Z, 1.0f );
		}
		
		public Vector4f ToDirectionVector4f() {
			return new Vector4f( X, Y, Z, 0.0f );
		}

		public override bool Equals( object obj ) {
			if( obj == null )
				return false;
				
			Vector3f value = (Vector3f) obj;
			return value == this;
		}
		
		public bool Equals( Vector3f other ) {
			return other == this;
		}

		public override int GetHashCode() {
			return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
		}
		
		public static bool operator ==( Vector3f v1, Vector3f v2 ) {
			return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
		}
		
		public static bool operator !=( Vector3f v1, Vector3f v2 ) {
			return !(v1 == v2);
		}
	}
}