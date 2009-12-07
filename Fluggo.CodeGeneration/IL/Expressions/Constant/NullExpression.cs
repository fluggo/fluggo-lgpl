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
	/// Represents an expression that evaluates to <see langword='null'/>.
	/// </summary>
	public sealed class NullExpression : Expression {
		Type _type;

		/// <summary>
		/// Creates a new instance of the <see cref="NullExpression"/> class.
		/// </summary>
		/// <param name="type"><see cref="Type"/> of the null reference.</param>
		/// <remarks>IL does not consider the type of a null reference. The <paramref name="type"/> parameter is used to
		///   resolve the resulting type of the expression.</remarks>
		/// <exception cref='ArgumentNullException'><paramref name='type'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="type"/> represents a value type.</exception>
		public NullExpression( Type type ) {
			if( type == null )
				throw new ArgumentNullException( "type" );
				
			if( type.IsValueType )
				throw new ArgumentException( "The type is a value type and does not have a null representation.", "type" );

			_type = type;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the result left at the top of the stack when the expression is evaluated.
		///   This is the type supplied in the constructor.</value>
		public override Type ResultType {
			get {
				return _type;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return "null";
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			cxt.Generator.Emit( OpCodes.Ldnull );
		}
	}
}