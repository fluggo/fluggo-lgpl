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

namespace Fluggo.Communications {
	public interface IMessageBuffer {
		/// <summary>
		/// Copies the contents of the message buffer to the given byte array.
		/// </summary>
		/// <param name="buffer">Buffer to receive the results.</param>
		/// <param name="index">Index in <paramref name="buffer"/> at which to start copying.</param>
		/// <remarks>There must be enough room in the buffer to store <see cref="Length"/> bytes.</remarks>
		void CopyTo( byte[] buffer, int index );

		void CopyTo( int sourceIndex, byte[] destBuffer, int destIndex, int length );
		
		/// <summary>
		/// Gets a read-only stream of the buffer data.
		/// </summary>
		/// <returns>A read-only stream of the buffer data.</returns>
		Stream GetStream();

		/// <summary>
		/// Gets the length of the message buffer, in bytes.
		/// </summary>
		/// <value>The length of the message buffer, in bytes.</value>
		int Length { get; }
	}
}