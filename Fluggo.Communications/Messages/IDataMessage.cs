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
	/// Represents a message containing user data.
	/// </summary>
	public interface IDataMessage : IMessage {
		/// <summary>
		/// Gets the message buffer containing the user data for this message.
		/// </summary>
		/// <value>An <see cref="IMessageBuffer"/> containing the data for this message.</value>
		IMessageBuffer MessageBuffer { get; }

		/// <summary>
		/// Gets the protocol ID, if any, associated with this message.
		/// </summary>
		/// <value>The message's protocol ID, or -1 if the message does not have one.</value>
		int Protocol { get; }

		/// <summary>
		/// Gets the number of milliseconds in which the message must be sent.
		/// </summary>
		/// <value>The number of milliseconds in which the message must be sent, or -1 if the message can be
		///   sent at any time. If this timeout expires before the message can be sent, the message is not delivered.</value>
		int MillisecondsLifetime { get; }

		/// <summary>
		/// Gets the delivery options associated with this message.
		/// </summary>
		/// <value>The delivery options associated with this message. These may differ from the delivery
		///   options specified at the source if the transport does not preserve these values or if the
		///   transport dropped unsupported flags.</value>
		DeliveryOptions Options { get; }
	}
}