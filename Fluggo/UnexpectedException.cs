/*
	Fluggo Common Library
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
using System.Runtime.Serialization;

namespace Fluggo {
	/// <summary>
	/// Represents the exception that occurs when a condition is unexpected.
	/// </summary>
	/// <remarks>This exception is meant to be thrown under conditions that are theoretically impossible, but practically possible.
	///     Catching an <see cref="UnexpectedException"/> means that the called code is not working as intended. This is different from
	///     exceptions that occur as a result of run-time factors such as bad input. One can anticipate that an <see cref="ArgumentException"/>
	///     or even a <see cref="System.Threading.ThreadAbortException"/> may occur in working code, but if a section of code is working as designed,
	///     the <see cref="UnexpectedException"/> should never occur.
	///   <para>Throw this exception in the middle of complicated algorithms or switch statements where an alternative is possible, but
	///     should never occur, even as a result of bad input. Consider, for example, an invalid <see cref="TypeCode"/> value, maybe even one that doesn't appear
	///     in the enumeration at all. If you received the value as a parameter, that's an <see cref="ArgumentException"/>
	///     or an <see cref="ArgumentOutOfRangeException"/>. If the value was returned from <see cref="Type.GetTypeCode"/> or produced by your
	///     own code, that's an <see cref="UnexpectedException"/>.</para></remarks>
	[Serializable]
	public sealed class UnexpectedException : Exception {
		/// <summary>
		/// Creates a new instance of the <see cref='UnexpectedException'/> class.
		/// </summary>
		public UnexpectedException() {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='UnexpectedException'/> class.
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		public UnexpectedException( string message )
			: base( message ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='UnexpectedException'/> class.
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		/// <param name="innerException">The exception that caused the current exception, or <see langword='null'/> if no inner exception occured.</param>
		public UnexpectedException( string message, Exception innerException )
			: base( message, innerException ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='UnexpectedException'/> class.
		/// </summary>
		/// <param name="info">A <see cref="SerializationInfo"/> instance that contains the data for the exception.</param>
		/// <param name="context"><see cref="StreamingContext"/> that contains context information about the source or destination.</param>
		/// <exception cref='ArgumentNullException'><paramref name='info'/> is <see langword='null'/>.</exception>
		private UnexpectedException( SerializationInfo info, StreamingContext context )
			: base( info, context ) {
		}
	}
}