using System;
namespace Fluggo.Graphics {
	/// <summary>
	/// Describes an element of a vertex.
	/// </summary>
	public class VertexElement : IEquatable<VertexElement> {
		VertexUsage _usage;
		VertexElementType _type;
		int _offset, _usageIndex;

		public VertexElement( int offset, VertexElementType elementType, VertexUsage usage, int usageIndex ) {
			_offset = offset;
			_type = elementType;
			_usage = usage;
			_usageIndex = usageIndex;
		}

		public static int GetElementSize( VertexElementType elementType ) {
			switch( elementType ) {
				case VertexElementType.Float:
					return 4;
				case VertexElementType.Float2:
					return 4 * 2;
				case VertexElementType.Float3:
					return 4 * 3;
				case VertexElementType.Float4:
					return 4 * 4;
				case VertexElementType.Byte:
					return 1;
				case VertexElementType.Byte2:
					return 2;
				case VertexElementType.Byte3:
					return 3;
				case VertexElementType.Byte4:
					return 4;
				default:
					throw new ArgumentOutOfRangeException( "elementType" );
			}
		}

		public int Offset { get { return _offset; } }
		public VertexElementType ElementType { get { return _type; } }
		public VertexUsage Usage { get { return _usage; } }
		public int UsageIndex { get { return _usageIndex; } }
		
		public override bool Equals( object other ) {
			return Equals( other as VertexElement );
		}

		public override int GetHashCode() {
			return _offset.GetHashCode() ^ _type.GetHashCode() ^ _usage.GetHashCode() ^ _usageIndex.GetHashCode();
		}
		
		public bool Equals( VertexElement other ) {
			if( other == null )
				return false;
				
			if( ElementType != other.ElementType ||
				Offset != other.Offset ||
				Usage != other.Usage ||
				UsageIndex != other.UsageIndex )
				return false;
				
			return true;
		}
		
		public static bool operator ==( VertexElement v1, VertexElement v2 ) {
			if( (object) v1 == (object) v2 )
				return true;
				
			if( (object) v1 == null )
				return false;
			
			return v1.Equals( v2 );
		}
		
		public static bool operator !=( VertexElement v1, VertexElement v2 ) {
			return !(v1 == v2);
		}
	}
}