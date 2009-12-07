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
	/// Provides methods that store and retrieve values in byte arrays in network byte order.
	/// </summary>
	public static class NetworkBitConverter {
		/// <summary>
		/// Copies the given 64-bit value to a buffer in network byte order.
		/// </summary>
		/// <param name="value">Value to copy.</param>
		/// <param name="buffer">Target buffer.</param>
		/// <param name="index">Index in the buffer at which copying should begin.</param>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		public static void Copy( long value, byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				for( int i = 7; i >= 0; i-- ) {
					buffer[index + i] = (byte) value;
					value >>= 8;
				}
			}
		}

		/// <summary>
		/// Copies the given 32-bit value to a buffer in network byte order.
		/// </summary>
		/// <param name="value">Value to copy.</param>
		/// <param name="buffer">Target buffer.</param>
		/// <param name="index">Index in the buffer at which copying should begin.</param>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		public static void Copy( int value, byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				for( int i = 3; i >= 0; i-- ) {
					buffer[index + i] = (byte) value;
					value >>= 8;
				}
			}
		}

		/// <summary>
		/// Copies the given 16-bit value to a buffer in network byte order.
		/// </summary>
		/// <param name="value">Value to copy.</param>
		/// <param name="buffer">Target buffer.</param>
		/// <param name="index">Index in the buffer at which copying should begin.</param>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		public static void Copy( short value, byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				for( int i = 1; i >= 0; i-- ) {
					buffer[index + i] = (byte) value;
					value >>= 8;
				}
			}
		}

		/// <summary>
		/// Copies the given 64-bit unsigned value to a buffer in network byte order.
		/// </summary>
		/// <param name="value">Value to copy.</param>
		/// <param name="buffer">Target buffer.</param>
		/// <param name="index">Index in the buffer at which copying should begin.</param>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		[CLSCompliant( false )]
		public static void Copy( ulong value, byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				for( int i = 7; i >= 0; i-- ) {
					buffer[index + i] = (byte) value;
					value >>= 8;
				}
			}
		}

		/// <summary>
		/// Copies the given 32-bit unsigned value to a buffer in network byte order.
		/// </summary>
		/// <param name="value">Value to copy.</param>
		/// <param name="buffer">Target buffer.</param>
		/// <param name="index">Index in the buffer at which copying should begin.</param>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		[CLSCompliant( false )]
		public static void Copy( uint value, byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				for( int i = 3; i >= 0; i-- ) {
					buffer[index + i] = (byte) value;
					value >>= 8;
				}
			}
		}

		/// <summary>
		/// Copies the given 16-bit unsigned value to a buffer in network byte order.
		/// </summary>
		/// <param name="value">Value to copy.</param>
		/// <param name="buffer">Target buffer.</param>
		/// <param name="index">Index in the buffer at which copying should begin.</param>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		[CLSCompliant( false )]
		public static void Copy( ushort value, byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				for( int i = 1; i >= 0; i-- ) {
					buffer[index + i] = (byte) value;
					value >>= 8;
				}
			}
		}

		/// <summary>
		/// Retrieves a 64-bit value stored in the given buffer in network byte order.
		/// </summary>
		/// <param name="buffer">Source buffer.</param>
		/// <param name="index">Index in the buffer at which the value is stored.</param>
		/// <returns>The integer stored at the given index.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		public static long ToInt64( byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				long result = 0L;

				for( int i = 0; i < 8; i++ ) {
					result <<= 8;
					result |= (long) buffer[index + i];
				}

				return result;
			}
		}

		/// <summary>
		/// Retrieves a 32-bit value stored in the given buffer in network byte order.
		/// </summary>
		/// <param name="buffer">Source buffer.</param>
		/// <param name="index">Index in the buffer at which the value is stored.</param>
		/// <returns>The integer stored at the given index.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		public static int ToInt32( byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				int result = 0;

				for( int i = 0; i < 4; i++ ) {
					result <<= 8;
					result |= (int) buffer[index + i];
				}

				return result;
			}
		}

		/// <summary>
		/// Retrieves a 16-bit value stored in the given buffer in network byte order.
		/// </summary>
		/// <param name="buffer">Source buffer.</param>
		/// <param name="index">Index in the buffer at which the value is stored.</param>
		/// <returns>The integer stored at the given index.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		public static short ToInt16( byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				int result = 0;

				for( int i = 0; i < 2; i++ ) {
					result <<= 8;
					result |= (int) buffer[index + i];
				}

				return (short) result;
			}
		}

		/// <summary>
		/// Retrieves a 64-bit unsigned value stored in the given buffer in network byte order.
		/// </summary>
		/// <param name="buffer">Source buffer.</param>
		/// <param name="index">Index in the buffer at which the value is stored.</param>
		/// <returns>The integer stored at the given index.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		[CLSCompliant( false )]
		public static ulong ToUInt64( byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				ulong result = 0L;

				for( int i = 0; i < 8; i++ ) {
					result <<= 8;
					result |= (ulong) buffer[index + i];
				}

				return result;
			}
		}

		/// <summary>
		/// Retrieves a 64-bit unsigned value stored in the given buffer in network byte order.
		/// </summary>
		/// <param name="buffer">Source buffer.</param>
		/// <param name="index">Index in the buffer at which the value is stored.</param>
		/// <returns>The integer stored at the given index.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		[CLSCompliant( false )]
		public static uint ToUInt32( byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				uint result = 0;

				for( int i = 0; i < 4; i++ ) {
					result <<= 8;
					result |= (uint) buffer[index + i];
				}

				return result;
			}
		}

		/// <summary>
		/// Retrieves a 64-bit unsigned value stored in the given buffer in network byte order.
		/// </summary>
		/// <param name="buffer">Source buffer.</param>
		/// <param name="index">Index in the buffer at which the value is stored.</param>
		/// <returns>The integer stored at the given index.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='buffer'/> is <see langword='null'/>.</exception>
		[CLSCompliant( false )]
		public static ushort ToUInt16( byte[] buffer, int index ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );

			unchecked {
				ushort result = 0;

				for( int i = 0; i < 2; i++ ) {
					result <<= 8;
					result |= (ushort) buffer[index + i];
				}

				return result;
			}
		}
	}
}