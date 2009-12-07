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
	/// Ignores a field.
	/// </summary>
	/// <remarks>A field marked with this attribute will not be stored in the resulting bit stream.</remarks>
	[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field, AllowMultiple = false )]
	public sealed class IgnoreAttribute : SerializationAttribute
	{
		/// <summary>
		/// Creates a new instance of the <see cref='IgnoreAttribute'/> class.
		/// </summary>
		public IgnoreAttribute() {
		}
	}
}
