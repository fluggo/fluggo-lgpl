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
	/// Represents an expression that throws an exception.
	/// </summary>
	public sealed class ThrowExpression : Expression {
		Expression _exp;

		/// <summary>
		/// Creates a new instance of the <see cref="ThrowExpression"/> class.
		/// </summary>
		/// <param name="exp">Expression that evaluates to an <see cref="Exception"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="exp"/> is <see langword='null'/>.</exception>
		/// <exception cref="StackArgumentException"><paramref name="exp"/> does not evaluate to an <see cref="Exception"/>.</exception>
		public ThrowExpression( Expression exp ) {
			if( exp == null )
				throw new ArgumentNullException( "exp" );

			if( !typeof( Exception ).IsAssignableFrom( exp.ResultType ) )
				throw new StackArgumentException( "exp" );

			_exp = exp;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ThrowExpression"/> class.
		/// </summary>
		/// <param name="ctor"><see cref="ConstructorInfo"/> for a constructor on an exception class.</param>
		/// <param name="args">Arguments for the exception's constructor.</param>
		/// <exception cref="ArgumentNullException"><paramref name="ctor"/> is <see langword='null'/>.</exception>
		/// <exception cref="StackArgumentException"><paramref name="ctor"/> does not belong to a type that derives from
		///   <see cref="Exception"/>.</exception>
		public ThrowExpression( ConstructorInfo ctor, Expression[] args )
			: this( new NewObjectExpression( ctor, args ) ) {
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return "throw " + _exp.ToString();
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			_exp.Emit( cxt );
			cxt.Generator.Emit( OpCodes.Throw );
		}
	}
}