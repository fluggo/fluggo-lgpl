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

namespace Fluggo
{
	/// <summary>
	/// Implements the basic features of the <see cref='IAsyncResult'/> interface.
	/// </summary>
	public class BaseAsyncResult : IAsyncResult {
		object _state, _lock = new object();
		ManualResetEvent _event;
		bool _isCompleted, _completedSynchronously;
		static ManualResetEvent __completeSyncEvent = new ManualResetEvent( true );
		AsyncCallback _callback;
		Exception _ex;
		int _timeout;

		/// <summary>
		/// Creates a new instance of the <see cref='BaseAsyncResult'/> class.
		/// </summary>
		/// <param name="callback">Optional <see cref="AsyncCallback"/> to call when the operation is complete.</param>
		/// <param name="state">Optional user-supplied state object to pass to <paramref name="callback"/>.</param>
		public BaseAsyncResult( AsyncCallback callback, object state ) : this( callback, state, -1 ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='BaseAsyncResult'/> class.
		/// </summary>
		/// <param name="callback">Optional <see cref="AsyncCallback"/> to call when the operation is complete.</param>
		/// <param name="state">Optional user-supplied state object to pass to <paramref name="callback"/>.</param>
		/// <param name="timeout">Timeout, in milliseconds, for the wait. Specify -1 to never time out.</param>
		public BaseAsyncResult( AsyncCallback callback, object state, int timeout ) {
			_state = state;
			_isCompleted = false;
			_callback = callback;
			_timeout = timeout;
		}

		/// <summary>
		/// Gets the state object passed to the constructor.
		/// </summary>
		/// <value>The state object passed to the constructor, if any.</value>
		public object AsyncState {
			get { return _state; }
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="BaseAsyncResult"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access to the <see cref="BaseAsyncResult"/>.</value>
		protected object SyncRoot {
			get {
				return _lock;
			}
		}

		/// <summary>
		/// Gets a <see cref="WaitHandle"/> that can be used to wait for an asynchronous operation to complete.
		/// </summary>
		/// <value>A <see cref="WaitHandle"/> that can be used to wait for an asynchronous operation to complete.</value>
		/// <remarks>
		///   A <see cref="WaitHandle"/> for this property is not created until the property is accessed.
		///   The <see cref="WaitHandle"/> retrieved from this property is signalled when a derived class calls <see cref="Complete(bool)"/>
		///   or <see cref="CompleteError(Exception)"/>.</remarks>
		public WaitHandle AsyncWaitHandle {
			get {
				// Avoid taking the lock if possible
				if( _event != null )
					return _event;
				
				lock( _lock ) {
					if( _event != null )
						return _event;
						
					if( _isCompleted )
						return (_event = __completeSyncEvent);
					else
						return (_event = new ManualResetEvent( false ));
				}
			}
		}

		/// <summary>
		/// Gets a value that represents whether the asynchronous operation completed synchronously.
		/// </summary>
		/// <value>True if the asynchronous operation completed synchronously, false otherwise.</value>
		/// <remarks>This property is false unless <see cref="Complete(bool)"/> is called with its parameter set to true.</remarks>
		public bool CompletedSynchronously {
			get {
				return _completedSynchronously;
			}
		}

		/// <summary>
		/// Gets a value that represents whether the asynchronous operation has completed.
		/// </summary>
		/// <value>True if the asynchronous operation has completed, false otherwise.</value>
		public bool IsCompleted
		{
			get { return _isCompleted; }
		}

		/// <summary>
		/// Gets the exception that occured during the asynchronous operation, if any.
		/// </summary>
		/// <value>The exception that occured during the asynchronous operation, or <see langword='null'/> if no exception occured.</value>
		/// <exception cref="InvalidOperationException">The operation has not completed yet.</exception>
		public Exception Exception {
			get {
				if( !_isCompleted )
					throw new InvalidOperationException( "The operation has not completed yet." );
					
				return _ex;
			}
		}

		/// <summary>
		/// Marks the result as complete and releases any operations waiting on this result.
		/// </summary>
		/// <param name="synchronous">True if the operation completed synchronously, false otherwise.</param>
		/// <exception cref="InvalidOperationException">The operation is already complete.</exception>
		/// <remarks>By default, the user's callback, if any, will be raised synchronously.</remarks>
		protected void Complete( bool synchronous ) {
			Complete( synchronous, true );
		}
		
		/// <summary>
		/// Marks the result as complete and releases any operations waiting on this result.
		/// </summary>
		/// <param name="synchronous">True if the operation completed synchronously, false otherwise.</param>
		/// <param name="raiseSynchronous">True if the callback should be raised on this thread, false otherwise.</param>
		/// <exception cref="InvalidOperationException">The operation is already complete.</exception>
		protected void Complete( bool synchronous, bool raiseSynchronous ) {
			lock( _lock ) {
				if( _isCompleted )
					throw new InvalidOperationException( "The operation is already complete." );

				_isCompleted = true;
				_completedSynchronously = synchronous;

				// If the user picked up an event, signal it
				if( _event != null )
					_event.Set();

				// Raise the user's callback
				if( _callback != null ) {
					if( raiseSynchronous ) {
						try {
							_callback( this );
						}
						catch {}
					}
					else {
						_callback.BeginInvoke( this, delegate( IAsyncResult result ) {
							try {
								_callback.EndInvoke( result );
							}
							catch {}
						}, null );
					}
				}
			}
		}
		
		/// <summary>
		/// Blocks until the operation has finished and releases the <see cref="BaseAsyncResult"/>'s resources.
		/// </summary>
		protected void End() {
			if( !_isCompleted ) {
				// The likelihood of a race condition is small here; we're doing this more
				// to cause a memory sync
				WaitHandle handle = null;
				
				lock( _lock ) {
					if( !_isCompleted )
						handle = AsyncWaitHandle;
				}
				
				if( handle != null && !AsyncWaitHandle.WaitOne( _timeout, false ) )
					throw new TimeoutException();
			}
				
			if( _event != null )
				_event.Close();

			if( _ex != null )
				throw _ex;
		}
		
		/// <summary>
		/// Marks the result as complete with an error and releases any operations waiting on this result.
		/// </summary>
		/// <param name="ex">Exception that occured during the operation.</param>
		/// <exception cref='ArgumentNullException'><paramref name='ex'/> is <see langword='null'/>.</exception>
		/// <exception cref="InvalidOperationException">The operation is already complete.</exception>
		/// <remarks>By default, the user's callback, if any, will be raised synchronously.</remarks>
		public void CompleteError( Exception ex ) {
			CompleteError( ex, true );
		}
		
		/// <summary>
		/// Marks the result as complete with an error and releases any operations waiting on this result.
		/// </summary>
		/// <param name="ex">Exception that occured during the operation.</param>
		/// <param name="raiseSynchronous">True if the callback should be raised on this thread, false otherwise.</param>
		/// <exception cref='ArgumentNullException'><paramref name='ex'/> is <see langword='null'/>.</exception>
		/// <exception cref="InvalidOperationException">The operation is already complete.</exception>
		public void CompleteError( Exception ex, bool raiseSynchronous ) {
			if( ex == null )
				throw new ArgumentNullException( "ex" );
			
			lock( _lock ) {
				if( _isCompleted )
					throw new InvalidOperationException( "The operation is already complete." );

				_ex = ex;
				Complete( false, raiseSynchronous );
			}
		}
	}

	/// <summary>
	/// Represents an asynchronous result that is meant to be completed synchronously and with one result.
	/// </summary>
	/// <remarks>This asynchronous result only works properly if <see cref="Complete"/> or <see cref="BaseAsyncResult.CompleteError(Exception)"/> is called
	///   before returning it to the end user. Otherwise, the caller can modify the result's state.</remarks>
	/// <typeparam name="T">Type of the result.</typeparam>
	public class EmptyAsyncResult<T> : BaseAsyncResult {
		T _result;

		public EmptyAsyncResult( AsyncCallback callback, object state )
			: base( callback, state ) {
		}

		public void Complete( T result ) {
			if( !IsCompleted )
				_result = result;
			
			base.Complete( true, true );
		}

		public new T End() {
			base.End();
			return _result;
		}
	}
}