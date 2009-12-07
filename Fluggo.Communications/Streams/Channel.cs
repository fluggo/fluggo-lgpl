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

namespace Fluggo.Communications {
	public abstract class Channel {
		sealed class EmptyRead : BaseAsyncResult {
			IMessageBuffer _value;
		
			public EmptyRead( AsyncCallback callback, object state ) : base( callback, state ) {
			}
			
			public void Complete( IMessageBuffer value ) {
				_value = value;
				Complete( true );
			}
			
			public new IMessageBuffer End() {
				base.End();
				return _value;
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
		/// Begins an asynchronous receive operation.
		/// </summary>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the receive operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the asynchronous operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <remarks>The default implementation of this method invokes <see cref="Receive"/> synchronously.</remarks>
		public virtual IAsyncResult BeginReceive( AsyncCallback callback, object state ) {
			EmptyRead read = new EmptyRead( callback, state );

			try {
				read.Complete( Receive() );
			}
			catch( Exception ex ) {
				read.CompleteError( ex );
			}

			return read;
		}
		
		public abstract IMessageBuffer Receive();
		
		public virtual IMessageBuffer EndReceive( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyRead read = result as EmptyRead;

			if( read == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginReceive call on this object.", "result" );

			return read.End();
		}

		/// <summary>
		/// Begins an asynchronous send operation.
		/// </summary>
		/// <param name="buffer">Byte array containing the message to send.</param>
		/// <param name="offset">Index in <paramref name="buffer"/> at which the message begins.</param>
		/// <param name="count">Length of the message in bytes.</param>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the send operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the send operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <remarks>The default implementation of this method invokes <see cref="Send"/> synchronously.</remarks>
		public virtual IAsyncResult BeginSend( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			EmptyWrite write = new EmptyWrite( callback, state );

			try {
				Send( buffer, offset, count );
				write.Complete();
			}
			catch( Exception ex ) {
				write.CompleteError( ex );
			}

			return write;
		}

		/// <summary>
		/// Ends an asynchronous send operation.
		/// </summary>
		/// <param name="result">A reference to the outstanding asynchronous request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name='result'/> did not originate from a <see cref='BeginSend'/>
		///   call on the current object.</exception>
		public virtual void EndSend( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyWrite write = result as EmptyWrite;

			if( write == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginSend call on this object.", "result" );

			write.End();
		}

		public abstract void Send( byte[] buffer, int offset, int count );
		
		public virtual void Close() { Dispose(); }
		public void Dispose() {
			Dispose( true );
		}
		
		~Channel() {
			Dispose( false );
		}
		
		protected virtual void Dispose( bool disposing ) {
			if( disposing )
				GC.SuppressFinalize( this );
		}

		public abstract bool CanReceive { get; }
		public abstract bool CanSend { get; }

		/// <summary>
		/// Gets the maximum number of bytes that can be send or received in a single message.
		/// </summary>
		/// <value>The maximum number of bytes that can be send or received in a single message, or -1 if there is no limit.</value>
		/// <remarks>The default implementation returns -1.</remarks>
		public virtual int MaximumPayloadLength { get { return -1; } }

		/// <summary>
		/// Gets the current size of the receive window.
		/// </summary>
		/// <value>The current size of the receive window, which is the maximum number of bytes which can be sent without delay,
		///   or -1 if there is no receive window.</value>
		/// <remarks>The default implementation returns -1.</remarks>
		public virtual int ReceiveWindow { get { return -1; } }
	}
	
	public abstract class Channel<T> {
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
		/// Begins an asynchronous receive operation.
		/// </summary>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the receive operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the asynchronous operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <remarks>The default implementation of this method invokes <see cref="Receive"/> synchronously.</remarks>
		public virtual IAsyncResult BeginReceive( AsyncCallback callback, object state ) {
			EmptyRead read = new EmptyRead( callback, state );

			try {
				T value;
				read.Complete( Receive( out value ), value );
			}
			catch( Exception ex ) {
				read.CompleteError( ex );
			}

			return read;
		}
		
		public abstract bool Receive( out T value );
		
		public virtual bool EndReceive( IAsyncResult result, out T value ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyRead read = result as EmptyRead;

			if( read == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginReceive call on this object.", "result" );

			return read.End( out value );
		}

		/// <summary>
		/// Begins an asynchronous send operation.
		/// </summary>
		/// <param name="message">Message to send.</param>
		/// <param name="callback">An optional <see cref="AsyncCallback"/> delegate that references the method to invoke
		///   when the send operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the write operation.
		///   This object is passed to the <paramref name="callback"/> delegate when the operation completes.</param>
		/// <returns>An <see cref='IAsyncResult'/> object indicating the status of the asynchronous operation.</returns>
		/// <remarks>The default implementation of this method invokes <see cref="WriteValue"/> synchronously.</remarks>
		public virtual IAsyncResult BeginSend( T message, AsyncCallback callback, object state ) {
			EmptyWrite write = new EmptyWrite( callback, state );

			try {
				Send( message );
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
		/// <exception cref="ArgumentException"><paramref name='result'/> did not originate from a <see cref='BeginSend'/>
		///   call on the current object.</exception>
		public virtual void EndSend( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyWrite write = result as EmptyWrite;

			if( write == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginSend call on this object.", "result" );

			write.End();
		}

		public abstract void Send( T value );
		
		public virtual void Close() { Dispose(); }
		public void Dispose() {
			Dispose( true );
		}
		
		~Channel() {
			Dispose( false );
		}
		
		protected virtual void Dispose( bool disposing ) {
			if( disposing )
				GC.SuppressFinalize( this );
		}

		public abstract bool CanReceive { get; }
		public abstract bool CanSend { get; }

		/// <summary>
		/// Gets the maximum number of bytes that can be send or received in a single message.
		/// </summary>
		/// <value>The maximum number of bytes that can be send or received in a single message, not including meta-message data, or -1 if there is no limit.</value>
		/// <remarks>The default implementation returns -1.</remarks>
		public virtual int MaximumPayloadLength { get { return -1; } }

		/// <summary>
		/// Gets the current size of the receive window.
		/// </summary>
		/// <value>The current size of the receive window, which is the maximum number of bytes which can be sent without delay,
		///   not including meta-message data, or -1 if there is no receive window.</value>
		/// <remarks>The default implementation returns -1.</remarks>
		public virtual int ReceiveWindow { get { return -1; } }
	}
}
