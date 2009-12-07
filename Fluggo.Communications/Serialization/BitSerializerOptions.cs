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

namespace Fluggo.Communications.Serialization
{
	public class BitSerializerOptions {
		int _maxArrayLength, _maxStringLength;

		/// <summary>
		/// Creates a new instance of the <see cref='BitSerializerOptions'/> class.
		/// </summary>
		public BitSerializerOptions() {
			_maxArrayLength = int.MaxValue;
			_maxStringLength = short.MaxValue;
		}
		
		public BitSerializerOptions( int maxArrayLength, int maxStringLength ) {
			if( maxArrayLength < 0 )
				throw new ArgumentOutOfRangeException( "maxArrayLength" );
			
			if( maxStringLength < 0 )
				throw new ArgumentOutOfRangeException( "maxStringLength" );
			
			_maxArrayLength = maxArrayLength;
			_maxStringLength = maxStringLength;
		}

		/// <summary>
		/// Gets the maximum length of serialized arrays.
		/// </summary>
		/// <value>The maximum length of serialized arrays. You can override this limit
		///   using an attribute. The default limit is <see cref='Int32.MaxValue'/>.</value>
		/// <remarks>This property is used to determine the number of bits to assign to array lengths.
		///   The lower the number, the fewer bits are used.</remarks>
		public int MaxArrayLength {
			get {
				return _maxArrayLength;
			}
		}

		/// <summary>
		/// Gets the maximum length of serialized strings.
		/// </summary>
		/// <value>The maximum length of serialized strings, in characters. You can override this limit
		///   using an attribute. The default limit is <see cref='Int16.MaxValue'/>.</value>
		/// <remarks>This property is used to determine the number of bits to assign to string lengths.
		///     The lower the number, the fewer bits are used.
		///   <para>Unlike <see cref="MaxArrayLength"/>, the value of this property does not directly translate to
		///     a bit count. When strings are serialized, they are prefixed by the number of encoded symbols needed to express the
		///     string in their encoding. Because this number can be significantly higher than the number of characters
		///     in the string, the bit length is based on the maximum possible number of encoded symbols and not the maximum number of characters.</para></remarks>
		public int MaxStringLength {
			get {
				return _maxStringLength;
			}
		}
	}
}
