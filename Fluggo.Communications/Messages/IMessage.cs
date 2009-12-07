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
	/// Represents a generic message, which may carry user or channel-specific data.
	/// </summary>
	public interface IMessage {
		/// <summary>
		/// Gets or sets an object that contains information about this message.
		/// </summary>
		/// <value>An object that contains information about this message.</value>
		/// <remarks>This value is not transmitted with the message.</remarks>
		object Tag { get; set; }

		/// <summary>
		/// Gets the index of the substream, if any, associated with this message.
		/// </summary>
		/// <value>The index of the message's substream, if any, or -1 if there was none.</value>
		int Channel { get; }
	}
}