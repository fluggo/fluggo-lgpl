using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo.Graphics
{
	/// <summary>
	/// Represents a triangle consisting of three indexes.
	/// </summary>
	public struct Triangle : IEquatable<Triangle> {
		/// <summary>
		/// Initializes a <see cref='Triangle'/> value.
		/// </summary>
		/// <param name="v1">Index of the first vertex.</param>
		/// <param name="v2">Index of the second vertex.</param>
		/// <param name="v3">Index of the third vertex.</param>
		public Triangle( int v1, int v2, int v3 ) {
			V1 = v1;
			V2 = v2;
			V3 = v3;
		}
		
		/// <summary>
		/// Index of the first vertex.
		/// </summary>
		public int V1;
		
		/// <summary>
		/// Index of the second vertex.
		/// </summary>
		public int V2;

		/// <summary>
		/// Index of the third vertex.
		/// </summary>
		public int V3;

		public override int GetHashCode() {
			return V1.GetHashCode() ^ V2.GetHashCode() ^ V3.GetHashCode();
		}
		
		public override bool Equals( object obj ) {
			if( obj == null || !(obj is Triangle) )
				return false;
				
			return Equals( (Triangle) obj );
		}

		public bool Equals( Triangle other ) {
			return this == other;
		}
		
		public static bool operator ==( Triangle t1, Triangle t2 ) {
			return t1.V1 == t2.V1 && t1.V2 == t2.V2 && t1.V3 == t2.V3;
		}
		
		public static bool operator !=( Triangle t1, Triangle t2 ) {
			return !(t1 == t2);
		}
	}
}
