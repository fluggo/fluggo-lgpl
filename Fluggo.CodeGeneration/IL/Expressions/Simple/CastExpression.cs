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
	/// Represents a conversion from one type to another.
	/// </summary>
	public class CastExpression : Expression {
		Expression _baseExp;
		Type _targetType;
		bool _throwOnOverflow;

		/// <summary>
		/// Creates a new instance of the <see cref='CastExpression'/> class.
		/// </summary>
		/// <param name="baseExp">Expression to cast.</param>
		/// <param name="targetType">Target type of the cast.</param>
		/// <param name="throwOnOverflow">If true, the cast throws an <see cref="OverflowException"/>
		///   when an overflow condition is produced on a numeric conversion.</param>
		/// <exception cref='ArgumentNullException'><paramref name='baseExp'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='targetType'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='StackArgumentException'><paramref name='baseExp'/> is void.</exception>
		public CastExpression( Expression baseExp, Type targetType, bool throwOnOverflow ) {
			if( baseExp == null )
				throw new ArgumentNullException( "baseExp" );

			if( targetType == null )
				throw new ArgumentNullException( "targetType" );
				
			if( baseExp.ResultType == typeof(void) )
				throw new StackArgumentException( "baseExp" );

			switch( Type.GetTypeCode( baseExp.ResultType ) ) {
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					switch( Type.GetTypeCode( targetType ) ) {
						case TypeCode.Byte:
						case TypeCode.Char:
						case TypeCode.Double:
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.SByte:
						case TypeCode.Single:
						case TypeCode.UInt16:
						case TypeCode.UInt32:
						case TypeCode.UInt64:
							break;

						case TypeCode.Boolean:
						default:
							throw new ArgumentException( "Cannot perform a cast from " + GetSimpleTypeName( baseExp.ResultType )
								+ " to " + GetSimpleTypeName( targetType ) + "." );
					}
					break;

				default:
					if( baseExp.ResultType.IsValueType )
						throw new ArgumentException( baseExp.ResultType.Name + " is a value type and cannot be cast to another type.", "baseExp" );

					break;
			}

			_baseExp = baseExp;
			_targetType = targetType;
			_throwOnOverflow = throwOnOverflow;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The target <see cref="Type"/> of the cast.</value>
		public override Type ResultType {
			get {
				return _targetType;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return "(" + GetSimpleTypeName( _targetType ) + ") " + _baseExp.ToString();
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			_baseExp.Emit( cxt );
			
			switch( Type.GetTypeCode( _baseExp.ResultType ) ) {
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					EmitNumericConversion( cxt );
					break;

				default:
					if( _baseExp.ResultType.IsValueType )
						throw new UnexpectedException();
					
					cxt.Generator.Emit( OpCodes.Castclass, _targetType );
					break;
			}
		}
		
		private static bool IsSigned( Type type ) {
			switch( Type.GetTypeCode( type ) ) {
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
					return true;
				
				default:
					return false;
			}
		}
		
		private void EmitNumericConversion( ILGeneratorContext cxt ) {
			switch( Type.GetTypeCode( _targetType ) ) {
				case TypeCode.SByte:
					if( _throwOnOverflow )
						cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_Ovf_I1 : OpCodes.Conv_Ovf_I1_Un );
					else
						cxt.Generator.Emit( OpCodes.Conv_I1 );
					break;

				case TypeCode.Int16:
					if( _throwOnOverflow )
						cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_Ovf_I2 : OpCodes.Conv_Ovf_I2_Un );
					else
						cxt.Generator.Emit( OpCodes.Conv_I2 );
					break;

				case TypeCode.Int32:
					if( _throwOnOverflow )
						cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_Ovf_I4 : OpCodes.Conv_Ovf_I4_Un );
					else
						cxt.Generator.Emit( OpCodes.Conv_I4 );
					break;

				case TypeCode.Int64:
					if( _throwOnOverflow )
						cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_Ovf_I8 : OpCodes.Conv_Ovf_I8_Un );
					else
						cxt.Generator.Emit( OpCodes.Conv_I8 );
					break;

				case TypeCode.Single:
					cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_R4 : OpCodes.Conv_R_Un );
					break;
				
				case TypeCode.Double:
					cxt.Generator.Emit( OpCodes.Conv_R8 );
					break;

				case TypeCode.Byte:
					if( _throwOnOverflow )
						cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_Ovf_U1 : OpCodes.Conv_Ovf_U1_Un );
					else
						cxt.Generator.Emit( OpCodes.Conv_U1 );
					break;

				case TypeCode.Char:
				case TypeCode.UInt16:
					if( _throwOnOverflow )
						cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_Ovf_U2 : OpCodes.Conv_Ovf_U2_Un );
					else
						cxt.Generator.Emit( OpCodes.Conv_U2 );
					break;
				
				case TypeCode.UInt32:
					if( _throwOnOverflow )
						cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_Ovf_U4 : OpCodes.Conv_Ovf_U4_Un );
					else
						cxt.Generator.Emit( OpCodes.Conv_U4 );
					break;

				case TypeCode.UInt64:
					if( _throwOnOverflow )
						cxt.Generator.Emit( IsSigned( _baseExp.ResultType ) ? OpCodes.Conv_Ovf_U8 : OpCodes.Conv_Ovf_U8_Un );
					else
						cxt.Generator.Emit( OpCodes.Conv_U8 );
					break;

				case TypeCode.Boolean:
				default:
					throw new UnexpectedException();
			}
		}
	}
}