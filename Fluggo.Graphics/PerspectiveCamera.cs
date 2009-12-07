using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo.Graphics
{
	/// <summary>
	/// Represents a simple perspective camera.
	/// </summary>
	public class PerspectiveCamera {
		Vector3f _position;
		float _aspectRatio = 1.0f, _fieldOfView = (float) Math.PI * 0.5f, _nearZ = -5.0f, _farZ = -100.0f;
		float _yaw, _pitch, _roll;

		/// <summary>
		/// Gets or sets the distance of the near clipping plane in world units.
		/// </summary>
		/// <value>The distance of the near clipping plane in world units. The default is -5.0.</value>
		/// <remarks>The camera only "sees" objects between the near clipping plane and the far clipping plane.
		///   Objects at the near clipping plane have Z values near -1 after the camera transform. Carefully controlling
		///   the ratio between <see cref="FarZ"/> and <see cref="NearZ"/> is the key to reducing quantization errors
		///   when using depth buffers.</remarks>
		public float NearZ {
			get {
				return _nearZ;
			}
			set {
				_nearZ = value;
			}
		}

		/// <summary>
		/// Gets or sets the distance of the far clipping plane in world units.
		/// </summary>
		/// <value>The distance of the far clipping plane in world units. The default is -100.0.</value>
		/// <remarks>The camera only "sees" objects between the near clipping plane and the far clipping plane.
		///   Objects at the far clipping plane have Z values near 1 after the camera transform. Carefully controlling
		///   the ratio between <see cref="FarZ"/> and <see cref="NearZ"/> is the key to reducing quantization errors
		///   when using depth buffers.</remarks>
		public float FarZ {
			get {
				return _farZ;
			}
			set {
				_farZ = value;
			}
		}

		/// <summary>
		/// Gets or sets the position of the camera in world coordinates.
		/// </summary>
		/// <value>The position of the camera in world coordinates. By default, the camera is at the origin.</value>
		public Vector3f Position {
			get {
				return _position;
			}
			set {
				_position = value;
			}
		}

		/// <summary>
		/// Gets or sets the aspect ratio of the camera.
		/// </summary>
		/// <value>The aspect ratio of the camera, which is the ratio of height to width. The default is 1.0.</value>
		/// <remarks>Set the aspect ratio to match the aspect ratio of your viewport or window.</remarks>
		public float AspectRatio {
			get {
				return _aspectRatio;
			}
			set {
				_aspectRatio = value;
			}
		}

		/// <summary>
		/// Gets or sets the camera's vertical field of view.
		/// </summary>
		/// <value>The camera's vertical field of view, in radians. The default is half-PI radians, or 90 degrees.</value>
		/// <remarks>The field of view is the angle between the camera, the bottom of the clipping planes at their center,
		///   and the top of the clipping planes. This determines how much "zoom" a camera has. The lower the field of view,
		///   the more "zoomed in" the camera is, and vice-versa.</remarks>
		public float FieldOfView {
			get {
				return _fieldOfView;
			}
			set {
				_fieldOfView = value;
			}
		}

		/// <summary>
		/// Gets or sets the camera's yaw.
		/// </summary>
		/// <value>The camera's yaw, in radians. The default is zero.</value>
		/// <remarks>The yaw is the camera's left-to-right direction. A positive value will point the camera to the left.
		///   <para>The camera's transforms are performed in the order yaw-pitch-roll.</para></remarks>
		public float Yaw {
			get {
				return _yaw;
			}
			set {
				_yaw = value;
			}
		}

		/// <summary>
		/// Gets or sets the camera's pitch.
		/// </summary>
		/// <value>The camera's pitch, in radians. The default is zero.</value>
		/// <remarks>The pitch, or tilt, determines how high the camera looks. A positive value will point the camera up.
		///   <para>The camera's transforms are performed in the order yaw-pitch-roll.</para></remarks>
		public float Pitch {
			get {
				return _pitch;
			}
			set {
				_pitch = value;
			}
		}

		/// <summary>
		/// Gets or sets the camera's roll.
		/// </summary>
		/// <value>The camera's roll, in radians. The default is zero.</value>
		/// <remarks>The roll turns the camera sideways. A positive value will turn the camera counter-clockwise.
		///   <para>The camera's transforms are performed in the order yaw-pitch-roll.</para></remarks>
		public float Roll {
			get {
				return _roll;
			}
			set {
				_roll = value;
			}
		}
		
		/// <summary>
		/// Changes the camera's yaw and pitch to point at the given position.
		/// </summary>
		/// <param name="position">Position to look at.</param>
		public void LookAt( Vector3f position ) {
			float diffZ = position.Z - _position.Z,
				diffY = position.Y - _position.Y,
				diffX = position.X - _position.X;
				
			// The yaw is the atan(cz-oz/ox-cx)
			_yaw = (float) Math.Atan2( -diffX, -diffZ );
			
			// Pitch is atan(oy-cy/sqrt((ox-cx)^2 + (cz-oz)^2))
			_pitch = (float) Math.Atan2( diffY, Math.Sqrt( diffX * diffX + diffZ * diffZ ) );
		}

		/// <summary>
		/// Gets the combined view and projection transform matrix for the camera.
		/// </summary>
		/// <returns>The combined view and projection transform matrix for the camera.</returns>
		public Matrix4f GetTransformMatrix() {
			return
				Matrix4f.CreateTranslation( -_position ) *
				Matrix4f.CreateYawPitchRollRotation( -_yaw, -_pitch, -_roll ) *
				Matrix4f.CreateFieldOfViewProjection( _fieldOfView, _aspectRatio, _nearZ, _farZ );
		}
	}
}
