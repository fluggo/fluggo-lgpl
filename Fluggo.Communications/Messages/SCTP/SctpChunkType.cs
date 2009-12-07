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

using System;

namespace Fluggo.Communications {
	public enum SctpChunkType : byte {
		/*
		   0          - Payload Data (DATA)
		   1          - Initiation (INIT)
		   2          - Initiation Acknowledgement (INIT ACK)
		   3          - Selective Acknowledgement (SACK)
		   4          - Heartbeat Request (HEARTBEAT)
		   5          - Heartbeat Acknowledgement (HEARTBEAT ACK)
		   6          - Abort (ABORT)
		   7          - Shutdown (SHUTDOWN)
		   8          - Shutdown Acknowledgement (SHUTDOWN ACK)
		   9          - Operation Error (ERROR)
		   10         - State Cookie (COOKIE ECHO)
		   11         - Cookie Acknowledgement (COOKIE ACK)
		   12         - Reserved for Explicit Congestion Notification Echo (ECNE)
		   13         - Reserved for Congestion Window Reduced (CWR)
		   14         - Shutdown Complete (SHUTDOWN COMPLETE)
		   15 to 62   - reserved by IETF
		   63         - IETF-defined Chunk Extensions
		   64 to 126  - reserved by IETF
		   127        - IETF-defined Chunk Extensions
		   128 to 190 - reserved by IETF
		   191        - IETF-defined Chunk Extensions
		   192 to 254 - reserved by IETF
		   255        - IETF-defined Chunk Extensions
		 */

		/// <summary>
		/// Payload data. An <see cref="ISctpMessage"/> with this type must support the <see cref="IDataMessage"/> interface.
		/// </summary>
		Data = 0,
		
		/// <summary>
		/// Initiation.
		/// </summary>
		Init = 1,
		
		/// <summary>
		/// Initiation acknowledgement.
		/// </summary>
		InitAck = 2,
		
		/// <summary>
		/// Selective acknowledgement.
		/// </summary>
		SelectAck = 3,
		
		/// <summary>
		/// Heartbeat request.
		/// </summary>
		Heartbeat = 4,
		
		/// <summary>
		/// Heartbeat acknowledgement.
		/// </summary>
		HeartbeatAck = 5,
		
		/// <summary>
		/// Abort.
		/// </summary>
		Abort = 6,
		
		/// <summary>
		/// Shutdown.
		/// </summary>
		Shutdown = 7,
		
		/// <summary>
		/// Shutdown acknowledgement.
		/// </summary>
		ShutdownAck = 8,
		
		/// <summary>
		/// Operation error.
		/// </summary>
		Error = 9,
		
		/// <summary>
		/// State cookie.
		/// </summary>
		CookieEcho = 10,
		
		/// <summary>
		/// Cookie acknowledgement.
		/// </summary>
		CookieAck = 11,
		
		/// <summary>
		/// Reserved for Explicit Congestion Notification Echo.
		/// </summary>
		EcnEcho = 12,
		
		/// <summary>
		/// Reserved for Congestion Window Reduced.
		/// </summary>
		CongestionWindowReduced = 13,
		
		/// <summary>
		/// Shutdown complete.
		/// </summary>
		ShutdownComplete = 14,
		
		/// <summary>
		/// Mask used to determine the action taken when a chunk type is not recognized.
		/// </summary>
		UnrecognizedTypeMask = 0xC0,
		
		/// <summary>
		/// Stop processing chunks in the same packet if this chunk type is not recognized.
		/// </summary>
		UnrecognizedStop = 0x00,
		
		/// <summary>
		/// Stop processing chunks in the same packet and report an error if this chunk type is not recognized.
		/// </summary>
		UnrecognizedStopError = 0x40,
		
		/// <summary>
		/// Skip this chunk if the chunk type is not recognized, but process the rest of the chunks in the packet.
		/// </summary>
		UnrecognizedSkip = 0x80,

		/// <summary>
		/// Skip this chunk if the chunk type is not recognized, but process the rest of the chunks in the packet.
		/// </summary>
		UnrecognizedSkipError = 0xC0,
	}
}