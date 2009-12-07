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
	/// Multiplexes and demultiplexes messages across a simple channel.
	/// </summary>
	/// <remarks>This stream can be backed by a channel that only preserves stream IDs, such
	///   as the <see cref="MessageChannelOverStream"/>.</remarks>
	public class ChannelMultiplexer : IDisposable {
		QueueList<ReceiverQueueItem> _rcvWaitQueueList;
		QueueList<IDataMessage> _rcvMessageQueueList;
		const int __rwndProtID = -1;
		int[] _rwnd;
		int[] _maxRwnd;
		SendingQueue[] _sndQueues;
		object _rcvLock = new object();
		Channel<IDataMessage> _channel;
#if DEBUG
		const int _rwndTimeout = -1;
#else
		const int _rwndTimeout = 2000;
#endif
		WaitOrTimerCallback _rwndCallback;
		static TraceSource _ts = new TraceSource( "ChannelMultiplexer", SourceLevels.Error );
		int _rwndOutChannel, _rwndInChannel;
		
	#region SubStream
		sealed class SubStream : Channel {
			ChannelMultiplexer _mux;
			int _channel;

			public SubStream( ChannelMultiplexer mux, int channel ) {
				if( mux == null )
					throw new ArgumentNullException( "mux" );

				_mux = mux;
				_channel = channel;
			}

			public override int MaximumPayloadLength {
				get {
					return Math.Min( _mux._channel.MaximumPayloadLength, _mux._maxRwnd[_channel] >> 1 );
				}
			}
			
			public override int ReceiveWindow { 
				get {
					return _mux._sndQueues[_channel].ReceiveWindow;
				}
			}

			public override IAsyncResult BeginReceive( AsyncCallback callback, object state ) {
				return _mux.BeginReceive( _channel, callback, state );
			}
			
			public override IMessageBuffer Receive() {
				IDataMessage message = _mux.Receive( _channel );
				
				if( message == null )
					return null;
					
				return message.MessageBuffer;
			}

			public override IMessageBuffer EndReceive( IAsyncResult result ) {
				IDataMessage message = _mux.EndReceive( result );
				
				if( message == null )
					return null;
				
				return message.MessageBuffer;
			}

			public override IAsyncResult BeginSend( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
				return _mux.BeginSend( new SimpleDataMessage( _channel, buffer, offset, count ), callback, state );
			}

			public override void Send( byte[] buffer, int offset, int count ) {
				_mux.Send( new SimpleDataMessage( _channel, buffer, offset, count ) );
			}

			public override void EndSend( IAsyncResult result ) {
				_mux.EndSend( result );
			}

			public override bool CanReceive {
				get { return true; }
			}

			public override bool CanSend {
				get { return true; }
			}
		}
	#endregion
		
		public ChannelMultiplexer( Stream stream, int inboundChannelCount, int outboundChannelCount, int maxDataPerChannel )
			: this( new MessageChannelOverStream( stream ), inboundChannelCount, outboundChannelCount, maxDataPerChannel ) {
		}
		
		public ChannelMultiplexer( Channel<IDataMessage> channel, int inboundChannelCount, int outboundChannelCount, int maxDataPerChannel ) {
			if( inboundChannelCount < 0 || inboundChannelCount > ushort.MaxValue )
				throw new ArgumentOutOfRangeException( "inboundStreamCount" );

			if( outboundChannelCount < 0 || outboundChannelCount > ushort.MaxValue )
				throw new ArgumentOutOfRangeException( "outboundStreamCount" );

			if( channel == null )
				throw new ArgumentNullException( "channel" );

			_channel = channel;
			_rcvMessageQueueList = new QueueList<IDataMessage>( inboundChannelCount );
			_rcvWaitQueueList = new QueueList<ReceiverQueueItem>( inboundChannelCount );

			_sndQueues = new SendingQueue[outboundChannelCount];
			_rwnd = new int[inboundChannelCount];
			_maxRwnd = new int[inboundChannelCount];
			_rwndCallback = RwndCallback;
			_rwndOutChannel = outboundChannelCount;
			_rwndInChannel = inboundChannelCount;

			for( int i = 0; i < outboundChannelCount; i++ )
				_sndQueues[i] = new SendingQueue( this, maxDataPerChannel );

			for( int i = 0; i < inboundChannelCount; i++ ) {
				_rwnd[i] = maxDataPerChannel;
				_maxRwnd[i] = maxDataPerChannel;
			}
			
			StartReceiving();
		}
		
		public int ChannelCount
			{ get { return _rcvMessageQueueList.QueueCount; } }
			
		public int GetMaxReceiveWindow( int channel ) {
			if( channel < 0 || channel >= _maxRwnd.Length )
				throw new ArgumentOutOfRangeException( "channel" );
				
			return _maxRwnd[channel];
		}
		
	#region Receive support
		#region ReceiverQueueItem
		class ReceiverQueueItem : BaseAsyncResult
		{
			IDataMessage _message;
#if TRACE
			Stopwatch _stopwatch;
#endif

			public ReceiverQueueItem( AsyncCallback callback, object state )
				: base( callback, state ) {

#if TRACE
				if( _ts.Switch.ShouldTrace( TraceEventType.Verbose ) )
					_stopwatch = Stopwatch.StartNew();
#endif
			}

			public ReceiverQueueItem( IDataMessage message, AsyncCallback callback, object state )
				: base( callback, state ) {
				if( message == null )
					throw new ArgumentNullException( "message" );

				_message = message;
				Complete( true );
			}

			public void CompleteSuccess( IDataMessage result, bool synchronous ) {
				lock( SyncRoot ) {
					if( IsCompleted )
						throw new InvalidOperationException();

					_message = result;
#if TRACE
					if( _stopwatch != null )
						_stopwatch.Stop();
#endif
					Complete( synchronous );

#if TRACE
					if( _stopwatch != null )
						_ts.TraceEvent( TraceEventType.Verbose, 0, "MessageMultiStream: Async receive request in {0} ms", _stopwatch.ElapsedMilliseconds );
#endif
				}
			}

			public new IDataMessage End() {
				base.End();
				return _message;
			}
		}
		#endregion

		volatile bool _rcvRunning = false;
		AsyncCallback _rcvCallback;

		/// <summary>
		/// Starts queuing data from the underlying stream.
		/// </summary>
		public void StartReceiving() {
			lock( _rcvLock ) {
				if( !_rcvRunning ) {
					_rcvCallback = AsyncReceiveThread;
					_channel.BeginReceive( _rcvCallback, null );
					_rcvRunning = true;
				}
			}
		}

		private void SendRwndUpdate( int queue, int size ) {
			if( size == 0 )
				return;

			byte[] buffer = new byte[8];
			NetworkBitConverter.Copy( queue, buffer, 0 );
			NetworkBitConverter.Copy( size, buffer, 4 );

			SimpleDataMessage message = new SimpleDataMessage( _rwndOutChannel, buffer );
			IAsyncResult result = _channel.BeginSend( message, null, null );

			if( !result.CompletedSynchronously && !result.IsCompleted ) {
				// Monitor for timeouts
				ThreadPool.RegisterWaitForSingleObject( result.AsyncWaitHandle, _rwndCallback, result, _rwndTimeout, true );
			}
		}

		private void RwndCallback( object state, bool timedOut ) {
			if( timedOut ) {
				_ts.TraceEvent( TraceEventType.Error, 0, "Timed out while sending rwnd update, aborting" );
				Abort();
			}
			else {
				try {
					_channel.EndSend( (IAsyncResult) state );
				}
				catch( Exception ex ) {
					_ts.TraceEvent( TraceEventType.Error, 0, "Failed to send rwnd update, \"{0}\", aborting", ex.Message );
					Abort();
				}
			}
		}

		private void AsyncReceiveThread( IAsyncResult result ) {
			try {
				IDataMessage message;

				if( !_channel.EndReceive( result, out message ) ) {
					_ts.TraceEvent( TraceEventType.Stop, 0, "AsyncReceiveThread: End of stream, exiting loop" );
					_channel.Close();
					return;
				}
				
				ReceiverQueueItem itemToComplete = null;

				lock( _rcvLock ) {
					// Details: WaitQueueList contains waiting IAsyncResult operations from calls to begin receive.
					//   These should be serviced first. MessageQueueList is where messages go when there are no waiting
					//   receive operations so that they are synchronously serviced by BeginReceive. Therefore, a race
					//   condition here could put a late-arriving packet at the beginning of the queue, a situation that
					//   has been seen occasionally in testing. I don't know yet what causes the problem, but this seems
					//   like a good candidate.

					if( message.Channel == _rwndInChannel ) {
						// Is it an rwnd reply?
						if( message.MessageBuffer.Length != 8 )
							throw new IOException( "Fake rwnd message was sent." );

						byte[] buffer = new byte[8];
						message.MessageBuffer.CopyTo( buffer, 0 );

						int queue = NetworkBitConverter.ToInt32( buffer, 0 );
						int rwndDelta = NetworkBitConverter.ToInt32( buffer, 4 );
						_ts.TraceEvent( TraceEventType.Information, 0, "AsyncReceiveThread: Received rwnd up {0} bytes for stream {1}", rwndDelta, message.Channel );
						_sndQueues[queue].UpReceiveWindow( rwndDelta );
					}
					else if( _rcvWaitQueueList.GetQueueItemCount( message.Channel ) != 0 ) {
						_ts.TraceEvent( TraceEventType.Verbose, 0, "AsyncReceiveThread: Received message on stream {0} that satisfies pending request", message.Channel );

						// Try to service a pending request first
						itemToComplete = _rcvWaitQueueList.Dequeue( message.Channel );
					}
					else {
						_ts.TraceEvent( TraceEventType.Verbose, 0, "AsyncReceiveThread: Received message on stream {0}, queueing", message.Channel );

						// Add to message-waiting queues
						_rcvMessageQueueList.Enqueue( message.Channel, message );

						// Decrease rwnd here and increase it when unqueued
						_rwnd[message.Channel] -= message.MessageBuffer.Length;
					}
				}

				_channel.BeginReceive( _rcvCallback, null );
				
				if( itemToComplete != null ) {
					itemToComplete.CompleteSuccess( message, false );

					// Ack the message, but don't modify rwnd (it's already been received by the app)
					if( message.MessageBuffer.Length != 0 )
						SendRwndUpdate( message.Channel, message.MessageBuffer.Length );
				}
			}
			catch( Exception ex ) {
				_ts.TraceEvent( TraceEventType.Error, 0, "Error while receiving, \"{0}\"", ex.Message );
				Abort();
				return;
			}
		}

		/// <summary>
		/// Begins an asynchronous receive operation.
		/// </summary>
		/// <param name="channel">Index of the channel from which to receive a message.</param>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the receive operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the send operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="channel"/> is an invalid channel index. It must be an index between
		///   zero and one less than the number of inbound channels.</exception>
		public IAsyncResult BeginReceive( int channel, AsyncCallback callback, object state ) {
			if( channel < 0 || channel >= _rcvWaitQueueList.QueueCount )
				throw new ArgumentOutOfRangeException( "stream", "The stream index is invalid. It must be an index between zero and one less than the number of inbound streams." );
			
			// Determine if we can complete synchronously; if so, return immediately
			lock( _rcvLock ) {
				StartReceiving();
				
				if( _rcvMessageQueueList.GetQueueItemCount( channel ) != 0 ) {
					_ts.TraceEvent( TraceEventType.Verbose, 0, "BeginReceive: Queued message satisfies request" );
					IDataMessage msg = _rcvMessageQueueList.Dequeue( channel );

					SendRwndUpdate( msg.Channel, msg.MessageBuffer.Length );
					_rwnd[channel] += msg.MessageBuffer.Length;

					return new ReceiverQueueItem( msg, callback, state );
				}

				_ts.TraceEvent( TraceEventType.Verbose, 0, "BeginReceive: Adding to request queue" );
				ReceiverQueueItem queueItem = new ReceiverQueueItem( callback, state );
				_rcvWaitQueueList.Enqueue( channel, queueItem );

				return queueItem;
			}
		}

		/// <summary>
		/// Ends an asynchronous receive operation.
		/// </summary>
		/// <param name="result">A reference to the outstanding asynchronous request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name='result'/> did not originate from a <see cref='BeginReceive'/>
		///   call on the current object.</exception>
		/// <returns>An <see cref="IDataMessage"/> that represents the received message.</returns>
		public IDataMessage EndReceive( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			ReceiverQueueItem item = result as ReceiverQueueItem;
			
			if( item == null )
				throw new ArgumentException( "The result did not originate from a BeginReceive call on the current object." );
			
			return item.End();
		}

		/// <summary>
		/// Receives a message synchronously.
		/// </summary>
		/// <param name="channel">Index of the stream from which to receive a message.</param>
		/// <returns>An <see cref="IDataMessage"/> that represents the received message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="channel"/> is an invalid channel index. It must be an index between
		///   zero and one less than the number of inbound channels.</exception>
		public IDataMessage Receive( int channel ) {
			return EndReceive( BeginReceive( channel, null, null ) );
		}
	#endregion

	#region Send support
		#region SendingQueue
		/// <summary>
		/// Represents the process that manages message-sending.
		/// </summary>
		class SendingQueue {
			ChannelMultiplexer _owner;
			ProcessingQueue<SenderQueueItem> _queue;
			int _rwnd;
			int _maxRwnd;
			AutoResetEvent _rwndSignal = new AutoResetEvent( false );

			#region SenderQueueItem
			/// <summary>
			/// Represents an asynchronous send operation.
			/// </summary>
			class SenderQueueItem : BaseAsyncResult {
				enum SendItemState {
					New,
					ClearedRwnd,
					Sent
				}

				SendItemState _state = SendItemState.New;
				SendingQueue _queue;
				IDataMessage _message;
				bool _willCompleteSync;

				public SenderQueueItem( SendingQueue queue, IDataMessage message, AsyncCallback callback, object state )
					: base( callback, state ) {
					if( queue == null )
						throw new ArgumentNullException( "queue" );

					if( message == null )
						throw new ArgumentNullException( "message" );

					_queue = queue;
					_message = message;
				}

				public void GoAsync() {
					_willCompleteSync = false;
				}

				public WaitHandle TrySend() {
					switch( _state ) {
						case SendItemState.New:
							if( _message.MessageBuffer.Length > Thread.VolatileRead( ref _queue._rwnd ) ) {
								if( _queue._rwndSignal.WaitOne( 0, false ) ) {
									if( _message.MessageBuffer.Length > Thread.VolatileRead( ref _queue._rwnd ) ) {
										_ts.TraceEvent( TraceEventType.Verbose, 0, "Waiting to send, message size {0} > rwnd {1}",
											_message.MessageBuffer.Length, _queue._rwnd );
										return _queue._rwndSignal;
									}
								}
								else {
									_ts.TraceEvent( TraceEventType.Verbose, 0, "Waiting to send, message size {0} > rwnd {1}",
										_message.MessageBuffer.Length, _queue._rwnd );
									return _queue._rwndSignal;
								}
							}

							_state = SendItemState.ClearedRwnd;
							goto case SendItemState.ClearedRwnd;

						case SendItemState.ClearedRwnd:
							// Begin the send here, but complete on another thread (unless it's synchronous)
							Interlocked.Add( ref _queue._rwnd, -_message.MessageBuffer.Length );
							_queue._owner._channel.BeginSend( _message, HandleEndSend, null );
							_state = SendItemState.Sent;
							return null;
					}

					return null;
				}

				private void HandleEndSend( IAsyncResult result ) {
					try {
						_queue._owner._channel.EndSend( result );
					}
					catch( Exception ex ) {
						_ts.TraceEvent( TraceEventType.Error, 0, "Exception \"{0}\" occured while sending message", ex.Message );
						CompleteError( ex );
						return;
					}

					Complete( _willCompleteSync );
				}

				public new void End() {
					base.End();
				}
			}
			#endregion

			/// <summary>
			/// Creates a new instance of the <see cref='SendingQueue'/> class.
			/// </summary>
			/// <param name="owner"><see cref="ChannelMultiplexer"/> that owns this sending queue.</param>
			/// <param name="maxRwnd">Initial and maximum size of the receive window on the far end. In other words, the maximum number
			///   of unacknowledged bytes that may be send on this stream.</param>
			public SendingQueue( ChannelMultiplexer owner, int maxRwnd ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );

				if( maxRwnd < 0 )
					throw new ArgumentOutOfRangeException( "maxRwnd" );

				_owner = owner;
				_queue = new ProcessingQueue<SenderQueueItem>( SendQueueHandler );
				_maxRwnd = maxRwnd;
				_rwnd = maxRwnd;
			}

			public IAsyncResult BeginQueue( IDataMessage message, AsyncCallback callback, object obj ) {
				// Queue the request and return immediately
				SenderQueueItem item = new SenderQueueItem( this, message, callback, obj );

				_queue.Enqueue( item );

				return item;
			}

			/// <summary>
			/// Increases the estimated receive window.
			/// </summary>
			/// <param name="rwndDelta">Number of bytes that have been acknowledged by the far end.</param>
			/// <exception cref="InvalidOperationException"><paramref name="rwndDelta"/> represents an illegal case, such as a zero-acknowledge.</exception>
			public void UpReceiveWindow( int rwndDelta ) {
				if( (rwndDelta <= 0) || (rwndDelta + _rwnd > _maxRwnd) ) {
					_ts.TraceEvent( TraceEventType.Error, 0, "Received illegal rwnd delta of {0}", rwndDelta );
					throw new InvalidOperationException( "Receive window decreased or increased illegally." );
				}

				Interlocked.Add( ref _rwnd, rwndDelta );
				_rwndSignal.Set();
			}

			private WaitHandle SendQueueHandler( SenderQueueItem item, bool async ) {
				if( async )
					item.GoAsync();

				return item.TrySend();
			}

			public static void EndQueue( IAsyncResult result ) {
				((SenderQueueItem) result).End();
			}

			public int ReceiveWindow {
				get {
					return _rwnd;
				}
			}
		}
		#endregion

		/// <summary>
		/// Begins an asynchronous send operation.
		/// </summary>
		/// <param name="message">Reference to an <see cref="IDataMessage"/> to send.</param>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the send operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the send operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="message"/> has an invalid channel ID. It must have a channel ID between
		///   zero and one less than the number of outbound channels.</exception>
		public IAsyncResult BeginSend( IDataMessage message, AsyncCallback callback, object state ) {
			if( message == null )
				throw new ArgumentNullException( "message" );

			if( message.Channel < 0 || message.Channel >= _sndQueues.Length )
				throw new ArgumentException( "The message has an invalid stream ID. It must have a stream ID between zero and one less than the number of outbound streams.", "message" );

			StartReceiving();
			
			return _sndQueues[message.Channel].BeginQueue( message, callback, state );
		}

		/// <summary>
		/// Ends an asynchronous send operation.
		/// </summary>
		/// <param name="result">A reference to the outstanding asynchronous request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name='result'/> did not originate from a <see cref='BeginSend'/>
		///   call on the current object.</exception>
		public void EndSend( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			SendingQueue.EndQueue( result );
		}

		/// <summary>
		/// Sends an <see cref="IDataMessage"/> synchronously.
		/// </summary>
		/// <param name="message">Reference to an <see cref="IDataMessage"/> to send.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="message"/> has an invalid stream ID. It must have a stream ID between
		///   zero and one less than the number of outbound streams.</exception>
		public void Send( IDataMessage message ) {
			EndSend( BeginSend( message, null, null ) );
		}
	#endregion
		
		public Channel GetChannel( int channel ) {
			if( channel < 0 || channel >= _sndQueues.Length || channel >= _rcvWaitQueueList.QueueCount )
				throw new ArgumentException( "The channel ID was invalid. The channel ID must be between zero and one less than the number of outbound or inbound channels.", "channel" );

			return new SubStream( this, channel );
		}

		public void Close() {
			if( _channel != null ) {
				_channel.Close();
				_channel = null;
			}
		}
		
		public void Dispose() {
			Close();
		}
		
		private void Abort() {
			_channel.Close();
		}
	}
}