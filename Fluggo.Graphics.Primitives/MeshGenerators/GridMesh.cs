using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo.Graphics.MeshGenerators {
	public static class GridMesh {
		public static Mesh CreateXZGrid( int gridCountX, int gridCountZ ) {
			Vector3f[] positions = new Vector3f[(gridCountX + gridCountZ + 2) * 2];

			float minX = -((float) gridCountX) * 0.5f, minZ = -((float) gridCountX) * 0.5f;
			float maxX = -minX, maxZ = -minZ;
			
			for( int i = 0; i <= gridCountX; i++ ) {
				positions[i * 2] = new Vector3f( minX + 1.0f * i, 0.0f, minZ );
				positions[i * 2 + 1] = new Vector3f( minX + 1.0f * i, 0.0f, maxZ );
			}
			
			for( int i = 0; i <= gridCountZ; i++ ) {
				positions[(gridCountX + 1) * 2 + i * 2] = new Vector3f( minX, 0.0f, minZ + 1.0f * i );
				positions[(gridCountX + 1) * 2 + i * 2 + 1] = new Vector3f( maxX, 0.0f, minZ + 1.0f * i );
			}
			
			int[] indexes = new int[positions.Length];
			
			for( int i = 0; i < positions.Length; i++ )
				indexes[i] = i;
				
			byte[] vertexBuffer = new byte[positions.Length * 12];
			StrideBuffer.SetData<Vector3f>( vertexBuffer, 0, positions, 0, positions.Length, 12 );
			
			return new Mesh( new VertexDeclaration( 12, new VertexElement( 0, VertexElementType.Float3, VertexUsage.Position, 0 ) ),
				vertexBuffer, indexes, new BasicMaterial(), GeometryType.LineList );
		}
	}
}
