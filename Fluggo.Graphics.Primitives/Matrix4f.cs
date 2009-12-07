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
	public struct Matrix4f {
		public float M11, M12, M13, M14;
		public float M21, M22, M23, M24;
		public float M31, M32, M33, M34;
		public float M41, M42, M43, M44;
		
		public Matrix4f(
			float m11, float m12, float m13, float m14,
			float m21, float m22, float m23, float m24,
			float m31, float m32, float m33, float m34,
			float m41, float m42, float m43, float m44 )
		{
			M11 = m11; M12 = m12; M13 = m13; M14 = m14;
			M21 = m21; M22 = m22; M23 = m23; M24 = m24;
			M31 = m31; M32 = m32; M33 = m33; M34 = m34;
			M41 = m41; M42 = m42; M43 = m43; M44 = m44;
		}
		
		public Matrix4f( Matrix3f matrix ) {
			M11 = matrix.M11; M12 = matrix.M12; M13 = matrix.M13; M14 = 0.0f;
			M21 = matrix.M21; M22 = matrix.M22; M23 = matrix.M23; M24 = 0.0f;
			M31 = matrix.M31; M32 = matrix.M32; M33 = matrix.M33; M34 = 0.0f;
			M41 = 0.0f; M42 = 0.0f; M43 = 0.0f; M44 = 1.0f;
		}
		
		public Matrix4f( Vector4f row1, Vector4f row2, Vector4f row3, Vector4f row4 ) :
				this( row1.X, row1.Y, row1.Z, row1.W, row2.X, row2.Y, row2.Z, row2.W,
				row3.X, row3.Y, row3.Z, row3.W, row4.X, row4.Y, row4.Z, row4.W ) {
		}
		
		public static Matrix4f Concatenate( Matrix4f m1, Matrix4f m2 ) {
			return new Matrix4f(
				m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41,
				m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42,
				m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43,
				m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44,

				m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41,
				m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42,
				m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43,
				m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44,

				m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41,
				m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42,
				m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43,
				m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44,
																		  
				m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41,
				m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42,
				m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43,
				m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44
			);
		}
		
		public Vector4f Transform( Vector4f v ) {
			return new Vector4f(
				v.X * M11 + v.Y * M21 + v.Z * M31 + v.W * M41,
				v.X * M12 + v.Y * M22 + v.Z * M32 + v.W * M42,
				v.X * M13 + v.Y * M23 + v.Z * M33 + v.W * M43,
				v.X * M14 + v.Y * M24 + v.Z * M34 + v.W * M44
			);
		}
		
		/* A transform matrix is:
		 *    X->X   X->Y   X->Z   X->W
		 *    Y->X   Y->Y   Y->Z   Y->W
		 *    Z->X   Z->Y   Z->Z   Z->W
		 *    W->X   W->Y   W->Z   W->W
		 */
		
		public static Matrix4f operator *( Matrix4f m, float scalar ) {
			return new Matrix4f(
				m.M11 * scalar, m.M12 * scalar, m.M13 * scalar, m.M14 * scalar,
				m.M21 * scalar, m.M22 * scalar, m.M23 * scalar, m.M24 * scalar,
				m.M31 * scalar, m.M32 * scalar, m.M33 * scalar, m.M34 * scalar,
				m.M41 * scalar, m.M42 * scalar, m.M43 * scalar, m.M44 * scalar );
		}

		public static Matrix4f operator *( float scalar, Matrix4f m ) {
			return m * scalar;
		}

		public static Matrix4f operator /( Matrix4f m, float scalar ) {
			return m * (1.0f / scalar);
		}
		
		public static Vector4f operator *( Vector4f v, Matrix4f m ) {
			return m.Transform( v );
		}
		
		public static Matrix4f operator *( Matrix4f m1, Matrix4f m2 ) {
			return Concatenate( m1, m2 );
		}
		
		public static Matrix4f CreateTranslation( float tx, float ty, float tz ) {
			return new Matrix4f(
				1.0f,	0.0f,	0.0f,	0.0f,
				0.0f,	1.0f,	0.0f,	0.0f,
				0.0f,	0.0f,	1.0f,	0.0f,
				tx,		ty,		tz,		1.0f
			);
		}
		
		public static Matrix4f CreateTranslation( Vector3f v ) {
			return CreateTranslation( v.X, v.Y, v.Z );
		}
		
		public static Matrix4f CreateScale( float sx, float sy, float sz ) {
			return new Matrix4f(
				sx,		0.0f,	0.0f,	0.0f,
				0.0f,	sy,		0.0f,	0.0f,
				0.0f,	0.0f,	sz,		0.0f,
				0.0f,	0.0f,	0.0f,	1.0f
			);
		}
		
		public static Matrix4f CreateScale( float factor ) {
			return CreateScale( factor, factor, factor );
		}
		
		public static Matrix4f CreateScale( Vector3f v ) {
			return CreateScale( v.X, v.Y, v.Z );
		}
		
		public static Matrix4f CreateLookAt( Vector3f camera, Vector3f target, Vector3f up ) {
			Vector3f z = (camera - target).Normalize();
			Vector3f x = up.Cross( z ).Normalize();
			Vector3f y = z.Cross( x );
			
			return new Matrix4f(
				x.X,				y.X,				z.X,				0.0f,
				x.Y,				y.Y,				z.Y,				0.0f,
				x.Z,				y.Z,				z.Z,				0.0f,
				-x.Dot( camera ),	-y.Dot( camera ),	-z.Dot( camera ),	1.0f
			);
		}
		
		public static Matrix4f CreatePerspectiveProjection( float nearWidth, float nearHeight, float nearZ, float farZ ) {
			if( nearWidth == 0.0f )
				throw new ArgumentOutOfRangeException( "nearWidth", "This value cannot be zero." );

			if( nearHeight == 0.0f )
				throw new ArgumentOutOfRangeException( "nearHeight", "This value cannot be zero." );
				
			if( nearZ == farZ )
				throw new ArgumentException( "The near Z and the far Z planes cannot be equal." );

			float scaleZ = 1.0f / (farZ - nearZ);
				
			return new Matrix4f(
				nearWidth,		0.0f,			0.0f,				0.0f,
				0.0f,			nearHeight,		0.0f,				0.0f,
				0.0f,			0.0f,			scaleZ,				1.0f / nearZ,
				0.0f,			0.0f,			-nearZ * scaleZ,	0.0f
			);
		}
		
		/// <summary>
		/// Creates a perspective projection from a field-of-view.
		/// </summary>
		/// <param name="fieldOfViewY">Field of view in the Y direction, in radians.</param>
		/// <param name="aspectRatio">Aspect ratio, as width divided by height.</param>
		/// <param name="nearZ">Value of the near Z plane.</param>
		/// <param name="farZ">Value of the far Z plane.</param>
		/// <returns>Returns a right-handed perspective projection transform.</returns>
		public static Matrix4f CreateFieldOfViewProjection( float fieldOfViewY, float aspectRatio, float nearZ, float farZ ) {
			float scaleY = -1.0f / (nearZ * (float) Math.Tan( fieldOfViewY * 0.5f ));
			return CreatePerspectiveProjection( scaleY, scaleY * aspectRatio, nearZ, farZ );
		}

		public static Matrix4f CreateOrthogonalOffCenterProjection( float left, float right, float bottom, float top, float nearZ, float farZ ) {
			if( left == right )
				throw new ArgumentException( "The left and right planes cannot be equal." );

			if( bottom == top )
				throw new ArgumentException( "The bottom and top planes cannot be equal." );

			if( nearZ == farZ )
				throw new ArgumentException( "The near Z and the far Z planes cannot be equal." );

			float invDepth = 1.0f / (nearZ - farZ);

			return new Matrix4f(
				2.0f / (right - left),				0.0f,								0.0f, 				0.0f,
				0.0f,								2.0f / (top - bottom),				0.0f, 				0.0f,
				0.0f,								0.0f,								invDepth,			0.0f,
				(left + right) / (left - right),	(bottom + top) / (bottom - top),	nearZ * invDepth,	1.0f
			);
		}

		public static Matrix4f CreateOrthogonalProjection( float width, float height, float nearZ, float farZ ) {
			return CreateOrthogonalOffCenterProjection( width * -0.5f, width * 0.5f, height * -0.5f, height * 0.5f, nearZ, farZ );
		}
		
		public static Matrix4f CreateXRotation( float angle ) {
			float cos = (float) Math.Cos( angle );
			float sin = (float) Math.Sin( angle );
			
			return new Matrix4f(
				1.0f, 0.0f, 0.0f, 0.0f,
				0.0f,  cos,  sin, 0.0f,
				0.0f, -sin,  cos, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f
			);
		}

		public static Matrix4f CreateYRotation( float angle ) {
			float cos = (float) Math.Cos( angle );
			float sin = (float) Math.Sin( angle );

			return new Matrix4f(
				 cos, 0.0f, -sin, 0.0f,
				0.0f, 1.0f, 0.0f, 0.0f,
				 sin, 0.0f,  cos, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f
			);
		}

		public static Matrix4f CreateZRotation( float angle ) {
			float cos = (float) Math.Cos( angle );
			float sin = (float) Math.Sin( angle );

			return new Matrix4f(
				 cos,  sin, 0.0f, 0.0f,
				-sin,  cos, 0.0f, 0.0f,
				0.0f, 0.0f, 1.0f, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f
			);
		}
		
		public static Matrix4f CreateYawPitchRollRotation( float yaw, float pitch, float roll ) {
			return CreateYRotation( yaw ) * CreateXRotation( pitch ) * CreateZRotation( roll );
		}
		
		public static readonly Matrix4f Zero = new Matrix4f();
		
		public static readonly Matrix4f Identity = new Matrix4f(
			1.0f, 0.0f, 0.0f, 0.0f,
			0.0f, 1.0f, 0.0f, 0.0f,
			0.0f, 0.0f, 1.0f, 0.0f,
			0.0f, 0.0f, 0.0f, 1.0f );

		public override string ToString() {
			return string.Format( "[{0:0.##} {1:0.##} {2:0.##} {3:0.##}; {4:0.##} {5:0.##} {6:0.##} {7:0.##}; {8:0.##} {9:0.##} {10:0.##} {11:0.##}; {12:0.##} {13:0.##} {14:0.##} {15:0.##}]",
				M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 );
		}
		
		/// <summary>
		/// Gets or sets the value of a matrix element at the given position.
		/// </summary>
		/// <param name="row">Zero-based row of the element.</param>
		/// <param name="column">Zero-based column of the element.</param>
		/// <value></value>
		private float this[int row, int column] {
			get {
				switch( row ) {
					case 0:
						switch( column ) {
							case 0:		return M11;
							case 1:		return M12;
							case 2: 	return M13;
							case 3: 	return M14;
							default:
								throw new ArgumentOutOfRangeException( "column" );
						}
					
					case 1:
						switch( column ) {
							case 0: 	return M21;
							case 1: 	return M22;
							case 2: 	return M23;
							case 3: 	return M24;
							default:
								throw new ArgumentOutOfRangeException( "column" );
						}

					case 2:
						switch( column ) {
							case 0: 	return M31;
							case 1: 	return M32;
							case 2: 	return M33;
							case 3: 	return M34;
							default:
								throw new ArgumentOutOfRangeException( "column" );
						}

					case 3:
						switch( column ) {
							case 0: 	return M41;
							case 1: 	return M42;
							case 2: 	return M43;
							case 3: 	return M44;
							default:
								throw new ArgumentOutOfRangeException( "column" );
						}
					

					default:
						throw new ArgumentOutOfRangeException( "row" );
				}
			}
			set {
				switch( row ) {
					case 0:
						switch( column ) {
							case 0: 	M11 = value; return;
							case 1:		M12 = value; return;
							case 2: 	M13 = value; return;
							case 3: 	M14 = value; return;
							default:
								throw new ArgumentOutOfRangeException( "column" );
						}

					case 1:
						switch( column ) {
							case 0: 	M21 = value; return;
							case 1: 	M22 = value; return;
							case 2: 	M23 = value; return;
							case 3: 	M24 = value; return;
							default:
								throw new ArgumentOutOfRangeException( "column" );
						}

					case 2:
						switch( column ) {
							case 0: 	M31 = value; return;
							case 1: 	M32 = value; return;
							case 2: 	M33 = value; return;
							case 3: 	M34 = value; return;
							default:
								throw new ArgumentOutOfRangeException( "column" );
						}

					case 3:
						switch( column ) {
							case 0: 	M41 = value; return;
							case 1: 	M42 = value; return;
							case 2: 	M43 = value; return;
							case 3: 	M44 = value; return;
							default:
								throw new ArgumentOutOfRangeException( "column" );
						}


					default:
						throw new ArgumentOutOfRangeException( "row" );
				}
			}
		}
		
		public Matrix4f Transpose() {
			return new Matrix4f(
				M44, M34, M24, M14,
				M43, M33, M23, M13,
				M42, M32, M22, M12,
				M41, M31, M21, M11 );
		}
		
		public Matrix4f Invert() {
			// BJC: This code is borrowed from the OGRE engine, licensed under the LGPL.
			//		http://www.ogre3d.org/
			
			float v0 = M31 * M42 - M32 * M41;
			float v1 = M31 * M43 - M33 * M41;
			float v2 = M31 * M44 - M34 * M41;
			float v3 = M32 * M43 - M33 * M42;
			float v4 = M32 * M44 - M34 * M42;
			float v5 = M33 * M44 - M34 * M43;

			float t00 = +(v5 * M22 - v4 * M23 + v3 * M24);
			float t10 = -(v5 * M21 - v2 * M23 + v1 * M24);
			float t20 = +(v4 * M21 - v2 * M22 + v0 * M24);
			float t30 = -(v3 * M21 - v1 * M22 + v0 * M23);

			float invDet = 1 / (t00 * M11 + t10 * M12 + t20 * M13 + t30 * M14);

			float d11 = t00 * invDet;
			float d21 = t10 * invDet;
			float d31 = t20 * invDet;
			float d41 = t30 * invDet;

			float d12 = -(v5 * M12 - v4 * M13 + v3 * M14) * invDet;
			float d22 = +(v5 * M11 - v2 * M13 + v1 * M14) * invDet;
			float d32 = -(v4 * M11 - v2 * M12 + v0 * M14) * invDet;
			float d42 = +(v3 * M11 - v1 * M12 + v0 * M13) * invDet;

			v0 = M21 * M42 - M22 * M41;
			v1 = M21 * M43 - M23 * M41;
			v2 = M21 * M44 - M24 * M41;
			v3 = M22 * M43 - M23 * M42;
			v4 = M22 * M44 - M24 * M42;
			v5 = M23 * M44 - M24 * M43;

			float d13 = +(v5 * M12 - v4 * M13 + v3 * M14) * invDet;
			float d23 = -(v5 * M11 - v2 * M13 + v1 * M14) * invDet;
			float d33 = +(v4 * M11 - v2 * M12 + v0 * M14) * invDet;
			float d43 = -(v3 * M11 - v1 * M12 + v0 * M13) * invDet;

			v0 = M32 * M21 - M31 * M22;
			v1 = M33 * M21 - M31 * M23;
			v2 = M34 * M21 - M31 * M24;
			v3 = M33 * M22 - M32 * M23;
			v4 = M34 * M22 - M32 * M24;
			v5 = M34 * M23 - M33 * M24;

			float d14 = -(v5 * M12 - v4 * M13 + v3 * M14) * invDet;
			float d24 = +(v5 * M11 - v2 * M13 + v1 * M14) * invDet;
			float d34 = -(v4 * M11 - v2 * M12 + v0 * M14) * invDet;
			float d44 = +(v3 * M11 - v1 * M12 + v0 * M13) * invDet;

			return new Matrix4f(
				d11, d12, d13, d14,
				d21, d22, d23, d24,
				d31, d32, d33, d34,
				d41, d42, d43, d44 );
		}
	}
}