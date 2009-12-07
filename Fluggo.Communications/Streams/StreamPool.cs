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
using System.Text;
using System.Threading;
using System.IO;

namespace Fluggo.Communications
{
	/// <summary>
	/// Represents a pool of streams that communicate in one direction across a <see cref="ChannelMultiplexer"/>.
	/// </summary>
	public sealed class UnidirectionalStreamPool : IDisposable {
		ChannelMultiplexer _transport;
		EventHandler _firstMessageCallback;
		int _baseIndex, _count;
		EventHandler _readStreamBoundaryCallback, _writeStreamBoundaryCallback;
		
		public UnidirectionalStreamPool( ChannelMultiplexer transport, int baseIndex, int count ) {
			if( transport == null )
				throw new ArgumentNullException( "transport" );

			if( baseIndex < 0 || baseIndex >= transport.ChannelCount )
				throw new ArgumentOutOfRangeException( "baseIndex" );
				
			if( count < 0 )
				throw new ArgumentOutOfRangeException( "count" );
				
			if( baseIndex + count > transport.ChannelCount )
				throw new ArgumentException( "There are not enough queues in the transport to support the requested number of streams." );
				
			_transport = transport;
			_baseIndex = baseIndex;
			_count = count;
			
			_firstMessageCallback = FirstMessageCallback;
			_readStreamBoundaryCallback = LastReadCompleted;
			_writeStreamBoundaryCallback = LastWriteCompleted;
			
			for( int i = _baseIndex; i < _baseIndex + _count; i++ ) {
				HoldbackTransport readHoldback = new HoldbackTransport( transport.GetChannel( i ), i, _firstMessageCallback );
				readHoldback.ReadClosed += _readStreamBoundaryCallback;
				
				HoldbackTransport writeHoldback = new HoldbackTransport( transport.GetChannel( i ), i, null );
				writeHoldback.WriteClosed += _writeStreamBoundaryCallback;
				
				_writeOnlyQueue.Enqueue( new WriteStreamOverChannel( writeHoldback ) );
			}
		}

	#region Receiving read-only streams
		AsynchronousQueue<ReadStreamOverChannel> _readOnlyQueue = new AsynchronousQueue<ReadStreamOverChannel>();
		
		void FirstMessageCallback( object sender, EventArgs e ) {
			_readOnlyQueue.Enqueue( new ReadStreamOverChannel( (HoldbackTransport) sender ) );
		}
		
		void LastReadCompleted( object sender, EventArgs e ) {
			// Stream boundary was read on one of our read-only streams; take over and start monitoring it again
			HoldbackTransport transport = (HoldbackTransport) sender;
			int i = transport.SubStream;
			
			HoldbackTransport readHoldback = new HoldbackTransport( _transport.GetChannel( i ), i, _firstMessageCallback );
			readHoldback.ReadClosed += _readStreamBoundaryCallback;
		}

		public IAsyncResult BeginReceiveStream( AsyncCallback callback, object state ) {
			return _readOnlyQueue.BeginDequeue( callback, state );
		}

		public Stream EndReceiveStream( IAsyncResult result ) {
			return _readOnlyQueue.EndDequeue( result );
		}

		/// <summary>
		/// Receives a new inbound read-only stream.
		/// </summary>
		/// <returns>The new read-only stream.</returns>
		/// <remarks>This method blocks until a new read-only stream is received. You can read data from the stream until the other
		///   end calls <see cref="Stream.Close"/>, at which point the underlying transport will be returned to the list of available transports.</remarks>
		public Stream ReceiveStream() {
			return _readOnlyQueue.Dequeue();
		}
	#endregion
		
	#region Getting write-only streams
		AsynchronousQueue<WriteStreamOverChannel> _writeOnlyQueue = new AsynchronousQueue<WriteStreamOverChannel>();
		
		void LastWriteCompleted( object sender, EventArgs e ) {
			// Stream boundary was read on one of our read-only streams; take over and start monitoring it again
			HoldbackTransport transport = (HoldbackTransport) sender;
			transport = new HoldbackTransport( _transport.GetChannel( transport.SubStream ), transport.SubStream, null );
			transport.WriteClosed += _writeStreamBoundaryCallback;
			
			_writeOnlyQueue.Enqueue( new WriteStreamOverChannel( transport ) );
		}

		public IAsyncResult BeginGetStream( AsyncCallback callback, object state ) {
			return _writeOnlyQueue.BeginDequeue( callback, state );
		}

		public Stream EndGetStream( IAsyncResult result ) {
			return _writeOnlyQueue.EndDequeue( result );
		}

		/// <summary>
		/// Allocates a new outbound write-only stream.
		/// </summary>
		/// <returns>The new write-only stream.</returns>
		/// <remarks>This method blocks until a new write-only stream is available. You can write data to the stream until you call
		///   <see cref="Stream.Close"/>, at which point the underlying transport will be returned to the list of available transports.</remarks>
		public Stream GetStream() {
			return _writeOnlyQueue.Dequeue();
		}
	#endregion

		public void Dispose() {
			_readOnlyQueue.Dispose();
			_writeOnlyQueue.Dispose();
		}
	}

#if false
	/// <summary>
	/// Represents a pool of streams that communicate in one direction across a <see cref="MessageMultiStreamTransport"/>.
	/// </summary>
	public class BidirectionalMessageStreamPool
	{
		MessageMultiStreamTransport _transport;
		EventHandler _firstMessageCallback;
		int _baseIndex, _count;
		EventHandler _streamBoundaryCallback;
		MessageBoundStream[] _streams;
		HoldbackTransport[] _holdbacks;
		object _lock = new object();

		public BidirectionalMessageStreamPool( MessageMultiStreamTransport transport, int baseIndex, int inboundCount, int outboundCount ) {
			if( transport == null )
				throw new ArgumentNullException( "transport" );

			if( baseIndex < 0 || baseIndex >= transport.StreamCount )
				throw new ArgumentOutOfRangeException( "baseIndex" );

			if( count < 0 )
				throw new ArgumentOutOfRangeException( "count" );

			if( baseIndex + count > transport.StreamCount )
				throw new ArgumentException( "There are not enough queues in the transport to support the requested number of streams." );

			_transport = transport;
			_baseIndex = baseIndex;
			_count = count;

			_firstMessageCallback = FirstMessageCallback;
			_streamBoundaryCallback = CloseCompleted;
			_streams = new MessageBoundStream[count];
			_holdbacks = new HoldbackTransport[count];

			for( int i = _baseIndex; i < _baseIndex + _count; i++ ) {
				_holdbacks[i - _baseIndex] = new HoldbackTransport( transport.GetSubStream( i ), i, _firstMessageCallback );
				_holdbacks[i - _baseIndex].BothClosed += _streamBoundaryCallback;
			}
		}

		AsynchronousQueue<MessageBoundStream> _receiveQueue = new AsynchronousQueue<MessageBoundStream>();
		AsynchronousQueue<MessageBoundStream> _waitQueue = new AsynchronousQueue<MessageBoundStream>();
		
		void FirstMessageCallback( object sender, EventArgs e ) {
			HoldbackTransport transport = (HoldbackTransport) sender;
			MessageBoundStream stream = new MessageBoundStream( transport );

			lock( _lock ) {
				if( _streams[transport.Stream - _baseIndex] != null )
					return;
					
				_streams[transport.Stream - _baseIndex] = stream;
			}

			_receiveQueue.Enqueue( stream );
		}

		void CloseCompleted( object sender, EventArgs e ) {
			// Stream boundary on both directions... clear it all out and start over
			HoldbackTransport transport = (HoldbackTransport) sender;
			int index = transport.Stream - _baseIndex;

			lock( _lock ) {
				if( _waitQueue.QueuedCount != 0 ) {
					// Satisfy an existing outbound request with this one
					_holdbacks[index] = new HoldbackTransport( _transport.GetSubStream( transport.Stream ), transport.Stream, null );
					_streams[index] = new MessageBoundStream( _holdbacks[index] ); 
					_waitQueue.Enqueue( _streams[index] );
				}
				else {
					// Wait for an inbound message
					_holdbacks[index] = new HoldbackTransport( _transport.GetSubStream( transport.Stream ), transport.Stream, _firstMessageCallback );
					_streams[index] = null;
				}

				_holdbacks[index].BothClosed += _streamBoundaryCallback;
			}
		}

		public IAsyncResult BeginReceiveStream( AsyncCallback callback, object state ) {
			return _receiveQueue.BeginDequeue( callback, state );
		}

		public Stream EndReceiveStream( IAsyncResult result ) {
			return _receiveQueue.EndDequeue( result );
		}

		/// <summary>
		/// Receives a new inbound bidirectional stream.
		/// </summary>
		/// <returns>The new bidirectional stream.</returns>
		/// <remarks>This method blocks until a new bidirectional stream is received. You can pass data over the stream until you call
		///   <see cref="Stream.Close"/>, at which point the underlying transport will be returned to the list of available transports.</remarks>
		public Stream ReceiveStream() {
			return _receiveQueue.Dequeue();
		}

		public IAsyncResult BeginGetStream( AsyncCallback callback, object state ) {
			MessageBoundStream newStream = null;
			
			lock( _lock ) {
				// Search for an open slot and use it
				for( int i = 0; i < _streams.Length; i++ ) {
					if( _streams[i] == null ) {
						newStream = _streams[i] = new MessageBoundStream( _holdbacks[i] );
						break;
					}
				}
			}
			
			// Queue it so that we can dequeue it right away
			if( newStream != null )
				_waitQueue.Enqueue( newStream );
			
			return _waitQueue.BeginDequeue( callback, state );
		}

		public Stream EndGetStream( IAsyncResult result ) {
			return _waitQueue.EndDequeue( result );
		}

		/// <summary>
		/// Allocates a new outbound bidirectional stream.
		/// </summary>
		/// <returns>The new bidirectional stream.</returns>
		/// <remarks>This method blocks until a new bidirectional stream is available. You can pass data over the stream until you call
		///   <see cref="Stream.Close"/>, at which point the underlying transport will be returned to the list of available transports.</remarks>
		public Stream GetStream() {
			lock( _lock ) {
				// Search for an open slot and use it
				for( int i = 0; i < _streams.Length; i++ ) {
					if( _streams[i] == null )
						return _streams[i] = new MessageBoundStream( _holdbacks[i] );
				}
			}

			return _waitQueue.Dequeue();
		}
	}
#endif

	#region HoldbackTransport
	sealed class HoldbackTransport : Channel
	{
		Channel _root;
		int _subStream;
		object _lock = new object();
		bool _readClosed, _writeClosed, _couldRead, _couldWrite;
		AsynchronousQueue<IMessageBuffer> _holdbackQueue;
		bool _holdbackPerformed = false;

		public HoldbackTransport( Channel root, int subStream, EventHandler firstMessageCallback ) {
			if( root == null )
				throw new ArgumentNullException( "root" );

			_root = root;
			_subStream = subStream;
			_couldRead = _root.CanReceive;
			_couldWrite = _root.CanSend;
			
			if( firstMessageCallback != null ) {
				_holdbackQueue = new AsynchronousQueue<IMessageBuffer>();
				_root.BeginReceive( FirstMessageReceivedCallback, firstMessageCallback );
			}
		}

		class EmptyRead : BaseAsyncResult
		{
			public EmptyRead( IMessageBuffer data, AsyncCallback callback, object state )
				: base( callback, state ) {
				_data = data;
				Complete( true );
			}

			IMessageBuffer _data;
			
			public new IMessageBuffer End() {
				base.End();
				return _data;
			}
		}
		
		void FirstMessageReceivedCallback( IAsyncResult result ) {
			_holdbackQueue.Enqueue( _root.EndReceive( result ) );
			((EventHandler) result.AsyncState)( this, EventArgs.Empty );
		}

		class MyWrapper : IAsyncResult
		{
			public IAsyncResult _root;
			public MyWrapper( IAsyncResult root ) { _root = root; }
			
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
		}

		public override IAsyncResult BeginReceive( AsyncCallback callback, object state ) {
			if( _readClosed )
				return new EmptyRead( null, callback, state );

			if( !_couldRead )
				throw new NotSupportedException();

			if( _holdbackQueue != null ) {
				AsynchronousQueue<IMessageBuffer> holdbackQueue;
				bool holdbackPerformed;

				lock( _lock ) {
					holdbackQueue = _holdbackQueue;
					holdbackPerformed = _holdbackPerformed;
					_holdbackPerformed = true;
				}

				if( holdbackQueue != null && !holdbackPerformed ) {
					return new MyWrapper( holdbackQueue.BeginDequeue( callback, state ) );
				}
			}

			return _root.BeginReceive( callback, state );
		}

		public override IMessageBuffer EndReceive( IAsyncResult result ) {
			if( !_couldRead )
				throw new NotSupportedException();

			IMessageBuffer message = null;
			
			if( _holdbackQueue != null ) {
				MyWrapper wrapper = result as MyWrapper;
			
				if( wrapper != null ) {
					AsynchronousQueue<IMessageBuffer> holdbackQueue = _holdbackQueue;
					_holdbackQueue = null;
					
					message = holdbackQueue.EndDequeue( wrapper._root );
				}
			}
			
			EmptyRead emptyRead = result as EmptyRead;
			
			if( emptyRead != null )
				return emptyRead.End();
				
			if( message == null )
				message = _root.EndReceive( result );
			
			if( message == null || message.Length == 0 ) {
				OnReadClosed();
				return null;
			}
			
			return message;
		}

		public override IMessageBuffer Receive() {
			if( !_couldRead )
				throw new NotSupportedException();
			
			return EndReceive( BeginReceive( null, null ) );
		}
		
		public override IAsyncResult BeginSend( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( _writeClosed )
				throw new InvalidOperationException( "The stream is closed." );

			if( !_couldWrite )
				throw new NotSupportedException();

			if( buffer == null )
				throw new ArgumentNullException( "message" );
				
			new ArraySegment<byte>( buffer, offset, count );

			if( count == 0 )
				OnWriteClosed();

			return _root.BeginSend( buffer, offset, count, callback, state );
		}

		public override void EndSend( IAsyncResult result ) {
			if( !_couldWrite )
				throw new NotSupportedException();

			_root.EndSend( result );
		}

		public override void Send( byte[] buffer, int offset, int count ) {
			if( _writeClosed )
				throw new InvalidOperationException( "The stream is closed." );
				
			if( !_couldWrite )
				throw new NotSupportedException();

			if( buffer == null )
				throw new ArgumentNullException( "message" );

			new ArraySegment<byte>( buffer, offset, count );

			if( count == 0 )
				OnWriteClosed();

			_root.Send( buffer, offset, count );
		}

		public override int MaximumPayloadLength {
			get { return _root.MaximumPayloadLength; }
		}

		public override int ReceiveWindow {
			get { return _root.ReceiveWindow; }
		}

		public override void Close() {
			base.Close();
			_root.Close();
		}

		public override bool CanReceive {
			get { return !_writeClosed && _couldRead; }
		}

		public override bool CanSend {
			get { return !_writeClosed && _couldWrite; }
		}

		protected override void Dispose( bool disposing ) {
			base.Dispose( disposing );
			
			if( disposing ) {
				_root.Dispose();
			}
		}

		public int SubStream { get { return _subStream; } }
		public event EventHandler ReadClosed;
		public event EventHandler WriteClosed;
		public event EventHandler BothClosed;

		private void OnReadClosed() {
			_readClosed = true;
			EventHandler readHandler = ReadClosed;
			EventHandler bothHandler = BothClosed;

			if( readHandler != null )
				readHandler( this, EventArgs.Empty );

			if( bothHandler != null && _writeClosed )
				bothHandler( this, EventArgs.Empty );
		}

		private void OnWriteClosed() {
			_writeClosed = true;
			EventHandler writeHandler = WriteClosed;
			EventHandler bothHandler = BothClosed;

			if( writeHandler != null )
				writeHandler( this, EventArgs.Empty );

			if( bothHandler != null && _readClosed )
				bothHandler( this, EventArgs.Empty );
		}
	}
	#endregion
}
