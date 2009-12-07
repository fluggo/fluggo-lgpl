/*
	Fluggo Communications Library
	Copyright (C) 2005-6  Brian J. Crowell

	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

namespace Fluggo.Communications.Terminals {
	public enum SetGraphicRenditionParam : int
	{
		/// <summary>
		/// Default rendition (implementation-defined). Cancels the effect of any preceding occurence of SGR
		/// in the data stream regardless of the setting of the graphic rendition combination mode.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Bold or increased intensity.
		/// </summary>
		Bold = 1,

		/// <summary>
		/// Faint, decreased intensity or second color.
		/// </summary>
		Faint = 2,

		/// <summary>
		/// Italicized.
		/// </summary>
		Italic = 3,

		/// <summary>
		/// Singly underlined.
		/// </summary>
		Underline = 4,

		SlowBlink = 5,
		FastBlink = 6,
		NegativeImage = 7,
		ConcealedCharacters = 8,
		StrikeOut = 9,
		PrimaryFont = 10,
		AlternativeFont1 = 11,
		AlternativeFont2 = 12,
		AlternativeFont3 = 13,
		AlternativeFont4 = 14,
		AlternativeFont5 = 15,
		AlternativeFont6 = 16,
		AlternativeFont7 = 17,
		AlternativeFont8 = 18,
		AlternativeFont9 = 19,
		Fraktur = 20,
		DoubleUnderline = 21,

		/// <summary>
		/// Normal color or normal intensity. Cancels the effect of Bold or Faint.
		/// </summary>
		NormalIntensity = 22,

		/// <summary>
		/// Cancels the effect of Italic and Fraktur.
		/// </summary>
		NoItalics = 23,

		/// <summary>
		/// Cancels the effect of Underline and DoubleUnderline.
		/// </summary>
		NoUnderline = 24,

		/// <summary>
		/// Cancels the effect of SlowBlink and FastBlink.
		/// </summary>
		Steady = 25,

		/// <summary>
		/// Cancels the effect of NegativeImage.
		/// </summary>
		PositiveImage = 27,

		/// <summary>
		/// Cancels the effect of ConcealedCharacters.
		/// </summary>
		RevealedCharacters = 28,

		/// <summary>
		/// Cancels the effect of StrikeOut.
		/// </summary>
		NoStrikeOut = 29,

		/// <summary>
		/// Sets the foreground color to black (dark grey if bold).
		/// </summary>
		Black = 30,
		Red = 31,
		Green = 32,
		Yellow = 33,
		Blue = 34,
		Magenta = 35,
		Cyan = 36,
		White = 37,

		DefaultColor = 39,

		BlackBackground = 40,
		RedBackground = 41,
		GreenBackground = 42,
		YellowBackground = 43,
		BlueBackground = 44,
		MagentaBackground = 45,
		CyanBackground = 46,
		WhiteBackground = 47,

		DefaultBackgroundColor = 49,

		Frame = 51,
		Circle = 52,
		Overline = 53,

		/// <summary>
		/// Cancels the effect of Frame and Circle.
		/// </summary>
		NoFrame = 54,

		/// <summary>
		/// Cancels the effect of Overline.
		/// </summary>
		NoOverline = 55
	}
}