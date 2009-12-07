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
	/// Represents an expression that stores a value into a field.
	/// </summary>
	public sealed class StoreFieldExpression : Expression {
		Expression _expr, _valueExpr;
		FieldInfo _field;

		/// <summary>
		/// Creates a new instance of the <see cref="StoreFieldExpression"/> class.
		/// </summary>
		/// <param name="baseExpression">Expression for the instance containing the target field.</param>
		/// <param name="field"><see cref="FieldInfo"/> describing the field into which data will be stored.</param>
		/// <param name="valueExpression">Expression for the value to store.</param>
		/// <exception cref="ArgumentNullException"><paramref name="baseExpression"/> is <see langword='null'/>.
		///   <para>- OR -</para>
		///   <para><paramref name="field"/> is <see langword='null'/>.</para>
		///   <para>- OR -</para>
		///   <para><paramref name="valueExpression"/> is <see langword='null'/>.</para></exception>
		/// <exception cref="ArgumentExpression"><paramref name="field"/> refers to a static field.</exception>
		public StoreFieldExpression( Expression baseExpression, FieldInfo field, Expression valueExpression ) {
			if( baseExpression == null )
				throw new ArgumentNullException( "baseExpression" );

			if( field == null )
				throw new ArgumentNullException( "field" );

			if( field.IsStatic )
				throw new ArgumentException( "Static fields are not supported by this constructor.", "field" );

			if( valueExpression == null )
				throw new ArgumentNullException( "valueExpression" );

			if( baseExpression.ResultType == typeof( void ) )
				throw new StackArgumentException( "baseExpression" );

			if( !field.FieldType.IsAssignableFrom( valueExpression.ResultType ) )
				throw new StackArgumentException( "valueExpression" );

			_expr = baseExpression;
			_field = field;
			_valueExpr = valueExpression;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="StoreFieldExpression"/> class.
		/// </summary>
		/// <param name="field"><see cref="FieldInfo"/> describing the field into which data will be stored.</param>
		/// <param name="valueExpression">Expression for the value to store.</param>
		/// <exception cref="ArgumentNullException"><paramref name="baseExpression"/> is <see langword='null'/>.
		///   <para>- OR -</para>
		///   <para><paramref name="field"/> is <see langword='null'/>.</para>
		///   <para>- OR -</para>
		///   <para><paramref name="valueExpression"/> is <see langword='null'/>.</para></exception>
		/// <exception cref="ArgumentExpression"><paramref name="field"/> refers to an instance field.</exception>
		public StoreFieldExpression( FieldInfo field, Expression valueExpression ) {
			if( field == null )
				throw new ArgumentNullException( "field" );

			if( !field.IsStatic )
				throw new ArgumentException( "Instance fields are not supported by this constructor.", "field" );

			if( valueExpression == null )
				throw new ArgumentNullException( "valueExpression" );

			if( field.FieldType.IsAssignableFrom( valueExpression.ResultType ) )
				throw new StackArgumentException( "valueExpression" );

			_field = field;
			_valueExpr = valueExpression;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _expr == null ) {
				return _field.DeclaringType.Name + "." + _field.Name + " = " +
					_valueExpr.ToString();
			}
			else {
				if( _expr is ThisExpression )
					return _field.Name + " = " + _valueExpr.ToString();
				else
					return _expr.ToString() + "." + _field.Name + " = " + _valueExpr.ToString();
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( _expr != null ) {
				// Instance field
				if( _expr.ResultType.IsValueType )
					_expr.EmitAddress( cxt );
				else
					_expr.Emit( cxt );
					
				_valueExpr.Emit( cxt );
				cxt.Generator.Emit( OpCodes.Stfld, _field );
			}
			else {
				// Static field
				_valueExpr.Emit( cxt );
				cxt.Generator.Emit( OpCodes.Stsfld, _field );
			}
		}
	}
}