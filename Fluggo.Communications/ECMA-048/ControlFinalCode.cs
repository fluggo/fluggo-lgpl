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
	/// Describes the final character of an escape control sequence with no intermediates.
	/// </summary>
	public enum ControlFinalCode : byte {
		InsertCharacter = 0x40,				// ICH
		CursorUp = 0x41,					// CUU
		CursorDown = 0x42,					// CUD
		CursorRight = 0x43,					// CUF
		CursorLeft = 0x44,					// CUB
		CursorNextLine = 0x45,				// CNL
		CurporPreviousLine = 0x46,			// CPL
		CursorCharAbsolute = 0x47,			// CHA
		CursorPosition = 0x48,				// CUP
		CursorForwardTab = 0x49,			// CHT
		ErasePage = 0x4A,					// ED
		EraseLine = 0x4B,					// EL
		InsertLine = 0x4C,					// IL
		DeleteLine = 0x4D,					// DL
		EraseField = 0x4E,					// EF
		EraseArea = 0x4F,					// EA
		DeleteCharacter = 0x50,				// DCH
		SelectEditingExtent = 0x51,			// SEE
		ActivePositionReport = 0x52,		// CPR
		ScrollUp = 0x53,					// SU
		ScrollDown = 0x54,					// SD
		NextPage = 0x55,					// NP
		PrecedingPage = 0x56,				// PP
		CursorTabControl = 0x57,			// CTC
		EraseCharacter = 0x58,				// ECH
		CursorLineTab = 0x59,				// CVT
		CursorBackTab = 0x5A,				// CBT
		StartReversedString = 0x5B,			// SRS
		ParallelTexts = 0x5C,				// PTX
		StartDirectedString = 0x5D,			// SDS
		SelectImplicitMovementDirection = 0x5E,	// SIMD
		CharacterPositionAbsolute = 0x60,	// HPA
		CharacterPositionForward = 0x61,	// HPR
		Repeat = 0x62,						// REP
		DeviceAttributes = 0x63,			// DA
		LinePositionAbsolute = 0x64,		// VPA
		LinePositionForward = 0x65,			// VPR
		CharacterAndLinePosition = 0x66,	// HVP
		TabClear = 0x67,					// TBC
		SetMode = 0x68,						// SM
		MediaCopy = 0x69,					// MC
		CharacterPositionBackward = 0x6A,	// HPB
		LinePositionBackward = 0x6B,		// VPB
		ResetMode = 0x6C,					// RM
		SetGraphicRendition = 0x6D,			// SGR
		DeviceStatusReport = 0x6E,			// DSR
		DefineAreaQualification = 0x6F		// DAQ
	}
}