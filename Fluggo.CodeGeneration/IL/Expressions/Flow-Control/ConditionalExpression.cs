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
	/// Represents a conditional or branching expression.
	/// </summary>
	public sealed class ConditionalExpression : Expression {
		Expression _trueExp, _falseExp;
		BooleanExpression _condition;

		/// <summary>
		/// Creates a new instance of the <see cref="ConditionalExpression"/> class.
		/// </summary>
		/// <param name="condition"><see cref="BooleanExpression"/> for the condition.</param>
		/// <param name="trueExpression"><see cref="Expression"/> to evaluate if the condition evaluates to true (nonzero).</param>
		/// <param name="falseExpression"><see cref="Expression"/> to evaluate if the condition evaluates to false (zero).</param>
		/// <remarks>Either <paramref name="trueExpression"/> or <paramref name="falseExpression"/> can be <see langword="null"/>,
		///   but not both. If both are specified, their <see cref="Expression.ResultType"/> properties must match. If only one is
		///   specified, its <see cref="Expression.ResultType"/> must be <see cref="Void"/>. This is to ensure that the state of the stack is the
		///   same after this expression regardless of how <paramref name="condition"/> evaluates.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="condition"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException">Both <paramref name="trueExpression"/> and <paramref name="falseExpression"/> are null.</exception>
		/// <exception cref="StackArgumentException"><paramref name="condition"/> has a <see cref="Expression.ResultType"/> of <see cref="Void"/>.</exception>
		public ConditionalExpression( BooleanExpression condition, Expression trueExpression, Expression falseExpression ) {
			if( condition == null )
				throw new ArgumentNullException( "condition" );

			if( trueExpression != null ) {
				if( falseExpression != null ) {
					if( trueExpression.ResultType != falseExpression.ResultType )
						throw new ArgumentException( "The result types for the true and false expressions don't match." );
				}
				else {
//					if( trueExpression.ResultType != typeof( void ) )
//						throw new StackArgumentException( "trueExpression" );
				}
			}
			else if( falseExpression != null ) {
//				if( falseExpression.ResultType != typeof( void ) )
//					throw new StackArgumentException( "falseExpression" );
			}
			else {
				throw new ArgumentException( "No branches were specified." );
			}

			_condition = condition;
			_trueExp = trueExpression;
			_falseExp = falseExpression;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _trueExp == null || _falseExp == null )
				return "/* bad conditional expression */";

			return "(" + _condition + ") ? (" + _trueExp + ") : (" + _falseExp + ")";
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the result left at the top of the stack when the expression is evaluated.
		///   If both branches are supplied, this is the <see cref="ResultType"/> of the expressions, which must match.
		///   If only one branch was supplied, this is the <see cref="Void"/> type.</value>
		public override Type ResultType {
			get {
				return (_trueExp == null || _falseExp == null) ? typeof( void ) : _trueExp.ResultType;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/property[@name="MarksOwnSequence"]/*'/>
		public override bool MarksOwnSequence {
			get {
				return ResultType == typeof( void );
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			bool writeBlock = MarksOwnSequence;

			if( writeBlock ) {
				if( _trueExp != null )
					cxt.WriteMarkedCode( "if( " + _condition.ToString() + " )" );
				else
					cxt.WriteMarkedCode( "if( !(" + _condition.ToString() + ") )" );
			}

			Label endLabel = cxt.Generator.DefineLabel();

			if( _trueExp != null ) {
				if( _falseExp != null ) {
					// Skip the true branch if the condition is false
					Label falseLabel = cxt.Generator.DefineLabel();
					_condition.EmitFalseBranch( cxt, falseLabel );

					using( cxt.BeginScope() ) {
						if( writeBlock )
							WriteBlock( cxt, _trueExp, false );
						else
							_trueExp.Emit( cxt );
							
						if( _trueExp.ResultType != typeof(void) && _trueExp.ResultType != _falseExp.ResultType )
							cxt.Generator.Emit( OpCodes.Pop );

						cxt.Generator.Emit( OpCodes.Br, endLabel );
					}

					if( writeBlock )
						cxt.WriteCode( "else" );

					cxt.Generator.MarkLabel( falseLabel );

					using( cxt.BeginScope() ) {
						if( writeBlock )
							WriteBlock( cxt, _falseExp, true );
						else
							_falseExp.Emit( cxt );

						if( _falseExp.ResultType != typeof( void ) && _trueExp.ResultType != _falseExp.ResultType )
							cxt.Generator.Emit( OpCodes.Pop );
					}

					cxt.Generator.MarkLabel( endLabel );
				}
				else {
					// Only execute anything if the condition is true
					_condition.EmitFalseBranch( cxt, endLabel );

					using( cxt.BeginScope() ) {
						if( writeBlock )
							WriteBlock( cxt, _trueExp, false );
						else
							_trueExp.Emit( cxt );
							
						if( _trueExp.ResultType != typeof(void) )
							cxt.Generator.Emit( OpCodes.Pop );
					}

					cxt.Generator.MarkLabel( endLabel );
				}
			}
			else if( _falseExp != null ) {
				// Only execute anything if the condition is not true
				_condition.EmitTrueBranch( cxt, endLabel );

				using( cxt.BeginScope() ) {
					if( writeBlock )
						WriteBlock( cxt, _falseExp, true );
					else
						_falseExp.Emit( cxt );

					if( _falseExp.ResultType != typeof( void ) )
						cxt.Generator.Emit( OpCodes.Pop );
				}

				cxt.Generator.MarkLabel( endLabel );
			}
		}

		private static void WriteBlock( ILGeneratorContext cxt, Expression exp, bool falseBlock ) {
			if( falseBlock && (exp is ConditionalExpression) ) {
				cxt.WriteCode( " " );
				exp.Emit( cxt );
			}
			else if( exp.MarksOwnSequence ) {
				cxt.WriteCodeLine( " {" );

				using( cxt.Indentation ) {
					exp.Emit( cxt );
				}

				cxt.WriteCodeLine( "}" );
			}
			else {
				cxt.WriteCodeLine();

				using( cxt.Indentation ) {
					cxt.WriteLineMarkedCode( exp.ToString() + ";" );
					exp.Emit( cxt );
				}
			}
		}
	}
}