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
	/// <summary>
	/// Specifies the maximum length of an array or string.
	/// </summary>
	[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field, AllowMultiple = false )]
	public sealed class MaxLengthAttribute : SerializationAttribute
	{
		int _maxLength;
		int _precision;
		
		public MaxLengthAttribute( int maxLength ) {
			if( maxLength < 0 )
				throw new ArgumentOutOfRangeException( "maxLength" );
				
			_maxLength = maxLength;
			_precision = BitWriter.GetPrecision( 0, maxLength );
		}

		/// <summary>
		/// Gets the number of bits needed to store the length of the field.
		/// </summary>
		/// <value>The minimum number of bits needed to store the length of the field.</value>
		public int Precision {
			get {
				return _precision;
			}
		}

		/// <summary>
		/// Gets the maximum length of the field.
		/// </summary>
		/// <value>The maximum length of the field. This refers to either the number of elements in an array or the number of characters in a string.</value>
		public int MaxLength {
			get {
				return _maxLength;
			}
		}
	}
}
