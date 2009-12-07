using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Fluggo.Graphics {

	public class EditableTriangleMesh {
		/// <summary>
		/// Creates a new instance of the <see cref='EditableTriangleMesh'/> class.
		/// </summary>
		public EditableTriangleMesh() {
			_positionList = new List<Vector3f>();
			_triangleList = new List<Triangle>();
			_attributeTable = new Dictionary<VertexElementKey,IVertexAttribute>();
			_attributeToPositionList = new List<int>();
		}
		
		public EditableTriangleMesh( Mesh mesh, bool removeDuplicatePositions ) {
			if( removeDuplicatePositions )
				throw new NotImplementedException();

			if( mesh == null )
				throw new ArgumentNullException( "mesh" );
				
			if( mesh.GeometryType != GeometryType.TriangleList )
				throw new NotSupportedException( "Only triangle lists are currently supported for this operation." );

			VertexElement positionElement = mesh.VertexDeclaration.GetPositionElement();
			
			if( positionElement == null )
				throw new ArgumentException( "The given mesh does not have a usable position element." );
				
			int minIndex, maxIndex;
			mesh.GetMinMaxIndices( out minIndex, out maxIndex );
			int count = maxIndex + 1;
			
#if OPTIMIZE_FOR_MEMORY
			_positionList = new List<Vector3f>( new SubStrideBuffer<Vector3f>(
				mesh.VertexBuffer, positionElement.Offset, count, mesh.VertexDeclaration.Stride ) );

			_attributeToPositionList = new List<int>( count );
			
			for( int i = 0; i < count; i++ )
				_attributeToPositionList.Add( i );
				
			// Note that each index here is validated because we called GetMinMaxIndices
			_triangleList = new List<Triangle>( mesh.GeometryCount );
			
			if( mesh.Indices == null ) {
				// Straight run of the indices
				for( int i = 0; i < mesh.GeometryCount; i++ )
					_triangleList.Add( new Triangle( i * 3, i * 3 + 1, i * 3 + 2 ) );
			}
			else {
				// Copy the indices out
				for( int i = 0; i < mesh.GeometryCount; i++ )
					_triangleList.Add( new Triangle( mesh.Indices[i * 3], mesh.Indices[i * 3 + 1], mesh.Indices[i * 3 + 2] );
			}
#else
			Vector3f[] positions = new Vector3f[count];
			StrideBuffer.GetData<Vector3f>( mesh.VertexBuffer, positionElement.Offset, positions, 0, count, mesh.VertexDeclaration.Stride );
			_positionList = new List<Vector3f>( positions );
			
			int[] attributeMaps = new int[count];
			
			for( int i = 0; i < attributeMaps.Length; i++ )
				attributeMaps[i] = i;
				
			_attributeToPositionList = new List<int>( attributeMaps );

			// Note that each index here is validated because we called GetMinMaxIndices
			Triangle[] triangles = new Triangle[mesh.GeometryCount];

			if( mesh.Indices == null ) {
				// Straight run of the indices
				for( int i = 0; i < triangles.Length; i++ )
					triangles[i] = new Triangle( i * 3, i * 3 + 1, i * 3 + 2 );
			}
			else {
				// Copy the indices out
				for( int i = 0; i < triangles.Length; i++ )
					triangles[i] = new Triangle( mesh.Indices[i * 3], mesh.Indices[i * 3 + 1], mesh.Indices[i * 3 + 2] );
			}

			_triangleList = new List<Triangle>( triangles );
#endif

			_attributeTable = new Dictionary<VertexElementKey,IVertexAttribute>( mesh.VertexDeclaration.Elements.Count - 1 );
			
			foreach( VertexElement element in mesh.VertexDeclaration.Elements ) {
				if( element.Usage == VertexUsage.Position && element.UsageIndex == 0 )
					continue;
					
				_attributeTable.Add( new VertexElementKey( element.Usage, element.UsageIndex ),
					CreateVertexAttribute( element, mesh.VertexBuffer, count, mesh.VertexDeclaration.Stride ) );
			}
		}
		
		struct Edge {
			public int P1, P2;
		}
		
	#region VertexElementKey
		struct VertexElementKey : IEquatable<VertexElementKey> {
			public VertexElementKey( VertexUsage usage, int usageIndex ) {
				_usage = usage;
				_usageIndex = usageIndex;
			}
			
			VertexUsage _usage;
			int _usageIndex;
			
			public VertexUsage Usage { get { return _usage; } }
			public int UsageIndex { get { return _usageIndex; } }
		
			public bool Equals( VertexElementKey other ) {
 				return _usage == other._usage && _usageIndex == other._usageIndex;
			}

			public override bool Equals( object obj ) {
				if( obj == null )
					return false;
					
				return Equals( (VertexElementKey) obj );
			}

			public override int GetHashCode() {
				return _usageIndex.GetHashCode() ^ _usage.GetHashCode();
			}
			
			public static bool operator ==( VertexElementKey key1, VertexElementKey key2 ) {
				return key1.Equals( key2 );
			}
			
			public static bool operator !=( VertexElementKey key1, VertexElementKey key2 ) {
				return !key1.Equals( key2 );
			}
		}
	#endregion
		
		interface IVertexAttribute : System.Collections.IList {
			/// <summary>
			/// Adds an empty value to the end of the list.
			/// </summary>
			void AddEmptyValue();
			Type AttributeType { get; }
			void CopyTo( byte[] buffer, int offset, int stride );
		}
		
		private IVertexAttribute CreateVertexAttribute( VertexElement element, byte[] buffer, int count, int stride ) {
			if( element == null )
				throw new ArgumentNullException( "element" );

			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			switch( element.ElementType ) {
				case VertexElementType.Float:
					return new VertexAttribute<float>( buffer, element.Offset, count, stride );
				case VertexElementType.Float2:
					return new VertexAttribute<Vector2f>( buffer, element.Offset, count, stride );
				case VertexElementType.Float3:
					if( element.Usage == VertexUsage.Color )
						return new VertexAttribute<Color3f>( buffer, element.Offset, count, stride );
						
					return new VertexAttribute<Vector3f>( buffer, element.Offset, count, stride );
				case VertexElementType.Float4:
					if( element.Usage == VertexUsage.Color )
						return new VertexAttribute<Color4f>( buffer, element.Offset, count, stride );

					return new VertexAttribute<Vector4f>( buffer, element.Offset, count, stride );
				case VertexElementType.Byte:
					return new VertexAttribute<byte>( buffer, element.Offset, count, stride );
				case VertexElementType.Byte2:
				case VertexElementType.Byte3:
					throw new NotSupportedException( "No native type exists for the given element type." );
				case VertexElementType.Byte4:
					if( element.Usage == VertexUsage.Color )
						return new VertexAttribute<ColorARGB>( buffer, element.Offset, count, stride );

					throw new NotSupportedException( "No native type exists for the given element type." );
				default:
					throw new ArgumentException( "An invalid element type was given." );
			}
		}
		
		class VertexAttribute<T> : List<T>, IVertexAttribute where T : struct {
			public VertexAttribute() {
			}
			
			public VertexAttribute( int count ) : base( count ) {
			}
			
			public VertexAttribute( IEnumerable<T> collection ) : base( collection ) {
			}
			
			public VertexAttribute( byte[] buffer, int offset, int count, int stride ) : base( From( buffer, offset, count, stride ) ) {
			}
			
			private static IEnumerable<T> From( byte[] buffer, int offset, int count, int stride ) {
				if( buffer == null )
					throw new ArgumentNullException( "buffer" );

#if OPTIMIZE_FOR_MEMORY
				// Uses less memory, but may be slower
				return new SubStrideBuffer<T>( buffer, offset, count, stride );
#else
				// Uses more memory, but may be faster
				T[] values = new T[count];
				StrideBuffer.GetData<T>( buffer, offset, values, 0, count, stride );
				return values;
#endif
			}

			public void CopyTo( byte[] buffer, int offset, int stride ) {
#if OPTIMIZE_FOR_MEMORY
				// Uses less memory, but may be slower
				SubStrideBuffer<T> subBuffer = new SubStrideBuffer<T>( buffer, offset, Count, stride );
				
				for( int i = 0; i < Count; i++ )
					subBuffer[i] = this[i];
#else
				// Uses more memory, but may be faster
				StrideBuffer.SetData<T>( buffer, offset, ToArray(), 0, Count, stride );
#endif
			}
			
			public void AddEmptyValue() {
				Add( default(T) );
			}
			
			public Type AttributeType
				{ get { return typeof(T); } }
		}
		
		List<Vector3f> _positionList;
		List<int> _attributeToPositionList;
		
		Dictionary<VertexElementKey, IVertexAttribute> _attributeTable;
		
		// These triangles are indexes into the attribute lists; one can produce the position
		// list by looking every attribute up in the _attributeToPositionList
		List<Triangle> _triangleList;
		
		public IList<Vector3f> Positions {
			get {
				return new FixedLengthList<Vector3f>( _positionList );
			}
		}
		
		public IList<Triangle> PositionTriangles {
			get {
				return new PositionTriangleList( _triangleList, _attributeToPositionList );
			}
		}
		
		#region PositionTriangleList
		class PositionTriangleList : IList<Triangle> {
			List<Triangle> _tris;
			List<int> _attToPos;
			
			public PositionTriangleList( List<Triangle> tris, List<int> attToPos ) {
				if( tris == null )
					throw new ArgumentNullException( "tris" );
					
				if( attToPos == null )
					throw new ArgumentNullException( "attToPos" );

				_tris = tris;
				_attToPos = attToPos;
			}
			
			public int IndexOf( Triangle item ) {
				for( int i = 0; i < Count; i++ ) {
					if( this[i] == item )
						return i;
				}
				
				return -1;
			}

			public void Insert( int index, Triangle item ) {
				throw new NotSupportedException();
			}

			public void RemoveAt( int index ) {
				throw new NotSupportedException();
			}

			public Triangle this[int index] {
				get {
					return new Triangle( _attToPos[_tris[index].V1], _attToPos[_tris[index].V2], _attToPos[_tris[index].V3] );
				}
				set {
					throw new NotSupportedException();
				}
			}

			public void Add( Triangle item ) {
				throw new NotSupportedException();
			}

			public void Clear() {
				throw new NotSupportedException();
			}

			public bool Contains( Triangle item ) {
				return IndexOf( item ) != -1;
			}

			public void CopyTo( Triangle[] array, int arrayIndex ) {
				new ArraySegment<Triangle>( array, arrayIndex, Count );
				
				for( int i = 0; i < Count; i++ )
					array[arrayIndex + i] = this[i];
			}

			public int Count {
				get { return _tris.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public bool Remove( Triangle item ) {
				throw new NotSupportedException();
			}

			public IEnumerator<Triangle> GetEnumerator() {
				for( int i = 0; i < Count; i++ )
					yield return this[i];
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}
		}
		#endregion
		
		public IList<Triangle> AttributeTriangles {
			get {
				return new ReadOnlyCollection<Triangle>( _triangleList );
			}
		}
		
		public int AddPosition( Vector3f position ) {
			_positionList.Add( position );
			return _positionList.Count - 1;
		}
		
		public void AddTriangle( int v1, int v2, int v3 ) {
			AddTriangle( new Triangle( v1, v2, v3 ) );
		}
		
		public void AddTriangle( Triangle triangle ) {
			if( triangle.V1 < 0 || triangle.V1 >= _attributeToPositionList.Count ||
				triangle.V2 < 0 || triangle.V2 >= _attributeToPositionList.Count ||
				triangle.V3 < 0 || triangle.V3 >= _attributeToPositionList.Count )
				throw new ArgumentException( "One of the triangle indices referred to an invalid index." );
			
			_triangleList.Add( triangle );
		}
		
		public IList<T> AddAttribute<T>( VertexUsage usage, int usageIndex ) where T : struct {
			if( usageIndex < 0 )
				throw new ArgumentOutOfRangeException( "usageIndex" );
			
			switch( usage ) {
				case VertexUsage.Position:
					if( usageIndex == 0 )
						throw new ArgumentException( "This class already stores vertex positions." );
						
					break;
					
				case VertexUsage.Normal:
					if( typeof(T) != typeof(Vector3f) )
						throw new ArgumentException( "Normal attributes must have the type Vector3f." );

					break;
					
				case VertexUsage.Color:
					if( typeof(T) != typeof(Color3f) && typeof(T) != typeof(Color4f) && typeof(T) != typeof(ColorARGB) )
						throw new ArgumentException( "Color attributes must be of type Color3f, Color4f, or ColorARGB." );
				
					break;

				case VertexUsage.TextureCoordinate:
					if( typeof(T) != typeof(float) && typeof(T) != typeof(Vector2f) && typeof(T) != typeof(Vector3f) )
						throw new ArgumentException( "Texture coordinate attributes must be of type Single, Vector2f, or Vector3f." );
					
					break;

				default:
					throw new ArgumentOutOfRangeException( "usage" );
			}

			VertexAttribute<T> attr = new VertexAttribute<T>( _attributeToPositionList.Count );
			_attributeTable.Add( new VertexElementKey( usage, usageIndex ), attr );
			
			return new FixedLengthList<T>( attr );
		}
		
		/// <summary>
		/// Gets an <see cref="IList"/> containing the data for an attribute.
		/// </summary>
		/// <param name="usage"><see cref="VertexUsage"/> describing the usage of the attribute.</param>
		/// <param name="usageIndex">The index of the usage.</param>
		/// <returns>An <see cref="IList"/> containing the data for the requested attribute.</returns>
		/// <exception cref="KeyNotFoundException">An attribute with the given <paramref name="usage"/> and <paramref name="usageIndex"/> was not found in this mesh.</exception>
		public System.Collections.IList GetAttributeList( VertexUsage usage, int usageIndex ) {
			return System.Collections.ArrayList.FixedSize( _attributeTable[new VertexElementKey( usage, usageIndex )] );
		}
		
		/// <summary>
		/// Gets an <see cref="IList{T}"/> containing the data for an attribute.
		/// </summary>
		/// <typeparam name="T">Type of the data in the attribute. This must match the attribute's underlying type.</typeparam>
		/// <param name="usage"><see cref="VertexUsage"/> describing the usage of the attribute.</param>
		/// <param name="usageIndex">The index of the usage.</param>
		/// <returns>An <see cref="IList"/> containing the data for the requested attribute.</returns>
		/// <exception cref="KeyNotFoundException">An attribute with the given <paramref name="usage"/> and <paramref name="usageIndex"/> was not found in this mesh.</exception>
		/// <exception cref="ArgumentException">The attribute does not contain values of type <typeparamref name="T"/>.</exception>
		public IList<T> GetAttributeList<T>( VertexUsage usage, int usageIndex ) where T : struct {
			System.Collections.IList list = _attributeTable[new VertexElementKey( usage, usageIndex )];
			IList<T> result = list as IList<T>;
			
			if( result == null )
				throw new ArgumentException( "Attribute is not of type " + typeof(T).Name + "." );
				
			return new FixedLengthList<T>( result );
		}
		
		public bool ContainsAttribute( VertexUsage usage, int usageIndex ) {
			return _attributeTable.ContainsKey( new VertexElementKey( usage, usageIndex ) );
		}
		
		public bool RemoveAttribute( VertexUsage usage, int usageIndex ) {
			return _attributeTable.Remove( new VertexElementKey( usage, usageIndex ) );
		}
		
		public int AddAttributeToPosition( int positionIndex ) {
			if( positionIndex < 0 || positionIndex >= _positionList.Count )
				throw new ArgumentOutOfRangeException( "positionIndex" );
			
			_attributeToPositionList.Add( positionIndex );
			
			foreach( IVertexAttribute attribute in _attributeTable.Values )
				attribute.AddEmptyValue();
		
			return _attributeToPositionList.Count - 1;
		}
		
		/// <summary>
		/// Converts this editable mesh to a <see cref="Mesh"/>.
		/// </summary>
		/// <returns>A <see cref="Mesh"/> containing the data of this editable mesh.</returns>
		/// <remarks>The returned mesh may not have all of the topology or custom attribute data of this editable mesh.</remarks>
		public Mesh ToMesh() {
			// Construct a vertex declaration for the mesh
			List<VertexElement> elements = new List<VertexElement>();
			int stride = 0;
			
			elements.Add( new VertexElement( 0, VertexElementType.Float3, VertexUsage.Position, 0 ) );
			stride += 12;
			
			foreach( KeyValuePair<VertexElementKey, IVertexAttribute> pair in _attributeTable ) {
				VertexElement element = new VertexElement( stride, ToVertexElementType( pair.Value.AttributeType ), pair.Key.Usage, pair.Key.UsageIndex );
				stride += VertexElement.GetElementSize( element.ElementType );
				elements.Add( element );
			}
			
			VertexDeclaration decl = new VertexDeclaration( stride, elements.ToArray() );
			byte[] vertexBuffer = new byte[stride * _attributeToPositionList.Count];
			
			// Copy positions into where they meet attributes
#if OPTIMIZE_FOR_MEMORY
			SubStrideBuffer<Vector3f> positionSubBuffer = new SubStrideBuffer<Vector3f>( vertexBuffer, 0, _attributeToPositionList.Count, stride );
			
			for( int i = 0; i < _attributeToPositionList.Count; i++ )
				positionSubBuffer[i] = _positionList[_attributeToPositionList[i]];
#else
			Vector3f[] positions = new Vector3f[_attributeToPositionList.Count];
			
			for( int i = 0; i < _attributeToPositionList.Count; i++ )
				positions[i] = _positionList[_attributeToPositionList[i]];
				
			StrideBuffer.SetData<Vector3f>( vertexBuffer, 0, positions, stride );
#endif
		
			int offset = 12;
			
			foreach( KeyValuePair<VertexElementKey, IVertexAttribute> pair in _attributeTable ) {
				pair.Value.CopyTo( vertexBuffer, offset, stride );
				offset += VertexElement.GetElementSize( ToVertexElementType( pair.Value.AttributeType ) );
			}
			
			// Now construct index buffer
			int[] indices = new int[_triangleList.Count * 3];
			
			for( int i = 0; i < _triangleList.Count; i++ ) {
				indices[i * 3] = _triangleList[i].V1;
				indices[i * 3 + 1] = _triangleList[i].V2;
				indices[i * 3 + 2] = _triangleList[i].V3;
			}
			
			return new Mesh( decl, vertexBuffer, indices );
		}
		
		private VertexElementType ToVertexElementType( Type type ) {
			if( type == typeof(float) )
				return VertexElementType.Float;
			
			if( type == typeof(Vector2f) )
				return VertexElementType.Float2;

			if( type == typeof(Vector3f) || type == typeof(Color3f) )
				return VertexElementType.Float3;

			if( type == typeof(Vector4f) || type == typeof(Color4f) )
				return VertexElementType.Float4;
				
			if( type == typeof(byte) )
				return VertexElementType.Byte;
				
			if( type == typeof(ColorARGB) )
				return VertexElementType.Byte4;
				
			throw new ArgumentOutOfRangeException( "type" );
		}
		
		public static void FlattenNormals( EditableTriangleMesh mesh ) {
			if( mesh == null )
				throw new ArgumentNullException( "mesh" );
				
			if( !mesh.ContainsAttribute( VertexUsage.Normal, 0 ) )
				throw new ArgumentException( "The given mesh does not contain a normal attribute.", "mesh" );

			IList<Vector3f> positions = mesh.Positions;
			IList<Vector3f> normals = mesh.GetAttributeList<Vector3f>( VertexUsage.Normal, 0 );
			IList<Triangle> posTriangles = mesh.PositionTriangles;
			IList<Triangle> attTriangles = mesh.AttributeTriangles;
			
			for( int i = 0; i < posTriangles.Count; i++ ) {
				Triangle posTri = posTriangles[i];
				Triangle attTri = attTriangles[i];
				
				Vector3f normal = -Vector3f.Cross( positions[posTri.V2] - positions[posTri.V1], positions[posTri.V3] - positions[posTri.V1] ).Normalize();

				normals[attTri.V1] = normal;
				normals[attTri.V2] = normal;
				normals[attTri.V3] = normal;
			}
		}
	}
	
	
}
