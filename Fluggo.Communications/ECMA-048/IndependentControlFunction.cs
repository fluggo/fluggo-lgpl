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
	/// <summary>
	/// Describes an intermediate control function.
	/// </summary>
	public enum IndependentControlFunction : byte
	{
		Dmi = 0x60,
		Int = 0x61,
		Emi = 0x62,
		Ris = 0x63,
		Cmd = 0x64,
		Ls2 = 0x6E,
		Ls3 = 0x6F,
		Ls3r = 0x7B,
		Ls2r = 0x7C,
		Ls1r = 0x7D
	}
}