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
	/// Represents an expression that retrieves the value of a field.
	/// </summary>
	public sealed class FieldExpression : Expression {
		Expression _expr;
		FieldInfo _field;

		/// <summary>
		/// Creates a new instance of the <see cref='FieldExpression'/> class that accesses an instance field.
		/// </summary>
		/// <param name="baseExpression">Expression for an instance on which the field is found.</param>
		/// <param name="field"><see cref="FieldInfo"/> describing an instance field on <paramref name="baseExpression"/>.</param>
		/// <exception cref='ArgumentNullException'><paramref name='baseExpression'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='field'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='ArgumentException'><paramref name='field'/> is not an instance field.
		///   <para>— OR —</para>
		///   <para><paramref name='field'/> is not present on the type represented by <paramref name="baseExpression"/>.</para></exception>
		public FieldExpression( Expression baseExpression, FieldInfo field ) {
			if( baseExpression == null )
				throw new ArgumentNullException( "baseExpression" );

			if( field == null )
				throw new ArgumentNullException( "field" );

			if( field.IsStatic )
				throw new ArgumentException( "The given field must be an instance field.", "field" );

			if( !field.DeclaringType.IsAssignableFrom( baseExpression.ResultType ) )
				throw new ArgumentException( string.Format( "{0} does not contain a definition for {1}.", baseExpression.ResultType,
					field.Name ), "baseExpression" );

			_expr = baseExpression;
			_field = field;
		}

		/// <summary>
		/// Creates a new instance of the <see cref='FieldExpression'/> class that accesses a static field.
		/// </summary>
		/// <param name="field"><see cref="FieldInfo"/> describing a static field.</param>
		/// <exception cref='ArgumentNullException'><paramref name='field'/> is <see langword='null'/>.</exception>
		/// <exception cref='ArgumentException'><paramref name='field'/> is not a static field.</exception>
		public FieldExpression( FieldInfo field ) {
			if( field == null )
				throw new ArgumentNullException( "field" );

			if( !field.IsStatic )
				throw new ArgumentException( "The given field must be a static field.", "field" );

			_field = field;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _expr is ThisExpression )
				return _field.Name;

			return _expr.ToString() + "." + _field.Name;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the field referenced by this expression.</value>
		public override Type ResultType {
			get {
				return _field.FieldType;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( _expr == null ) {
				// Static load
				cxt.Generator.Emit( OpCodes.Ldsfld, _field );
			}
			else {
				// Instance load
				_expr.Emit( cxt );
				cxt.Generator.Emit( OpCodes.Ldfld, _field );
			}
		}

		/// <summary>
		/// Evaluates the address of the expression and stores the resulting IL in the given context.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <exception cref='ArgumentNullException'><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public override void EmitAddress( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( _expr == null ) {
				// Static load
				cxt.Generator.Emit( OpCodes.Ldsflda, _field );
			}
			else {
				// Instance load
				_expr.Emit( cxt );
				cxt.Generator.Emit( OpCodes.Ldflda, _field );
			}
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