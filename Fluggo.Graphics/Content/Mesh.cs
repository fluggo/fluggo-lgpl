using System;
using Fluggo.Graphics;
using System.Collections.Generic;

namespace Fluggo.Graphics.Content {
	public class Mesh {
		object _position;
		object _normal;
		List<object> _texCoords;
	}
	
	public class MeshAttribute<T> {
		MeshAttributeUsage _usage;
		T[] _values;
		
	}
	
	public enum MeshAttributeUsage {
		Position = 0,
		Normal = 1,
		TextureCoordinate = 2,
		BlendWeight = 3
	}
}