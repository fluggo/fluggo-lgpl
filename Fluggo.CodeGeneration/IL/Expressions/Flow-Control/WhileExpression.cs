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
	/// Represents an expression for a while loop.
	/// </summary>
	public class WhileExpression : Expression {
		Expression _body;
		BooleanExpression _condition;

		/// <summary>
		/// Creates a new instance of the <see cref='WhileExpression'/> class.
		/// </summary>
		/// <param name="condition">Condition to test before the beginning of each iteration.</param>
		/// <param name="body">Body of the loop to execute when <paramref name="condition"/> is true. This must have a <see cref="Void"/>
		///	  <see cref="Expression.ResultType"/>.</param>
		/// <exception cref='ArgumentNullException'><paramref name='condition'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='body'/> is <see langword='null'/>.</para></exception>
		public WhileExpression( BooleanExpression condition, Expression body ) {
			if( condition == null )
				throw new ArgumentNullException( "condition" );

			if( body == null )
				throw new ArgumentNullException( "body" );

			if( body.ResultType != typeof( void ) )
				throw new StackArgumentException( "body" );

			_condition = condition;
			_body = body;
		}

		/// <include file='../Common.xml' path='/root/Expression/property[@name="MarksOwnSequence"]/*'/>
		public override bool MarksOwnSequence {
			get {
				return true;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			using( cxt.BeginScope() ) {
				cxt.WriteMarkedCode( "while( {0} )", _condition.ToString() );
				Label startLoop = cxt.DefineLabel(), endLoop = cxt.DefineLabel();

				if( _body.MarksOwnSequence )
					cxt.WriteCodeLine( " {" );
				else
					cxt.WriteCodeLine();

				cxt.Generator.MarkLabel( startLoop );
				_condition.EmitFalseBranch( cxt, endLoop );

				using( cxt.Indentation ) {
					_body.Emit( cxt );
				}

				cxt.Generator.Emit( OpCodes.Br, startLoop );

				if( _body.MarksOwnSequence )
					cxt.WriteCodeLine( "}" );

				cxt.Generator.MarkLabel( endLoop );
			}
		}
	}
}