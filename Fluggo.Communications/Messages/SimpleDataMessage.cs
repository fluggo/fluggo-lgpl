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
	/// <summary>
	/// Represents a simple data message.
	/// </summary>
	/// <remarks>The <see cref="SimpleDataMessage"/> does not support multiple protocols, lifetimes, or delivery options.
	///   It does support multiple channels.</remarks>
	public class SimpleDataMessage : IDataMessage {
		IMessageBuffer _data;
		object _tag;
		int _channel;

		/// <summary>
		/// Creates a new instance of the <see cref='SimpleDataMessage'/> class.
		/// </summary>
		/// <param name="channel">Channel on which the message should be sent, if the transport supports multiple channels.</param>
		/// <param name="data"><see cref="IMessageBuffer"/> containing the payload data for the message.</param>
		/// <exception cref='ArgumentNullException'><paramref name='data'/> is <see langword='null'/>.</exception>
		/// <exception cref='ArgumentOutOfRangeException'><paramref name='streamID'/> is less than zero.</exception>
		public SimpleDataMessage( int channel, IMessageBuffer data ) {
			if( data == null )
				throw new ArgumentNullException( "data" );
				
			if( channel < 0 )
				throw new ArgumentOutOfRangeException( "streamID" );

			_channel = channel;
			_data = data;
		}

		/// <summary>
		/// Creates a new instance of the <see cref='SimpleDataMessage'/> class.
		/// </summary>
		/// <param name="channel">Channel on which the message should be sent, if the transport supports multiple channels.</param>
		/// <param name="data">Byte array containing the payload data for the message.</param>
		/// <exception cref='ArgumentNullException'><paramref name='data'/> is <see langword='null'/>.</exception>
		/// <exception cref='ArgumentOutOfRangeException'><paramref name='streamID'/> is less than zero.</exception>
		public SimpleDataMessage( int channel, byte[] data ) {
			if( data == null )
				throw new ArgumentNullException( "data" );

			if( channel < 0 )
				throw new ArgumentOutOfRangeException( "streamID" );

			_channel = channel;
			_data = new MessageBufferWrapper( data );
		}

		/// <summary>
		/// Creates a new instance of the <see cref='SimpleDataMessage'/> class.
		/// </summary>
		/// <param name="channel">Channel on which the message should be sent, if the transport supports multiple channels.</param>
		/// <param name="data">Byte array containing the payload data for the message.</param>
		/// <param name="offset">Offset into <paramref name="data"/> at which the payload data begins.</param>
		/// <param name="count">Number of bytes after <paramref name="offset"/> that contain the data.</param>
		/// <exception cref='ArgumentNullException'><paramref name='data'/> is <see langword='null'/>.</exception>
		/// <exception cref='ArgumentOutOfRangeException'><paramref name='channel'/> is less than zero.</exception>
		public SimpleDataMessage( int channel, byte[] data, int offset, int count ) {
			if( data == null )
				throw new ArgumentNullException( "data" );

			if( channel < 0 )
				throw new ArgumentOutOfRangeException( "streamID" );

			_channel = channel;
			_data = new MessageBufferWrapper( data, offset, count );
		}

		/// <summary>
		/// Gets the message buffer containing the user data for this message.
		/// </summary>
		/// <value>An <see cref="IMessageBuffer"/> containing the data for this message.</value>
		public IMessageBuffer MessageBuffer {
			get { return _data; }
		}

		/// <summary>
		/// Gets the protocol ID, if any, associated with this message.
		/// </summary>
		/// <value>The <see cref="SimpleDataMessage"/> type doesn't support multiple protocols, so this property always returns -1.</value>
		public int Protocol {
			get { return -1; }
		}

		/// <summary>
		/// Gets the number of milliseconds in which the message must be sent.
		/// </summary>
		/// <value>The <see cref="SimpleDataMessage"/> type doesn't support message timeouts, so this property always returns -1.</value>
		public int MillisecondsLifetime {
			get { return -1; }
		}

		/// <summary>
		/// Gets the delivery options associated with this message.
		/// </summary>
		/// <value>The <see cref="SimpleDataMessage"/> type doesn't support delivery options,
		///   so this property always returns <see cref="DeliveryOptions.None"/>.</value>
		public DeliveryOptions Options {
			get { return DeliveryOptions.None; }
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
		/// Gets the index of the channel, if any, associated with this message.
		/// </summary>
		/// <value>The index of the message's channel.</value>
		public int Channel {
			get { return _channel; }
		}
	}
}