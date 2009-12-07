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
	/// Represents an expression for a constant 8-bit unsigned integer.
	/// </summary>
	public sealed class ByteConstantExpression : Expression {
		byte _value;

		/// <summary>
		/// Creates a new instance of the <see cref="ByteConstantExpression"/> class.
		/// </summary>
		/// <param name="value">Value of the integer.</param>
		public ByteConstantExpression( byte value ) {
			_value = value;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the result left at the top of the stack when the expression is evaluated.
		///   This property always returns <see cref="Byte"/>.</value>
		public override Type ResultType {
			get {
				return typeof(byte);
			}
		}

		/// <summary>
		/// Gets the value of the expression.
		/// </summary>
		/// <value>The value of the expression.</value>
		public short Value {
			get {
				return _value;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return "(byte) " + _value.ToString( CultureInfo.InvariantCulture );
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			switch( _value ) {
				case 0:
					cxt.Generator.Emit( OpCodes.Ldc_I4_0 );
					break;

				case 1:
					cxt.Generator.Emit( OpCodes.Ldc_I4_1 );
					break;

				case 2:
					cxt.Generator.Emit( OpCodes.Ldc_I4_2 );
					break;

				case 3:
					cxt.Generator.Emit( OpCodes.Ldc_I4_3 );
					break;

				case 4:
					cxt.Generator.Emit( OpCodes.Ldc_I4_4 );
					break;

				case 5:
					cxt.Generator.Emit( OpCodes.Ldc_I4_5 );
					break;

				case 6:
					cxt.Generator.Emit( OpCodes.Ldc_I4_6 );
					break;

				case 7:
					cxt.Generator.Emit( OpCodes.Ldc_I4_7 );
					break;

				case 8:
					cxt.Generator.Emit( OpCodes.Ldc_I4_8 );
					break;

				default:
					if( _value <= 127 ) {
						// Encode the short form
						cxt.Generator.Emit( OpCodes.Ldc_I4_S, _value );
					}
					else {
						cxt.Generator.Emit( OpCodes.Ldc_I4, (int) _value );
					}
					
					break;
			}
		}

	}
}