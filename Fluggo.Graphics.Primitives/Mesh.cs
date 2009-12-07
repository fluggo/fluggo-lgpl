using System;
using System.Collections.Generic;

namespace Fluggo.Graphics {
	public class Mesh {
		byte[] _vbuffer;
		VertexDeclaration _decl;
		GeometryType _geometryType;
		int _geometryCount;
		int[] _indices;
		Material _material;

		public Mesh( VertexDeclaration declaration, byte[] vertexBuffer )
			: this( declaration, vertexBuffer, null ) {
		}
		
		public Mesh( VertexDeclaration declaration, byte[] vertexBuffer, int[] indices )
			: this( declaration, vertexBuffer, indices, new BasicMaterial() ) {
		}
		
		public Mesh( VertexDeclaration declaration, byte[] vertexBuffer, int[] indices, Material material )
			: this( declaration, vertexBuffer, indices, material, GeometryType.TriangleList ) {
		}

		public Mesh( VertexDeclaration declaration, byte[] vertexBuffer, int[] indices, Material material, GeometryType geometryType )
			: this( declaration, vertexBuffer, indices, material, geometryType, GetGeometryCount( geometryType, declaration, vertexBuffer, indices ) ) {
		}

		public Mesh( VertexDeclaration declaration, byte[] vertexBuffer, int[] indices, Material material, GeometryType geometryType, int geometryCount ) {
			if( declaration == null )
				throw new ArgumentNullException( "declaration" );

			if( vertexBuffer == null )
				throw new ArgumentNullException( "vertexBuffer" );

			if( material == null )
				throw new ArgumentNullException( "material" );
			
			_decl = declaration;
			_vbuffer = vertexBuffer;
			_indices = indices;
			_material = material;
			
			if( indices != null ) {
				if( GetGeometryVertexCount( geometryType, geometryCount ) > indices.Length )
					throw new ArgumentException( "Fewer indices were specified than required for the given geometry count." );
					
				// BJC: A case is purposely ignored here, and that's the case where an index references a
				// vertex not in the vertex buffer. We don't want to walk the index buffer in the constructor,
				// so we defer that task until someone calls GetMinMaxIndices, where we have to walk the
				// index buffer anyhow, and the caller has the expectation that it might take awhile.
			}
			else {
				if( GetGeometryVertexCount( geometryType, geometryCount ) > (vertexBuffer.Length / declaration.Stride) )
					throw new ArgumentException( "Fewer vertices were specified than required for the given geometry count." );
			}
			
			_geometryType = geometryType;
			_geometryCount = geometryCount;
		}
		
		public VertexDeclaration VertexDeclaration { get { return _decl; } }

		/// <summary>
		/// Gets the buffer containing vertices for this mesh.
		/// </summary>
		/// <value>The buffer containing vertices for this mesh.</value>
		/// <remarks>This array can be modified in-place, but, in general, library methods should avoid changing this value
		///   and should not assume that successive calls with the same mesh object will contain the same values.</remarks>
		public byte[] VertexBuffer { get { return _vbuffer; } }

		/// <summary>
		/// Gets the array of indices for this mesh, if any.
		/// </summary>
		/// <value>The array of indices for this mesh, if any, or <see langword='null'/> if the mesh has no indices.</value>
		/// <remarks>
		///	    A mesh without indices uses each vertex from its vertex buffer once and in ascending order. This is the same as if
		///     the mesh had indices (0, 1, 2, 3, ..., N - 1), where N is the return value from <see cref="GetGeometryVertexCount"/>.
		///   <para>This array can be modified in-place, but, in general, library methods should avoid changing this value
		///   and should not assume that successive calls with the same mesh object will contain the same values.</para></remarks>
		public int[] Indices { get { return _indices; } }
		public GeometryType GeometryType { get { return _geometryType; } }
		public int GeometryCount { get { return _geometryCount; } }
		public Material Material { get { return _material; } }
		
		private static int GetGeometryCount( GeometryType geometryType, VertexDeclaration declaration, byte[] vertexBuffer, int[] indices ) {
			if( vertexBuffer == null )
				throw new ArgumentNullException( "vertexBuffer" );
				
			if( declaration == null )
				throw new ArgumentNullException( "declaration" );
			
			int elementCount;
			
			if( indices == null )
				elementCount = vertexBuffer.Length / declaration.Stride;
			else
				elementCount = indices.Length;
			
			switch( geometryType ) {
				case GeometryType.PointList:
					return elementCount;

				case GeometryType.LineList:
					return elementCount / 2;

				case GeometryType.LineStrip:
					return elementCount - 1;

				case GeometryType.TriangleList:
					return elementCount / 3;

				case GeometryType.TriangleFan:
				case GeometryType.TriangleStrip:
					return elementCount - 2;

				default:
					throw new ArgumentOutOfRangeException( "geometryType" );
			}
		}
		
		/// <summary>
		/// Gets the number of vertices (or indices) required to specify the given type and number of geometric primitives.
		/// </summary>
		/// <param name="geometryType">A type of geometry.</param>
		/// <param name="geometryCount">The number of primitives.</param>
		/// <returns>The number of vertices or indices required to specify the given number of primitives.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="geometryCount"/> is less than one.
		///	  <para>- or -</para>
		///	  <para><paramref name="geometryType"/> is not a valid geometry type for a mesh.</para></exception>
		public static int GetGeometryVertexCount( GeometryType geometryType, int geometryCount ) {
			if( geometryCount < 1 )
				throw new ArgumentOutOfRangeException( "geometryCount" );
			
			switch( geometryType ) {
				case GeometryType.PointList:
					return geometryCount;

				case GeometryType.LineList:
					return geometryCount * 2;
					
				case GeometryType.LineStrip:
					return geometryCount + 1;
					
				case GeometryType.TriangleList:
					return geometryCount * 3;

				case GeometryType.TriangleFan:
				case GeometryType.TriangleStrip:
					return geometryCount + 2;

				default:
					throw new ArgumentOutOfRangeException( "geometryType" );
			}
		}

		/// <summary>
		/// Determines the minimum and maximum indices found in the index list.
		/// </summary>
		/// <param name="minIndex">Reference to an integer value. On return, this contains the minimum index in the buffer.</param>
		/// <param name="maxIndex">Reference to an integer value. On return, this contains the maximum index in the buffer.</param>
		/// <exception cref="InvalidOperationException">This mesh does not have indices (<see cref="Indices"/> is <see langword='null'/>).
		///   <para>- or -</para>
		///   <para>The index list for this mesh contains one or more indices outside its vertex buffer.</para></exception>
		public void GetMinMaxIndices( out int minIndex, out int maxIndex ) {
			if( _indices == null )
				throw new InvalidOperationException( "This mesh does not have indices." );
				
			int min = int.MaxValue, max = 0;

			for( int i = 0; i < _indices.Length; i++ ) {
				if( _indices[i] > max )
					max = _indices[i];

				if( _indices[i] < min )
					min = _indices[i];
			}

			// Deferred check for valid indices
			if( min < 0 || max >= (_vbuffer.Length / _decl.Stride) )
				throw new InvalidOperationException( "The index list for this mesh contains one or more indices outside its vertex buffer." );
			
			minIndex = min;
			maxIndex = max;
		}

		public float GetBoundingRadius() {
			// Find the position element with usage index = 0
			VertexElement positionElement = null;

			foreach( VertexElement element in _decl.Elements ) {
				if( element.Usage == VertexUsage.Position && element.UsageIndex == 0 ) {
					if( element.ElementType != VertexElementType.Float3 )
						throw new InvalidOperationException( "The mesh must have a position(0) element with the type Float3." );

					positionElement = element;
					break;
				}
			}

			if( positionElement == null )
				throw new InvalidOperationException( "This mesh does not have a position(0) element." );

			// Find the range of vertices to check
			int minIndex, maxIndex;
			GetMinMaxIndices( out minIndex, out maxIndex );

			// Iterate through that range and determine the max length
			Vector3f[] positions = new Vector3f[maxIndex - minIndex];
			StrideBuffer.GetData<Vector3f>( _vbuffer, minIndex * _decl.Stride + positionElement.Offset, positions, 0, positions.Length, _decl.Stride );

			float maxSquaredLength = 0.0f;

			for( int i = 0; i < positions.Length; i++ ) {
				float squaredLength = positions[i].SquaredLength;

				if( squaredLength > maxSquaredLength )
					maxSquaredLength = squaredLength;
			}

			return (float) Math.Sqrt( maxSquaredLength );
		}
		
		/// <summary>
		/// Adds a per-vertex color with the given color.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public static Mesh CreateSolidColorMesh( Mesh mesh, ColorARGB color ) {
			int vertexCount = mesh.VertexBuffer.Length / mesh.VertexDeclaration.Stride;
			ColorARGB[] colorBuffer = new ColorARGB[vertexCount];
			
			for( int i = 0; i < colorBuffer.Length; i++ )
				colorBuffer[i] = color;
			
			VertexElement colorElement = mesh.VertexDeclaration.FindElement( VertexElementType.Byte4, VertexUsage.Color, 0 );
			
			if( colorElement != null ) {
				// Just set the contents of the existing element
				byte[] newVBuf = (byte[]) mesh.VertexBuffer.Clone();
				
				StrideBuffer.SetData<ColorARGB>( newVBuf, colorElement.Offset, colorBuffer, mesh.VertexDeclaration.Stride );
				return new Mesh( mesh.VertexDeclaration, newVBuf, mesh.Indices, mesh.Material, mesh.GeometryType, mesh.GeometryCount );
			}
			else {
				List<VertexElement> elements = new List<VertexElement>( mesh.VertexDeclaration.Elements );
				elements.Add( new VertexElement( mesh.VertexDeclaration.Stride, VertexElementType.Byte4, VertexUsage.Color, 0 ) );
				
				VertexDeclaration newDecl = new VertexDeclaration( mesh.VertexDeclaration.Stride + 4, elements.ToArray() );

				byte[] newVBuf = mesh.VertexDeclaration.MapVerticesToNewDeclaration( mesh.VertexBuffer, 0, vertexCount, newDecl );
				StrideBuffer.SetData<ColorARGB>( newVBuf, mesh.VertexDeclaration.Stride, colorBuffer, newDecl.Stride );

				return new Mesh( newDecl, newVBuf, mesh.Indices, mesh.Material, mesh.GeometryType, mesh.GeometryCount );
			}
		}
		
		/// <summary>
		/// Creates a mesh with a flat appearance from the given mesh.
		/// </summary>
		/// <param name="mesh">Mesh to convert.</param>
		/// <param name="addNormal">True to generate a set of normals if none exists.</param>
		/// <returns>A mesh with a flat appearance.</returns>
		/// <remarks>If the given mesh has normals, the normals are remapped to be perpendicular to their triangles.</remarks>
		public static Mesh CreateFlatMesh( Mesh mesh, bool addNormal ) {
			if( mesh.GeometryType != GeometryType.TriangleList )
				throw new NotImplementedException( "Only triangle-list meshes are supported at this time." );

			VertexElement positionElement = mesh.VertexDeclaration.GetPositionElement();
			VertexElement normalElement = mesh.VertexDeclaration.GetNormalElement();
			
			int targetStride = mesh.VertexDeclaration.Stride;
			VertexDeclaration targetDecl = mesh.VertexDeclaration;
			
			if( normalElement == null && addNormal ) {
				List<VertexElement> elements = new List<VertexElement>( targetDecl.Elements );
				elements.Add( normalElement = new VertexElement( targetStride, VertexElementType.Float3, VertexUsage.Normal, 0 ) );

				targetStride += 12;
				targetDecl = new VertexDeclaration( targetStride, elements.ToArray() );
			}

			// Untangle the vertices
			mesh = UntangleVertices( mesh, targetStride );
				
			// Flatten normals!
			if( positionElement != null && normalElement != null ) {
				int vertexCount = GetGeometryVertexCount( mesh.GeometryType, mesh.GeometryCount );
				
				Vector3f[] positions = new Vector3f[vertexCount];
				Vector3f[] normals = new Vector3f[vertexCount];
				
				StrideBuffer.GetData<Vector3f>( mesh.VertexBuffer, positionElement.Offset, positions, 0, vertexCount, targetStride );
				StrideBuffer.GetData<Vector3f>( mesh.VertexBuffer, normalElement.Offset, normals, 0, vertexCount, targetStride );
				
				for( int i = 0; i < positions.Length; i += 3 ) {
					Vector3f normal = -Vector3f.Cross( positions[i + 1] - positions[i], positions[i + 2] - positions[i] ).Normalize();
					
					normals[i] = normal;
					normals[i + 1] = normal;
					normals[i + 2] = normal;
				}

				StrideBuffer.SetData<Vector3f>( mesh.VertexBuffer, normalElement.Offset, normals, 0, vertexCount, targetStride );
			}

			return new Mesh( targetDecl, mesh.VertexBuffer, null, mesh.Material, GeometryType.TriangleList, mesh.GeometryCount );
		}
		
		public static int PickColorFromIndex( int index, bool swizzle ) {
			if( swizzle ) {
				// BJC: Swizzles the bits around so that, when looking at the pick buffer,
				// there's something interesting to look at. Basically, it makes sure that
				// the least significant 24 bits end up in the most significant color positions.
				int result = 0;

				unchecked {
					for( int i = 0; i < 8; i++ )
						for( int j = 0; j < 3; j++ )
							result |= (int) ((((uint) index >> ((i * 3) + j)) & 0x1) << (23 - (j * 8) - i));
				}

				return result;
			}
			else {
				return unchecked((int) index);
			}
		}

		/// <summary>
		/// Creates a mesh in which each face is rendered in a different color.
		/// </summary>
		/// <param name="mesh">Mesh to convert.</param>
		/// <param name="startColorIndex">Index of the first face.</param>
		/// <param name="swizzleBits">True to swizzle the index bits so that each face has a visually distinct color, false otherwise.</param>
		/// <returns>Returns a mesh with a different solid color for each face.</returns>
		/// <remarks>Any existing normal or color channel on the mesh is stripped.</remarks>
		public static Mesh CreateFacePickMesh( Mesh mesh, int startColorIndex, bool swizzleBits ) {
			if( mesh == null )
				throw new ArgumentNullException( "mesh" );

			// Create a new vertex declaration for this mesh
			List<VertexElement> elementList = new List<VertexElement>( mesh.VertexDeclaration.Elements.Count );
			int newStride = 0;
			
			foreach( VertexElement element in mesh.VertexDeclaration.Elements ) {
				if( element.Usage == VertexUsage.Normal || element.Usage == VertexUsage.Color )
					continue;
					
				elementList.Add( new VertexElement( newStride, element.ElementType, element.Usage, element.UsageIndex ) );
				newStride += VertexElement.GetElementSize( element.ElementType );
			}
			
			VertexElement colorElement = new VertexElement( newStride, VertexElementType.Byte4, VertexUsage.Color, 0 );
			elementList.Add( colorElement );
			newStride += VertexElement.GetElementSize( VertexElementType.Byte4 );
			
			VertexDeclaration newDeclaration = new VertexDeclaration( newStride, elementList.ToArray() );
			
			// Map everything to the new declaration
			mesh = UntangleVertices( mesh );
			byte[] newVertexBuffer = mesh.VertexDeclaration.MapVerticesToNewDeclaration( mesh.VertexBuffer, 0, mesh.GeometryCount * 3, newDeclaration );
			
			// Set colors!
			int[] colors = new int[mesh.GeometryCount * 3];
			
			for( int i = 0; i < mesh.GeometryCount; i++ )
				colors[i * 3 + 2] = colors[i * 3 + 1] = colors[i * 3] = PickColorFromIndex( startColorIndex + i, swizzleBits );
				
			StrideBuffer.SetData<int>( newVertexBuffer, colorElement.Offset, colors, 0, colors.Length, newStride );
			
			return new Mesh( newDeclaration, newVertexBuffer, null, mesh.Material, GeometryType.TriangleList, mesh.GeometryCount );
		}
		
		public static int IndexFromPickColor( int color, bool swizzled ) {
			if( swizzled ) {
				// BJC: Swizzles the bits around so that, when looking at the pick buffer,
				// there's something interesting to look at. Basically, it makes sure that
				// the least significant 24 bits end up in the most significant color positions.
				uint result = 0;

				for( int i = 0; i < 8; i++ )
					for( int j = 0; j < 3; j++ )
						result |= ((unchecked((uint) color) >> (23 - (j * 8) - i)) & 0x1) << ((i * 3) + j);

				return (int) result;
			}
			else {
				return color;
			}
		}
		
		/// <summary>
		/// Creates a vertex buffer in which each position is used exactly once in ascending order.
		/// </summary>
		/// <param name="indices">Indices into the source vertex buffer specifying the vertices.</param>
		/// <param name="sourceVertices">Vertex buffer containing the vertices to untangle.</param>
		/// <param name="sourceStride">Stride of the source vertex buffer.</param>
		/// <param name="targetStride">Target stride of the new vertex buffer. If less than <paramref name="sourceStride"/>, the vertices
		///     will be trimmed. If greater than <paramref name="sourceStride"/>, zero padding will be added at the end of the vertices.</param>
		/// <returns>A vertex buffer in which each vertex is used once and in ascending order.</returns>
		private static byte[] UntangleVertices( int[] indices, byte[] sourceVertices, int sourceStride, int targetStride ) {
			if( indices == null )
				throw new ArgumentNullException( "indices" );
			
			byte[] result = new byte[targetStride * indices.Length];

			for( int i = 0; i < indices.Length; i++ )
				StrideBuffer.Copy( sourceVertices, sourceStride * indices[i], sourceStride, result, targetStride * i, targetStride, 1 );
			
			return result;
		}
		
		/// <summary>
		/// Creates a mesh in which each vertex is used exactly once in ascending order.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="targetStride"></param>
		/// <returns></returns>
		public static Mesh UntangleVertices( Mesh mesh, int targetStride ) {
			if( mesh == null )
				throw new ArgumentNullException( "mesh" );
			
			if( mesh.Indices == null )
				return mesh;
			
			return new Mesh( mesh.VertexDeclaration,
				UntangleVertices( mesh.Indices, mesh.VertexBuffer, mesh.VertexDeclaration.Stride, targetStride ),
				null, mesh.Material, mesh.GeometryType, mesh.GeometryCount );
		}
		
		public static Mesh UntangleVertices( Mesh mesh ) {
			if( mesh == null )
				throw new ArgumentNullException( "mesh" );

			return UntangleVertices( mesh, mesh.VertexDeclaration.Stride );
		}
	}
}