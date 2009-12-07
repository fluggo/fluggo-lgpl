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
	/// Represents an expression that branches to multiple targets.
	/// </summary>
	public sealed class SwitchExpression : Expression {
		Expression _value, _defaultCase;
		Expression[] _cases;

		/// <summary>
		/// Creates a new instance of the <see cref='SwitchExpression'/> class.
		/// </summary>
		/// <param name="valueExpr">Integer-based value on which to branch.</param>
		/// <param name="cases">Array of expressions describing the various switch cases. An expression is chosen
		///	  based on its index in this array. For example, if the <paramref name="valueExpr"/> evaluates to two,
		///   the third case is chosen in <paramref name="cases"/> because it is zero-indexed. If the corresponding entry is <see langword='null'/>,
		///   the switch goes to the default case.</param>
		/// <param name="defaultCase">Optional expression to evaluate if none of the cases were chosen.</param>
		/// <exception cref='ArgumentNullException'><paramref name='valueExpr'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='cases'/> is <see langword='null'/>.</para></exception>
		/// <exception cref="StackArgumentException"><paramref name="valueExpr"/> has a <see cref='Void'/> result type.</exception>
		public SwitchExpression( Expression valueExpr, Expression[] cases, Expression defaultCase ) {
			if( valueExpr == null )
				throw new ArgumentNullException( "valueExpr" );

			if( valueExpr.ResultType == typeof( void ) )
				throw new StackArgumentException( "valueExpr" );

			if( cases == null )
				throw new ArgumentNullException( "cases" );

			_value = valueExpr;
			_cases = cases;
			_defaultCase = defaultCase;
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

			Label endLabel = cxt.DefineLabel(), defaultLabel = cxt.DefineLabel();
			Label[] targets = new Label[_cases.Length];

			for( int i = 0; i < targets.Length; i++ ) {
				targets[i] = (_cases[i] == null) ? defaultLabel : cxt.DefineLabel();
			}

			cxt.WriteMarkedCode( "switch( " + _value + " )" );
			cxt.WriteCodeLine( " {" );
			_value.Emit( cxt );
			
			bool breakFuss, blockFuss;

			using( cxt.Indentation ) {
				cxt.Generator.Emit( OpCodes.Switch, targets );

				if( _defaultCase != null ) {
					GetCaseOptions( _defaultCase, out blockFuss, out breakFuss );
					
					cxt.WriteCode( "default:" );
					
					if( blockFuss )
						cxt.WriteCode( " {" );
					
					cxt.WriteCodeLine();

					using( cxt.BeginScope() ) { using( cxt.Indentation ) {
						cxt.Generator.MarkLabel( defaultLabel );

						if( !_defaultCase.MarksOwnSequence )
							cxt.WriteLineMarkedCode( _defaultCase.ToString() + ";" );

						_defaultCase.Emit( cxt );
					}}

					if( blockFuss ) {
						cxt.WriteCodeLine( "}" );
					}

					if( breakFuss ) {
						using( cxt.Indentation ) {
							cxt.WriteLineMarkedCode( "break;" );
						}
					}

					cxt.WriteCodeLine();
				}

				cxt.Generator.Emit( OpCodes.Br, endLabel );

				for( int i = 0; i < _cases.Length; i++ ) {
					if( _cases[i] != null ) {
						GetCaseOptions( _cases[i], out blockFuss, out breakFuss );
					
						cxt.WriteCode( "case (" + GetSimpleTypeName( _value.ResultType ) + ") " + i + ":" );
						
						if( blockFuss )
							cxt.WriteCode( " {" );
						
						cxt.WriteCodeLine();
						
						cxt.Generator.MarkLabel( targets[i] );

						using( cxt.BeginScope() ) { using( cxt.Indentation ) {
							if( !_cases[i].MarksOwnSequence )
								cxt.WriteLineMarkedCode( _cases[i].ToString() + ";" );

							_cases[i].Emit( cxt );
						} }

						if( blockFuss ) {
							cxt.WriteCodeLine( "}" );
						}

						if( breakFuss ) {
							using( cxt.Indentation ) {
								cxt.WriteLineMarkedCode( "break;" );
								cxt.Generator.Emit( OpCodes.Br, endLabel );
							}
						}

						cxt.WriteCodeLine();
					}
				}
			}

			cxt.WriteCodeLine( "}" );
			cxt.Generator.MarkLabel( endLabel );
			
			if( _defaultCase == null )
				cxt.Generator.MarkLabel( defaultLabel );
		}
		
		private static void GetCaseOptions( Expression exp, out bool blockFuss, out bool breakFuss ) {
			Expression lastExp = exp;
			
			if( exp is ListExpression ) {
				lastExp = ((ListExpression) exp).LastExpression;
			}
			
			blockFuss = !(lastExp is ThrowExpression);
			breakFuss = blockFuss && !(lastExp is ReturnExpression);
		}
	}
}