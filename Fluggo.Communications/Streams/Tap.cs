using System;
using System.IO;
using System.Threading;

namespace Fluggo.Communications {
	/// <summary>
	/// Copies stream data to another stream without interfering with the flow of the original.
	/// </summary>
	/// <remarks>The stream created by this class emulates the root stream specified in its constructor. When a class
	///     reads from or writes to the tap, it is read from or written to the root and optionally copied to another stream.
	///     Seek operations are not duplicated on the target streams, though.</remarks>
	public sealed class Tap : Stream {
		Stream _root, _writeTap, _readTap;

		/// <summary>
		/// Creates a new instance of the <see cref='Tap'/> class.
		/// </summary>
		/// <param name="root">A reference to the stream to tap.</param>
		/// <param name="readTap">Optional stream to receive data read from the root stream.</param>
		/// <param name="writeTap">Optional stream to receive data written to the root stream.</param>
		/// <exception cref="ArgumentException">Either <paramref name="readTap"/> or <paramref name="writeTap"/> is specified and
		///   does not support writing.</exception>
		public Tap( Stream root, Stream readTap, Stream writeTap ) {
			if( root == null )
				throw new ArgumentNullException( "root" );

			_root = root;
			
			if( readTap != null && !readTap.CanWrite )
				throw new ArgumentException( "The specified read tap stream does not support writing." );

			if( writeTap != null && !writeTap.CanWrite )
				throw new ArgumentException( "The specified write tap stream does not support writing." );
				
			_readTap = readTap;
			_writeTap = writeTap;
		}

		public override bool CanRead {
			get { return _root.CanRead; }
		}

		public override bool CanSeek {
			get { return _root.CanSeek; }
		}

		public override bool CanWrite {
			get { return _root.CanWrite; }
		}

		public override bool CanTimeout {
			get {
				return _root.CanTimeout;
			}
		}

		public override void Flush() {
			_root.Flush();
		}

		public override long Length {
			get { return _root.Length; }
		}

		public override long Position {
			get {
				return _root.Position;
			}
			set {
				_root.Position = value;
			}
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			int result = _root.Read( buffer, offset, count );
			
			if( _readTap != null && _readTap.CanWrite ) {
				_readTap.BeginWrite( buffer, offset, result, delegate( IAsyncResult ar ) {
					_readTap.EndWrite( ar );
				}, null );
			}
			
			return result;
		}

		class ReadOp : IAsyncResult {
			public byte[] Buffer;
			public int Offset;
			public int Count;
			public AsyncCallback Callback;
			public object State;
			public Tap Owner;
			public IAsyncResult Root;
			public Exception Ex;
			
			public void HandleCallback( IAsyncResult result ) {
				try {
					Count = Owner._root.EndRead( result );
				}
				catch( Exception ex ) {
					Ex = ex;
					return;
				}
				
				if( Owner._readTap.CanWrite ) {
					Owner._readTap.BeginWrite( Buffer, Offset, Count, delegate( IAsyncResult ar ) {
						Owner._readTap.EndWrite( ar );
					}, null );
				}
				
				if( Callback != null )
					Callback( this );
			}

			public object AsyncState {
				get { return State; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return Root.AsyncWaitHandle; }
			}

			public bool CompletedSynchronously {
				get { return Root.CompletedSynchronously; }
			}

			public bool IsCompleted {
				get { return Root.IsCompleted; }
			}
		}
		
		public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( _readTap != null && _readTap.CanWrite ) {
				ReadOp op = new ReadOp();
				op.Buffer = buffer;
				op.Offset = offset;
				op.Callback = callback;
				op.State = state;
				op.Owner = this;
				
				op.Root = _root.BeginRead( buffer, offset, count, op.HandleCallback, state );
				return op;
			}
			else {
				return _root.BeginRead( buffer, offset, count, callback, state );
			}
		}

		public override int EndRead( IAsyncResult asyncResult ) {
			ReadOp op = asyncResult as ReadOp;
			
			if( op != null ) {
				return op.Count;
			}
			
			return _root.EndRead( asyncResult );
		}
		
		public override long Seek( long offset, SeekOrigin origin ) {
			return _root.Seek( offset, origin );
		}

		public override void SetLength( long value ) {
			_root.SetLength( value );
		}

		public override void Write( byte[] buffer, int offset, int count ) {
			if( _writeTap != null && _writeTap.CanWrite ) {
				_writeTap.BeginWrite( buffer, offset, count, delegate( IAsyncResult result ) {
					_writeTap.EndWrite( result );
				}, null );
			}
			
			_root.Write( buffer, offset, count );
		}

		public override IAsyncResult BeginWrite( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( _writeTap != null && _writeTap.CanWrite ) {
				_writeTap.BeginWrite( buffer, offset, count, delegate( IAsyncResult result ) {
					_writeTap.EndWrite( result );
				}, null );
			}

			return _root.BeginWrite( buffer, offset, count, callback, state );
		}

		public override void EndWrite( IAsyncResult asyncResult ) {
			_root.EndWrite( asyncResult );
		}
	}
}