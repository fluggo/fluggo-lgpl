/*
	Fluggo Graphics Primitives Library
	Copyright (C) 2005-7  Brian J. Crowell <brian@fluggo.com>

	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
	Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

using System;
namespace Fluggo.Graphics
{
    /// <summary>
    /// The nasty, evil Quaternion that is a standard part of any graphics library. And which I'm sure we all steal from each other.
    /// </summary>
    public struct Quaternion {
		// Source: http://www.gamedev.net/reference/articles/article1095.asp
		// Note that this article uses column vectors, where as the Fluggo primitives library
		// uses row vectors, so all matrices are transposed.
		
		public float W, X, Y, Z;

		public Quaternion( float w, float x, float y, float z ) {
			W = w;
			X = x;
			Y = y;
			Z = z;
		}
		
		public Matrix3f ToMatrix3f() {
			return new Matrix3f(
				W * W + X * X - Y * Y - Z * Z,
				2.0f * X * Y + 2.0f * W * Z,
				2.0f * X * Z - 2.0f * W * Y,
				
				2.0f * X * Y - 2.0f * W * Z,
				W * W - X * X + Y * Y - Z * Z,
				2.0f * Y * Z + 2.0f * W * X,
				
				2.0f * X * Z + 2.0f * W * Y,
				2.0f * Y * Z - 2.0f * W * X,
				W * W - X * X - Y * Y + Z * Z );
		}
		
		public void ToAxisAngle( out Vector3f axis, out float angle ) {
			angle = 2.0f * (float) Math.Acos( W );

			float scale = (float)(1.0 / Math.Sqrt( X * X + Y * Y + Z * Z ));
			axis = new Vector3f( X * scale, Y * scale, Z * scale );
		}
		
		public static Quaternion FromAxisAngle( Vector3f axis, float angle ) {
			float sin = (float) Math.Sin( angle * 0.5f );
			
			return new Quaternion( (float) Math.Cos( angle * 0.5f ),
				axis.X * sin, axis.Y * sin, axis.Z * sin );
		}
		
		public static Quaternion Add( Quaternion q1, Quaternion q2 ) {
			return new Quaternion( q1.W + q2.W, q1.X + q2.X, q1.Y + q2.Y, q1.Z + q2.Z );
		}

		public static Quaternion operator +( Quaternion q1, Quaternion q2 ) {
			return Quaternion.Add( q1, q2 );
		}
		
		public static Quaternion Subtract( Quaternion q1, Quaternion q2 ) {
			return new Quaternion( q1.W - q2.W, q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z );
		}
		
		public static Quaternion operator -( Quaternion q1, Quaternion q2 ) {
			return Quaternion.Subtract( q1, q2 );
		}

		public static Quaternion Multiply( Quaternion q1, Quaternion q2 ) {
			// This is from: http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/arithmetic/index.htm
			return new Quaternion(
				q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z,
				q1.X * q2.W + q1.W * q2.X + q1.Y * q2.Z - q1.Z * q2.Y,
				q1.W * q2.Y - q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X,
				q1.W * q2.Z + q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W );
		}
		
		public static Quaternion operator *( Quaternion q1, Quaternion q2 ) {
			return Quaternion.Multiply( q1, q2 );
		}
    }
}
