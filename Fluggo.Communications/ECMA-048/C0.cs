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
	/// Describes a control code in the C0 set.
	/// </summary>
	public enum C0 : byte
	{
		/// <summary>
		/// Null (NUL). Used for media-fill or time-fill. NUL characters may be inserted into, or removed from, a data stream without affecting the information content of that stream, but such action may affect the information layout and/or the control of equipment.
		/// </summary>
		Null = 0x00,					// NUL
		
		/// <summary>
		/// Start of heading (SOH). Used to indicate the beginning of a heading. The use of SOH is defined in ISO 1745.
		/// </summary>
		StartHeading = 0x01,			// SOH
		
		/// <summary>
		/// Start of text (STX). Used to indicate the beginning of a text and the end of a heading. The use of STX is defined in ISO 1745.
		/// </summary>
		StartText = 0x02,				// STX
		EndText = 0x03,					// ETX
		EndTransmission = 0x04,			// EOT
		Enquiry = 0x05,					// ENQ
		Acknowledge = 0x06,				// ACK
		Bell = 0x07,					// BEL
		
		/// <summary>
		/// Backspace (BS). Causes the active data position to be moved one character position in the data component in the direction opposite to that of the implicit movement.
		/// The direction of the implicit movement depends on the parameter value of <see cref="ControlFinalCode.SelectImplicitMovementDirection"/> (SIMD).
		/// </summary>
		Backspace = 0x08,				// BS
		CharacterTab = 0x09,			// HT
		LineFeed = 0x0A,				// LF
		LineTab = 0x0B,					// VT
		FormFeed = 0x0C,				// FF
		
		/// <summary>
		/// Carriage return (CR). The effect of CR depends on the setting of the DEVICE COMPONENT SELECT MODE (DCSM) and on the parameter value of <see cref="ControlFinalCode.SelectImplicitMovementDirection"/> (SIMD).
		/// <para>If the DEVICE COMPONENT SELECT MODE (DCSM) is set to PRESENTATION and with the parameter value of SIMD equal to 0, CR causes the active presentation position to be moved to the line home position of the same line in the presentation component. The line home position is established by the parameter value of SET LINE HOME (SLH).</para>
		/// <para>With a parameter value of SIMD equal to 1, CR causes the active presentation position to be moved to the line limit position of the same line in the presentation component. The line limit position is established by the parameter value of SET LINE LIMIT (SLL).</para>
		/// <para>If the DEVICE COMPONENT SELECT MODE (DCSM) is set to DATA and with a parameter value of SIMD equal to 0, CR causes the active data position to be moved to the line home position of the same line in the data component. The line home position is established by the parameter value of SET LINE HOME (SLH).</para>
		/// <para>With a parameter value of SIMD equal to 1, CR causes the active data position to be moved to the line limit position of the same line in the data component. The line limit position is established by the parameter value of SET LINE LIMIT (SLL).</para>
		/// </summary>
		CarriageReturn = 0x0D,			// CR
		
		/// <summary>
		/// Shift-out (SO). Used for code extension purposes. It causes the meanings of the bit combinations following it in the data stream to be changed. The use of SO is defined in Standard ECMA-35.
		/// <para>SO is used in 7-bit environments only; in 8-bit environments LOCKING-SHIFT ONE (LS1) is used instead.</para>
		/// </summary>
		ShiftOut = 0x0E,				// SO
		
		/// <summary>
		/// Shift-in (SI). Used for code extension purposes. It causes the meanings of the bit combinations following it in the data stream to be changed.
		/// <para>SI is used in 7-bit environments only; in 8-bit environments LOCKING-SHIFT ZERO (LS0) is used instead.</para>
		/// </summary>
		ShiftIn = 0x0F,					// SI
		DataLinkEscape = 0x10,			// DLE
		
		/// <summary>
		/// Device control one (DC1). Primarily intended for turning on or starting an ancillary device. If it is not required for this purpose, it may be used to restore a device to the basic mode of operation (see also DC2 and DC3), or any other device control function not provided by other DCs.
		/// <para>When used for data flow control, DC1 is sometimes called "X-ON".</para>
		/// </summary>
		DeviceControl1 = 0x11,			// DC1
		
		/// <summary>
		/// Device control two (DC2). Primarily intended for turning on or starting an ancillary device. If it is not required for this purpose, it may be used to set a device to a special mode of operation (in which case DC1 is used to restore the device to the basic mode), or for any other device control function not provided by other DCs.
		/// </summary>
		DeviceControl2 = 0x12,			// DC2
		
		/// <summary>
		/// Device control three (DC3). Primarily intended for turning off or stopping an ancillary device. This function may be a secondary level stop, for example wait, pause, stand-by or halt (in which case DC1 is used to restore normal operation). If it is not required for this purpose, it may be used for any other device control function not provided by other DCs.
		/// <para>When used for data flow control, DC3 is sometimes called "X-OFF".</para>
		/// </summary>
		DeviceControl3 = 0x13,			// DC3
		
		/// <summary>
		/// Device control four (DC4). Primarily intended for turning off, stopping or interrupting an ancillary device. If it is not required for this purpose, it may be used for any other device control function not provided by other DCs.
		/// </summary>
		DeviceControl4 = 0x14,			// DC4
		NegativeAcknowledge = 0x15,		// NAK
		SynchronousIdle = 0x16,			// SYN
		
		/// <summary>
		/// End of transmission block (ETB). Used to indicate the end of a block of data where the data are divided into such blocks for transmission purposes. The use of ETB is defined in ISO 1745.
		/// </summary>
		EndTransmissionBlock = 0x17,	// ETB
		
		/// <summary>
		/// Cancel (CAN). Used to indicate that the data preceding it in the data stream is in error. As a result, this data shall be ignored. The specific meaning of this control function shall be defined for each application and/or between sender and recipient.
		/// </summary>
		Cancel = 0x18,					// CAN
		
		/// <summary>
		/// End of medium (EM). Used to identify the physical end of a medium, or the end of the used portion of a medium, or the end of the wanted portion of data recorded on a medium.
		/// </summary>
		EndMedium = 0x19,				// EM
		Substitute = 0x1A,				// SUB
		
		/// <summary>
		/// Escape (ESC). Used for code extension purposes. It causes the meanings of a limited number of bit combinations following it in the data stream to be changed. The use of ESC is defined in Standard ECMA-35.
		/// </summary>
		Escape = 0x1B,					// ESC
		InfoSeparator4 = 0x1C,			// IS4
		InfoSeparator3 = 0x1D,			// IS3
		InfoSeparator2 = 0x1E,			// IS2
		InfoSeparator1 = 0x1F			// IS1
	}
}