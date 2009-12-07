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
	/// Represents an expression that can cause an IL branch.
	/// </summary>
	/// <remarks>Expressions derived from <see cref="BooleanExpression"/> usually produce the shortest IL
	///   when using the <see cref="EmitTrueBranch"/> or <see cref="EmitFalseBranch"/> methods. If the expression
	///   is evaluated using the <see cref="Expression.Emit"/> method, the expression produces a result of one for true and
	///   zero for false.</remarks>
	public abstract class BooleanExpression : Expression {
		/// <summary>
		/// Emits a branch that occurs if the expression evaluates to true.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <param name="target">Target of the branch.</param>
		/// <exception cref="ArgumentNullException"><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public abstract void EmitTrueBranch( ILGeneratorContext cxt, Label target );

		/// <summary>
		/// Emits a branch that occurs if the expression evaluates to false.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <param name="target">Target of the branch.</param>
		/// <exception cref="ArgumentNullException"><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public abstract void EmitFalseBranch( ILGeneratorContext cxt, Label target );

		/// <summary>
		/// Gets the result type of the expression.
		/// </summary>
		/// <value>This property always returns <see cref="Boolean"/>.</value>
		public sealed override Type ResultType {
			get {
				return typeof( bool );
			}
		}
	}
}