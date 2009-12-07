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
	/// Represents an expression that creates a new object.
	/// </summary>
	public sealed class NewObjectExpression : Expression {
		ConstructorInfo _ctor;
		Expression[] _args;

		/// <summary>
		/// Creates a new instance of the <see cref="NewObjectExpression"/> class.
		/// </summary>
		/// <param name="ctor"><see cref="ConstructorInfo"/> describing a constructor on the object to create.</param>
		/// <param name="args">Arguments to the constructor.</param>
		public NewObjectExpression( ConstructorInfo ctor, Expression[] args ) {
			if( ctor == null )
				throw new ArgumentNullException( "ctor" );
				
			if( args == null )
				args = new Expression[0];
		
			_ctor = ctor;
			_args = args;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			string @base = "new " + _ctor.DeclaringType.Name;

			if( _args.Length == 0 )
				return @base + "()";
			else
				return @base + "( " + string.Join( ", ", ToStringArray( _args, 0, _args.Length ) ) + " )";
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the new object created by this expression.</value>
		public override Type ResultType {
			get {
				return _ctor.DeclaringType;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			foreach( Expression exp in _args )
				exp.Emit( cxt );

			cxt.Generator.Emit( OpCodes.Newobj, _ctor );
		}
	}
}