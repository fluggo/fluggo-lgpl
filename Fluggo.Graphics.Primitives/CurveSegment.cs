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
	/// Represents a segment of a cubic parametric curve in t on the interval [0,1].
	/// </summary>
	public class CurveSegment {
		// Based on section 11.2.1 of Computer Graphics: Principles and Practice, 2nd edition. The blend matrix
		// contains the coefficients of the parametric curve in four dimensions, of which normally only the first
		// three are used (being as the book defines this matrix as 4x3).
		Matrix4f _blend;
		
		static readonly Matrix4f __hermiteBasis = new Matrix4f(
			2.0f, -2.0f, 1.0f, 1.0f,
			-3.0f, 3.0f, -2.0f, -1.0f,
			0.0f, 0.0f, 1.0f, 0.0f,
			1.0f, 0.0f, 0.0f, 0.0f );

		static readonly Matrix4f __bezierBasis = new Matrix4f(
			-1.0f, 3.0f, -3.0f, 1.0f,
			3.0f, -6.0f, 3.0f, 0.0f,
			-3.0f, 3.0f, 0.0f, 0.0f,
			1.0f, 0.0f, 0.0f, 0.0f );
			
		static readonly Matrix4f __uniformBSplineBasis = (1.0f / 6.0f) * new Matrix4f(
			-1.0f, 3.0f, -3.0f, 1.0f,
			3.0f, -6.0f, 3.0f, 0.0f,
			-3.0f, 0.0f, 3.0f, 0.0f,
			1.0f, 4.0f, 1.0f, 0.0f );
		
		private CurveSegment( Matrix4f blendMatrix ) {
			_blend = blendMatrix;
		}
		
		Vector4f GetTVector( float t ) {
			float t2 = t * t;
			float t3 = t2 * t;
			
			return new Vector4f( t3, t2, t, 1.0f );
		}
		
		/// <summary>
		/// Evaluates the curve segment for the given value of t.
		/// </summary>
		/// <param name="t">Value of the parameter to the curve, from zero to one inclusive.</param>
		/// <returns>A <see cref="Vector3f"/> describing the value of the curve at the given point.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="t"/> is less than zero or greater than one.</exception>
		public Vector3f Evaluate( float t ) {
			if( t < 0.0f || t > 1.0f )
				throw new ArgumentOutOfRangeException( "t" );

			return (Vector3f)(GetTVector( t ) * _blend);
		}
		
		/// <summary>
		/// Creates a curve segment from two endpoints and two tangent vectors.
		/// </summary>
		/// <param name="point1">Endpoint of the curve at which t is zero.</param>
		/// <param name="tangent1">Tangent vector to the curve at <paramref name="point1"/>. This is the first derivative of the curve at t = 0.</param>
		/// <param name="point2">Endpoint of the curve at which t is one.</param>
		/// <param name="tangent2">Tangent vector to the curve at <paramref name="point2"/>. This is the first derivative of the curve at t = 1.</param>
		/// <returns>A <see cref="CurveSegment"/> instance representing the requested curve.</returns>
		public static CurveSegment FromHermite( Vector3f point1, Vector3f tangent1, Vector3f point2, Vector3f tangent2 ) {
			return new CurveSegment(
				__hermiteBasis * new Matrix4f(
					(Vector4f) point1, (Vector4f) point2, (Vector4f) tangent1, (Vector4f) tangent2 ) );
		}
		
		/// <summary>
		/// Creates a curve segment from four Bézier control points.
		/// </summary>
		/// <param name="point1">First control point, which is the endpoint of the curve at t = 0.</param>
		/// <param name="point2">Second control point, which the curve approximates.</param>
		/// <param name="point3">Third control point, which the curve approximates.</param>
		/// <param name="point4">Fourth control point, which is the endpoint of the curve at t = 1.</param>
		/// <returns>A <see cref="CurveSegment"/> instance representing the requested curve.</returns>
		public static CurveSegment FromBezier( Vector3f point1, Vector3f point2, Vector3f point3, Vector3f point4 ) {
			return new CurveSegment(
				__bezierBasis * new Matrix4f(
					(Vector4f) point1, (Vector4f) point2, (Vector4f) point3, (Vector4f) point4 ) );
		}

		/// <summary>
		/// Creates a curve segment from four uniform B-spline control points.
		/// </summary>
		/// <param name="point1">First control point.</param>
		/// <param name="point2">Second control point.</param>
		/// <param name="point3">Third control point.</param>
		/// <param name="point4">Fourth control point.</param>
		/// <returns>A <see cref="CurveSegment"/> instance representing the requested curve.</returns>
		/// <remarks>A continuous B-spline can be represented as a sequence of parametric curve segments, each one given four of the
		///   control points. The first segment would be constructed from points zero, one, two, and three; the second segment
		///   would be constructed from points one, two, three, and four; and so on, with the parameter t for each curve segment
		///   mapped to [0,1]. The endpoints of each curve segment created this way would match, producing a single curve.</remarks>
		public static CurveSegment FromUniformBSpline( Vector3f point1, Vector3f point2, Vector3f point3, Vector3f point4 ) {
			return new CurveSegment(
				__uniformBSplineBasis * new Matrix4f(
					(Vector4f) point1, (Vector4f) point2, (Vector4f) point3, (Vector4f) point4 ) );
		}
	}
}
