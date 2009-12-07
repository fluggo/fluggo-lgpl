using System;
using System.Collections.ObjectModel;

namespace Fluggo.Graphics {
	/// <summary>
	/// Represents the layout of a vertex.
	/// </summary>
	public class VertexDeclaration : IEquatable<VertexDeclaration> {
		VertexElement[] _elements;
		int _stride;
		
		/// <summary>
		/// Creates a new instance of the <see cref='VertexDeclaration'/> class.
		/// </summary>
		/// <param name="elements"><see cref="VertexElement"/> values that describe the elements of the vertex.</param>
		/// <exception cref='ArgumentNullException'><paramref name='elements'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="elements"/> contains a value that is <see langword='null'/>.</exception>
		public VertexDeclaration( int stride, params VertexElement[] elements ) {
			if( elements == null )
				throw new ArgumentNullException( "elements" );
			
			if( GetMinimumStride( elements ) > stride )
				throw new ArgumentException( "The stride is too small for the given vertex elements." );
			
			_elements = elements;
			_stride = stride;
		}
		
		public static int GetMinimumStride( VertexElement[] elements ) {
			if( elements == null )
				throw new ArgumentNullException( "elements" );
				
			//if( pack <= 0 )
			//    throw new ArgumentOutOfRangeException( "pack" );
			
			int minStride = 0;

			// Find the element that extends the furthest out
			foreach( VertexElement element in elements ) {
				if( element == null )
					throw new ArgumentException( "One of the given VertexElement values was null.", "elements" );

				int maxLength = element.Offset + VertexElement.GetElementSize( element.ElementType );

				if( maxLength > minStride )
					minStride = maxLength;
			}
			
			// Round up to the pack
/*			if( pack != null ) {
				int missedPadding = minStride % pack.Value;
				
				if( missedPadding != 0 )
					minStride += pack.Value - missedPadding;
			}*/
			
			return minStride;
		}
		
		/// <summary>
		/// Gets a read-only collection of <see cref="VertexElements"/> that make up this declaration.
		/// </summary>
		/// <value>A read-only collection of <see cref="VertexElements"/> that make up this declaration.</value>
		public ReadOnlyCollection<VertexElement> Elements
			{ get { return Array.AsReadOnly<VertexElement>( _elements ); } }
		
		public int Stride
			{ get { return _stride; } }
			
		/// <summary>
		/// Finds a <see cref="VertexElement"/> of type <see cref="VertexElementType.Float3"/> with the usage <see cref="VertexUsage.Position"/> and a usage index of zero.
		/// </summary>
		/// <returns>The <see cref="VertexElement"/>, if found in this declaration, or <see langword='null'/> if one is not found.</returns>
		public VertexElement GetPositionElement() {
			return FindElement( VertexElementType.Float3, VertexUsage.Position, 0 );
		}

		/// <summary>
		/// Finds a <see cref="VertexElement"/> of type <see cref="VertexElementType.Float3"/> with the usage <see cref="VertexUsage.Normal"/> and a usage index of zero.
		/// </summary>
		/// <returns>The <see cref="VertexElement"/>, if found in this declaration, or <see langword='null'/> if one is not found.</returns>
		public VertexElement GetNormalElement() {
			return FindElement( VertexElementType.Float3, VertexUsage.Normal, 0 );
		}
		
		public VertexElement FindElement( VertexElementType elementType, VertexUsage usage, int usageIndex ) {
			foreach( VertexElement element in _elements ) {
				if( element.ElementType == elementType && element.Usage == usage && element.UsageIndex == usageIndex )
					return element;
			}
			
			return null;
		}
		
		/// <summary>
		/// Copies the given vertices into a new vertex buffer with a new declaration, mapping elements to new offsets and strides.
		/// </summary>
		/// <param name="sourceBuffer">Source vertex buffer.</param>
		/// <param name="sourceOffset">Offset in <paramref name="sourceBuffer"/> where the source vertices begin.</param>
		/// <param name="targetBuffer">Target vertex buffer.</param>
		/// <param name="targetOffset">Offset in <paramref name="targetBuffer"/> where the new vertices should begin.</param>
		/// <param name="vertexCount">Number of vertices to copy.</param>
		/// <param name="targetDeclaration">Declaration for the new vertex buffer.</param>
		public void MapVerticesToNewDeclaration( byte[] sourceBuffer, int sourceOffset, byte[] targetBuffer, int targetOffset, int vertexCount, VertexDeclaration targetDeclaration ) {
			if( targetDeclaration == null )
				throw new ArgumentNullException( "targetDeclaration" );
			
			new ArraySegment<byte>( sourceBuffer, sourceOffset, vertexCount * _stride );
			new ArraySegment<byte>( targetBuffer, targetOffset, vertexCount * targetDeclaration.Stride );
			
			foreach( VertexElement oldElement in _elements ) {
				foreach( VertexElement newElement in targetDeclaration.Elements ) {
					if( oldElement.Usage != newElement.Usage || oldElement.UsageIndex != newElement.UsageIndex || oldElement.ElementType != newElement.ElementType )
						continue;
						
					int elementSize = VertexElement.GetElementSize( oldElement.ElementType );
					
					for( int i = 0; i < vertexCount; i++ ) {
						for( int j = 0; j < elementSize; j++ ) {
							targetBuffer[targetOffset + targetDeclaration.Stride * i + newElement.Offset + j] =
								sourceBuffer[sourceOffset + _stride * i + oldElement.Offset + j];
						}
					}
				}
			}
		}
		
		public byte[] MapVerticesToNewDeclaration( byte[] sourceBuffer, int sourceOffset, int vertexCount, VertexDeclaration targetDeclaration ) {
			byte[] result = new byte[vertexCount * targetDeclaration.Stride];
			MapVerticesToNewDeclaration( sourceBuffer, sourceOffset, result, 0, vertexCount, targetDeclaration );
			
			return result;
		}

		public override bool Equals( object obj ) {
			return base.Equals( obj as VertexDeclaration );
		}
		
		public bool Equals( VertexDeclaration other ) {
			if( other == null )
				return false;
			
			if( _stride != other._stride || _elements.Length != other._elements.Length )
				return false;
				
			for( int i = 0; i < _elements.Length; i++ ) {
				bool match = false;
				
				for( int j = 0; j < _elements.Length; j++ ) {
					if( _elements[i] == other._elements[j] ) {
						match = true;
						break;
					}
				}
				
				if( !match )
					return false;
			}
			
			return true;
		}

		public override int GetHashCode() {
			int hashCode = _stride.GetHashCode();
			
			for( int i = 0; i < _elements.Length; i++ )
				hashCode ^= _elements[i].GetHashCode();
				
			return hashCode;
		}
		
		public static bool operator ==( VertexDeclaration v1, VertexDeclaration v2 ) {
			if( (object) v1 == (object) v2 )
				return true;

			if( (object) v1 == null )
				return false;

			return v1.Equals( v2 );
		}
		
		public static bool operator !=( VertexDeclaration v1, VertexDeclaration v2 ) {
			return !(v1 == v2);
		}
	}
}