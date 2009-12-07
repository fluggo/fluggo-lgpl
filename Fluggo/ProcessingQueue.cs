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
using System.Diagnostics;

namespace Fluggo
{
	public delegate WaitHandle ProcessItemCallback<T>( T item, bool async ) where T : class;

	public sealed class ProcessingQueue<T> : IDisposable where T : class {
		ProcessItemCallback<T> _processCallback;
		AsynchronousQueue<T> _asyncQueue;
		WaitOrTimerCallback _retryCallback;
		AsyncCallback _dequeueCallback;
		RegisteredWaitHandle _lastRegisteredWaitHandle;
		bool _closing;
		int _processTimeout = -1;
		
		/*
		 * A: Where should this integer go?
		 * B: Over here. And let's make sure that that method makes it in this class, too.
		 * A: You mean this one? [invokes it]
		 * B: Yeah, that one.
		 * A: I'm so glad we're game programmers.
		 * B: Me, too.
		 */

		/// <summary>
		/// Creates a new instance of the <see cref='ProcessingQueue{T}'/> class.
		/// </summary>
		/// <param name="processCallback">Delegate that attempts to process the queue items.
		///   When called, the target must try to process the item. If the item cannot be completed,
		///   the delegate should prepare it for asynchronous completion if it has not already done so.
		///   The incomplete item will then be scheduled for completion on a thread pool thread.</param>
		public ProcessingQueue( ProcessItemCallback<T> processCallback ) {
			if( processCallback == null )
				throw new ArgumentNullException( "processCallback" );

			_processCallback = processCallback;
			_asyncQueue = new AsynchronousQueue<T>();
			_dequeueCallback = HandleDequeueCallback;
			_retryCallback = HandleRetryCallback;
			
			IAsyncResult result = _asyncQueue.BeginDequeue( HandleDequeueCallback, null );
		}

		/// <summary>
		/// Gets or sets the amount of time to wait for a queue item to complete.
		/// </summary>
		/// <value>The timeout of the queue, in milliseconds, or -1 if the queue should never timeout.</value>
		public int ProcessTimeout {
			get {
				return _processTimeout;
			}
			set {
				if( value < 0 && value != -1 )
					throw new ArgumentOutOfRangeException();
					
				_processTimeout = value;
			}
		}

		public void Enqueue( T item ) {
			if( _closing )
				throw new ObjectDisposedException( null );

			_asyncQueue.Enqueue( item );
		}
		
		private void HandleDequeueCallback( IAsyncResult result ) {
			for( ;; ) {
				if( result != null && !ProcessItem( _asyncQueue.EndDequeue( result ), !result.CompletedSynchronously ) )
					return;
				
				// Try to limit the number of callbacks we do
				if( _asyncQueue.QueuedCount == 0 ) {
					// Nothing is waiting; go ahead and do a callback
					_asyncQueue.BeginDequeue( _dequeueCallback, null );
					return;
				}
				
				// Something is waiting; do no callback
				result = _asyncQueue.BeginDequeue( null, null );
			}
		}
		
		private void HandleRetryCallback( object state, bool timedout ) {
			if( ProcessItem( (T) state, true ) )
				HandleDequeueCallback( null );
		}
		
		private bool ProcessItem( T item, bool async ) {
			try {
				WaitHandle handle = _processCallback( item, true );
				
				if( handle == null )
					return true;

				_lastRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject( handle, _retryCallback, item, -1, true );
				return false;
			}
			catch {
				return true;
			}
		}

		/// <summary>
		/// Closes the processing queue and attempts to abort all waiting queue items.
		/// </summary>
		/// <remarks>It is much better to let a processing queue finish all waiting items than to
		///   try to close it prematurely. If an item is waiting on an external <see cref="WaitHandle"/>,
		///   it may stay waiting until that handle is signaled. It will not, however, invoke another
		///   callback.</remarks>
		public void Close() {
			Dispose();
		}

		public void Dispose() {
			Dispose( true );
		}
		
		void Dispose( bool disposing ) {
			if( _closing )
				return;

			_closing = true;

			// Important: throw away reference to owner so that he can be garbage-collected
			_processCallback = null;

			if( disposing ) {
				GC.SuppressFinalize( this );
			}
		}

		~ProcessingQueue() {
			Dispose( false );
		}
	}

#if false
	public class ProcessingQueue<T> : IDisposable where T : class {
		ProcessItemCallback<T> _processCallback;
		SynchronizedQueue<T> _itemQueue;
		AutoResetEvent _allowProcessEvent;
		WaitOrTimerCallback _asyncCallback;
		WaitCallback _asyncWaitCallback;
		object _enqueueLock = new object();
		volatile bool _closing;
		
		/*
		 * A: Where should this integer go?
		 * B: Over here. And let's make sure that that method makes it in this class, too.
		 * A: You mean this one? [invokes it]
		 * B: Yeah, that one.
		 * A: I'm so glad we're game programmers.
		 * B: Me, too.
		 */

		/// <summary>
		/// Creates a new instance of the <see cref='ProcessingQueue'/> class.
		/// </summary>
		/// <param name="processCallback">Delegate that attempts to process the queue items.
		///   When called, the target must try to process the item. If the item cannot be completed,
		///   the delegate should prepare it for asynchronous completion if it has not already done so.
		///   The incomplete item will then be scheduled for completion on a thread pool thread.</param>
		public ProcessingQueue( ProcessItemCallback<T> processCallback ) {
			if( processCallback == null )
				throw new ArgumentNullException( "processCallback" );
			
			_processCallback = processCallback;
			_allowProcessEvent = new AutoResetEvent( true );
			_itemQueue = new SynchronizedQueue<T>();
			_asyncCallback = AsyncProcessCallback;
			_asyncWaitCallback = AsyncProcessCallback;
		}
		
		public void Enqueue( T item ) {
			if( _closing )
				throw new ObjectDisposedException( null );

			if( _allowProcessEvent.WaitOne( 0, false ) ) {
				// The callback is not waiting for anything, so we can do what we want here
				WaitHandle waitObj = _processCallback( item, false );
				
				if( waitObj == null ) {
					// Allow processing to continue
					if( _itemQueue.Count != 0 ) {
						// Items were queued after we started; go async to process them
						ThreadPool.QueueUserWorkItem( _asyncWaitCallback, null );
					}
					else {
						// Check again while under the lock; if we stall, we're only stalling a user thread
						// for another user thread, and there's a good chance we'll have to activate the async thread
						lock( _enqueueLock ) {
							// Allow processing to continue
							if( _itemQueue.Count != 0 ) {
								// Items were queued after we started; go async to process them
								ThreadPool.QueueUserWorkItem( _asyncWaitCallback, null );
							}
							else {
								// Nobody (so far as we know) is waiting for processing
								_allowProcessEvent.Set();
							}
						}
					}
				}
				else {
					// Item is not complete; sic the async process on it (jumping ahead of any queue items; they came after us)
					ThreadPool.RegisterWaitForSingleObject( waitObj, _asyncCallback, item, -1, true );
				}
			}
			else {
				// Async queue is running; must go async
				lock( _enqueueLock ) {
					_itemQueue.Enqueue( item );
				}
				
				if( _allowProcessEvent.WaitOne( 0, false ) ) {
					// We were able to acquire the "lock," so one of three things happened:
					//		Another Enqueue call had held the lock, missed our queued item, and gave up the lock
					//			without activating the callback
					//		The asynchronous queue processed our item and quit
					//		The asynchronous queue missed our item and quit
					// We'll know if it's #1 or #3 if the queue isn't empty
					
					if( _itemQueue.Count != 0 ) {
						ThreadPool.QueueUserWorkItem( _asyncWaitCallback, null );
					}
				}
				else {
					// Ways to reach here:
					//		The asynchronous callback is running and will process our item
					//		The asynchronous callback is in the process of quitting, hasn't noticed our item, and hasn't signaled the event yet
					//		Another Enqueue call has seen our item and is about to activate the AsyncProcessCallback
					//		Another Enqueue call is about to signal the event and has not noticed our item
					Debug.WriteLine( "How often does this happen?" );
					//throw new UnexpectedException();
				}
			}
		}
		
		private void AsyncProcessCallback( object state ) {
			AsyncProcessCallback( state, false );
		}
		
		private void AsyncProcessCallback( object state, bool timedOut ) {
			if( _closing )
				return;
				
			for( ;; ) {
				// Try to finish processing the item
				T waitingItem;
				
				if( state == null ) {
					if( !_itemQueue.Dequeue( out waitingItem ) ) {
						// Try again with the lock (if we stall here, it's a pretty good
						// sign we're about to get another item to process)
						lock( _enqueueLock ) {
							if( !_itemQueue.Dequeue( out waitingItem ) ) {
								_allowProcessEvent.Set();
								return;
							}
						}
					}
				}
				else {
					waitingItem = (T) state;
				}
					
				WaitHandle waitObj = null;
				
				try {
					waitObj = _processCallback( waitingItem, true );
				}
				catch {}
				
				if( waitObj != null ) {
					ThreadPool.RegisterWaitForSingleObject( waitObj, _asyncCallback, waitingItem, -1, true );
					return;
				}
				
				// Loop around and process the next item
				state = null;
			}
		}
		
		/// <summary>
		/// Closes the processing queue and attempts to abort all waiting queue items.
		/// </summary>
		/// <remarks>It is much better to let a processing queue finish all waiting items than to
		///   try to close it prematurely. If an item is waiting on an external <see cref="WaitHandle"/>,
		///   it may stay waiting until that handle is signaled. It will not, however, invoke another
		///   callback.</remarks>
		public void Close() {
			if( _closing )
				return;
				
			_closing = true;
			
			// Important: throw away reference to owner so that he can be garbage-collected
			_processCallback = null;
				
			while( !_allowProcessEvent.WaitOne( 0, false ) ) {
				_allowProcessEvent.Set();
			}
			
			GC.SuppressFinalize( this );
		}
		
		public void Dispose() {
			Close();
		}
		
		~ProcessingQueue() {
			Close();
		}
	}
#endif
}
