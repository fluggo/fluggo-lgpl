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
	/// Represents an expression that creates a new CLR vector.
	/// </summary>
	public class NewArrayExpression : Expression {
		Type _type;
		Expression _countExpr;

		/// <summary>
		/// Creates a new instance of the <see cref='NewArrayExpression'/> class.
		/// </summary>
		/// <param name="elementType">Type of the elements in the array.</param>
		/// <param name="countExpr">Expression for the number of elements in the new array.</param>
		public NewArrayExpression( Type elementType, Expression countExpr ) {
			if( elementType == null )
				throw new ArgumentNullException( "elementType" );

			if( countExpr == null )
				throw new ArgumentNullException( "countExpr" );
				
			_type = elementType;
			_countExpr = countExpr;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the new object created by this expression.</value>
		public override Type ResultType {
			get {
				return _type.MakeArrayType();
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return "new " + _type.Name + "[" + _countExpr.ToString() + "]";
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			_countExpr.Emit( cxt );
			cxt.Generator.Emit( OpCodes.Newarr, _type );
		}
	}
}