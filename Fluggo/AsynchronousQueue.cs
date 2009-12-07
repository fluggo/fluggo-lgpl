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

namespace Fluggo
{
	/// <summary>
	/// Represents an asynchronous queue.
	/// </summary>
	/// <typeparam name="T">Type of the items in the queue.</typeparam>
	/// <remarks>As part of my effort to capture every kind of queue imaginable, this is a queue in which you can wait
	///   for a value to be queued by someone else. You can choose to wait synchronously or asynchronously.</remarks>
	public sealed class AsynchronousQueue<T> : IDisposable {
		class WaitQueueRequest : BaseAsyncResult {
			T _result;
			bool _willCompleteAsync;
			
			public WaitQueueRequest( AsyncCallback callback, object state, int timeout ) : base( callback, state, timeout ) {
				_willCompleteAsync = true;
			}

			public WaitQueueRequest( T result, AsyncCallback callback, object state, int timeout ) : base( callback, state, timeout ) {
				Complete( result, true );
			}

			public void Complete( T result, bool raiseSynchronous ) {
				_result = result;
				Complete( _willCompleteAsync, raiseSynchronous );
			}
			
			public new T End() {
				base.End();
				return _result;
			}
		}
		
		SynchronizedQueue<WaitQueueRequest> _dequeueRequests = new SynchronizedQueue<WaitQueueRequest>();
		SynchronizedQueue<T> _queuedItems = new SynchronizedQueue<T>();
		object _lock = new object();
		bool _isClosed;
		int _timeout;

		/// <summary>
		/// Creates a new instance of the <see cref='AsynchronousQueue{T}'/> class.
		/// </summary>
		public AsynchronousQueue() : this( -1 ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='AsynchronousQueue{T}'/> class.
		/// </summary>
		/// <param name="timeout">The default timeout for the queue, in milliseconds. If this value is -1, the
		///   queue will never timeout.</param>
		public AsynchronousQueue( int timeout ) {
			_timeout = timeout;
		}

		/// <summary>
		/// Gets or sets the timeout of the queue, in milliseconds.
		/// </summary>
		/// <value>The timeout of the queue, in milliseconds, or -1 if the queue should never timeout. The default value is -1.</value>
		/// <remarks>When setting this property, the new setting will only affect new requests.</remarks>
		public int Timeout {
			get {
				return _timeout;
			}
			set {
				if( value < 0 && value != -1 )
					throw new ArgumentOutOfRangeException();
				
				_timeout = value;
			}
		}
		
		public int QueuedCount
			{ get { lock( _lock ) { return _queuedItems.Count; } } }
		public int RequestCount
			{ get { lock( _lock ) { return _dequeueRequests.Count; } } }
		
		/// <summary>
		/// Begins to dequeue an item.
		/// </summary>
		/// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
		/// <param name="state">A user-provided object that distinguishes this particular asynchronous dequeue request from other requests.</param>
		/// <returns>An <see cref='IAsyncResult'/> that represents the asynchronous dequeue, which could still be pending.</returns>
		public IAsyncResult BeginDequeue( AsyncCallback callback, object state ) {
			if( _isClosed )
				throw new ObjectDisposedException( null );
		
			T value;

			// BJC: Remember this trick. This is a trick to avoid taking a lock unnecessarily.
			// We only need to take the lock if we want to add a queue request. We can check
			// for the availability of an item before taking the lock for free, and we'll probably
			// avoid a context switch while we're at it.
			//
			// One of the great things about this technique is that it needs no CERs or other
			// critical section markers. It just works.
			if( !_queuedItems.Dequeue( out value ) ) {
				// BJC: Now that we've determined the queue is empty, we can add a queue request.
				// But we need to take the lock to do that, and since our first check wasn't performed
				// under the lock, there's still a chance that another item appeared in the mean time,
				// so we check again once we're under the lock to be sure. Checking is cheap.
				lock( _lock ) {
					if( !_queuedItems.Dequeue( out value ) ) {
						// Add to asynchronous queue
						WaitQueueRequest request = new WaitQueueRequest( callback, state, _timeout );

						_dequeueRequests.Enqueue( request );
						return request;
					}
				}
			}

			// Synchronous complete
			return new WaitQueueRequest( value, callback, state, -1 );
		}
		
		public T EndDequeue( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );
				
			WaitQueueRequest request = result as WaitQueueRequest;
				
			if( request == null )
				throw new ArgumentException( "Value is not an asyncrhonous result from this class.", "result" );
				
			return request.End();
		}
		
		/// <summary>
		/// Removes an item from the beginning of the queue.
		/// </summary>
		/// <returns>The item removed from the queue.</returns>
		/// <exception cref="TimeoutException">The <see cref="Timeout"/> was reached while waiting for an item.</exception>
		/// <remarks>If an item is present in the queue, this method returns immediately. If there are no items,
		///     it will block the thread and wait for an item to be queued. If <see cref="Timeout"/> is set to a value
		///     other than -1 before the call is made, then <see cref="Dequeue"/> will only wait for the timeout interval.
		///     If the method times out before an item appears, a <see cref="TimeoutException"/> is thrown.
		///   <para>You can continue doing other things while waiting if you use the <see cref="BeginDequeue"/> and <see cref="EndDequeue"/>
		///     methods.</para></remarks>
		public T Dequeue() {
			return EndDequeue( BeginDequeue( null, null ) );
		}
		
		public void Enqueue( T value ) {
			if( _isClosed )
				throw new ObjectDisposedException( null );
			
			// Check to see if someone's waiting, and if so, satisfy them
			WaitQueueRequest request;

			if( !_dequeueRequests.Dequeue( out request ) ) {
				// We'll want to check again under the lock just to make sure...
				lock( _lock ) {
					if( !_dequeueRequests.Dequeue( out request ) ) {
						// Add to synchronous queue
						_queuedItems.Enqueue( value );
						return;
					}
				}
			}

			// Satisfy asynchronous queue
			request.Complete( value, false );
		}
		
		public void Dispose() {
			_isClosed = true;
		}
	}
}
