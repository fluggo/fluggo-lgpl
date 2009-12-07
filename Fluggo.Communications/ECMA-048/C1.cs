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
	/// Describes a control code in the C1 set.
	/// </summary>
	public enum C1 : byte {
		BreakPermittedHere = 0x82,			// BPH
		NoBreakHere = 0x83,					// NBH
		NextLine = 0x85,					// NEL
		StartSelectedArea = 0x86,			// SSA
		EndSelectedArea = 0x87,				// ESA
		CharacterTabSet = 0x88,				// HTS
		CharacterTabJustify = 0x89,			// HTJ
		SetLineTab = 0x8A,					// VTS
		PartialLineForward = 0x8B,			// PLD
		PartialLineBackward = 0x8C,			// PLU
		ReverseLineFeed = 0x8D,				// RI
		SingleShift2 = 0x8E,				// SS2
		SingleShift3 = 0x8F,				// SS3
		DeviceControlString = 0x90,			// DCS
		PrivateUse1 = 0x91,					// PU1
		PrivateUse2 = 0x92,					// PU2
		SetTransmitState = 0x93,			// STS
		CancelCharacter = 0x94,				// CCH
		MessageWaiting = 0x95,				// MW
		StartGuardedArea = 0x96,			// SPA
		EndGuardedArea = 0x97,				// EPA
		StartOfString = 0x98,				// SOS
		SingleCharacterIntroducer = 0x9A,	// SCI
		ControlSequenceIntroducer = 0x9B,	// CSI
		StringTerminator = 0x9C,			// ST
		OperatingSystemCommand = 0x9D,		// OPC
		PrivateMessage = 0x9E,				// PM
		ApplicationProgramCommand = 0x9F	// APC
	}
}