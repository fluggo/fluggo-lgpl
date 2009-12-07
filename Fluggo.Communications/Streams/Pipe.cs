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
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Fluggo.Communications
{
	/// <summary>
	/// Represents a stream that reads from itself.
	/// </summary>
	public class Pipe : Stream {
		byte[] _buffer;
		AutoResetEvent _writeUpdateWaitingEvent = new AutoResetEvent( false ), _readUpdateWaitingEvent = new AutoResetEvent( false );
		ProcessingQueue<ReadRequest> _readQueue;
		ProcessingQueue<WriteRequest> _writeQueue;
		static TraceSource _ts = new TraceSource( "Pipe", SourceLevels.Error );
		
	#region Read/write pointers
		const int __ptrFlag = unchecked((int)0x80000000U);		// empty or full
		int _readPtr = __ptrFlag, _writePtr = 0;
		static readonly int __procCount = Environment.ProcessorCount;
		
		// The way this works:
		// The write pointer is always "ahead" of the read pointer in the sense that
		// when the read pointer catches up to the write pointer, the reader cannot
		// continue. This is true of all streams; and in this stream, whenever the
		// pointers differ, the write pointer is ahead.
		//
		// Unlike an ordinary stream, though, the write pointer can also catch up
		// to the read pointer. This means that when the pointers coincide, the buffer
		// can be either full or empty, and there must be a way to distinguish between
		// the two states.
		//
		// In a multithreaded situation, multiple variables can be guarded and updated
		// simultaneously to avoid this ambiguity. Synchronization primitives, however,
		// usually waste time with context switches and unused time slices. This stream
		// uses a method that avoids most of these problems.
		//
		// The read pointer and the write pointer are contained in separate variables
		// and can be updated independently. Only one reader and one writer may be active
		// at any time, but they can work independently. Each pointer contains a flag
		// that helps disambiguate the pointer-coincide situation. For the read pointer,
		// this flag is the empty-flag, and the write pointer has the full-flag. Each are
		// contained in the topmost bit of the variable.
		//
		// These flags are hints that the reader or writer was starved at the end of their
		// last read or write. For example, the reader may determine that there are 12 bytes
		// available in the buffer and reads them all. Regardless of how many bytes are
		// available at the end of the read operation (because of intervening write operations)
		// the reader is moving the read pointer as far as it can, so it sets its empty
		// hint flag. The writer does the same when writing the write pointer. Each has
		// to guard against one special case, described below.
		//
		// Whenever the actual pointer values differ, the flags can be ignored, because
		// the write pointer is always ahead of the read pointer. When they are the same,
		// the flags can be used to distinguish several situations. If the pointers both
		// point to byte 6, for example:
		//
		//   6,  6    It is impossible for both flags to be off. For one pointer to reach the
		//            other, the reader/writer that advanced their pointer must have known to
		//            stop at the other pointer, and if they knew that, they would have set
		//            their flag. The only way both flags can be off is if the pointers
		//            differ.
		//
		//   6E, 6    The buffer is empty. The reader reached the end of the buffer and set
		//            its flag. Since the writer hasn't moved the write pointer since, the
		//            buffer must be empty.
		//
		//   6,  6F   The buffer is full. The writer reached the end of the buffer and set
		//			  its flag. Since the reader hasn't moved the read pointer since, the
		//            buffer must be full.
		//
		//   6E, 6F   Intermediate state. This can happen, for example, when the buffer starts
		//            as full, and then the reader reads the entire buffer all at once.
		//            When it reaches the write pointer again, it sets its own flag. However,
		//            the same thing could happen in the reverse situation, so this is
		//            ambiguous. Therefore, the reader must detect when the full-flag is set
		//            and reset it. It does this by writing its value first (creating the
		//            intermediate state) and then writing the write pointer with the flag
		//            cleared. It's okay for the reader to write the write pointer in this
		//            situation because the writer cannot run in the 6, 6F or 6E, 6F situations.
		//            If the writer happens to see the 6E, 6F configuration, it must go back
		//            and re-read the pointers until at least one flag is cleared. This is the
		//            only situation where a stall may be necessary. The reverse of this example
		//            is also true.

		private void GetNextReadableRegion( out int offset, out int length ) {
			// Read the pointers
			int readPtr, writePtr;

			writePtr = Thread.VolatileRead( ref _writePtr );

			for( ; ; ) {
				readPtr = Thread.VolatileRead( ref _readPtr );

				if( (readPtr & writePtr & __ptrFlag) != __ptrFlag )
					break;

				// Both pointers have their flags on. This can't have been caused by the
				// previous reader, or else he would have turned off the write flag.
				// Therefore, we just need to wait for the read flag to be cleared.
				// Which probably happened while we were talking here.

				_ts.TraceEvent( TraceEventType.Information, 0, "Both buffer pointer flags on, stalling the read..." );
				Stall();
			}

			bool emptyFlag = (readPtr & __ptrFlag) == __ptrFlag;
			bool fullFlag = (writePtr & __ptrFlag) == __ptrFlag;
			readPtr &= ~__ptrFlag;
			writePtr &= ~__ptrFlag;
			
			offset = readPtr;

			if( readPtr == writePtr ) {
				if( fullFlag ) {
					// Buffer is full
					length = _buffer.Length;
				}
				else if( emptyFlag ) {
					// Buffer is empty
					length = 0;
				}
				else {
					_ts.TraceEvent(TraceEventType.Error, 0, "The read and write pointers were equal, but neither flag was on." );
					throw new UnexpectedException();
				}
			}
			else if( readPtr > writePtr ) {
				// It's the long way around
				// 012345 = length 6
				//  W  R  = 3 bytes readable (4,5,0)
				length = _buffer.Length - readPtr + writePtr;
			}
			else {
				// It's the short way
				// 012345 = length 6
				//  R  W  = 3 bytes readable (1,2,3)
				length = writePtr - readPtr;
			}
		}
		
		private void GetNextWritableRegion( out int offset, out int length ) {
			// Read the pointers
			int readPtr, writePtr;

			readPtr = Thread.VolatileRead( ref _readPtr );

			for( ; ; ) {
				writePtr = Thread.VolatileRead( ref _writePtr );

				if( (readPtr & writePtr & __ptrFlag) != __ptrFlag )
					break;

				// Both pointers have their flags on. This can't have been caused by the
				// previous writer, or else he would have turned off the read flag.
				// Therefore, we just need to wait for the write flag to be cleared.
				// Which probably happened while we were talking here.

				_ts.TraceEvent( TraceEventType.Information, 0, "Both buffer pointer flags on, stalling the write..." );
				Stall();
			}

			bool emptyFlag = (readPtr & __ptrFlag) == __ptrFlag;
			bool fullFlag = (writePtr & __ptrFlag) == __ptrFlag;
			readPtr &= ~__ptrFlag;
			writePtr &= ~__ptrFlag;

			offset = writePtr;

			if( readPtr == writePtr ) {
				if( fullFlag ) {
					// Buffer is full
					length = 0;
				}
				else if( emptyFlag ) {
					// Buffer is empty
					length = _buffer.Length;
				}
				else {
					_ts.TraceEvent( TraceEventType.Error, 0, "The read and write pointers were equal, but neither flag was on." );
					throw new UnexpectedException();
				}
			}
			else if( readPtr > writePtr ) {
				// It's the short way
				// 012345 = length 6
				//  W  R  = 3 bytes writeable (1,2,3)
				length = readPtr - writePtr;
			}
			else {
				// It's the long way around
				// 012345 = length 6
				//  R  W  = 3 bytes writeable (4,5,0)
				length = _buffer.Length - writePtr + readPtr;
			}
		}
		
		private static void Stall() {
			// BJC: This is a nifty trick. First off, why a stall? Well, we were given a specific
			// timeslice when our thread was chosen to run. It'd be a shame to give up the rest of
			// that timeslice if we only have a few milliseconds to wait for another processor to
			// fix our stall condition.
			//
			// So we wait, and that's the purpose of Thread.SpinWait. But wait! If there is only
			// one processor in the system, waiting doesn't do any good. There are no other processors
			// that can fix our condition. We're sitting on the only one. The best thing to do then
			// *is* to give up our timeslice and not waste the rest of it waiting for something that
			// can't happen.
			if( __procCount == 1 ) {
				Thread.Sleep( 0 );
			}
			else {
				Thread.SpinWait( 1 );
			}
		}
		
		private void AdvanceReadPointer( int newPosition, bool isEmpty, bool wasFull ) {
			// Wrap around to beginning if at the end; this is critical for
			// the part that determines whether the buffer is empty or full
			if( newPosition == _buffer.Length )
				newPosition = 0;
			
			Thread.VolatileWrite( ref _readPtr, newPosition | (isEmpty ? __ptrFlag : 0) );
			
			// Clear the full flag if both are now on; unfortunately, this may happen often
			if( isEmpty ) {
				int writePtr = Thread.VolatileRead( ref _writePtr );
				
				if( (writePtr & __ptrFlag) == __ptrFlag )
					Thread.VolatileWrite( ref _writePtr, writePtr & ~__ptrFlag );
			}
		}
		
		private void AdvanceWritePointer( int newPosition, bool isFull, bool wasEmpty ) {
			// Wrap around to beginning if at the end; this is critical for
			// the part that determines whether the buffer is empty or full
			if( newPosition == _buffer.Length )
				newPosition = 0;

			Thread.VolatileWrite( ref _writePtr, newPosition | (isFull ? __ptrFlag : 0) );

			if( isFull ) {
				int readPtr = Thread.VolatileRead( ref _readPtr );

				if( (readPtr & __ptrFlag) == __ptrFlag )
					Thread.VolatileWrite( ref _readPtr, readPtr & ~__ptrFlag );
			}
		}
	#endregion
		
		/// <summary>
		/// Creates a new instance of the <see cref='Pipe'/> class.
		/// </summary>
		/// <param name="length">Size of the buffer. This is the maximum amount of data, in bytes, that can be stored
		///	  between the read and write pointers.</param>
		public Pipe( int length ) {
			_buffer = new byte[length];
			
			_readQueue = new ProcessingQueue<ReadRequest>( ReadQueueHandler );
			_writeQueue = new ProcessingQueue<WriteRequest>( WriteQueueHandler );
		}

		public override void Close() {
			_readQueue.Close();
			_writeQueue.Close();
		}

	#region Convenience methods
		/// <summary>
		/// Creates a pair of streams that communicate to each other.
		/// </summary>
		/// <param name="bufferSize">Number of bytes that can be buffered in a stream before in blocks.
		///   Each stream blocks independently of the other.</param>
		/// <param name="firstStream">Reference to a variable. On return, this contains a reference to the
		///   first stream created.</param>
		/// <param name="secondStream">Reference to a variable. On return, this contains a reference to the
		///   second stream created.</param>
		/// <remarks>Writes to the first stream can be read from the second stream, and vice versa. This is an effective
		///   way of creating an in-process loopback, similar to opening a pipe for in-process communication.</remarks>
		public static void CreateLoopback( int bufferSize, out Stream firstStream, out Stream secondStream ) {
			Stream s1 = new Pipe( bufferSize ), s2 = new Pipe( bufferSize );

			firstStream = new RedirectStream( s1, s2 );
			secondStream = new RedirectStream( s2, s1 );
		}
		
		const int __defaultRedirectBufferSize = 512;

		/// <summary>
		/// Redirects one stream to another.
		/// </summary>
		/// <param name="source"><see cref="Stream"/> from which to copy data.</param>
		/// <param name="target"><see cref="Stream"/> to which to copy data.</param>
		/// <returns>A <see cref="StreamRedirection"/> object which represents the redirection. Call <see cref="IDisposable.Dispose"/>
		///   to end the redirection.</returns>
		/// <remarks>Data is copied from <paramref name="source"/> to <paramref name="target"/> until the source stream ends, one of
		///   the streams is closed, or <see cref="IDisposable.Dispose"/> is called on the returned object.</remarks>
		public static StreamRedirection Redirect( Stream source, Stream target ) {
			return Redirect( source, target, __defaultRedirectBufferSize, true );
		}

		/// <summary>
		/// Redirects one stream to another.
		/// </summary>
		/// <param name="source"><see cref="Stream"/> from which to copy data.</param>
		/// <param name="target"><see cref="Stream"/> to which to copy data.</param>
		/// <param name="bufferSize">Size of the buffers allocated for the redirection. At any given time, at least one buffer of
		///   this size will be allocated.</param>
		/// <returns>A <see cref="StreamRedirection"/> object which represents the redirection. Call <see cref="IDisposable.Dispose"/>
		///   to end the redirection.</returns>
		/// <remarks>Data is copied from <paramref name="source"/> to <paramref name="target"/> until the source stream ends, one of
		///   the streams is closed, or <see cref="IDisposable.Dispose"/> is called on the returned object.</remarks>
		public static StreamRedirection Redirect( Stream source, Stream target, int bufferSize ) {
			return Redirect( source, target, bufferSize, true );
		}

		/// <summary>
		/// Redirects one stream to another.
		/// </summary>
		/// <param name="source"><see cref="Stream"/> from which to copy data.</param>
		/// <param name="target"><see cref="Stream"/> to which to copy data.</param>
		/// <param name="bufferSize">Size of the buffers allocated for the redirection. At any given time, at least one buffer of
		///   this size will be allocated.</param>
		/// <param name="autoFlush">True to flush the <paramref name="target"/> stream at the end of every write, false otherwise.</param>
		/// <returns>A <see cref="StreamRedirection"/> object which represents the redirection. Call <see cref="IDisposable.Dispose"/>
		///   to end the redirection.</returns>
		/// <remarks>Data is copied from <paramref name="source"/> to <paramref name="target"/> until the source stream ends, one of
		///   the streams is closed, or <see cref="IDisposable.Dispose"/> is called on the returned object.</remarks>
		public static StreamRedirection Redirect( Stream source, Stream target, int bufferSize, bool autoFlush ) {
			return new StreamRedirection( source, target, bufferSize, autoFlush );
		}

		/// <summary>
		/// Synchronously copies one stream to another.
		/// </summary>
		/// <param name="source"><see cref="Stream"/> from which to copy data.</param>
		/// <param name="target"><see cref="Stream"/> to which to copy data.</param>
		/// <returns>The number of bytes copied.</returns>
		/// <remarks>Data is copied from <paramref name="source"/> to <paramref name="target"/> until the source stream ends or one of
		///   the streams is closed.</remarks>
		/// <exception cref='ArgumentNullException'><paramref name='source'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='target'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='ArgumentException'><paramref name='source'/> cannot be read.
		///   <para>— OR —</para>
		///   <para><paramref name='target'/> cannot be written.</para></exception>
		public static long Copy( Stream source, Stream target ) {
			return Copy( source, target, -1L, __defaultRedirectBufferSize );
		}

		/// <summary>
		/// Synchronously copies one stream to another.
		/// </summary>
		/// <param name="source"><see cref="Stream"/> from which to copy data.</param>
		/// <param name="target"><see cref="Stream"/> to which to copy data.</param>
		/// <param name="count">The number of bytes to copy. Specify -1 to copy data until the stream ends.</param>
		/// <returns>The number of bytes copied.</returns>
		/// <remarks>Data is copied from <paramref name="source"/> to <paramref name="target"/> until the source stream ends, one of
		///   the streams is closed, or <paramref name="count"/> bytes have been copied.</remarks>
		/// <exception cref='ArgumentNullException'><paramref name='source'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='target'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='ArgumentException'><paramref name='source'/> cannot be read.
		///   <para>— OR —</para>
		///   <para><paramref name='target'/> cannot be written.</para></exception>
		public static long Copy( Stream source, Stream target, long count ) {
			return Copy( source, target, count, __defaultRedirectBufferSize );
		}

		/// <summary>
		/// Synchronously copies one stream to another.
		/// </summary>
		/// <param name="source"><see cref="Stream"/> from which to copy data.</param>
		/// <param name="target"><see cref="Stream"/> to which to copy data.</param>
		/// <param name="count">The number of bytes to copy. Specify -1 to copy data until the stream ends.</param>
		/// <param name="bufferSize">The size of the buffer used to copy the data.</param>
		/// <returns>The number of bytes copied.</returns>
		/// <remarks>Data is copied from <paramref name="source"/> to <paramref name="target"/> until the source stream ends, one of
		///   the streams is closed, or <paramref name="count"/> bytes have been copied.</remarks>
		/// <exception cref='ArgumentNullException'><paramref name='source'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='target'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='ArgumentException'><paramref name='source'/> cannot be read.
		///   <para>— OR —</para>
		///   <para><paramref name='target'/> cannot be written.</para></exception>
		public static long Copy( Stream source, Stream target, long count, int bufferSize ) {
			if( source == null )
				throw new ArgumentNullException( "source" );

			if( target == null )
				throw new ArgumentNullException( "target" );
				
			if( !source.CanRead )
				throw new ArgumentException( "The source stream cannot be read.", "source" );
				
			if( !target.CanWrite )
				throw new ArgumentException( "The target stream cannot be written.", "target" );

			if( count == 0 )
				return 0;

			if( bufferSize < 8 )
				bufferSize = 8;

			byte[] buffer = new byte[bufferSize];
			int readCount, nextReadLength;
			long totalReadCount = 0;
			
			if( count < 0 )
				nextReadLength = bufferSize;
			else
				nextReadLength = (int) Math.Min( count, buffer.Length );
				
			try {
				while( (readCount = source.Read( buffer, 0, nextReadLength )) != 0 ) {
					target.Write( buffer, 0, readCount );
					unchecked { totalReadCount += readCount; }
					count -= readCount;
					
					if( count == 0 )
						return totalReadCount;

					if( count < 0 )
						nextReadLength = bufferSize;
					else
						nextReadLength = (int) Math.Min( count, buffer.Length );
				}
			}
			catch( ObjectDisposedException ) {
			}
			
			return totalReadCount;
		}
	#endregion

	#region Standard stream support
		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override void Flush() {
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

		public override long Seek( long offset, SeekOrigin origin ) {
			throw new NotSupportedException();
		}

		public override void SetLength( long value ) {
			throw new NotSupportedException();
		}
	#endregion

	#region Read support
		class ReadRequest : BaseAsyncResult
		{
			byte[] _readBuffer;
			int _offset, _count, _readCount;
			bool _willCompleteSync = true;
			Pipe _stream;

			public ReadRequest( Pipe stream, byte[] buffer, int offset, int count, AsyncCallback callback, object state )
				: base( callback, state ) {
				if( stream == null )
					throw new ArgumentNullException( "stream" );

				_stream = stream;

				new ArraySegment<byte>( buffer, offset, count );

				_readBuffer = buffer;
				_offset = offset;
				_count = count;

				if( count == 0 )
					Complete( true );
			}

			public void GoAsync() {
				_willCompleteSync = false;
			}

			public WaitHandle TryRead() {
				if( _count == 0 )
					return null;

				int ptrOffset, readableLength, newPtrOffset;
				_stream.GetNextReadableRegion( out ptrOffset, out readableLength );
				
				if( readableLength == 0 ) {
					if( _stream._writeUpdateWaitingEvent.WaitOne( 0, false ) ) {
						// We may have moved
						_stream.GetNextReadableRegion( out ptrOffset, out readableLength );
						
						if( readableLength == 0 ) {
							_ts.TraceEvent( TraceEventType.Verbose, 0, "Buffer is empty while reading, waiting for a write" );
							return _stream._writeUpdateWaitingEvent;
						}
					}
					else {
						// We didn't move
						_ts.TraceEvent( TraceEventType.Verbose, 0, "Buffer is empty while reading, waiting for a write" );
						return _stream._writeUpdateWaitingEvent;
					}
				}

				// What we will be able to read this time around?
				int length = Math.Min( readableLength, _count );

				// Read as much as we can to the end
				int firstRead = Math.Min( length, _stream._buffer.Length - ptrOffset );
				Buffer.BlockCopy( _stream._buffer, ptrOffset, _readBuffer, _offset, firstRead );

				// Now try the second read
				if( firstRead != length ) {
					Buffer.BlockCopy( _stream._buffer, 0, _readBuffer, _offset + firstRead, length - firstRead );
					newPtrOffset = length - firstRead;
				}
				else {
					// No second read necessary
					newPtrOffset = ptrOffset + firstRead;
				}
					
				_stream.AdvanceReadPointer( newPtrOffset, readableLength == length, _stream._buffer.Length == readableLength );

				_stream._readUpdateWaitingEvent.Set();
				_readCount = length;

				_ts.TraceEvent( TraceEventType.Information, 0, "Read completed " + (_willCompleteSync ? "synchronously" : "asynchronously") );
				Complete( _willCompleteSync );

				return null;
			}

			public new int End() {
				base.End();
				return _readCount;
			}
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="BeginRead"]/*'/>
		public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			ReadRequest req = new ReadRequest( this, buffer, offset, count, callback, state );
			
			if( count != 0 )
				_readQueue.Enqueue( req );

			return req;
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="EndRead"]/*'/>
		public override int EndRead( IAsyncResult asyncResult ) {
			ReadRequest req = (ReadRequest) asyncResult;
			return req.End();
		}

		private WaitHandle ReadQueueHandler( ReadRequest request, bool async ) {
			if( async )
				request.GoAsync();

			WaitHandle handle = request.TryRead();
			
			return handle;
		}
		
		public override int Read( byte[] buffer, int offset, int count ) {
			return EndRead( BeginRead( buffer, offset, count, null, null ) );
		}
	#endregion

	#region Write support
		class WriteRequest : BaseAsyncResult
		{
			byte[] _writeBuffer;
			int _offset, _count;
			bool _willCompleteSync = true;
			Pipe _stream;
			
			public WriteRequest( Pipe stream, byte[] buffer, int offset, int count, AsyncCallback callback, object state ) : base( callback, state ) {
				if( stream == null )
					throw new ArgumentNullException( "stream" );
					
				_stream = stream;
				
				new ArraySegment<byte>( buffer, offset, count );

				_writeBuffer = buffer;
				_offset = offset;
				_count = count;
				
				if( count == 0 )
					Complete( true );
			}
			
			public void GoAsync() {
				_willCompleteSync = false;
			}

			public WaitHandle TryWrite() {
				if( _count == 0 )
					return null;
					
				for( ;; ) {
					int ptrOffset, writableLength;
					_stream.GetNextWritableRegion( out ptrOffset, out writableLength );

					if( writableLength == 0 ) {
						if( _stream._readUpdateWaitingEvent.WaitOne( 0, false ) ) {
							// We may have moved
							_stream.GetNextWritableRegion( out ptrOffset, out writableLength );

							if( writableLength == 0 ) {
								_ts.TraceEvent( TraceEventType.Verbose, 0, "Buffer is full while writing, waiting for a read" );
								return _stream._readUpdateWaitingEvent;
							}
						}

						// We didn't move
						_ts.TraceEvent( TraceEventType.Verbose, 0, "Buffer is full while writing, waiting for a read" );
						return _stream._readUpdateWaitingEvent;
					}

					// What we will actually be able to write this time around
					int length = Math.Min( writableLength, _count );

					// Write as much as we can to the end
					int firstWrite = Math.Min( length, _stream._buffer.Length - ptrOffset );
					Buffer.BlockCopy( _writeBuffer, _offset, _stream._buffer, ptrOffset, firstWrite );

					// Now try the second write
					if( firstWrite != length ) {
						Buffer.BlockCopy( _writeBuffer, _offset + firstWrite, _stream._buffer, 0, length - firstWrite );
					}

					// Update the position
					if( firstWrite == length )			// Advance linearly
						ptrOffset += firstWrite;
					else
						ptrOffset = length - firstWrite;	// Wrap

					_stream.AdvanceWritePointer( ptrOffset, (length == writableLength), writableLength == _stream._buffer.Length );
					
					// Release people waiting for the position to update
					_stream._writeUpdateWaitingEvent.Set();

					_offset += length;
					_count -= length;

					if( _count == 0 ) {
						_ts.TraceEvent( TraceEventType.Information, 0, "Write completed " + (_willCompleteSync ? "synchronously" : "asynchronously") );
						Complete( _willCompleteSync );
						return null;
					}
				}
			}
			
			public new void End() {
				base.End();
			}
		}
		
		/// <include file='Common.xml' path='/root/Stream/method[@name="BeginWrite"]/*'/>
		public override IAsyncResult BeginWrite( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			WriteRequest req = new WriteRequest( this, buffer, offset, count, callback, state );

			if( count != 0 )
				_writeQueue.Enqueue( req );
				
			return req;
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="EndWrite"]/*'/>
		public override void EndWrite( IAsyncResult asyncResult ) {
			WriteRequest req = (WriteRequest) asyncResult;
			req.End();
		}
		
		private WaitHandle WriteQueueHandler( WriteRequest request, bool async ) {
			if( async )
				request.GoAsync();

			WaitHandle handle = request.TryWrite();

			return handle;
		}

		/// <include file='Common.xml' path='/root/Stream/method[@name="Write"]/*'/>
		public override void Write( byte[] buffer, int offset, int count ) {
			EndWrite( BeginWrite( buffer, offset, count, null, null ) );
		}
	#endregion

	#region RedirectStream
		/// <summary>
		/// Allows a stream to read or write its data to or from different sources.
		/// </summary>
		/// <remarks>The <see cref="RedirectStream"/> can read its input from one stream and write its
		///   output to another. You can also specify <see langword='null'/> for either stream to create
		///   a read-only or write-only stream.</remarks>
		class RedirectStream : Stream {
			Stream _writeStream;
			Stream _readStream;

			/// <summary>
			/// Creates a new instance of the <see cref='RedirectStream'/> class.
			/// </summary>
			/// <param name="readStream">A <see cref="Stream"/> from which data will be read. If this parameter
			///   is <see langword='null'/> or the stream does not support reading, reading will be disabled.</param>
			/// <param name="writeStream">A <see cref="Stream"/> to which data will be written. If this parameter
			///   is <see langword='null'/> or the stream does not support writing, writing will be disabled.</param>
			public RedirectStream( Stream readStream, Stream writeStream ) {
				_readStream = readStream;
				_writeStream = writeStream;
			}

			/// <summary>
			/// Gets a value that represents whether the stream can be read.
			/// </summary>
			/// <value>True if the stream can be read, false otherwise.</value>
			/// <remarks>The stream can be read if the read stream is not <see langword='null'/> and supports reading.</remarks>
			public override bool CanRead {
				get { return _readStream != null && _readStream.CanRead; }
			}

			/// <summary>
			/// Gets a value that represents whether the stream supports seeking.
			/// </summary>
			/// <value>This property always returns false.</value>
			public override bool CanSeek {
				get { return false; }
			}

			/// <summary>
			/// Gets a value that represents whether the stream can be written.
			/// </summary>
			/// <value>True if the stream can be written, false otherwise.</value>
			/// <remarks>The stream can be written if the write stream is not <see langword='null'/> and supports writing.</remarks>
			public override bool CanWrite {
				get { return _writeStream != null && _writeStream.CanWrite; }
			}

			public override void Flush() {
				if( _writeStream != null )
					_writeStream.Flush();
			}

			/// <summary>
			/// Gets the length of the stream.
			/// </summary>
			/// <value>This property always throws an exception.</value>
			/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
			public override long Length {
				get { throw new NotSupportedException(); }
			}

			/// <summary>
			/// Gets or sets the current position in the stream.
			/// </summary>
			/// <value>This property always throws an exception.</value>
			/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
			public override long Position {
				get {
					throw new NotSupportedException();
				}
				set {
					throw new NotSupportedException();
				}
			}

			public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
				if( _readStream == null )
					throw new NotSupportedException();

				return _readStream.BeginRead( buffer, offset, count, callback, state );
			}

			public override int Read( byte[] buffer, int offset, int count ) {
				if( _readStream == null )
					throw new NotSupportedException();

				return _readStream.Read( buffer, offset, count );
			}

			public override int EndRead( IAsyncResult asyncResult ) {
				if( _readStream == null )
					throw new NotSupportedException();

				return _readStream.EndRead( asyncResult );
			}

			/// <summary>
			/// Seeks to another part of the stream.
			/// </summary>
			/// <param name="offset">The target offset.</param>
			/// <param name="origin">The origin.</param>
			/// <returns>This method always throws an exception.</returns>
			/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
			public override long Seek( long offset, SeekOrigin origin ) {
				throw new NotSupportedException();
			}

			/// <summary>
			/// Sets the length of the stream.
			/// </summary>
			/// <param name="value">The length of the stream.</param>
			/// <remarks>This method always throws an exception.</remarks>
			/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
			public override void SetLength( long value ) {
				throw new NotSupportedException();
			}

			public override IAsyncResult BeginWrite( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
				if( _writeStream == null )
					throw new NotSupportedException();

				return _writeStream.BeginWrite( buffer, offset, count, callback, state );
			}

			public override void Write( byte[] buffer, int offset, int count ) {
				if( _writeStream == null )
					throw new NotSupportedException();

				_writeStream.Write( buffer, offset, count );
			}

			public override void EndWrite( IAsyncResult asyncResult ) {
				if( _writeStream == null )
					throw new NotSupportedException();

				_writeStream.EndWrite( asyncResult );
			}
		}
	#endregion
	}

	#region StreamRedirection
	public sealed class StreamRedirection : IDisposable {
		Stream _source, _target;
		byte[] _buffer;
		AsyncCallback _callback;
		bool _disposed, _autoFlush;
		long _bytesCopied;

		public StreamRedirection( Stream source, Stream target, int bufferSize, bool autoFlush ) {
			if( source == null )
				throw new ArgumentNullException( "source" );

			if( !source.CanRead )
				throw new ArgumentException( "The source stream cannot be read.", "source" );

			if( target == null )
				throw new ArgumentNullException( "target" );

			if( !target.CanWrite )
				throw new ArgumentException( "The target stream cannot be written.", "target" );

			if( bufferSize < 8 )
				bufferSize = 8;

			_source = source;
			_target = target;
			_buffer = new byte[bufferSize];
			_callback = ReadCallback;
			_autoFlush = autoFlush;

			_source.BeginRead( _buffer, 0, _buffer.Length, _callback, null );
		}

		void ReadCallback( IAsyncResult result ) {
			try {
				int count = _source.EndRead( result );

				if( _disposed || count == 0 ) {
					Console.WriteLine( "Stream ended." );
					return;
				}

				_target.BeginWrite( _buffer, 0, count, WriteCallback, null );
				unchecked { _bytesCopied += count; }

				_buffer = new byte[_buffer.Length];
				_source.BeginRead( _buffer, 0, _buffer.Length, _callback, null );
			}
			catch( Exception ex ) {
				Console.WriteLine( ex.ToString() );
			}
		}

		void WriteCallback( IAsyncResult result ) {
			try {
				_target.EndWrite( result );

				if( _autoFlush )
					_target.Flush();
			}
			catch( Exception ex ) {
				Console.WriteLine( ex.ToString() );
			}
		}

		/// <summary>
		/// Gets the number of bytes copied so far.
		/// </summary>
		/// <value>The number of bytes copied so far. For very long streams, this value could become invalid.</value>
		public long BytesCopied {
			get {
				return _bytesCopied;
			}
		}

		public void Dispose() {
			_disposed = true;
		}
	}
	#endregion
}
