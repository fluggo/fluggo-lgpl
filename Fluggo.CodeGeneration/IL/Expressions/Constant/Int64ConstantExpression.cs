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
using System.Globalization;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents an expression for a constant 64-bit signed integer.
	/// </summary>
	public sealed class Int64ConstantExpression : Expression {
		long _value;

		/// <summary>
		/// Creates a new instance of the <see cref="Int64ConstantExpression"/> class.
		/// </summary>
		/// <param name="value">Value of the integer.</param>
		public Int64ConstantExpression( long value ) {
			_value = value;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the result left at the top of the stack when the expression is evaluated.
		///   This property always returns <see cref="Int64"/>.</value>
		public override Type ResultType {
			get {
				return typeof(long);
			}
		}

		/// <summary>
		/// Gets the value of the expression.
		/// </summary>
		/// <value>The value of the expression.</value>
		public long Value {
			get {
				return _value;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return _value.ToString( CultureInfo.InvariantCulture ) + "L";
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			cxt.Generator.Emit( OpCodes.Ldc_I8, _value );
		}
	}
}