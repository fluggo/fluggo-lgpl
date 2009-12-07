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
using System.IO;

namespace Fluggo.Communications {
	public class SctpDataMessage : SctpMessage, IDataMessage
	{
		uint _tsn;
		int _protocol;
		ushort _streamID, _ssn;
		DeliveryOptions _options;
		IMessageBuffer _payload;

		public SctpDataMessage( int streamID, IMessageBuffer data, DeliveryOptions options, int protocol )
			: this( streamID, data, options, protocol, 0, 0 ) {
		}

		[CLSCompliant(false)]
		public SctpDataMessage( int streamID, IMessageBuffer data, DeliveryOptions options, int protocol, uint tsn, ushort ssn )
			: base( SctpChunkType.Data, FlagsFromDeliveryOptions( options ), GetLength( data ) ) {
			if( streamID < 0 || streamID > ushort.MaxValue )
				throw new ArgumentOutOfRangeException( "streamID" );

			_options = options;
			_payload = data;
			_streamID = (ushort) streamID;
			_protocol = protocol;
			_tsn = tsn;
			_ssn = ssn;
		}

		private static byte FlagsFromDeliveryOptions( DeliveryOptions options ) {
			byte flags = 0;

			if( (options & DeliveryOptions.Unordered) == DeliveryOptions.Unordered )
				flags |= 0x04;

			return flags;
		}

		private static int GetLength( IMessageBuffer data ) {
			if( data == null )
				throw new ArgumentNullException( "data" );

			int length = data.Length + 16;

			if( length > ushort.MaxValue )
				throw new IOException( "The message buffer was too large." );

			return length;
		}

		public override void CopyChunk( byte[] buffer, int offset ) {
			base.CopyChunk( buffer, offset );

			NetworkBitConverter.Copy( _tsn, buffer, offset + 4 );
			NetworkBitConverter.Copy( _streamID, buffer, offset + 8 );
			NetworkBitConverter.Copy( _ssn, buffer, offset + 10 );
			NetworkBitConverter.Copy( _protocol, buffer, offset + 12 );

			_payload.CopyTo( buffer, offset + 16 );
		}

		public IMessageBuffer MessageBuffer {
			get { return _payload; }
		}

		public int Protocol {
			get { return _protocol; }
		}

		public int MillisecondsLifetime {
			get { return -1; }
		}

		public DeliveryOptions Options {
			get { return _options; }
		}
	}
}