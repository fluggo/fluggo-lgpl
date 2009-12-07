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
using System.IO;

namespace Fluggo.Communications {
	/// <summary>
	/// Represents a request-response mechanism over byte streams.
	/// </summary>
	public interface IStreamRequest {
		/// <summary>
		/// Gets a value that represents whether the request is being sent or received.
		/// </summary>
		/// <value>True if the request is being sent, false if it is being received.</value>
		bool IsOutbound { get; }

		/// <summary>
		/// Gets a value that represents whether a response to the request is expected.
		/// </summary>
		/// <value>True if a response to the request is expected, false otherwise.</value>
		bool IsOneWay { get; }
	
		/// <summary>
		/// Gets a <see cref="Stream"/> for writing data to the target or reading data from the client.
		/// </summary>
		/// <returns>If <see cref="IsOutbound"/> is true, a write-only <see cref="Stream"/> for writing data to the target.
		///   If <see cref="IsOutbound"/> is false, a read-only <see cref="Stream"/> for reading data from the client.</returns>
		/// <exception cref="InvalidOperationException"><see cref="GetRequestStream"/> has already been called.
		///   <para>-or-</para>
		///   <para>The request data has already been sent or received.</para></exception>
		Stream GetRequestStream();

		/// <summary>
		/// Gets a <see cref="Stream"/> for reading the response from the target or writing the response to the client.
		/// </summary>
		/// <returns>If <see cref="IsOutbound"/> is true, a read-only <see cref="Stream"/> for reading response data from the target.
		///   If <see cref="IsOutbound"/> is false, a write-only <see cref="Stream"/> for writing data to the client.</returns>
		/// <exception cref="NotSupportedException"><see cref="IsOneWay"/> is true.</exception>
		/// <exception cref="InvalidOperationException"><see cref="GetRequestStream"/> has not been called.
		///   <para>-or-</para>
		///   <para>The stream returned from <see cref="GetRequestStream"/> has not been closed.</para>
		///   <para>-or-</para>
		///   <para><see cref="GetResponseStream"/> has already been called.</para></exception>
		Stream GetResponseStream();
	}
}
