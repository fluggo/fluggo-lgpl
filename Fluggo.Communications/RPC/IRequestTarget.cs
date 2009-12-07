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

namespace Fluggo.Communications {
	/// <summary>
	/// Provides a method for starting requests to a remote target.
	/// </summary>
	public interface IRequestTarget {
		/// <summary>
		/// Gets the URI of the target service domain.
		/// </summary>
		/// <value>The URI of the target service domain.</value>
		string ServiceUri { get; }

		/// <summary>
		/// Gets the GUID of the target interface.
		/// </summary>
		/// <value>The GUID of the target interface.</value>
		Guid InterfaceID { get; }

		/// <summary>
		/// Begins a request for the remote service.
		/// </summary>
		/// <param name="oneWay">True if the request is a one-way request, false otherwise.</param>
		/// <returns>An <see cref="IStreamRequest"/>-based object for sending the request and receiving the response, if any. </returns>
		IStreamRequest StartRequest( bool oneWay );
	}
}
