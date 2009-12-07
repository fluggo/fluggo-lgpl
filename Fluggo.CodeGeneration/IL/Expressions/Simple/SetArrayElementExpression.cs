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
	/// Represents an expression that stores a value into an array.
	/// </summary>
	public class SetArrayElementExpression : Expression {
		Expression _baseExpr;
		Expression _index;
		Expression _value;

		/// <summary>
		/// Creates a new instance of the <see cref='SetArrayElementExpression'/> class.
		/// </summary>
		/// <param name="baseExpr">Expression for the array into which the value should be stored.</param>
		/// <param name="index">Expression for the index where the value should be stored.</param>
		/// <param name="value">Expression for the value to store in the array.</param>
		/// <exception cref='ArgumentNullException'><paramref name='baseExpr'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='index'/> is <see langword='null'/>.</para>
		///   <para>— OR —</para>
		///   <para><paramref name='value'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='StackArgumentException'><paramref name='baseExpr'/> is not an expression for an array.
		///   <para>— OR —</para>
		///   <para><paramref name='value'/> cannot be stored in an array of the given type.</para></exception>
		public SetArrayElementExpression( Expression baseExpr, Expression index, Expression value ) {
			if( baseExpr == null )
				throw new ArgumentNullException( "baseExpr" );

			if( index == null )
				throw new ArgumentNullException( "index" );

			if( value == null )
				throw new ArgumentNullException( "value" );
				
			if( !baseExpr.ResultType.IsArray )
				throw new StackArgumentException( "baseExpr" );
				
			if( !baseExpr.ResultType.GetElementType().IsAssignableFrom( value.ResultType ) )
				throw new StackArgumentException( "value" );
			
			_baseExpr = baseExpr;
			_index = index;
			_value = value;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return _baseExpr.ToString() + "[" + _index.ToString() + "] = " + _value.ToString();
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			_baseExpr.Emit( cxt );
			_index.Emit( cxt );
			
			switch( Type.GetTypeCode( _baseExpr.ResultType.GetElementType() ) ) {
				case TypeCode.Byte:
				case TypeCode.Boolean:
				case TypeCode.SByte:
					_value.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Stelem_I1 );
					break;
				
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Char:
					_value.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Stelem_I2 );
					break;
				
				case TypeCode.Int32:
				case TypeCode.UInt32:
					_value.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Stelem_I4 );
					break;
				
				case TypeCode.Int64:
				case TypeCode.UInt64:
					_value.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Stelem_I8 );
					break;
					
				case TypeCode.Single:
					_value.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Stelem_R4 );
					break;
					
				case TypeCode.Double:
					_value.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Stelem_R8 );
					break;
				
				default:
					if( _value.ResultType.IsValueType ) {
						// Here's what the C# 1.1 compiler does
						cxt.Generator.Emit( OpCodes.Ldelema, _value.ResultType );
						_value.Emit( cxt );
						cxt.Generator.Emit( OpCodes.Stobj, _value.ResultType );
					}
					else {
						_value.Emit( cxt );
						cxt.Generator.Emit( OpCodes.Stelem_Ref );
					}
					break;
			}
		}
	}
}