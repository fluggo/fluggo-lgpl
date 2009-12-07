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
	/// Represents an expression that retrieves a value from an array.
	/// </summary>
	public class ArrayElementExpression : Expression {
		Expression _baseExpr;
		Expression _index;

		/// <summary>
		/// Creates a new instance of the <see cref='SetArrayElementExpression'/> class.
		/// </summary>
		/// <param name="baseExpr">Expression for the array into which the value should be stored.</param>
		/// <param name="index">Expression for the index where the value should be stored.</param>
		/// <exception cref='ArgumentNullException'><paramref name='baseExpr'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='index'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='StackArgumentException'><paramref name='baseExpr'/> is not an expression for an array.</exception>
		public ArrayElementExpression( Expression baseExpr, Expression index ) {
			if( baseExpr == null )
				throw new ArgumentNullException( "baseExpr" );

			if( index == null )
				throw new ArgumentNullException( "index" );
				
			if( !baseExpr.ResultType.IsArray )
				throw new StackArgumentException( "baseExpr" );
				
			_baseExpr = baseExpr;
			_index = index;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the array element referenced by this expression.</value>
		public override Type ResultType {
			get {
				return _baseExpr.ResultType.GetElementType();
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return _baseExpr.ToString() + "[" + _index.ToString() + "]";
		}
		
		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			_baseExpr.Emit( cxt );
			_index.Emit( cxt );

			switch( Type.GetTypeCode( _baseExpr.ResultType.GetElementType() ) ) {
				case TypeCode.Boolean:
				case TypeCode.SByte:
					cxt.Generator.Emit( OpCodes.Ldelem_I1 );
					break;

				case TypeCode.Byte:
					cxt.Generator.Emit( OpCodes.Ldelem_U1 );
					break;

				case TypeCode.Int16:
					cxt.Generator.Emit( OpCodes.Stelem_I2 );
					break;

				case TypeCode.UInt16:
				case TypeCode.Char:
					cxt.Generator.Emit( OpCodes.Ldelem_U1 );
					break;

				case TypeCode.Int32:
					cxt.Generator.Emit( OpCodes.Ldelem_I4 );
					break;

				case TypeCode.UInt32:
					cxt.Generator.Emit( OpCodes.Ldelem_U4 );
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					cxt.Generator.Emit( OpCodes.Ldelem_I8 );
					break;

				case TypeCode.Single:
					cxt.Generator.Emit( OpCodes.Ldelem_R4 );
					break;

				case TypeCode.Double:
					cxt.Generator.Emit( OpCodes.Ldelem_R8 );
					break;

				default:
					if( ResultType.IsValueType ) {
						// Here's what the C# 1.1 compiler does
						cxt.Generator.Emit( OpCodes.Ldelema, ResultType );
						cxt.Generator.Emit( OpCodes.Ldobj, ResultType );
					}
					else {
						cxt.Generator.Emit( OpCodes.Ldelem_Ref );
					}
					break;
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

			_baseExpr.Emit( cxt );
			_index.Emit( cxt );
			cxt.Generator.Emit( OpCodes.Ldelema, _baseExpr.ResultType.GetElementType() );
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