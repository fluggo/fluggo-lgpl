using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo.Graphics
{
	public struct Color3f {
		const float __byteToFloat = 1.0f / 255.0f;

		/// <summary>
		/// Creates a new instance of the <see cref='Color3f'/> structure.
		/// </summary>
		/// <param name="red">The value of red for the color, from 0.0 (black) to 1.0 (red).</param>
		/// <param name="green">The value of green for the color, from 0.0 (black) to 1.0 (green).</param>
		/// <param name="blue">The value of blue for the color, from 0.0 (black) to 1.0 (blue).</param>
		public Color3f( float red, float green, float blue ) {
			R = red;
			G = green;
			B = blue;
		}
		
		public Color3f( byte red, byte green, byte blue )
			: this( (float) red * __byteToFloat, (float) green * __byteToFloat, (float) blue * __byteToFloat ) {
		}
		
		/// <summary>
		/// Contains the value of red for the color.
		/// </summary>
		/// <value>The value of red for the color, from 0.0 (black) to 1.0 (red).</value>
		public float R;

		/// <summary>
		/// Contains the value of green for the color.
		/// </summary>
		/// <value>The value of green for the color, from 0.0 (black) to 1.0 (green).</value>
		public float G;

		/// <summary>
		/// Contains the value of blue for the color.
		/// </summary>
		/// <value>The value of blue for the color, from 0.0 (black) to 1.0 (blue).</value>
		public float B;

		public Vector3f ToVector3f() {
			return new Vector3f( R, G, B );
		}
		
		public Vector4f ToVector4f() {
			return new Vector4f( R, G, B, 1.0f );
		}

		public static explicit operator Color3f( System.Drawing.Color color ) {
			return new Color3f( color.R, color.G, color.B );
		}
		
		public static readonly Color3f White = new Color3f( 1.0f, 1.0f, 1.0f );
		public static readonly Color3f Black = new Color3f( 0.0f, 0.0f, 0.0f );
		public static readonly Color3f Red = new Color3f( 1.0f, 0.0f, 0.0f );
		public static readonly Color3f Green = new Color3f( 0.0f, 1.0f, 0.0f );
		public static readonly Color3f Blue = new Color3f( 0.0f, 0.0f, 1.0f );
		
		public static readonly Color3f Yellow = (Color3f) System.Drawing.Color.Yellow;
		public static readonly Color3f Violet = (Color3f) System.Drawing.Color.Violet;
		public static readonly Color3f Purple = (Color3f) System.Drawing.Color.Purple;
		public static readonly Color3f Orange = (Color3f) System.Drawing.Color.Orange;
	}
}
