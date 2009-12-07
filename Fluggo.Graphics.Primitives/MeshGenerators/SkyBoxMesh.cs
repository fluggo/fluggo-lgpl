using Fluggo;
using Fluggo.Graphics;

namespace Fluggo.Graphics.MeshGenerators {
	public static class SkyBoxMesh {
		struct MyVertex {
			public MyVertex( float x, float y, float z, float u, float v ) {
				Position = new Vector3f( x, y, z );
				TexCoord0 = new Vector2f( u, v );
			}

			public Vector3f Position;
			public Vector2f TexCoord0;
		}
		
		/// <summary>
		/// Creates a sky box mesh with the same coordinates on all sides.
		/// </summary>
		/// <param name="size">Size of one side of the cube.</param>
		/// <param name="textureRepeat">Scale of the texture coordinates. A scale of 1.0 gives the same texture only once on each side.</param>
		/// <returns></returns>
		public static Mesh CreateSimpleSkyBox( float size, float textureRepeat ) {
			const int __vertexCount = 12;
			
			VertexDeclaration declaration = new VertexDeclaration( 20,
				new VertexElement( 0, VertexElementType.Float3, VertexUsage.Position, 0 ),
				new VertexElement( 12, VertexElementType.Float2, VertexUsage.TextureCoordinate, 0 ) );
			byte[] vbuf = new byte[declaration.Stride * __vertexCount];

			StrideBuffer.SetData<MyVertex>( vbuf, 0, new MyVertex[] {
				new MyVertex( -size, -size, -size, -textureRepeat, -textureRepeat ),
				new MyVertex(  size, -size, -size,  textureRepeat, -textureRepeat ),
				new MyVertex(  size,  size, -size,  textureRepeat,  textureRepeat ),
				new MyVertex( -size,  size, -size, -textureRepeat,  textureRepeat ),
				new MyVertex( -size, -size,  size,  textureRepeat, -textureRepeat ),
				new MyVertex(  size, -size,  size, -textureRepeat, -textureRepeat ),
				new MyVertex(  size,  size,  size, -textureRepeat,  textureRepeat ),
				new MyVertex( -size,  size,  size,  textureRepeat,  textureRepeat ),
				new MyVertex( -size, -size, -size,  textureRepeat,  textureRepeat ),
				new MyVertex(  size, -size, -size, -textureRepeat,  textureRepeat ),
				new MyVertex(  size,  size, -size, -textureRepeat, -textureRepeat ),
				new MyVertex( -size,  size, -size,  textureRepeat, -textureRepeat ),
			}, 0, __vertexCount, declaration.Stride );

			int[] indices = new int[] {
				0, 2, 1,
				0, 3, 2,
				4, 3, 0,
				4, 7, 3,
				5, 7, 4,
				5, 6, 7,
				5, 1, 2,
				5, 2, 6,
				4, 9, 5,		// TOP
				4, 8, 9,
				6, 11, 7,		// BOTTOM
				6, 10, 11
			};
			
			return new Mesh( declaration, vbuf, indices );
		}
	}
}