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
	/// Represents an expression that returns from a method.
	/// </summary>
	public sealed class ReturnExpression : Expression {
		Expression _value;

		/// <summary>
		/// Creates a new instance of the <see cref='ReturnExpression'/> class that returns nothing.
		/// </summary>
		public ReturnExpression() {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='ReturnExpression'/> class that returns a value.
		/// </summary>
		/// <param name="value">Expression for the value to return from the method or property.</param>
		/// <exception cref='ArgumentNullException'><paramref name='value'/> is <see langword='null'/>.</exception>
		public ReturnExpression( Expression value ) {
			if( value == null )
				throw new ArgumentNullException( "value" );

			_value = value;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _value != null )
				return "return " + _value.ToString();
			else
				return "return";
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			cxt.EmitReturn( _value );
		}

	}
}