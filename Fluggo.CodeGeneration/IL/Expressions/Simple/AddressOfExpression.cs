/*
	Fluggo Code Generation Library
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
using System.Reflection;
using System.Reflection.Emit;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents an expression that takes the managed address of another expression.
	/// </summary>
	public class AddressOfExpression : Expression {
		Expression _base;

		/// <summary>
		/// Creates a new instance of the <see cref='AddressOfExpression'/> class.
		/// </summary>
		/// <param name="base">Expression to take the address of.</param>
		/// <exception cref='ArgumentNullException'><paramref name='base'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException">The address of <paramref name="base"/> could not be taken.</exception>
		public AddressOfExpression( Expression @base ) {
			if( @base == null )
				throw new ArgumentNullException( "base" );

			if( !@base.CanTakeAddress )
				throw new ArgumentException( "The address of this expression cannot be taken." );
			
			_base = @base;
		}

		public override Type ResultType {
			get {
				return _base.ResultType.MakeByRefType();
			}
		}

		public override string ToString() {
			return "&" + _base.ToString();
		}
		
		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
				
			_base.EmitAddress( cxt );
		}
	}

}