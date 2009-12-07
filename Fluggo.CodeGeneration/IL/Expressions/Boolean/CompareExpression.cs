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
	/// Describes a binary comparison operator.
	/// </summary>
	public enum CompareOperator {
		/// <summary>
		/// True when the left operand is less than the right operand.
		/// </summary>
		LessThan,

		/// <summary>
		/// True when the left operand is less than or equal to the right operand.
		/// </summary>
		LessThanEquals,

		/// <summary>
		/// True when the left operand is greater than to the right operand.
		/// </summary>
		GreaterThan,

		/// <summary>
		/// True when the left operand is greater than or equal to the right operand.
		/// </summary>
		GreaterThanEquals,

		/// <summary>
		/// True when the left operand is equal to the right operand.
		/// </summary>
		Equals,

		/// <summary>
		/// True when the left operand is not equal to the right operand.
		/// </summary>
		NotEquals
	}

	/// <summary>
	/// Represents a comparison between two expressions with a boolean result.
	/// </summary>
	public class CompareExpression : BooleanExpression {
		Expression _leftOp, _rightOp;
		CompareOperator _oper;

		/// <summary>
		/// Creates a new instance of the <see cref='CompareExpression'/> class.
		/// </summary>
		/// <param name="leftOperand">Left expression for the comparison.</param>
		/// <param name="operator"><see cref="CompareOperator"/> value describing the type of comparison to perform.</param>
		/// <param name="rightOperand">Right expression for the comparison.</param>
		/// <exception cref='ArgumentNullException'><paramref name='leftOperand'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='rightOperand'/> is <see langword='null'/>.</para></exception>
		/// <exception cref="StackArgumentException"><paramref name="leftOperand"/> or <paramref name="rightOperand"/> is an expression of type <see cref="Void"/>.</exception>
		/// <exception cref="ArgumentException">The result types of <paramref name="leftOperand"/> and <paramref name="rightOperand"/> do not match.</exception>
		/// <remarks>This expression does not ensure that it makes sense to compare</remarks>
		public CompareExpression( Expression leftOperand, CompareOperator @operator, Expression rightOperand ) {
			if( leftOperand == null )
				throw new ArgumentNullException( "leftOperand" );

			if( rightOperand == null )
				throw new ArgumentNullException( "rightOperand" );

			if( leftOperand.ResultType == typeof( void ) )
				throw new StackArgumentException( "leftOperand" );

			if( rightOperand.ResultType == typeof( void ) )
				throw new StackArgumentException( "rightOperand" );
				
			if( leftOperand.ResultType != rightOperand.ResultType )
				throw new ArgumentException( "The given expression result types do not match." );

			switch( @operator ) {
				case CompareOperator.LessThan:
				case CompareOperator.LessThanEquals:
				case CompareOperator.GreaterThan:
				case CompareOperator.GreaterThanEquals:
				case CompareOperator.Equals:
				case CompareOperator.NotEquals:
					break;

				default:
					throw new ArgumentOutOfRangeException( "operator" );
			}

			_leftOp = leftOperand;
			_rightOp = rightOperand;
			_oper = @operator;
		}

		private static bool IsTypeUnsigned( Type type ) {
			switch( Type.GetTypeCode( type ) ) {
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
				default:
					return false;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			string op;

			switch( _oper ) {
				case CompareOperator.LessThan:
					op = "<";
					break;
				case CompareOperator.LessThanEquals:
					op = "<=";
					break;
				case CompareOperator.GreaterThan:
					op = ">";
					break;
				case CompareOperator.GreaterThanEquals:
					op = ">=";
					break;
				case CompareOperator.Equals:
					op = "==";
					break;
				case CompareOperator.NotEquals:
					op = "!=";
					break;
				default:
					throw new UnexpectedException();
			}

			return _leftOp + " " + op + " " + _rightOp;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			_leftOp.Emit( cxt );
			_rightOp.Emit( cxt );

			switch( _oper ) {
				case CompareOperator.LessThan:
					// a < b
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Clt_Un );
					else
						cxt.Generator.Emit( OpCodes.Clt );

					break;

				case CompareOperator.LessThanEquals:
					// !(a > b)
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Cgt_Un );
					else
						cxt.Generator.Emit( OpCodes.Cgt );

					cxt.Generator.Emit( OpCodes.Ldc_I4_0 );
					cxt.Generator.Emit( OpCodes.Ceq );

					break;

				case CompareOperator.GreaterThan:
					// a > b
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Cgt_Un );
					else
						cxt.Generator.Emit( OpCodes.Cgt );

					break;

				case CompareOperator.GreaterThanEquals:
					// !(a < b)
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Clt_Un );
					else
						cxt.Generator.Emit( OpCodes.Clt );

					cxt.Generator.Emit( OpCodes.Ldc_I4_0 );
					cxt.Generator.Emit( OpCodes.Ceq );

					break;

				case CompareOperator.Equals:
					// a == b
					cxt.Generator.Emit( OpCodes.Ceq );
					break;

				case CompareOperator.NotEquals:
					// !(a == b)
					cxt.Generator.Emit( OpCodes.Ceq );
					cxt.Generator.Emit( OpCodes.Ldc_I4_0 );
					cxt.Generator.Emit( OpCodes.Ceq );
					break;

				default:
					throw new UnexpectedException();
			}
		}

		/// <summary>
		/// Emits a branch that occurs if the expression evaluates to true.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <param name="target">Target of the branch.</param>
		/// <exception cref="ArgumentNullException"><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public override void EmitTrueBranch( ILGeneratorContext cxt, Label target ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			// Shortcut for == null and != null
			if( _oper == CompareOperator.Equals ) {
				if( _leftOp is NullExpression ) {
					// Branch if _rightOp is null
					_rightOp.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Brfalse, target );
					return;
				}
				else if( _rightOp is NullExpression ) {
					// Branch if _leftOp is null
					_leftOp.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Brfalse, target );
					return;
				}
			}
			else if( _oper == CompareOperator.NotEquals ) {
				if( _leftOp is NullExpression ) {
					// Branch if _rightOp is not null
					_rightOp.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Brtrue, target );
					return;
				}
				else if( _rightOp is NullExpression ) {
					// Branch if _leftOp is not null
					_leftOp.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Brtrue, target );
					return;
				}
			}
			
			_leftOp.Emit( cxt );
			_rightOp.Emit( cxt );

			switch( _oper ) {
				case CompareOperator.LessThan:
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Blt_Un, target );
					else
						cxt.Generator.Emit( OpCodes.Blt, target );

					break;

				case CompareOperator.LessThanEquals:
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Ble_Un, target );
					else
						cxt.Generator.Emit( OpCodes.Ble, target );

					break;

				case CompareOperator.GreaterThan:
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Bgt_Un, target );
					else
						cxt.Generator.Emit( OpCodes.Bgt, target );

					break;

				case CompareOperator.GreaterThanEquals:
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Bge_Un, target );
					else
						cxt.Generator.Emit( OpCodes.Bge, target );

					break;

				case CompareOperator.Equals:
					cxt.Generator.Emit( OpCodes.Beq, target );
					break;

				case CompareOperator.NotEquals:
					cxt.Generator.Emit( OpCodes.Ceq );
					cxt.Generator.Emit( OpCodes.Brfalse, target );
					break;

				default:
					throw new UnexpectedException();
			}
		}

		/// <summary>
		/// Emits a branch that occurs if the expression evaluates to false.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <param name="target">Target of the branch.</param>
		/// <exception cref="ArgumentNullException"><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public override void EmitFalseBranch( ILGeneratorContext cxt, Label target ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			_leftOp.Emit( cxt );
			_rightOp.Emit( cxt );

			switch( _oper ) {
				case CompareOperator.LessThan:
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Bge_Un, target );
					else
						cxt.Generator.Emit( OpCodes.Bge, target );

					break;

				case CompareOperator.LessThanEquals:
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Bgt_Un, target );
					else
						cxt.Generator.Emit( OpCodes.Bgt, target );

					break;

				case CompareOperator.GreaterThan:
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Ble_Un, target );
					else
						cxt.Generator.Emit( OpCodes.Ble, target );

					break;

				case CompareOperator.GreaterThanEquals:
					if( IsTypeUnsigned( _leftOp.ResultType ) )
						cxt.Generator.Emit( OpCodes.Blt_Un, target );
					else
						cxt.Generator.Emit( OpCodes.Blt, target );

					break;

				case CompareOperator.Equals:
					cxt.Generator.Emit( OpCodes.Ceq );
					cxt.Generator.Emit( OpCodes.Brfalse, target );
					break;

				case CompareOperator.NotEquals:
					cxt.Generator.Emit( OpCodes.Beq, target );
					break;

				default:
					throw new UnexpectedException();
			}
		}
	}
}