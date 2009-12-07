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

namespace Fluggo.Communications {
	public abstract class SimpleStream<T> : IDisposable {
		sealed class EmptyRead : BaseAsyncResult {
			bool _end;
			T _value;
		
			public EmptyRead( AsyncCallback callback, object state ) : base( callback, state ) {
			}
			
			public void Complete( bool end, T value ) {
				_end = end;
				_value = value;
				Complete( true );
			}
			
			public bool End( out T value ) {
				base.End();
				value = _value;
				return _end;
			}
		}

		sealed class EmptyWrite : BaseAsyncResult {
			public EmptyWrite( AsyncCallback callback, object state ) : base( callback, state ) {
			}

			public void Complete() {
				Complete( true );
			}

			public new void End() {
				base.End();
			}
		}

		/// <summary>
		/// Begins an asynchronous read operation.
		/// </summary>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the read operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the asynchronous operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <remarks>The default implementation of this method invokes <see cref="ReadValue"/> synchronously.</remarks>
		public virtual IAsyncResult BeginReadValue( AsyncCallback callback, object state ) {
			EmptyRead read = new EmptyRead( callback, state );

			try {
				T value;
				read.Complete( ReadValue( out value ), value );
			}
			catch( Exception ex ) {
				read.CompleteError( ex );
			}

			return read;
		}
		
		public abstract bool ReadValue( out T value );
		
		public virtual bool EndReadValue( IAsyncResult result, out T value ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyRead read = result as EmptyRead;

			if( read == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginRead call on this object.", "result" );

			return read.End( out value );
		}

		/// <summary>
		/// Begins an asynchronous write operation.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the write operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the write operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <remarks>The default implementation of this method invokes <see cref="WriteValue"/> synchronously.</remarks>
		public virtual IAsyncResult BeginWriteValue( T value, AsyncCallback callback, object state ) {
			EmptyWrite write = new EmptyWrite( callback, state );

			try {
				WriteValue( value );
				write.Complete();
			}
			catch( Exception ex ) {
				write.CompleteError( ex );
			}

			return write;
		}

		/// <summary>
		/// Ends an asynchronous write operation.
		/// </summary>
		/// <param name="result">A reference to the outstanding asynchronous request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name='result'/> did not originate from a <see cref='BeginWriteValue'/>
		///   call on the current object.</exception>
		public virtual void EndWriteValue( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyWrite write = result as EmptyWrite;

			if( write == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginSend call on this object.", "result" );

			write.End();
		}

		public abstract void WriteValue( T value );
		
		public virtual void Close() { Dispose(); }
		public void Dispose() {
			Dispose( true );
		}
		
		~SimpleStream() {
			Dispose( false );
		}
		
		protected virtual void Dispose( bool disposing ) {
			if( disposing )
				GC.SuppressFinalize( this );
		}

		public abstract bool CanRead { get; }
		public abstract bool CanWrite { get; }
	}
	
	/// <summary>
	/// Represents a sequence of objects that supports bulk reads or writes.
	/// </summary>
	/// <typeparam name="T">Type of elements of the stream.</typeparam>
	/// <remarks>Unlike members of the system <see cref="Stream"/> class, the default implementations of <see cref="BeginRead"/>
	///	  and <see cref="BeginWrite"/> do not perform asynchronous operations, and the default implementations of <see cref="Read"/>
	///	  and <see cref="Write"/> are not atomic. These are optional features of generic streams, and are provided by the
	///   implementers. However, you can use the <see cref="WrapAsync"/> method to create <see cref="Stream{T}"/> instances that
	///   perform asynchronous, atomic bulk reads and writes using <see cref="SimpleStream{T}"/> instances.</remarks>
	public abstract class Stream<T> : SimpleStream<T> {
		sealed class EmptyRead : BaseAsyncResult {
			int _count;
		
			public EmptyRead( AsyncCallback callback, object state ) : base( callback, state ) {
			}
			
			public void Complete( int count ) {
				_count = count;
				Complete( true );
			}
			
			public new int End() {
				base.End();
				return _count;
			}
		}

		sealed class EmptyWrite : BaseAsyncResult {
			public EmptyWrite( AsyncCallback callback, object state ) : base( callback, state ) {
			}

			public void Complete() {
				Complete( true );
			}

			public new void End() {
				base.End();
			}
		}
		
		/// <summary>
		/// Begins an asynchronous read operation.
		/// </summary>
		/// <param name="buffer">Buffer to store the read values.</param>
		/// <param name="offset">Offset into the buffer at which to begin storing values.</param>
		/// <param name="count">Maximum number of values to read.</param>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the receive operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the read operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <remarks>The default implementation of this method invokes <see cref="Read"/> synchronously.</remarks>
		public virtual IAsyncResult BeginRead( T[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( !CanRead )
				throw new NotSupportedException();
			
			new ArraySegment<T>( buffer, offset, count );
			EmptyRead receive = new EmptyRead( callback, state );
			
			try {
				receive.Complete( Read( buffer, offset, count ) );
			}
			catch( Exception ex ) {
				receive.CompleteError( ex );
			}
			
			return receive;
		}

		/// <summary>
		/// Ends an asynchronous read operation.
		/// </summary>
		/// <param name="result">A reference to the outstanding asynchronous request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name='result'/> did not originate from a <see cref='BeginRead'/>
		///   call on the current object.</exception>
		/// <returns>The number of values read, or zero if the end of the stream has been reached.</returns>
		public virtual int EndRead( IAsyncResult result ) {
			if( !CanRead )
				throw new NotSupportedException();
			
			if( result == null )
				throw new ArgumentNullException( "result" );
			
			EmptyRead read = result as EmptyRead;
			
			if( read == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginRead call on this object.", "result" );
				
			return read.End();
		}
		
		public virtual int Read( T[] buffer, int offset, int count ) {
			if( !CanRead )
				throw new NotSupportedException();
		
			new ArraySegment<T>( buffer, offset, count );
			
			if( count == 0 )
				throw new ArgumentOutOfRangeException( "count" );
			
			return ReadValue( out buffer[offset] ) ? 1 : 0;
		}
		
		/// <summary>
		/// Begins an asynchronous write operation.
		/// </summary>
		/// <param name="buffer">Array with values to write.</param>
		/// <param name="offset">Offset into the buffer at which to begin reading values.</param>
		/// <param name="count">Number of values to write.</param>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the write operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the write operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <remarks>The default implementation of this method invokes <see cref="Write"/> synchronously.</remarks>
		public virtual IAsyncResult BeginWrite( T[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( !CanWrite )
				throw new NotSupportedException();
			
			EmptyWrite write = new EmptyWrite( callback, state );
			
			try {
				Write( buffer, offset, count );
				write.Complete();
			}
			catch( Exception ex ) {
				write.CompleteError( ex );
			}
			
			return write;
		}

		/// <summary>
		/// Ends an asynchronous write operation.
		/// </summary>
		/// <param name="result">A reference to the outstanding asynchronous request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name='result'/> did not originate from a <see cref='BeginWrite'/>
		///   call on the current object.</exception>
		public virtual void EndWrite( IAsyncResult result ) {
			if( !CanWrite )
				throw new NotSupportedException();

			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyWrite write = result as EmptyWrite;

			if( write == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginWrite call on this object.", "result" );

			write.End();
		}

		/// <summary>
		/// Writes values to the given stream.
		/// </summary>
		/// <param name="buffer">Array with values to write.</param>
		/// <param name="offset">Offset into the buffer at which to begin reading values.</param>
		/// <param name="count">Number of values to write.</param>
		/// <remarks>Simultaneous calls to <see cref="Write"/> or <see cref="BeginWrite"/> are not guaranteed to be atomic. If you make more than
		///   one <see cref="Write"/> or <see cref="BeginWrite"/> call at a time, the stream may interleave the contents of the two calls.</remarks>
		public virtual void Write( T[] buffer, int offset, int count ) {
			if( !CanWrite )
				throw new NotSupportedException();

			new ArraySegment<T>( buffer, offset, count );
			
			for( int i = offset; i < (offset + count); i++ )
				WriteValue( buffer[i] );
		}
		
		/// <summary>
		/// Creates a <see cref="Stream{T}"/> that implements atomic, asynchronous reads and writes for the given <see cref="SimpleStream{T}"/>.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Stream<T> WrapAsync( SimpleStream<T> stream ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );
			
			Stream<T> result = stream as Stream<T>;
			
			if( result != null )
				return result;
				
			return new StreamWrapper<T>( stream );
		}
	}

	sealed class StreamWrapper<T> : Stream<T> {
		sealed class WriteRequest : BaseAsyncResult {
			SimpleStream<T> _root;
			T[] _array;
			int _startOffset, _currentOffset, _endOffset;
			IAsyncResult _lastResult;
			
			public WriteRequest( SimpleStream<T> root, T[] array, int offset, int count, AsyncCallback callback, object state )
					: base( callback, state ) {
				new ArraySegment<T>( array, offset, count );
				
				if( root == null )
					throw new ArgumentNullException( "root" );
					
				_root = root;
				_array = array;
				_startOffset = offset;
				_currentOffset = offset;
				_endOffset = offset + count;
			}
			
			public WaitHandle TryWrite( bool isAsync ) {
				for( ;; ) {
					if( _lastResult != null ) {
						try {
							_root.EndWriteValue( _lastResult );
							_currentOffset++;
						}
						catch( Exception ex ) {
							CompleteError( ex );
							return null;
						}
					}

					if( _currentOffset == _endOffset ) {
						Complete( !isAsync );
						return null;
					}
						
					_lastResult = _root.BeginWriteValue( _array[_currentOffset], null, null );
					
					if( !_lastResult.IsCompleted )
						return _lastResult.AsyncWaitHandle;
				}
			}
			
			public new void End() {
				base.End();
			}
		}

		sealed class ReadRequest : BaseAsyncResult
		{
			SimpleStream<T> _root;
			StreamWrapper<T> _owner;
			T[] _array;
			int _startOffset, _currentOffset, _endOffset, _count;
			bool _end;

			public ReadRequest( StreamWrapper<T> owner, T[] array, int offset, int count, AsyncCallback callback, object state )
					: base( callback, state ) {
				new ArraySegment<T>( array, offset, count );

				if( owner == null )
					throw new ArgumentNullException( "owner" );

				_owner = owner;
				_root = owner._root;
				_array = array;
				_startOffset = offset;
				_currentOffset = offset;
				_endOffset = offset + count;
			}

			public WaitHandle TryRead( bool isAsync ) {
				for( ; ; ) {
					if( _owner._lastRead != null ) {
						// Ensure the last one completed, otherwise, we have to wait
						if( !_owner._lastRead.IsCompleted ) {
							// We're not done if we haven't retrieved *anything*
							if( _count == 0 )
								return _owner._lastRead.AsyncWaitHandle;
								
							// We have read something, so instead of waiting, return
							Complete( !isAsync );
							return null;
						}
					
						// Try to complete the last operation
						try {
							_end = _root.EndReadValue( _owner._lastRead, out _array[_currentOffset] );
							
							// We don't really have a value if we've reached the end
							if( !_end ) {
								_currentOffset++;
								_count++;
							}
						}
						catch( Exception ex ) {
							// Report the error that occured
							CompleteError( ex );
							return null;
						}
						finally {
							_owner._lastRead = null;
						}
					}

					if( _end || (_currentOffset == _endOffset) ) {
						Complete( !isAsync );
						return null;
					}

					// Begin the next read
					_owner._lastRead = _root.BeginReadValue( null, null );
				}
			}

			public new int End() {
				base.End();
				return _count;
			}
		}
		
		class AsyncReadWrapper : IAsyncResult {
			ReadRequest _root;
			T[] _readArray;

			public AsyncReadWrapper( StreamWrapper<T> owner, AsyncCallback callback, object state ) {
				_readArray = new T[1];
				_root = (ReadRequest) owner.BeginRead( _readArray, 0, 1, callback, state );
			}
			
			public object AsyncState {
				get { return _root.AsyncState; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return _root.AsyncWaitHandle; }
			}

			public bool CompletedSynchronously {
				get { return _root.CompletedSynchronously; }
			}

			public bool IsCompleted {
				get { return _root.IsCompleted; }
			}
			
			public bool End( out T value ) {
				if( _root.End() == 0 ) {
					value = default(T);
					return false;
				}
				else {
					value = _readArray[0];
					return true;
				}
			}
		}

		SimpleStream<T> _root;
		ProcessingQueue<WriteRequest> _writeQueue;
		ProcessingQueue<ReadRequest> _readQueue;
		IAsyncResult _lastRead;
	
		public StreamWrapper( SimpleStream<T> root ) {
			if( root != null )
				throw new ArgumentNullException( "root" );
				
			_root = root;
			_writeQueue = new ProcessingQueue<WriteRequest>( delegate( WriteRequest request, bool async ) {
				return request.TryWrite( async );
			} );
			_readQueue = new ProcessingQueue<ReadRequest>( delegate( ReadRequest request, bool async ) {
				return request.TryRead( async );
			} );
		}

		public override IAsyncResult BeginWrite( T[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( !CanWrite )
				throw new NotSupportedException();

			WriteRequest request = new WriteRequest( _root, buffer, offset, count, callback, state );
			_writeQueue.Enqueue( request );
			
			return request;
		}

		public override void EndWrite( IAsyncResult result ) {
			if( !CanWrite )
				throw new NotSupportedException();

			if( result == null )
				throw new ArgumentNullException( "result" );
				
			WriteRequest request = result as WriteRequest;
			
			if( request == null )
				throw new ArgumentException( "The given asynchronous result is not from a call to BeginWrite.", "result" );
				
			request.End();
		}

		public override void Write( T[] buffer, int offset, int count ) {
			EndWrite( BeginWrite( buffer, offset, count, null, null ) );
		}

		public override IAsyncResult BeginRead( T[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( !CanRead )
				throw new NotSupportedException();

			ReadRequest request = new ReadRequest( this, buffer, offset, count, callback, state );
			_readQueue.Enqueue( request );
			
			return request;
		}

		public override int EndRead( IAsyncResult result ) {
			if( !CanRead )
				throw new NotSupportedException();

			if( result == null )
				throw new ArgumentNullException( "result" );

			ReadRequest request = result as ReadRequest;

			if( request == null )
				throw new ArgumentException( "The given asynchronous result is not from a call to BeginRead.", "result" );

			return request.End();
		}

		public override int Read( T[] buffer, int offset, int count ) {
			return EndRead( BeginRead( buffer, offset, count, null, null ) );
		}

		public override bool CanRead {
			get { return _root.CanRead; }
		}

		public override bool CanWrite {
			get { return _root.CanWrite; }
		}

		public override IAsyncResult BeginReadValue( AsyncCallback callback, object state ) {
			if( !CanRead )
				throw new NotSupportedException();
			
			return new AsyncReadWrapper( this, callback, state );
		}

		public override bool EndReadValue( IAsyncResult result, out T value ) {
			if( !CanRead )
				throw new NotSupportedException();

			if( result == null )
				throw new ArgumentNullException( "result" );

			AsyncReadWrapper request = result as AsyncReadWrapper;

			if( request == null )
				throw new ArgumentException( "The given asynchronous result is not from a call to BeginReadValue.", "result" );

			return request.End( out value );
		}
		
		public override bool ReadValue( out T value ) {
			return EndReadValue( BeginReadValue( null, null ), out value );
		}

		public override IAsyncResult BeginWriteValue( T value, AsyncCallback callback, object state ) {
			return BeginWrite( new T[] { value }, 0, 1, callback, state );
		}

		public override void EndWriteValue( IAsyncResult result ) {
			EndWrite( result );
		}

		public override void WriteValue( T value ) {
			EndWriteValue( BeginWriteValue( value, null, null ) );
		}
	}

	public abstract class MessageStream<T> : SimpleStream<T> where T : IMessage {
		public abstract int MaximumPayloadLength { get; }

		/// <summary>
		/// Gets the current receive window for the stream.
		/// </summary>
		/// <value>The current receive window for the stream. This is the maximum number of payload bytes that
		///   can be sent without delay. If the stream does not have a receive window, or the receive window is
		///   unknown, the value is <see cref="MaximumPayloadLength"/>.</value>
		public abstract int ReceiveWindow { get; }
	}
}
