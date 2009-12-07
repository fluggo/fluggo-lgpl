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
	/// Represents an expression for a constant boolean value.
	/// </summary>
	public sealed class BooleanConstantExpression : BooleanExpression {
		bool _value;

		/// <summary>
		/// Creates a new instance of the <see cref="BooleanConstantExpression"/> class.
		/// </summary>
		/// <param name="value">Value of the boolean.</param>
		public BooleanConstantExpression( bool value ) {
			_value = value;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( _value )
				cxt.Generator.Emit( OpCodes.Ldc_I4_1 );
			else
				cxt.Generator.Emit( OpCodes.Ldc_I4_0 );
		}

		public override string ToString() {
			return _value ? "true" : "false";
		}

		/// <summary>
		/// Emits a branch that occurs if the expression evaluates to true.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <param name="target">Target of the branch.</param>
		/// <exception cref="ArgumentNullException"><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public override void EmitTrueBranch( ILGeneratorContext cxt, Label target ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
				
			if( _value )
				cxt.Generator.Emit( OpCodes.Br, target );
		}

		/// <summary>
		/// Emits a branch that occurs if the expression evaluates to false.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <param name="target">Target of the branch.</param>
		/// <exception cref="ArgumentNullException"><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public override void EmitFalseBranch( ILGeneratorContext cxt, Label target ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( !_value )
				cxt.Generator.Emit( OpCodes.Br, target );
		}
	}
}