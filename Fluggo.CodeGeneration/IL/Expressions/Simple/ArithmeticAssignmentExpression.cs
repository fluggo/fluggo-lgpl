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
	/// Represents an expression that performs an arithmetic operation and stores it back into the left operand.
	/// </summary>
	public class ArithmeticAssignmentExpression : Expression {
		IDataStore _leftOper;
		Expression _rightOper;
		ArithmeticOperator _oper;
		bool _checked;

		/// <summary>
		/// Creates a new instance of the <see cref='ArithmeticAssignmentExpression'/> class.
		/// </summary>
		/// <param name="leftOperand"><see cref="IDataStore"/> for the left operand to the operation. This store will receive
		///   the result of the operation when this expression is evaluated.</param>
		/// <param name="operator"><see cref="ArithmeticOperator"/> describing the operation to perform.</param>
		/// <param name="rightOperand">An expression for the right operand to the operation.</param>
		/// <param name="checked">True if an exception should be thrown if an overflow occurs, or false to ignore overflow conditions.</param>
		/// <exception cref='ArgumentNullException'><paramref name='leftOperand'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='rightOperand'/> is <see langword='null'/>.</para></exception>
		/// <exception cref="Exception">The types of the operands do not match.</exception>
		public ArithmeticAssignmentExpression( IDataStore leftOperand, ArithmeticOperator @operator, Expression rightOperand, bool @checked ) {
			if( leftOperand == null )
				throw new ArgumentNullException( "leftOperand" );

			if( rightOperand == null )
				throw new ArgumentNullException( "rightOperand" );
			
			if( leftOperand.Type != rightOperand.ResultType )
				throw new ArgumentException( "The operand types do not match." );

			switch( @operator ) {
				case ArithmeticOperator.Add:
				case ArithmeticOperator.Subtract:
				case ArithmeticOperator.Multiply:
				case ArithmeticOperator.Divide:
					break;
					
				default:
					throw new ArgumentOutOfRangeException( "operator" );
			}

			_leftOper = leftOperand;
			_rightOper = rightOperand;
			_oper = @operator;
			_checked = @checked;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			string leftStr = _leftOper.Get().ToString();
			string rightStr = _rightOper.ToString();
			
			Int32ConstantExpression int32const = _rightOper as Int32ConstantExpression;
			
			if( int32const != null ) {
				if( _oper == ArithmeticOperator.Add && int32const.Value == 1 )
					return leftStr + "++";
					
				if( _oper == ArithmeticOperator.Subtract && int32const.Value == 1 )
					return leftStr + "--";
			}

			switch( _oper ) {
				case ArithmeticOperator.Add:
					return leftStr + " += " + rightStr;
				case ArithmeticOperator.Subtract:
					return leftStr + " -= " + rightStr;
				case ArithmeticOperator.Multiply:
					return leftStr + " *= " + rightStr;
				case ArithmeticOperator.Divide:
					return leftStr + " /= " + rightStr;
					
				default:
					throw new UnexpectedException();
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
				
			_leftOper.Set( new ArithmeticExpression( _leftOper.Get(), _oper, _rightOper, _checked ) ).Emit( cxt );
		}
	}
}