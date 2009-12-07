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

namespace Fluggo.Communications
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
		///   The <see cref="WaitHandle"/> retrieved from this property is signalled when a derived class calls <see cref="Complete"/>
		///   or <see cref="CompleteError"/>.</remarks>
		public WaitHandle AsyncWaitHandle {
			get {
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
		/// <remarks>This property is false unless <see cref="Complete"/> is called with its parameter set to true.</remarks>
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
		/// <remarks>This property is available after</remarks>
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
		protected void Complete( bool synchronous ) {
			Complete( synchronous, true );
		}
		
		/// <summary>
		/// Marks the result as complete and releases any operations waiting on this result.
		/// </summary>
		/// <param name="synchronous">True if the operation completed synchronously, false otherwise.</param>
		/// <param name="raiseEventSynchronously">True if the callback should be called in this thread or false if it should be called in another thread.</param>
		/// <exception cref="InvalidOperationException">The operation is already complete.</exception>
		protected void Complete( bool synchronous, bool raiseEventSynchronously ) {
			lock( _lock ) {
				if( _isCompleted )
					throw new InvalidOperationException( "The operation is already complete." );

				_isCompleted = true;
				_completedSynchronously = synchronous;

				if( _event != null )
					_event.Set();

				if( _callback != null ) {
					// Invoke the callback synchronously only if we completed synchronously;
					// otherwise, the operation that completed us probably doesn't want to block
					if( raiseEventSynchronously ) {
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
							catch {
							}
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
				if( !AsyncWaitHandle.WaitOne( _timeout, false ) ) {
					_event.Close();
					throw new TimeoutException();
				}
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
		/// <exception cref="InvalidOperationException">The operation is already complete.</exception>
		public void CompleteError( Exception ex ) {
			if( ex == null )
				throw new ArgumentNullException( "ex" );
			
			lock( _lock ) {
				if( _isCompleted )
					throw new InvalidOperationException( "The operation is already complete." );

				_ex = ex;
				Complete( false );
			}
		}
	}

	/// <summary>
	/// Represents an asynchronous result that is meant to be completed synchronously and with one result.
	/// </summary>
	/// <remarks>This asynchronous result only works properly if <see cref="Complete"/> or <see cref="CompleteError"/> is called
	///   before returning it to the end user. Otherwise, the caller can modify the result's state.</remarks>
	/// <typeparam name="T">Type of the result.</typeparam>
	class EmptyAsyncResult<T> : BaseAsyncResult {
		T _result;

		public EmptyAsyncResult( AsyncCallback callback, object state )
			: base( callback, state ) {
		}

		public void Complete( T result ) {
			if( !IsCompleted )
				_result = result;
			
			base.Complete( true );
		}

		public new T End() {
			base.End();
			return _result;
		}
	}
}