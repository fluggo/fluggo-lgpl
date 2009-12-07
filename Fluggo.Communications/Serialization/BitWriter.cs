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
	/// Writes a sequence of bits to a stream.
	/// </summary>
	public sealed class BitWriter : IDisposable
	{
		Stream _stream;
		byte[] _buffer;
		int _currentByte;
		int _bitsLeft;

		/// <summary>
		/// Creates a new instance of the <see cref="BitWriter"/> class.
		/// </summary>
		/// <param name="stream">Underlying stream to write to.</param>
		/// <exception cref="ArgumentException"><paramref name='stream'/> does not support writing.</exception>
		/// <exception cref="ArgumentNullException"><paramref name='stream'/> is <see langword='null'/>.</exception>
		/// <remarks>This overload creates a writer with a default buffer size of 16 bytes.</remarks>
		/// <exception cref="ArgumentException"><paramref name='stream'/> does not support writing.</exception>
		/// <exception cref="ArgumentNullException"><paramref name='stream'/> is <see langword='null'/>.</exception>
		public BitWriter( Stream stream )
			: this( stream, 16 ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref="BitWriter"/> class with the given buffer length.
		/// </summary>
		/// <param name="stream">Underlying stream to write to.</param>
		/// <param name="bufferLength">Number of bytes to queue up before writing them to <paramref name='stream'/>.</param>
		/// <exception cref="ArgumentException"><paramref name='stream'/> does not support writing.</exception>
		/// <exception cref="ArgumentNullException"><paramref name='stream'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='bufferLength'/> is less than or equal to zero.</exception>
		public BitWriter( Stream stream, int bufferLength ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );

			if( !stream.CanWrite )
				throw new ArgumentException( "This stream does not support writing.", "stream" );

			if( bufferLength <= 0 )
				throw new ArgumentOutOfRangeException( "bufferLength" );

			_stream = stream;
			_buffer = new byte[bufferLength];
			_bitsLeft = 8;
		}

		/// <summary>
		/// Gets the minimum number of bits necessary to store the given range of integers.
		/// </summary>
		/// <param name="minValue">Minimum value to store.</param>
		/// <param name="maxValue">Maximum value to store.</param>
		/// <returns>The number of bits needed to store the value. To achieve this, offset the
		///	  stored value by <paramref name='minValue'/> so that the result is positive before encoding.</returns>
		public static int GetPrecision( long minValue, long maxValue ) {
			if( minValue > maxValue )
				throw new ArgumentException( "The minimum value is greater than the maximum value." );

			ulong difference = (ulong) maxValue + ((ulong) -minValue);

			if( difference == 0 )
				return 0;

			// Construct a mask and walk it up until we get the minimum number of bits necessary
			ulong mask = 0x1UL;
			int precision = 1;

			while( mask < difference ) {
				mask <<= 1;
				mask |= 1;
				precision++;
			}

			return precision;
		}

		/// <summary>
		/// Writes unwritten bits and closes the underlying stream.
		/// </summary>
		public void Close() {
			Flush();
			_stream.Close();
		}

		/// <summary>
		/// Writes any unwritten bits to the underlying stream.
		/// </summary>
		/// <remarks>Any unwritten bits are written to the stream, and unused bits in the last byte are padded with zeros.
		///	  The next write will start on a byte boundary.</remarks>
		public void Flush() {
			// Finish out the last byte
			if( _bitsLeft != 8 )
				_currentByte++;

			if( _currentByte != 0 ) {
				// Flush the remaining contents to the stream
				_stream.Write( _buffer, 0, _currentByte );
				_currentByte = 0;
				_buffer[0] = 0;
			}
		}

		public void Dispose() {
			Close();
		}

		private void WriteFit( byte data, int bitCount ) {
			// The bits to be written can fit in the current byte
			unchecked {
				byte mask = (byte) ((1 << bitCount) - 1);
				_buffer[_currentByte] |= (byte) ((data & mask) << (_bitsLeft - bitCount));
			}

			_bitsLeft -= bitCount;

			if( _bitsLeft == 0 )
				AdvanceByte();
		}

		private void AdvanceByte() {
			_currentByte++;

			if( _currentByte == _buffer.Length ) {
				_stream.Write( _buffer, 0, _buffer.Length );
				_currentByte = 0;
			}

			_buffer[_currentByte] = 0;
			_bitsLeft = 8;
		}

		/// <summary>
		/// Writes a single bit to the stream.
		/// </summary>
		/// <param name="value">Value of the bit to write.</param>
		public void Write( bool value ) {
			WriteFit( value ? (byte) 1 : (byte) 0, 1 );
		}

		/// <summary>
		/// Writes the given number of least significant bits to the stream.
		/// </summary>
		/// <param name="value">Bits to be written.</param>
		/// <param name="bitCount">Number of least significant bits to write.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='bitCount'/> is less than zero or greater than 64.</exception>
		public void Write( long value, int bitCount ) {
			Write( unchecked( (ulong) value ), bitCount );
		}

		[CLSCompliant( false )]
		public void Write( ulong value, int bitCount ) {
			if( bitCount < 0 || bitCount > 64 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			unchecked {
				while( bitCount > _bitsLeft ) {
					int bitsToWrite = _bitsLeft;
					WriteFit( (byte) (value >> (bitCount - _bitsLeft)), _bitsLeft );
					bitCount -= bitsToWrite;
				}

				WriteFit( (byte) value, bitCount );
			}
		}

		/// <summary>
		/// Writes the given number of least significant bits to the stream.
		/// </summary>
		/// <param name="value">Bits to be written.</param>
		/// <param name="bitCount">Number of least significant bits to write.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='bitCount'/> is less than zero or greater than 32.</exception>
		public void Write( int value, int bitCount ) {
			if( bitCount > 32 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			Write( unchecked( (long) value ), bitCount );
		}

		/// <summary>
		/// Writes the given number of least significant bits to the stream.
		/// </summary>
		/// <param name="value">Bits to be written.</param>
		/// <param name="bitCount">Number of least significant bits to write.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='bitCount'/> is less than zero or greater than 16.</exception>
		public void Write( short value, int bitCount ) {
			if( bitCount > 16 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			Write( unchecked( (long) value ), bitCount );
		}

		/// <summary>
		/// Writes the given number of least significant bits to the stream.
		/// </summary>
		/// <param name="value">Bits to be written.</param>
		/// <param name="bitCount">Number of least significant bits to write.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='bitCount'/> is less than zero or greater than eight.</exception>
		public void Write( byte value, int bitCount ) {
			if( bitCount > 8 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			Write( unchecked( (long) value ), bitCount );
		}

		/// <summary>
		/// Writes the given number of least significant bits to the stream.
		/// </summary>
		/// <param name="value">Bits to be written.</param>
		/// <param name="bitCount">Number of least significant bits to write.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='bitCount'/> is less than zero or greater than 32.</exception>
		[CLSCompliant( false )]
		public void Write( uint value, int bitCount ) {
			if( bitCount > 32 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			Write( (ulong) value, bitCount );
		}

		/// <summary>
		/// Writes the given number of least significant bits to the stream.
		/// </summary>
		/// <param name="value">Bits to be written.</param>
		/// <param name="bitCount">Number of least significant bits to write.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='bitCount'/> is less than zero or greater than 16.</exception>
		[CLSCompliant( false )]
		public void Write( ushort value, int bitCount ) {
			if( bitCount > 16 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			Write( (ulong) value, bitCount );
		}

		/// <summary>
		/// Writes the given number of least significant bits to the stream.
		/// </summary>
		/// <param name="value">Bits to be written.</param>
		/// <param name="bitCount">Number of least significant bits to write.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='bitCount'/> is less than zero or greater than eight.</exception>
		[CLSCompliant( false )]
		public void Write( sbyte value, int bitCount ) {
			Write( unchecked( (byte) value ), bitCount );
		}

		public void WriteSingle( float value ) {
			WriteBytes( BitConverter.GetBytes( value ), 0, 4 );
		}

		public void WriteFixedSingle( float value, float minValue, float maxValue, int bitCount ) {
			if( bitCount < 0 || bitCount > 64 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			if( value < minValue || value > maxValue )
				throw new ArgumentOutOfRangeException( "value" );

			float difference = maxValue - minValue;

			if( difference <= 0.0f )
				throw new ArgumentException( "maxValue must be greater than minValue." );

			// Normalize the minValue
			value -= minValue;
			value *= (float) ((1L << bitCount) - 1L) / difference;

			Write( (long) value, bitCount );
		}

/*		public void Write( byte[] buffer, int byteIndex, int bitCount ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			if( byteIndex < 0 )
				throw new ArgumentOutOfRangeException( "byteIndex" );

			if( bitCount < 0 )
				throw new ArgumentOutOfRangeException( "bitCount" );

			while( bitCount >= 8 ) {
				Write( buffer[byteIndex++], 8 );
				bitCount -= 8;
			}

			if( bitCount != 0 )
				Write( buffer[byteIndex], bitCount );
		}*/

		public void WriteBytes( byte[] buffer, int byteIndex, int byteCount ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			if( byteIndex < 0 )
				throw new ArgumentOutOfRangeException( "byteIndex" );

			if( byteCount < 0 )
				throw new ArgumentOutOfRangeException( "byteCount" );

			new ArraySegment<byte>( buffer, byteIndex, byteCount );

			while( byteCount != 0 ) {
				Write( buffer[byteIndex++], 8 );
				byteCount--;
			}
		}

		/// <summary>
		/// Writes a string using a packed UTF-7 encoding.
		/// </summary>
		/// <param name="value">String to store.</param>
		/// <param name="maxLength">Maximum number of characters that might be stored in this string.</param>
		/// <exception cref="ArgumentNullException"><paramref name='value'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name='maxLength'/> is less than zero.</exception>
		/// <exception cref="ArgumentException"><paramref name='value'/> is longer than <paramref name='maxLength'/>.</exception>
		public void WriteString( string value, int maxLength ) {
			if( value == null )
				throw new ArgumentNullException( "value" );

			if( maxLength < 0 )
				throw new ArgumentOutOfRangeException( "maxLength" );

			if( value.Length > maxLength )
				throw new ArgumentException( "The given string is longer than the specified maximum length." );

			int maxByteCount = Encoding.UTF8.GetMaxByteCount( maxLength );
			int lengthPrecision = GetPrecision( 0, maxByteCount );

			// Encode string
			byte[] encodedString = Encoding.UTF8.GetBytes( value );

			// Write length in units of 8-bit words
			Write( encodedString.Length, lengthPrecision );

			// Write string as 8-bit words
			for( int i = 0; i < encodedString.Length; i++ )
				Write( encodedString[i], 8 );
		}

		/// <summary>
		/// Writes a single character using a packed UTF-7 encoding.
		/// </summary>
		/// <param name="value">Character to store.</param>
		public void Write( char value ) {
			WriteString( new string( value, 1 ), 1 );
		}
	}
}
