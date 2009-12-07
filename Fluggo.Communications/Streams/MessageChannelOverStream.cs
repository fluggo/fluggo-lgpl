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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Fluggo.Communications
{
	/// <summary>
	/// Represents a simple message channel based on an underlying byte <see cref="Stream"/>.
	/// </summary>
	/// <remarks>This stream only preserves the data and the channel ID of each message.
	///   This stream does not reorder messages among channels or handle windowing.</remarks>
	public class MessageChannelOverStream : Channel<IDataMessage>
	{
		Stream _stream;
		const int __headerLength = 8;
		static TraceSource _ts = new TraceSource( "MessageChannelOverStream", SourceLevels.Error );
		bool _canRead, _canWrite;

		public MessageChannelOverStream( Stream stream ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );

			_stream = stream;
			_canRead = stream.CanRead;
			_canWrite = stream.CanWrite;
		}

		public override int MaximumPayloadLength {
			get { return ushort.MaxValue - __headerLength; }
		}
		
		public override int ReceiveWindow {
			get { return MaximumPayloadLength; }
		}

		public override bool CanReceive {
			get { return _canRead; }
		}

		public override bool CanSend {
			get { return _canWrite; }
		}

	#region Read support
		class ReadRequest : BaseAsyncResult {
			byte[] _header = new byte[__headerLength];
			byte[] _messageBody;
			int _headerRead, _messageBodyRead, _messageBodyLength;
			bool _willCompleteSync = true;
			Stream _stream;
			SimpleDataMessage _completedMessage = null;
			bool _endOfStream;

			public ReadRequest( Stream stream, AsyncCallback callback, object state )
				: base( callback, state ) {
				if( stream == null )
					throw new ArgumentNullException( "stream" );

				_stream = stream;
				_stream.BeginRead( _header, 0, __headerLength, HeaderReadCallback, null );
			}

			private void HeaderReadCallback( IAsyncResult result ) {
				// Complete the read that invoked us
				try {
					int readCount = _stream.EndRead( result );
					_headerRead += readCount;
					
					if( !result.CompletedSynchronously )
						_willCompleteSync = false;
						
					if( _headerRead == 0 ) {
						_endOfStream = true;
						Complete( _willCompleteSync );
						return;
					}
				}
				catch( Exception ex ) {
					CompleteError( ex );
					return;
				}
				
				// Invoke another read if we weren't done
				if( _headerRead < __headerLength ) {
					_stream.BeginRead( _header, _headerRead, __headerLength - _headerRead, HeaderReadCallback, null );
					return;
				}

				// Decode parameters
				if( _header[0] != 0 ) {
					_ts.TraceEvent( TraceEventType.Error, 0, "An unknown message type was received." );
					CompleteError( new Exception( "An unknown message type was received." ) );
					return;
				}

				int length = NetworkBitConverter.ToUInt16( _header, 2 );

				_messageBodyLength = length - __headerLength;
				_messageBody = new byte[PadLength( _messageBodyLength )];
				
				// Read the message body along with any padding
				_stream.BeginRead( _messageBody, 0, _messageBody.Length, BodyReadCallback, null );
			}
			
			private void BodyReadCallback( IAsyncResult result ) {
				// Complete the read that invoked us
				try {
					int readCount = _stream.EndRead( result );
					_messageBodyRead += readCount;

					if( !result.CompletedSynchronously )
						_willCompleteSync = false;
				}
				catch( Exception ex ) {
					CompleteError( ex );
					return;
				}

				// Invoke another read if we weren't done
				if( _messageBodyRead < _messageBody.Length ) {
					_stream.BeginRead( _messageBody, _messageBodyRead, _messageBody.Length - _messageBodyRead, BodyReadCallback, null );
					return;
				}

				_completedMessage = new SimpleDataMessage( NetworkBitConverter.ToInt32( _header, 4 ), _messageBody, 0, _messageBodyLength );
				Complete( _willCompleteSync );
			}

			public bool End( out IDataMessage value ) {
				base.End();
				
				if( _endOfStream ) {
					value = null;
					return false;
				}
				else {
					value = _completedMessage;
					return true;
				}
			}
		}

		public override IAsyncResult BeginReceive( AsyncCallback callback, object state ) {
			if( !_canRead )
				throw new NotSupportedException();
			
			return new ReadRequest( _stream, callback, state );
		}

		public override bool EndReceive( IAsyncResult asyncResult, out IDataMessage value ) {
			if( !_canRead )
				throw new NotSupportedException();

			ReadRequest req = (ReadRequest) asyncResult;
			
			if( req.CompletedSynchronously )
				_ts.TraceEvent( TraceEventType.Verbose, 0, "Receive completed synchronously" );
			
			return req.End( out value );
		}

		public override bool Receive( out IDataMessage value ) {
			return EndReceive( BeginReceive( null, null ), out value );
		}
	#endregion

		public override IAsyncResult BeginSend( IDataMessage message, AsyncCallback callback, object state ) {
			if( !_canWrite )
				throw new NotSupportedException();

			if( message == null )
				throw new ArgumentNullException( "message" );

			int length = message.MessageBuffer.Length;

			if( length > MaximumPayloadLength )
				throw new ArgumentException( "The payload is larger than the maximum payload for this path.", "message" );

			// Add room for chunk type, flags, length, and PPI
			length += __headerLength;

			// Mod up for word boundary
			int paddedLength = PadLength( length );

			// Construct message (this is based roughly on the payload data structure of RFC2960)
			byte[] wireMessage = new byte[paddedLength];

			unchecked {
				wireMessage[0] = 0;
				wireMessage[1] = 0;
				NetworkBitConverter.Copy( (ushort) length, wireMessage, 2 );
				NetworkBitConverter.Copy( message.Channel, wireMessage, 4 );
				message.MessageBuffer.CopyTo( wireMessage, __headerLength );
			}

			return _stream.BeginWrite( wireMessage, 0, wireMessage.Length, callback, state );
		}

		public override void Send( IDataMessage message ) {
			EndSend( BeginSend( message, null, null ) );
		}

		public override void EndSend( IAsyncResult result ) {
			_stream.EndWrite( result );
		}

		public override void Close() {
			_stream.Close();
			_canRead = false;
			_canWrite = false;
		}

		private static int PadLength( int length ) {
			if( (length & 3) != 0 )
				return length + (4 - (length & 3));
			else
				return length;
		}
	}
}