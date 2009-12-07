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
using System.IO;

namespace Fluggo.Communications
{
	abstract class CompositeStream : Stream {
		Stream _root;
		bool _canRead, _canSeek, _canWrite, _closed;
		
		protected CompositeStream( Stream root ) {
			if( root == null )
				throw new ArgumentNullException( "root" );
				
			_root = root;
			_canRead = true;
			_canWrite = true;
			_canSeek = true;
		}

		protected CompositeStream( Stream root, bool canRead, bool canWrite, bool canSeek ) {
			if( root == null )
				throw new ArgumentNullException( "root" );

			_root = root;
			_canRead = canRead;
			_canWrite = canWrite;
			_canSeek = canSeek;
		}
		
		protected virtual Stream Root
			{ get { return _root; } }
	
		public override bool CanRead {
			get { return _canRead && _root.CanRead && !_closed; }
		}

		public override bool CanSeek {
			get { return _canSeek && _root.CanSeek && !_closed; }
		}

		public override bool CanWrite {
			get { return _canWrite && _root.CanWrite && !_closed; }
		}

		public override bool CanTimeout {
			get {
				return _root.CanTimeout;
			}
		}

		public override void Flush() {
			if( !_canWrite )
				throw new NotSupportedException();
			
			if( _closed )
				throw new ObjectDisposedException( null );
			
			_root.Flush();
		}

		public override long Length {
			get {
				if( !_canSeek )
					throw new NotSupportedException();

				if( _closed )
					throw new ObjectDisposedException( null );

				return _root.Length;
			}
		}

		public override long Position {
			get {
				if( !_canSeek )
					throw new NotSupportedException();

				if( _closed )
					throw new ObjectDisposedException( null );

				return _root.Position;
			}
			set {
				if( !_canSeek )
					throw new NotSupportedException();

				if( _closed )
					throw new ObjectDisposedException( null );

				_root.Position = value;
			}
		}

		public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( !_canRead )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			return _root.BeginRead( buffer, offset, count, callback, state );
		}

		public override int EndRead( IAsyncResult asyncResult ) {
			if( !_canRead )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			return _root.EndRead( asyncResult );
		}
		
		public override int Read( byte[] buffer, int offset, int count ) {
			if( !_canRead )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			return _root.Read( buffer, offset, count );
		}

		public override int ReadByte() {
			if( !_canRead )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			return _root.ReadByte();
		}

		public override int ReadTimeout {
			get {
				return _root.ReadTimeout;
			}
			set {
				_root.ReadTimeout = value;
			}
		}

		public override int WriteTimeout {
			get {
				return _root.WriteTimeout;
			}
			set {
				_root.WriteTimeout = value;
			}
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			if( !_canSeek )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			return _root.Seek( offset, origin );
		}

		public override void SetLength( long value ) {
			if( !_canSeek )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			_root.SetLength( value );
		}

		public override IAsyncResult BeginWrite( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
			if( !_canWrite )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			return _root.BeginWrite( buffer, offset, count, callback, state );
		}

		public override void EndWrite( IAsyncResult asyncResult ) {
			if( !_canWrite )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			_root.EndWrite( asyncResult );
		}
		
		public override void Write( byte[] buffer, int offset, int count ) {
			if( !_canWrite )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			_root.Write( buffer, offset, count );
		}

		public override void WriteByte( byte value ) {
			if( !_canWrite )
				throw new NotSupportedException();

			if( _closed )
				throw new ObjectDisposedException( null );

			_root.WriteByte( value );
		}

		public override void Close() {
			base.Close();
			_closed = true;
		}
	}
}
