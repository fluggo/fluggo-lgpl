using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo.Graphics {
	/// <summary>
	/// Represents a set of properties or instructions for rendering an object.
	/// </summary>
	public abstract class Material {
	}
	
	/// <summary>
	/// Specifies basic properties for rendering an object.
	/// </summary>
	public class BasicMaterial : Material {
		bool _useLighting, _usePerVertexColor;
		ColorARGB _diffuse, _specular, _emissive;
		float _specularPower;
		
		public ColorARGB DiffuseColor {
			get { return _diffuse; }
			set { _diffuse = value; }
		}
		
		public ColorARGB SpecularColor {
			get { return _specular; }
			set { _specular = value; }
		}
		
		public ColorARGB EmissiveColor {
			get { return _emissive; }
			set { _emissive = value; }
		}
		
		public float SpecularPower {
			get { return _specularPower; }
			set { _specularPower = value; }
		}

		/// <summary>
		/// Gets or sets a value that represents whether per-vertex lighting should be used.
		/// </summary>
		/// <value>True if per-vertex lighting should be used, false otherwise.</value>
		/// <remarks>Meshes rendered with this attribute must contain vertex normal information.</remarks>
		public bool UseLighting {
			get { return _useLighting; }
			set { _useLighting = value; }
		}
		
		public bool UsePerVertexColor {
			get { return _usePerVertexColor; }
			set { _usePerVertexColor = value; }
		}
	}
}
