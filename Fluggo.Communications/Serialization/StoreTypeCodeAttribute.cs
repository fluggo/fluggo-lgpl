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
	/// Stores the type code for the decorated field in another field.
	/// </summary>
	/// <remarks>Use this attribute on a field that requires a type code. On deserialization, the
	///   code will be stored in the designated field. To avoid setting the field twice on deserialization,
	///   mark the target field with the <see cref="IgnoreAttribute"/>.</remarks>
	[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field, AllowMultiple = false )]
	public sealed class StoreTypeCodeAttribute : SerializationAttribute
	{
		string _fieldName;
		
		public StoreTypeCodeAttribute( string fieldName ) {
			if( fieldName == null )
				throw new ArgumentNullException( "fieldName" );
			
			_fieldName = fieldName;
		}
		
		public string FieldName {
			get {
				return _fieldName;
			}
		}
	}
}
