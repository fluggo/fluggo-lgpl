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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a list of expressions that are evaluated sequentially.
	/// </summary>
	public sealed class ListExpression : Expression {
		List<Expression> _exps;
		bool _discardLast;

		/// <summary>
		/// Creates a new instance of the <see cref="ListExpression"/> class.
		/// </summary>
		/// <param name="exps">Expressions to evaluate.</param>
		/// <param name="discardLast">True to discard the result of the last expression, or false to leave it on the stack.</param>
		/// <remarks>The results of each expression are usually discarded, including the last expression. You can optionally keep the result of the last
		///   expression by setting <paramref name="discardLast"/> to false.</remarks>
		/// <exception cref='ArgumentNullException'><paramref name='exps'/> is <see langword='null'/>.</exception>
		public ListExpression( Expression[] exps, bool discardLast )
			: this( discardLast ) {
			AddRange( exps );
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ListExpression"/> class.
		/// </summary>
		/// <param name="exps">Expressions to evaluate.</param>
		/// <remarks>The results of each expression are usually discarded, including the last expression. You can optionally keep the result of the last
		///   expression by calling <see cref="ListExpression(Expression[],bool)"/> with <paramref name="discardLast"/> set to false.</remarks>
		/// <exception cref='ArgumentNullException'><paramref name='exps'/> is <see langword='null'/>.</exception>
		public ListExpression( Expression[] exps )
			: this( true ) {
			AddRange( exps );
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ListExpression"/> class.
		/// </summary>
		/// <param name="discardLast">True to discard the result of the last expression, or false to leave it on the stack.</param>
		/// <remarks>The results of each expression are usually discarded, including the last expression. You can optionally keep the result of the last
		///   expression by calling <see cref="ListExpression(Expression[],bool)"/> with <paramref name="discardLast"/> set to false.</remarks>
		public ListExpression( bool discardLast ) {
			_exps = new List<Expression>();
			_discardLast = discardLast;
		}

		/// <summary>
		/// Gets the last expression in the list.
		/// </summary>
		/// <value>The last expression in the list, or <see langword='null'/> if the list is empty.</value>
		public Expression LastExpression {
			get {
				if( _exps.Count == 0 )
					return null;

				return _exps[_exps.Count - 1];
			}
		}

		/// <summary>
		/// Adds an expression to the list.
		/// </summary>
		/// <param name="exp">Expression to add.</param>
		/// <exception cref='ArgumentNullException'><paramref name='exp'/> is <see langword='null'/>.</exception>
		public void Add( Expression exp ) {
			if( exp == null )
				throw new ArgumentNullException( "exp" );

			_exps.Add( exp );
		}

		/// <summary>
		/// Adds an array of expressions to the list.
		/// </summary>
		/// <param name="exps">List of expressions to add.</param>
		/// <exception cref='ArgumentNullException'><paramref name='exps'/> is <see langword='null'/>.</exception>
		public void AddRange( params Expression[] exps ) {
			if( exps == null )
				throw new ArgumentNullException( "exps" );

			for( int i = 0; i < exps.Length; i++ ) {
				if( exps[i] == null )
					throw new ArgumentException( "One of the given expressions was null.", "exps" );
			}

			_exps.AddRange( exps );
		}

		/// <summary>
		/// Adds an array of expressions to the list.
		/// </summary>
		/// <param name="list">List of expressions to add.</param>
		/// <exception cref='ArgumentNullException'><paramref name='list'/> is <see langword='null'/>.</exception>
		public void AddRange( ListExpression list ) {
			if( list == null )
				throw new ArgumentNullException( "list" );

			_exps.AddRange( list._exps );
		}

		/// <summary>
		/// Removes the expression at the given index.
		/// </summary>
		/// <param name="index">Index of the expression to remove.</param>
		public void RemoveAt( int index ) {
			_exps.RemoveAt( index );
		}

		/// <summary>
		/// Gets the number of expressions currently represented by this list.
		/// </summary>
		/// <value>The number of expressions currently represented by this list.</value>
		public int Count {
			get {
				return _exps.Count;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			// What they get if they try to use it as an expression
			if( _discardLast )
				return "void(" + string.Join( ", ", ToStringArray( _exps.ToArray(), 0, _exps.Count ) ) + ")";
			else
				return "(" + string.Join( ", ", ToStringArray( _exps.ToArray(), 0, _exps.Count ) ) + ")";
		}

		/// <include file='../Common.xml' path='/root/Expression/property[@name="MarksOwnSequence"]/*'/>
		public override bool MarksOwnSequence {
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Expression.ResultType"/> of the last expression evaluated in the list, or <see cref="Void"/>
		///   if the last expression's result will be discarded.</value>
		public override Type ResultType {
			get {
				if( _discardLast )
					return typeof(void);
					
				if( _exps.Count == 0 )
					throw new InvalidOperationException( "The list expression's result type cannot be determined. You may want to set discardLast to true." );
				
				return _exps[_exps.Count - 1].ResultType;
			}
		}

		private void GatherExpressions( IList<Expression> exps, List<Expression> list ) {
			foreach( Expression exp in exps ) {
				if( exp is ListExpression )
					GatherExpressions( ((ListExpression) exp)._exps, list );
				else
					list.Add( exp );
			}
		}
		
		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			// Recurse and bring all expressions to the top
			List<Expression> finalList = new List<Expression>();
			GatherExpressions( _exps, finalList );
			
			for( int i = 0; i < finalList.Count; i++ ) {
				if( finalList[i].MarksOwnSequence ) {
					// Combos that need blank lines:
					//	  ListExpression and ConditionalExpression
					//	  (any not line-after-me) and (any line-after-me)
					
					// Make sure there's a blank line before this expression
					if( i != 0 && !finalList[i - 1].MarksOwnSequence && finalList[i].MarksOwnSequence )
						cxt.WriteCodeLine();
				}
				else {
					// Just a simple write
					if( finalList[i] is CommentExpression || finalList[i] is BlankLineExpression )
						cxt.WriteCodeLine( finalList[i].ToString() );
					else if( !(finalList[i] is EmptyExpression) )
						cxt.WriteLineMarkedCode( finalList[i].ToString() + ";" );
				}

				finalList[i].Emit( cxt );

				if( i == (finalList.Count - 1) ) {
					if( _discardLast && (finalList[i].ResultType != typeof( void )) )
						cxt.Generator.Emit( OpCodes.Pop );
				}
				else {
					// If this expression marks its own sequence, make sure there's a blank line after it
					if( finalList[i].MarksOwnSequence )
						cxt.WriteCodeLine();
					
					// Middle expressions don't get to return a value
					if( finalList[i].ResultType != typeof( void ) )
						cxt.Generator.Emit( OpCodes.Pop );
				}
			}
		}

	}
}