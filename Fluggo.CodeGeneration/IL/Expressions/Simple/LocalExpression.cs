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
	/// Represents an expression for the stored value of a local variable.
	/// </summary>
	public sealed class LocalExpression : Expression {
		LocalBuilder _localBuilder;
		string _name;

		/// <summary>
		/// Creates a new instance of the <see cref="LocalExpression"/> class.
		/// </summary>
		/// <param name="local"><see cref="LocalBuilder"/> instance describing the local variable.</param>
		/// <exception cref="ArgumentNullException"><paramref name="local"/> is <see langword='null'/>.</exception>
		public LocalExpression( LocalBuilder local )
			: this( local, null ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref="LocalExpression"/> class.
		/// </summary>
		/// <param name="local"><see cref="LocalBuilder"/> instance describing the local variable.</param>
		/// <param name="name">Optional name of the local, for use in source generation.</param>
		/// <exception cref="ArgumentNullException"><paramref name="local"/> is <see langword='null'/>.</exception>
		public LocalExpression( LocalBuilder local, string name ) {
			if( local == null )
				throw new ArgumentNullException( "local" );

			_localBuilder = local;
			_name = name;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _name == null )
				return "local" + _localBuilder.LocalIndex;
			else
				return _name;
		}

		/// <summary>
		/// Gets the name of the local.
		/// </summary>
		/// <value>The name of the local.</value>
		public string Name {
			get {
				return _name;
			}
		}

		/// <summary>
		/// Gets a reference to the <see cref="LocalBuilder"/> that represents the local.
		/// </summary>
		/// <value>A reference to the <see cref="LocalBuilder"/> that represents the local.</value>
		public LocalBuilder LocalBuilder {
			get {
				return _localBuilder;
			}
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the local variable referenced by this expression.</value>
		public override Type ResultType {
			get {
				return _localBuilder.LocalType;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
				
			cxt.EmitLoadLocal( _localBuilder );
		}

		/// <summary>
		/// Evaluates the address of the expression and stores the resulting IL in the given context.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <exception cref='ArgumentNullException'><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public override void EmitAddress( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			cxt.EmitLoadLocalAddress( _localBuilder );
		}

		/// <summary>
		/// Gets a value that represents whether the address of the expression can be taken.
		/// </summary>
		/// <value>This property always returns true.</value>
		public override bool CanTakeAddress {
			get {
				return true;
			}
		}
	}
}