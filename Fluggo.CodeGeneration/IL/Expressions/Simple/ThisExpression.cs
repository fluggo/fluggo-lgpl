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
	/// Represents an expression for the current instance.
	/// </summary>
	/// <remarks>When used in static methods, this expression evaluates to the first argument.</remarks>
	public sealed class ThisExpression : Expression {
		Type _thisType;

		/// <summary>
		/// Creates a new instance of the <see cref="ThisExpression"/> class.
		/// </summary>
		/// <param name="thisType">Type of the object containing the current method.</param>
		public ThisExpression( Type thisType ) {
			if( thisType == null )
				throw new ArgumentNullException( "thisType" );

			_thisType = thisType;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return "this";
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( cxt.CodeMember.IsStatic )
				throw new InvalidOperationException( "ThisExpression cannot be evaluated in a static context." );

			cxt.Generator.Emit( OpCodes.Ldarg_0 );
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the current object, as supplied in the constructor.</value>
		public override Type ResultType {
			get {
				return _thisType;
			}
		}
	}
}