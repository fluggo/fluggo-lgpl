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
using System.IO;
using System.Threading;

namespace Fluggo.Communications {
	/// <summary>
	/// Represents a read-only stream bound to a <see cref="Channel"/>.
	/// </summary>
	/// <remarks>This class reads a stream of bytes from the message source until a zero-length message is received.</remarks>
	public class ReadStreamOverChannel : Stream {
		Channel _channel;

		/// <summary>
		/// Creates a new instance of the <see cref='ReadStreamOverChannel'/> class.
		/// </summary>
		/// <param name="channel"><see cref="Channel"/> used for receiving messages.</param>
		/// <exception cref='ArgumentNullException'><paramref name='channel'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="channel"/> is closed or does not support receiving messages.</exception>
		public ReadStreamOverChannel( Channel channel ) {
			if( channel == null )
				throw new ArgumentNullException( "channel" );
				
			if( !channel.CanReceive )
				throw new ArgumentException( "The given channel is closed or does not support receiving messages." );

			_channel = channel;
			_readQueue = new ProcessingQueue<ReadAsyncResult>( HandleReadRequest );
		}

		#region Read support
		/*
		 * How this side works
		 * 
		 * Here, we have one outstanding Receive request at a time. When the last bit of data has been read
		 * from the current read buffer, the _readBuffer is set to null. On the next read request, a new receive
		 * request is made, and the receive callback handles putting data into ReadAsyncResults correctly. If
		 * the callback runs out of data and there are more queue requests waiting, it queues another request.
		 */

		ProcessingQueue<ReadAsyncResult> _readQueue;
		volatile bool _readClosed, _disposed;
		IMessageBuffer _readBuffer = null;
		int _readCount = 0;
		IAsyncResult _lastRead = null;

		class ReadAsyncResult : BaseAsyncResult
		{
			byte[] _buffer;
			int _offset, _count;
			int _readCount = -1;
			bool willCompleteSync = true;

			public ReadAsyncResult( byte[] buffer, int offset, int count, AsyncCallback callback, object state )
				: base( callback, state ) {
				new ArraySegment<byte>( buffer, offset, count );

				_buffer = buffer;
				_offset = offset;
				_count = count;
			}

			public void GoAsync() {
				willCompleteSync = false;
			}

			public int ReadCount {
				get {
					if( _readCount == -1 )
						throw new InvalidOperationException();

					return _readCount;
				}
			}

			/// <summary>
			/// Writes as much of <paramref name="count"/> as is possible into the read buffer.
			/// </summary>
			/// <param name="sourceBuffer"></param>
			/// <param name="offset"></param>
			/// <param name="count"></param>
			public void Write( IMessageBuffer sourceBuffer, int offset, int count ) {
				if( count < 0 || _readCount != -1 )
					throw new InvalidOperationException();

				if( count != 0 ) {
					_readCount = Math.Min( count, _count );
					sourceBuffer.CopyTo( offset, _buffer, _offset, _readCount );
				}
				else {
					_readCount = 0;
				}

				Complete( willCompleteSync );
			}

			public new int End() {
				base.End();
				return _readCount;
			}
		}

		private WaitHandle HandleReadRequest( ReadAsyncResult result, bool async ) {
			for( int i = 0; i < 2; i++ ) {
				if( _lastRead != null ) {
					try {
						IMessageBuffer message = _channel.EndReceive( _lastRead );
						
						if( message != null && message.Length != 0 ) {
							_readBuffer = message;
							_readCount = 0;
						}
						else {
							// Complete with closure
							result.Write( null, 0, 0 );
							_readClosed = true;
							return null;
						}
					}
					catch( Exception ex ) {
						result.CompleteError( ex );
						return null;
					}
					finally {
						_lastRead = null;
					}
				}

				if( async )
					result.GoAsync();

				if( _readClosed ) {
					result.Write( null, 0, 0 );
					return null;
				}

				// Try to satisfy it with what's here
				if( _readBuffer != null ) {
					result.Write( _readBuffer, _readCount, _readBuffer.Length - _readCount );
					_readCount += result.ReadCount;

					if( _readCount == _readBuffer.Length )
						_readBuffer = null;

					return null;
				}

				if( _lastRead == null ) {
					try {
						_lastRead = _channel.BeginReceive( null, null );

						if( !_lastRead.IsCompleted )
							return _lastRead.AsyncWaitHandle;
					}
					catch( Exception ex ) {
						result.CompleteError( ex );
						return null;
					}
				}
			}

			throw new UnexpectedException();
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="BeginRead"]/*'/>
		public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( _disposed )
				throw new ObjectDisposedException( null );
			
			ReadAsyncResult result = new ReadAsyncResult( buffer, offset, count, callback, state );
			_readQueue.Enqueue( result );
			return result;
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="EndRead"]/*'/>
		public override int EndRead( IAsyncResult asyncResult ) {
			return ((ReadAsyncResult) asyncResult).End();
		}
		#endregion

		#region Basic stream support
		public override bool CanRead {
			get { return !_disposed; }
		}

		/// <include file='Common.xml' path='/root/Stream/property[@name="CanSeek:never"]/*'/>
		public override bool CanSeek {
			get { return false; }
		}

		/// <include file='Common.xml' path='/root/Stream/property[@name="CanSeek:never"]/*'/>
		public override bool CanWrite {
			get { return false; }
		}

		public override void Flush() {
			//throw new NotImplementedException();
		}

		public override long Length {
			get { throw new NotSupportedException(); }
		}

		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="Read"]/*'/>
		public override int Read( byte[] buffer, int offset, int count ) {
			return EndRead( BeginRead( buffer, offset, count, null, null ) );
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			throw new NotSupportedException();
		}

		public override void SetLength( long value ) {
			throw new NotSupportedException();
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="Write"]/*'/>
		public override void Write( byte[] buffer, int offset, int count ) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Closes the stream.
		/// </summary>
		/// <remarks>No notification is sent to the sending end to signal that the read stream has closed.
		///     Therefore, any stream bound to the underlying transport after this method is called can read
		///     messages that might have been intended for this stream.
		///   <para>The stream will automatically stop reading when a zero-length message is received from the other side.
		///     When this happens, any reads in progress will read buffered data, and then begin to return zero. Closing the
		///     stream causes the same thing to happen, but also begins to raise exceptions on new reads.</para></remarks>
		public override void Close() {
			base.Close();
			
			_readClosed = true;
			_disposed = true;
		}
		#endregion
	}

	/// <summary>
	/// Represents a write-only stream bound to a <see cref="Channel"/>.
	/// </summary>
	/// <remarks>This class writes a stream of bytes to a message channel using the Nagle algorithm. When it is closed, a zero-length message is sent.</remarks>
	public class WriteStreamOverChannel : Stream {
		Channel _channel;

		/// <summary>
		/// Creates a new instance of the <see cref='WriteStreamOverChannel'/> class.
		/// </summary>
		/// <param name="channel"><see cref="Channel"/> used for sending messages.</param>
		/// <exception cref='ArgumentNullException'><paramref name='channel'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="channel"/> is closed or does not support sending messages.</exception>
		public WriteStreamOverChannel( Channel channel ) {
			if( channel == null )
				throw new ArgumentNullException( "channel" );
				
			if( !channel.CanSend )
				throw new ArgumentException( "The given channel is closed or does not support sending messages." );

			_channel = channel;
			_writeBuffer = new byte[_channel.MaximumPayloadLength];
		}

		#region Write support
		/*
		 *	How this is supposed to work:
		 * 
		 * BeginWrite immediately writes data into the local buffer and enters a loop. Every time the local buffer becomes full, a
		 * send call is dispatched, its IAsyncResult is stored in _lastSend, a new write buffer is created,
		 * and the loop is restarted. If the buffer does not become full, and is not empty, but _lastSend is null or has completed,
		 * a send is dispatched just as above.
		 * 
		 * When _lastSend completes (and a check should be made to verify that the completion call does indeed refer
		 * to _lastSend) it should send off any data in the buffer, then set _lastSend to null.
		 */
		object _writeLock = new object();
		byte[] _writeBuffer;
		int _writeCount;
		IAsyncResult _lastSend;
		bool _writeClosed;

		private IAsyncResult DoSend( AsyncCallback callback, object state ) {
			if( _writeCount == 0 )
				throw new InvalidOperationException( "DoSend was called without any data in the buffer." );

			lock( _writeLock ) {
				WriteAsyncResult myResult = new WriteAsyncResult( this, callback, state );
				_lastSend = myResult;

				byte[] buffer = _writeBuffer;
				int count = _writeCount;
				_writeBuffer = new byte[_channel.MaximumPayloadLength];
				_writeCount = 0;

				_channel.BeginSend( buffer, 0, count, myResult.HandleWriteCompleted, null );

				return myResult;
			}
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="BeginWrite"]/*'/>
		public override IAsyncResult BeginWrite( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			new ArraySegment<byte>( buffer, offset, count );
			IAsyncResult returnValue = null;

			lock( _writeLock ) {
				if( _writeClosed )
					throw new ObjectDisposedException( null );

				if( count == 0 )
					return new EmptyWriteAsyncResult( callback, state );

				for( ; ; ) {
					int writableCount = Math.Min( count, _writeBuffer.Length - _writeCount );
					Buffer.BlockCopy( buffer, offset, _writeBuffer, _writeCount, writableCount );

					offset += writableCount;
					count -= writableCount;
					_writeCount += writableCount;

					// If this write filled the buffer
					if( _writeCount == _writeBuffer.Length ) {
						if( count == 0 ) {
							// This is the last send operation, since there is no more data
							return DoSend( callback, state );
						}
						if( count < _writeBuffer.Length ) {
							// This is the last send operation we will do, since the next loop will not fill the buffer
							// Invoke send in such a way as to make this IAsyncResult the return value
							returnValue = DoSend( callback, state );
						}
						else {
							// Send the buffer, but since we're not done, don't return the IAsyncResult
							DoSend( null, null );
						}
					}
					else if( _writeCount != 0 && _lastSend == null ) {
						// There's data, and we've got a go-ahead from Nagle
						return DoSend( callback, state );
					}
					else {
						// The data is recorded, but Nagle held it back
						// Return any valid IAsyncResult we have
						return (returnValue == null) ? new EmptyWriteAsyncResult( callback, state ) : returnValue;
					}
				}
			}
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="EndWrite"]/*'/>
		public override void EndWrite( IAsyncResult asyncResult ) {
			EmptyWriteAsyncResult empty = asyncResult as EmptyWriteAsyncResult;

			if( empty == null ) {
				WriteAsyncResult myResult = (WriteAsyncResult) asyncResult;
				myResult.End();
			}
		}

		class EmptyWriteAsyncResult : BaseAsyncResult
		{
			public EmptyWriteAsyncResult( AsyncCallback callback, object state )
				: base( callback, state ) {
				Complete( true );
			}
		}

		class WriteAsyncResult : BaseAsyncResult
		{
			WriteStreamOverChannel _owner;

			public WriteAsyncResult( WriteStreamOverChannel stream, AsyncCallback callback, object state )
				: base( callback, state ) {
				if( stream == null )
					throw new ArgumentNullException( "stream" );

				_owner = stream;
			}

			public void HandleWriteCompleted( IAsyncResult result ) {
				try {
					// Only attempt the lock if there's a chance we're the last
					if( _owner._lastSend == this ) {
						lock( _owner._writeLock ) {
							// If we were the last group to send...
							if( _owner._lastSend == this ) {
								// ...record that there are no more pending sends
								_owner._lastSend = null;

								// ...and send off any data that has been written
								if( _owner._writeCount != 0 )
									_owner.DoSend( null, null );
							}
						}
					}

					_owner._channel.EndSend( result );
					Complete( result.CompletedSynchronously );
				}
				catch( Exception ex ) {
					CompleteError( ex );
				}
			}

			public new void End() {
				base.End();
			}
		}

		public override void Close() {
			lock( _writeLock ) {
				if( _writeCount != 0 )
					DoSend( null, null );

				_channel.BeginSend( new byte[0], 0, 0, null, null );
				_writeClosed = true;
			}
		}
		#endregion

		#region Basic stream support
		/// <include file='Common.xml' path='/root/Stream/property[@name="CanRead:never"]/*'/>
		public override bool CanRead {
			get { return false; }
		}

		/// <include file='Common.xml' path='/root/Stream/property[@name="CanSeek:never"]/*'/>
		public override bool CanSeek {
			get { return false; }
		}

		/// <include file='Common.xml' path='/root/Stream/property[@name="CanWrite:always"]/*'/>
		public override bool CanWrite {
			get { return !_writeClosed; }
		}

		public override void Flush() {
			//throw new NotImplementedException();
		}

		public override long Length {
			get { throw new NotSupportedException(); }
		}

		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			throw new NotSupportedException();
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			throw new NotSupportedException();
		}

		public override void SetLength( long value ) {
			throw new NotSupportedException();
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="Write"]/*'/>
		public override void Write( byte[] buffer, int offset, int count ) {
			EndWrite( BeginWrite( buffer, offset, count, null, null ) );
		}
		#endregion
	}
	
	public class StreamOverChannel : Stream {
		// BJC: This is essentially a convenient duplicate of RedirectStream
		ReadStreamOverChannel _readStream;
		WriteStreamOverChannel _writeStream;

		/// <summary>
		/// Creates a new instance of the <see cref='StreamOverChannel'/> class.
		/// </summary>
		/// <param name="channel"><see cref="Channel"/> used for sending and receiving messages.</param>
		/// <exception cref='ArgumentNullException'><paramref name='channel'/> is <see langword='null'/>.</exception>
		/// <remarks>If <paramref name="channel"/> supports reading, read support will be available on the stream,
		///   and vice-versa. Seeking is never supported over a channel.</remarks>
		public StreamOverChannel( Channel channel ) {
			if( channel == null )
				throw new ArgumentNullException( "channel" );
				
			if( channel.CanReceive )
				_readStream = new ReadStreamOverChannel( channel );
				
			if( channel.CanSend )
				_writeStream = new WriteStreamOverChannel( channel );
		}

	#region Basic stream support
		/// <include file='Common.xml' path='/root/Stream/property[@name="CanSeek:always"]/*'/>
		public override bool CanRead {
			get { return _readStream != null && _readStream.CanRead; }
		}

		/// <include file='Common.xml' path='/root/Stream/property[@name="CanSeek:never"]/*'/>
		public override bool CanSeek {
			get { return false; }
		}

		/// <include file='Common.xml' path='/root/Stream/property[@name="CanSeek:always"]/*'/>
		public override bool CanWrite {
			get { return _writeStream != null && _writeStream.CanWrite; }
		}

		public override void Flush() {
			if( _writeStream == null )
				throw new NotSupportedException();
			
			_writeStream.Flush();
		}

		public override long Length {
			get { throw new NotSupportedException(); }
		}

		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="BeginRead"]/*'/>
		public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( _readStream == null )
				throw new NotSupportedException();
			
			return _readStream.BeginRead( buffer, offset, count, callback, state );
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="EndRead"]/*'/>
		public override int EndRead( IAsyncResult asyncResult ) {
			if( _readStream == null )
				throw new NotSupportedException();

			return _readStream.EndRead( asyncResult );
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="Read"]/*'/>
		public override int Read( byte[] buffer, int offset, int count ) {
			if( _readStream == null )
				throw new NotSupportedException();

			return EndRead( BeginRead( buffer, offset, count, null, null ) );
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			throw new NotSupportedException();
		}

		public override void SetLength( long value ) {
			throw new NotSupportedException();
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="BeginWrite"]/*'/>
		public override IAsyncResult BeginWrite( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( _writeStream == null )
				throw new NotSupportedException();

			return _writeStream.BeginWrite( buffer, offset, count, callback, state );
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="EndWrite"]/*'/>
		public override void EndWrite( IAsyncResult asyncResult ) {
			if( _writeStream == null )
				throw new NotSupportedException();

			_writeStream.EndWrite( asyncResult );
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="Write"]/*'/>
		public override void Write( byte[] buffer, int offset, int count ) {
			if( _writeStream == null )
				throw new NotSupportedException();

			EndWrite( BeginWrite( buffer, offset, count, null, null ) );
		}

		public override void Close() {
			base.Close();
			
			if( _writeStream != null )
				_writeStream.Close();

			if( _readStream != null )
				_readStream.Close();
		}

		protected override void Dispose( bool disposing ) {
			base.Dispose( disposing );
			
			if( disposing ) {
				if( _writeStream != null )
					_writeStream.Dispose();
					
				if( _readStream != null )
					_readStream.Dispose();
			}
		}
	#endregion
	}
}