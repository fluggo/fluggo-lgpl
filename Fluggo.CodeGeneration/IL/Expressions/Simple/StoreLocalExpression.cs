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
	/// Represents an expression that stores a value into a local variable.
	/// </summary>
	public sealed class StoreLocalExpression : Expression {
		LocalBuilder _localBuilder;
		string _localName;
		Expression _expr;

		/// <summary>
		/// Creates a new instance of the <see cref='StoreLocalExpression'/> class.
		/// </summary>
		/// <param name="local"><see cref="LocalBuilder"/> representing the local variable.</param>
		/// <param name="expr">An expression for the value to store.</param>
		/// <exception cref='ArgumentNullException'><paramref name='local'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='expr'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='StackArgumentException'><paramref name='expr'/> is not assignable to a local of the given type.</exception>
		public StoreLocalExpression( LocalBuilder local, Expression expr )
			: this( local, expr, null ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='StoreLocalExpression'/> class.
		/// </summary>
		/// <param name="local"><see cref="LocalBuilder"/> representing the local variable.</param>
		/// <param name="expr">An expression for the value to store.</param>
		/// <param name="name">Optional name of the local.</param>
		/// <exception cref='ArgumentNullException'><paramref name='local'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='expr'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='StackArgumentException'><paramref name='expr'/> is not assignable to a local of the given type.</exception>
		public StoreLocalExpression( LocalBuilder local, Expression expr, string name ) {
			if( local == null )
				throw new ArgumentNullException( "local" );

			if( expr == null )
				throw new ArgumentNullException( "expr" );

			if( !local.LocalType.IsAssignableFrom( expr.ResultType ) )
				throw new StackArgumentException( "expr" );

			_localBuilder = local;
			_localName = name;
			_expr = expr;
		}

		public StoreLocalExpression( LocalExpression local, Expression expr )
			: this( local.LocalBuilder, expr, local.Name ) {
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _localName == null )
				return "local" + _localBuilder.LocalIndex + " = " + _expr.ToString();
			else
				return _localName + " = " + _expr.ToString();
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
				
			cxt.EmitStoreLocal( _localBuilder, _expr );
		}
	}
}