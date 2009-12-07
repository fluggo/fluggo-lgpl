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
	/// Indicates options for delivering a message.
	/// </summary>
	[Flags]
	public enum DeliveryOptions {
		/// <summary>
		/// No options. These messages are delivered in order, unencrypted.
		/// </summary>
		None = 0,
		
		/// <summary>
		/// Messages with this flag may be delivered out of order with other messages.
		/// </summary>
		Unordered = 1,
		
		/// <summary>
		/// If possible, the message will not be bundled with other messages in an outgoing packet.
		/// </summary>
		NotBundled = 2,
	}
}
