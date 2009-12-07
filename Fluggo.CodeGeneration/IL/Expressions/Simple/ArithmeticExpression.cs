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
	/// Describes an arithmetic operation.
	/// </summary>
	public enum ArithmeticOperator {
		/// <summary>
		/// Addition.
		/// </summary>
		Add,
		
		/// <summary>
		/// Subtraction, where the right operand is subtracted from the left.
		/// </summary>
		Subtract,
		
		/// <summary>
		/// Multiplication.
		/// </summary>
		Multiply,
		
		/// <summary>
		/// Division, where the left operand is divided by the right operand.
		/// </summary>
		Divide
	}
	
	/// <summary>
	/// Represents an expression that performs an arithmetic operation.
	/// </summary>
	public class ArithmeticExpression : Expression {
		Expression _leftOp, _rightOp;
		ArithmeticOperator _oper;
		bool _checked;

		/// <summary>
		/// Creates a new instance of the <see cref='ArithmeticExpression'/> class.
		/// </summary>
		/// <param name="leftOperand">An expression for the left operand to the operation.</param>
		/// <param name="operator"><see cref="ArithmeticOperator"/> describing the operation to perform.</param>
		/// <param name="rightOperand">An expression for the right operand to the operation.</param>
		/// <param name="checked">True if an exception should be thrown if an overflow occurs, or false to ignore overflow conditions.</param>
		/// <exception cref='ArgumentNullException'><paramref name='leftOperand'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='rightOperand'/> is <see langword='null'/>.</para></exception>
		/// <exception cref="Exception">The types of the operands do not match.</exception>
		public ArithmeticExpression( Expression leftOperand, ArithmeticOperator @operator, Expression rightOperand, bool @checked ) {
			if( leftOperand == null )
				throw new ArgumentNullException( "leftOperand" );

			if( rightOperand == null )
				throw new ArgumentNullException( "rightOperand" );
				
			if( leftOperand.ResultType != rightOperand.ResultType )
				throw new ArgumentException( "The types of the operands do not match." );

			switch( @operator ) {
				case ArithmeticOperator.Add:
				case ArithmeticOperator.Subtract:
				case ArithmeticOperator.Multiply:
				case ArithmeticOperator.Divide:
					break;

				default:
					throw new ArgumentOutOfRangeException( "operator" );
			}

			_leftOp = leftOperand;
			_rightOp = rightOperand;
			_oper = @operator;
			_checked = @checked;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the argument, as supplied in the constructor.</value>
		public override Type ResultType {
			get {
				return _leftOp.ResultType;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			string str;
			
			switch( _oper ) {
				case ArithmeticOperator.Add:
					str = _leftOp.ToString() + " + " + _rightOp.ToString();
					break;
				case ArithmeticOperator.Subtract:
					str = _leftOp.ToString() + " - " + _rightOp.ToString();
					break;
				case ArithmeticOperator.Multiply:
					str = _leftOp.ToString() + " * " + _rightOp.ToString();
					break;
				case ArithmeticOperator.Divide:
					str = _leftOp.ToString() + " / " + _rightOp.ToString();
					break;
				default:
					throw new UnexpectedException();
			}
			
			if( !_checked )
				str = "unchecked(" + str + ")";
				
			return str;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
			
			_leftOp.Emit( cxt );
			_rightOp.Emit( cxt );
			
			switch( _oper ) {
				case ArithmeticOperator.Add:
					if( _checked )
						cxt.Generator.Emit( OpCodes.Add_Ovf );
					else
						cxt.Generator.Emit( OpCodes.Add );

					break;
				case ArithmeticOperator.Subtract:
					if( _checked )
						cxt.Generator.Emit( OpCodes.Sub_Ovf );
					else
						cxt.Generator.Emit( OpCodes.Sub );

					break;
				case ArithmeticOperator.Multiply:
					if( _checked )
						cxt.Generator.Emit( OpCodes.Mul_Ovf );
					else
						cxt.Generator.Emit( OpCodes.Mul );
					break;
				case ArithmeticOperator.Divide:
					cxt.Generator.Emit( OpCodes.Div );
					break;
				default:
					throw new UnexpectedException();
			}
		}
	}
}