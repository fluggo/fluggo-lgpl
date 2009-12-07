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
	/// Represents an expression for a for-loop.
	/// </summary>
	public class ForLoopExpression : Expression {
		Expression _init, _iter, _body;
		BooleanExpression _condition;

		/// <summary>
		/// Creates a new instance of the <see cref='ForLoopExpression'/> class.
		/// </summary>
		/// <param name="init">Optional expression for initializing loop variables. This expression is evaluated once before the loop begins.</param>
		/// <param name="condition">Optional expression for the loop condition. This expression is evaluated before each loop, and if it evaluates to
		///   false or zero, the loop body is skipped and the loop ends. Specify <see langword='null'/> to create a loop that runs indefinitely.</param>
		/// <param name="iter">Optional expression to evaluate after each loop.</param>
		/// <param name="body">Expression for the loop body. This expression is only evaluated if the condition is true.</param>
		/// <remarks>This expression represents a typical C, C++, or C# for loop, in the form:
		///   <code>for( init; condition; iter ) 
		///     body;</code></remarks>
		/// <exception cref='ArgumentNullException'><paramref name='body'/> is <see langword='null'/>.</exception>
		/// <exception cref='StackArgumentException'><paramref name='init'/>, <paramref name="iter"/>, or <paramref name="body"/> evaluate to a type
		///     other than <see cref="Void"/>.</exception>
		public ForLoopExpression( Expression init, BooleanExpression condition, Expression iter, Expression body ) {
			if( init != null && init.ResultType != typeof( void ) )
				throw new StackArgumentException( "init" );

			if( iter != null && iter.ResultType != typeof( void ) )
				throw new StackArgumentException( "iter" );

			if( body == null )
				throw new ArgumentNullException( "body" );

			if( body.ResultType != typeof( void ) )
				throw new StackArgumentException( "body" );

			_init = init;
			_iter = iter;
			_body = body;
			_condition = condition;
		}

		/// <include file='../Common.xml' path='/root/Expression/property[@name="MarksOwnSequence"]/*'/>
		public override bool MarksOwnSequence {
			get {
				return true;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
		
			Label beginLoop = cxt.DefineLabel(), endLoop = cxt.DefineLabel();
			IMarkable iterMarkable = null;

			using( cxt.BeginScope() ) {
				cxt.WriteCode( "for( " );

				// First, evaluate the initial expression
				if( _init != null ) {
					cxt.WriteMarkedCode( _init.ToString() );
					_init.Emit( cxt );
				}

				cxt.WriteCode( ";" );

				// The loop starts here
				cxt.Generator.MarkLabel( beginLoop );

				// Evaluate the condition, and if false, branch to after the loop (endLoop)
				if( _condition != null ) {
					cxt.WriteCode( " " );
					cxt.WriteMarkedCode( _condition.ToString() );
					_condition.EmitFalseBranch( cxt, endLoop );
				}

				cxt.WriteCode( ";" );

				// Write the string for the iterative statement, but don't emit it
				if( _iter != null ) {
					cxt.WriteCode( " " );
					iterMarkable = cxt.WriteMarkableCode( _iter.ToString() );
				}

				cxt.WriteCode( " )" );

				// Emit the body
				if( _body.MarksOwnSequence ) {
					cxt.WriteCodeLine( " {" );
					using( cxt.Indentation ) {
						_body.Emit( cxt );
					}
					cxt.WriteCodeLine( "}" );
				}
				else {
					cxt.WriteCodeLine();
					using( cxt.Indentation ) {
						cxt.WriteLineMarkedCode( _body.ToString() + ";" );
						_body.Emit( cxt );
					}
				}

				// Evaluate the iterative statement here at the end of the loop
				if( _iter != null ) {
					iterMarkable.Mark();
					_iter.Emit( cxt );
				}

				// Branch to the condition evaluation at the top
				cxt.Generator.Emit( OpCodes.Br, beginLoop );

				// Mark code that follows as the end of the loop
				cxt.Generator.MarkLabel( endLoop );
			}
		}
	}
}