using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo.Graphics
{
	public struct Color4f {
		const float __byteToFloat = 1.0f / 255.0f;

		/// <summary>
		/// Creates a new instance of the <see cref='Color4f'/> structure.
		/// </summary>
		/// <param name="red">The value of red for the color, from 0.0 (black) to 1.0 (red).</param>
		/// <param name="green">The value of green for the color, from 0.0 (black) to 1.0 (green).</param>
		/// <param name="blue">The value of blue for the color, from 0.0 (black) to 1.0 (blue).</param>
		/// <param name="alpha">The value of alpha for the color, from 0.0 (transparent) to 1.0 (opaque).</param>
		public Color4f( float red, float green, float blue, float alpha ) {
			R = red;
			G = green;
			B = blue;
			A = alpha;
		}

		public Color4f( float red, float green, float blue )
			: this( red, green, blue, 1.0f ) {
		}

		public Color4f( byte red, byte green, byte blue, byte alpha )
			: this( (float) red * __byteToFloat, (float) green * __byteToFloat, (float) blue * __byteToFloat, (float) alpha * __byteToFloat ) {
		}

		public Color4f( byte red, byte green, byte blue )
			: this( (float) red * __byteToFloat, (float) green * __byteToFloat, (float) blue * __byteToFloat, 1.0f ) {
		}

		public Color4f( ColorARGB color ) : this( color.Red, color.Green, color.Blue, color.Alpha ) {
		}

		public Color4f( ColorRGBA color ) : this( color.Red, color.Green, color.Blue, color.Alpha ) {
		}

		public Color4f( Color3f color ) : this( color.R, color.G, color.B, 1.0f ) {
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

		/// <summary>
		/// Contains the value of alpha (transparency) for the color.
		/// </summary>
		/// <value>The value of alpha for the color, from 0.0 (transparent) to 1.0 (opaque).</value>
		public float A;

		public static Color4f Max( Color4f color1, Color4f color2 ) {
			return new Color4f(
				Math.Max( color1.R, color2.R ),
				Math.Max( color1.G, color2.G ),
				Math.Max( color1.B, color2.B ),
				Math.Max( color1.A, color2.A ) );
		}
		
		public static Color4f Min( Color4f color1, Color4f color2 ) {
			return new Color4f(
				Math.Min( color1.R, color2.R ),
				Math.Min( color1.G, color2.G ),
				Math.Min( color1.B, color2.B ),
				Math.Min( color1.A, color2.A ) );
		}

		public Color4f Clamp() {
			return Max( Min( this, White ), TransparentBlack );
		}
		
		public Vector4f ToVector4f() {
			return new Vector4f( R, G, B, A );
		}

		public static implicit operator Color4f( System.Drawing.Color color ) {
			return new Color4f( color.R, color.G, color.B, color.A );
		}

		public static implicit operator Color4f( Color3f color ) {
			return new Color4f( color );
		}

		public static readonly Color4f White = new Color4f( 1.0f, 1.0f, 1.0f, 1.0f );
		public static readonly Color4f TransparentBlack = new Color4f( 0.0f, 0.0f, 0.0f, 0.0f );
	}
}
