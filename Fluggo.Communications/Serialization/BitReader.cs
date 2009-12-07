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
using System.IO;
using System.Text;

namespace Fluggo.Communications.Serialization
{
	/// <summary>
	/// Reads a sequence of bits from a stream.
	/// </summary>
	public sealed class BitReader : IDisposable
	{
		byte[] _buffer;
		Stream _stream;
		int _bitsLeft, _currentByte, _bytesInBuffer;

		/// <summary>
		/// Creates a new instance of the <see cref="BitReader"/> class.
		/// </summary>
		/// <param name="stream">Underlying stream to read from.</param>
		/// <exception cref="ArgumentException"><paramref name='stream'/> does not support reading.</exception>
		/// <exception cref="ArgumentNullException"><paramref name='stream'/> is <see langword='null'/>.</exception>
		/// <remarks>This overload creates a reader with a default buffer size of 16 bytes.</remarks>
		public BitReader( Stream stream )
			: this( stream, 16 ) {
		}

		public BitReader( Stream stream, int bufferLength ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );

			if( !stream.CanRead )
				throw new ArgumentException( "This stream does not support reading.", "stream" );

			if( bufferLength <= 0 )
				throw new ArgumentOutOfRangeException( "bufferLength" );

			_stream = stream;
			_buffer = new byte[bufferLength];

			// Place us at the end of the buffer
			// This is done so that the first read on the base stream is delayed until
			// the first read on the bit reader
			_currentByte = 0;
			_bytesInBuffer = 1;
			_bitsLeft = 0;
		}

		public void Close() {
			_stream.Close();
		}

		public void Dispose() {
			Close();
		}

		private byte ReadFit( int bitCount ) {
			if( _bitsLeft == 0 )
				AdvanceByte();

			// The bits to be read can fit in the current byte
			byte data;

			unchecked {
				byte mask = (byte) ((1 << bitCount) - 1);
				data = (byte) ((_buffer[_currentByte] >> (_bitsLeft - bitCount)) & mask);
			}

			_bitsLeft -= bitCount;

			return data;
		}

		private void AdvanceByte() {
			_currentByte++;

			if( _currentByte == _bytesInBuffer ) {
				_bytesInBuffer = _stream.Read( _buffer, 0, _buffer.Length );

				if( _bytesInBuffer == 0 )
					throw new EndOfStreamException();

				_currentByte = 0;
			}

			_bitsLeft = 8;
		}

		public long ReadInt64( int bitCount ) {
			if( bitCount < 0 || bitCount > 64 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			long value = 0L;

			while( bitCount > 0 ) {
				if( _bitsLeft == 0 )
					AdvanceByte();

				if( bitCount > _bitsLeft ) {
					int bitsLeftTemp = _bitsLeft;
					value <<= bitsLeftTemp;
					value |= ReadFit( bitsLeftTemp );
					bitCount -= bitsLeftTemp;
				}
				else {
					value <<= bitCount;
					value |= ReadFit( bitCount );
					bitCount = 0;
				}
			}

			return value;
		}

		[CLSCompliant( false )]
		public ulong ReadUInt64( int bitCount ) {
			return unchecked( (ulong) ReadInt64( bitCount ) );
		}

		public int ReadInt32( int bitCount ) {
			if( bitCount < 0 || bitCount > 32 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			int value = 0;

			while( bitCount > 0 ) {
				if( _bitsLeft == 0 )
					AdvanceByte();

				if( bitCount > _bitsLeft ) {
					int bitsLeftTemp = _bitsLeft;
					value <<= bitsLeftTemp;
					value |= ReadFit( bitsLeftTemp );
					bitCount -= bitsLeftTemp;
				}
				else {
					value <<= bitCount;
					value |= ReadFit( bitCount );
					bitCount = 0;
				}
			}

			return value;
		}

		[CLSCompliant( false )]
		public uint ReadUInt32( int bitCount ) {
			return unchecked( (uint) ReadInt64( bitCount ) );
		}

		public short ReadInt16( int bitCount ) {
			if( bitCount > 16 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			return unchecked( (short) ReadInt32( bitCount ) );
		}

		[CLSCompliant( false )]
		public ushort ReadUInt16( int bitCount ) {
			return unchecked( (ushort) ReadInt16( bitCount ) );
		}

		public byte ReadByte( int bitCount ) {
			if( bitCount > 8 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			return unchecked( (byte) ReadInt32( bitCount ) );
		}

		[CLSCompliant( false )]
		public sbyte ReadSByte( int bitCount ) {
			return unchecked( (sbyte) ReadByte( bitCount ) );
		}

		public byte[] ReadBytes( int count ) {
			byte[] buffer = new byte[count];

			for( int i = 0; i < count; i++ )
				buffer[i] = ReadByte( 8 );

			return buffer;
		}

		public bool ReadBoolean() {
			return (ReadFit( 1 ) == 1);
		}

		public string ReadString( int maxLength ) {
			if( maxLength < 0 )
				throw new ArgumentOutOfRangeException( "maxLength" );

			int maxByteCount = Encoding.UTF8.GetMaxByteCount( maxLength );
			int lengthPrecision = BitWriter.GetPrecision( 0, maxByteCount );

			// Read length
			int length = ReadInt32( lengthPrecision );

			// Read encoded string
			byte[] encodedString = new byte[length];

			for( int i = 0; i < length; i++ )
				encodedString[i] = ReadByte( 8 );

			// Decode string
			return Encoding.UTF8.GetString( encodedString );
		}

		public float ReadSingle() {
			return BitConverter.ToSingle( ReadBytes( 4 ), 0 );
		}

		public float ReadFixedSingle( float minValue, float maxValue, int bitCount ) {
			if( bitCount < 0 || bitCount > 64 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			float difference = maxValue - minValue;

			if( difference <= 0.0f )
				throw new ArgumentException( "maxValue must be greater than minValue." );

			// Normalize the minValue
			float value = (long) ReadInt64( bitCount );
			value *= difference / (float) ((1L << bitCount) - 1L);

			return value + minValue;
		}

		public char ReadChar() {
			string value = ReadString( 1 );

			if( value.Length == 0 )
				throw new IOException( "The character could not be decoded." );

			return value[0];
		}
	}
}
