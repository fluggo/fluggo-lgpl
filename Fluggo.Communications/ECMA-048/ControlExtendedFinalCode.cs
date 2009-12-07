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
	/// Describes the final character of an escape control sequence with a single 02/00 intermediate byte (hex 0x20).
	/// </summary>
	public enum ControlExtendedFinalCode : byte {
		ScrollLeft = 0x40,						// SL
		ScrollRight = 0x41,						// SR
		GraphicSizeModify = 0x42,				// GSM
		GraphicSizeSelect = 0x43,				// GSS
		FontSelect = 0x44,						// FNT
		ThinSpaceWidth = 0x45,					// TSS
		Justify = 0x46,							// JFY
		SpacingIncrement = 0x47,				// SPI
		Quad = 0x48,							// QUAD
		SelectSizeUnit = 0x49,					// SSU
		PageFormatSelect = 0x4A,				// PFS
		SelectCharacterSpacing = 0x4B,			// SHS
		SelectLineSpacing = 0x4C,				// SVS
		IdentifyGraphicSubrepertoire = 0x4D,	// IGS
		IdentifyDeviceControlString = 0x4F,		// IDCS
		PagePositionAbsolute = 0x50,			// PPA
		PagePositionForward = 0x51,				// PPR
		PagePositionBackward = 0x52,			// PPB
		SelectPresentationDirections = 0x53,	// SPD
		DimensionTextArea = 0x54,				// DTA
		SetLineHome = 0x55,						// SLH
		SetLineLimit = 0x56,					// SLL
		FunctionKey = 0x57,						// FNK
		SelectPrintQuality = 0x58,				// SPQR
		SheetEjectAndFeed = 0x59,				// SEF
		PresentationExpandContract = 0x5A,		// PEC
		SetSpaceWidth = 0x5B,					// SSW
		SetAdditionalCharacterSeparation = 0x5C,// SACS
		SelectAlternativePresentationVariants = 0x5D,	// SAPV
		SelectiveTab = 0x5E,					// STAB
		GraphicCharacterCombine = 0x5F,			// GCC
		TabAlignedTrailingEdge = 0x60,			// TATE
		TabAlignedLeadingEdge = 0x61,			// TALE
		TabAlignedCentered = 0x62,				// TAC
		TabCenteredOnCharacter = 0x63,			// TCC
		TabStopRemove = 0x64,					// TSR
		SeletCharacterOrientation = 0x65,		// SCO
		SetReducedCharacterSeparation = 0x66,	// SRCS
		SetCharacterSpacing = 0x67,				// SCS
		SetLineSpacing = 0x68,					// SLS
		SetPageHome = 0x69,						// SPH
		SetPageLimit = 0x6A,					// SPL
		SelectCharacterPath = 0x6B				// SCP
	}
}