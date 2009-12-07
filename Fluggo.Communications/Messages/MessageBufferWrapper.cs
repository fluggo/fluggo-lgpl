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

namespace Fluggo.Communications
{
	/// <summary>
	/// Allows a byte array to be used as a <see cref="IMessageBuffer"/>.
	/// </summary>
	/// <remarks>This class does not make a copy of the message buffer, so any
	///   changes made to the original array will affect the message buffer. Since most
	///   users of <see cref="IMessageBuffer"/> will assume that the buffer does not change,
	///   you should avoid changing the data once it's been wrapped.</remarks>
	public class MessageBufferWrapper : IMessageBuffer {
		ArraySegment<byte> _seg;

		/// <summary>
		/// Creates a new instance of the <see cref='MessageBufferWrapper'/> class.
		/// </summary>
		/// <param name="buffer">Array to wrap. The entire array will be used as a source for the message buffer.</param>
		public MessageBufferWrapper( byte[] buffer ) {
			_seg = new ArraySegment<byte>( buffer );
		}

		/// <summary>
		/// Creates a new instance of the <see cref='MessageBufferWrapper'/> class.
		/// </summary>
		/// <param name="buffer">Array to wrap.</param>
		/// <param name="offset">Offset into <paramref name="buffer"/> that the message begins.</param>
		/// <param name="length">Length of data in the array.</param>
		public MessageBufferWrapper( byte[] buffer, int offset, int length ) {
			_seg = new ArraySegment<byte>( buffer, offset, length );
		}

		/// <summary>
		/// Copies the contents of the message buffer to the given byte array.
		/// </summary>
		/// <param name="buffer">Buffer to receive the results.</param>
		/// <param name="index">Index in <paramref name="buffer"/> at which to start copying.</param>
		/// <remarks>There must be enough room in the buffer to store <see cref="Length"/> bytes.</remarks>
		public void CopyTo( byte[] buffer, int index ) {
			Buffer.BlockCopy( _seg.Array, _seg.Offset, buffer, index, _seg.Count );
		}

		public void CopyTo( int sourceIndex, byte[] destBuffer, int destIndex, int length ) {
			if( sourceIndex < 0 || sourceIndex >= _seg.Count )
				throw new ArgumentOutOfRangeException( "sourceIndex" );

			if( length < 0 || (sourceIndex + length) > _seg.Count )
				throw new ArgumentOutOfRangeException( "length" );

			Buffer.BlockCopy( _seg.Array, _seg.Offset + sourceIndex, destBuffer, destIndex, length );
		}
		
		public System.IO.Stream GetStream() {
			return new System.IO.MemoryStream( _seg.Array, _seg.Offset, _seg.Count, false, false );
		}

		/// <summary>
		/// Gets the length of the message buffer, in bytes.
		/// </summary>
		/// <value>The length of the message buffer, in bytes.</value>
		public int Length {
			get { return _seg.Count; }
		}
	}
}
