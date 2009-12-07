using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo.Graphics
{
	interface IParameter<T> {
		T Parameter { get; set; }
		bool IsReadOnly { get; }
	}
	
	/// <summary>
	/// Represents a node in a tree of transformable nodes.
	/// </summary>
	public abstract class Transform {
		Transform _parent;
		
		/// <summary>
		/// Gets the local transform matrix for the node.
		/// </summary>
		/// <returns>A <see cref="Matrix4f"/> value containing the local transform matrix for the node.</returns>
		public abstract Matrix4f GetLocalTransformMatrix();
		
		/// <summary>
		/// Calculates the world transform matrix for this node.
		/// </summary>
		/// <returns>A <see cref="Matrix4f"/> value containing the world transform matrix for this node.</returns>
		/// <remarks>The world transform is the combined transform for this node and all its ancestors. The base class does
		///   not cache this value.</remarks>
		public virtual Matrix4f GetWorldTransformMatrix() {
			if( _parent == null )
				return GetLocalTransformMatrix();
			
			return GetLocalTransformMatrix() * _parent.GetWorldTransformMatrix();
		}
		
		/// <summary>
		/// Clears any matrices that have been previously cached.
		/// </summary>
		/// <remarks>Call this method after a parameter affecting the world transform changes. The base implementation does nothing.</remarks>
		protected virtual void ClearCachedMatrices() {
		}

		/// <summary>
		/// Gets or sets the parent transform of this node.
		/// </summary>
		/// <value>The parent transform of this node.</value>
		public Transform Parent {
			get {
				return _parent;
			}
			set {
				_parent = value;
				ClearCachedMatrices();
			}
		}
	}
	
	public class BasicTransform : Transform {
		Vector3f _position;
		float _yaw, _pitch, _roll;
		Vector3f _scale = Vector3f.IdentityScale;

		public Vector3f Position {
			get { return _position; }
			set { _position = value; }
		}
		
		public float Yaw {
			get { return _yaw; }
			set { _yaw = value; }
		}
		
		public float Pitch {
			get { return _pitch; }
			set { _pitch = value; }
		}
		
		public float Roll {
			get { return _roll; }
			set { _roll = value; }
		}
		
		public Vector3f Scale {
			get { return _scale; }
			set { _scale = value; }
		}
		
		public override Matrix4f GetLocalTransformMatrix() {
			return Matrix4f.CreateScale( _scale ) *
				Matrix4f.CreateYawPitchRollRotation( _yaw, _pitch, _roll ) *
				Matrix4f.CreateTranslation( _position );
		}
	}
	
	class DirectionalLight : ITransformable<DirectionalLight> {
		Vector3f _direction;
		Color3f _color;

		/// <summary>
		/// Gets or sets the direction from the coordinate origin to the source of the light.
		/// </summary>
		/// <value>A unit vector from the coordinate origin to the source of the light.</value>
		public Vector3f Direction {
			get {
				return _direction;
			}
			set {
				_direction = value;
			}
		}

		public DirectionalLight Transform( Matrix4f transformMatrix ) {
			DirectionalLight light = new DirectionalLight();

			light._color = _color;
			light._direction = (Vector3f) transformMatrix.Transform( _direction.ToDirectionVector4f() );
			
			return light;
		}
	}
	
	interface ITransformable<T> where T : ITransformable<T> {
		T Transform( Matrix4f transformMatrix );
	}
	
	class PointLight : ITransformable<PointLight> {
		float _constAttenuation, _linearAttentuation, _quadraticAttentuation;
		Vector3f _position;
		Color3f _color;
		float _range;

		/// <summary>
		/// Gets or sets the range of the light.
		/// </summary>
		/// <value>The range of the light, in local units.</value>
		/// <remarks>A rendering system may use this value to determine whether to render the effect of the light at all.</remarks>
		public float Range {
			get {
				return _range;
			}
			set {
				if( _range < 0.0f )
					throw new ArgumentOutOfRangeException( "range" );
					
				_range = value;
			}
		}
		
		public PointLight Transform( Matrix4f transformMatrix ) {
			PointLight light = new PointLight();
			
			light._constAttenuation = _constAttenuation;
			light._linearAttentuation = _linearAttentuation;
			light._quadraticAttentuation = _quadraticAttentuation;
			light._range = _range;
			light._color = _color;
			light._position = (Vector3f) transformMatrix.Transform( _position.ToPointVector4f() );
			
			return light;
		}
	}
	
	abstract class Material {
	}
	
}
