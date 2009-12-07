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

namespace Fluggo.Communications {
	public abstract class SctpMessage : IMessage
	{
		private SctpChunkType _chunkType;
		private ushort _chunkLength;
		private byte _flags;
		private object _tag;

		protected SctpMessage( SctpChunkType type, byte flags, int chunkLength ) {
			_chunkType = type;
			_flags = flags;

			if( chunkLength < 4 || chunkLength > ushort.MaxValue )
				throw new ArgumentOutOfRangeException( "chunkLength" );

			_chunkLength = (ushort) chunkLength;
		}

		/// <summary>
		/// Gets the type of SCTP chunk to which this message corresponds.
		/// </summary>
		/// <value>A <see cref="SctpChunkType"/> value describing the type of SCTP chunk to which this message corresponds.</value>
		public SctpChunkType Type {
			get {
				return _chunkType;
			}
		}

		/// <summary>
		/// Gets the unpadded length of the full chunk.
		/// </summary>
		/// <value>The unpadded length of the full chunk, in bytes.</value>
		public int ChunkLength {
			get {
				return _chunkLength;
			}
		}

		/// <summary>
		/// Gets the flags used in the chunk header.
		/// </summary>
		/// <value>The flags used in the chunk header, if any.</value>
		public byte ChunkFlags {
			get {
				return _flags;
			}
		}

		public virtual void CopyChunk( byte[] buffer, int offset ) {
			new ArraySegment<byte>( buffer, offset, _chunkLength );

			buffer[offset] = (byte) _chunkType;
			buffer[offset + 1] = _flags;
			NetworkBitConverter.Copy( _chunkLength, buffer, offset + 2 );
		}

		/// <summary>
		/// Gets or sets an object that contains information about this message.
		/// </summary>
		/// <value>An object that contains information about this message.</value>
		/// <remarks>This value is not transmitted with the message.</remarks>
		public object Tag {
			get {
				return _tag;
			}
			set {
				_tag = value;
			}
		}

		/// <summary>
		/// Gets the index of the substream, if any, associated with this message.
		/// </summary>
		/// <value>The index of the message's substream, if any, or -1 if there was none.</value>
		public virtual int Channel {
			get { return -1; }
		}
	}
}