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
using System.Diagnostics;
using System.Diagnostics.SymbolStore;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a context for generating a static or instance method.
	/// </summary>
	public class MethodGeneratorContext : CodeMemberGeneratorContext {
		MethodBuilder _method;
		Type[] _paramTypes;
		string[] _paramNames;
		CodeTextWriter _writer;
		ListExpression _body;

		/// <summary>
		/// Creates a new instance of the <see cref="MethodGeneratorContext"/> class.
		/// </summary>
		/// <param name="typeContext"><see cref="TypeGeneratorContext"/> representing the type and generator context in which this method appears.</param>
		/// <param name="method"><see cref="MethodBuilder"/> for the method that this context will represent.</param>
		/// <param name="params">Array of types for the parameters to the method.</param>
		/// <param name="writer">Optional <see cref="CodeTextWriter"/> to which to write generated code text for the method.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeContext"/> is <see langword='null'/>.
		///   <para>- OR -</para>
		///   <para><paramref name="module"/> is <see langword="null"/>.</para></exception>
		public MethodGeneratorContext( TypeGeneratorContext typeContext, MethodBuilder method, Type[] @params, CodeTextWriter writer )
			: base( typeContext, method.GetILGenerator(), method.ReturnType, writer ) {
			if( method == null )
				throw new ArgumentNullException( "method" );

			_method = method;
			_paramTypes = @params;
			_writer = writer;
			_paramNames = new string[_paramTypes.Length];
			_body = new ListExpression( true );
		}
		
		/// <summary>
		/// Defines additional data about a parameter.
		/// </summary>
		/// <param name="arg">Zero-based index of the parameter, where zero is the first declared argument.</param>
		/// <param name="name">Name of the parameter.</param>
		/// <returns>A <see cref="Param"/> object describing the parameter.</returns>
		/// <remarks>The parameter is declared as an <see cref="ParameterAttributes.In"/> parameter.</remarks>
		public Param DefineParameter( int arg, string name ) {
			return DefineParameter( arg, name, ParameterAttributes.In );
		}

		/// <summary>
		/// Defines additional data about a parameter.
		/// </summary>
		/// <param name="arg">Zero-based index of the parameter, where zero is the first declared argument.</param>
		/// <param name="name">Name of the parameter.</param>
		/// <param name="attributes">Additional attributes for the parameter.</param>
		/// <returns>A <see cref="Param"/> object describing the parameter.</returns>
		public Param DefineParameter( int arg, string name, ParameterAttributes attributes ) {
			_paramNames[arg] = name;
			
			if( IsStatic ) {
				_method.DefineParameter( arg, attributes, name );
				return new Param( arg, _paramTypes[arg], name );
			}
			else {
				_method.DefineParameter( arg + 1, attributes, name );
				return new Param( arg + 1, _paramTypes[arg], name );
			}
		}
		
		/// <summary>
		/// Defines a local in the current scope.
		/// </summary>
		/// <param name="type">Type of the local.</param>
		/// <param name="name">Name of the local.</param>
		/// <returns>A <see cref="ILGeneratorContext.Local"/> representing the new local variable.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='type'/> is <see langword='null'/>.</exception>
		public ILGeneratorContext.Local DefineLocal( Type type, string name ) {
			return ILGenerator.DeclareLocal( type, name );
		}

		/// <summary>
		/// Creates an expression that gets the value of a method argument.
		/// </summary>
		/// <param name="arg">Zero-based index of the argument to retrieve. To ease reading, argument zero of
		///   an instance method is considered the first declared argument and not the this reference. Use
		///   the <see cref="CodeMemberGeneratorContext.This"/> property to retrieve a local reference.</param>
		/// <returns>An <see cref="ArgumentExpression"/> for the argument.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arg"/> is less than zero or greater than or equal to
		///   the number of declared parameters for the method.</exception>
		[Obsolete("Use the value returned from DefineParameter instead.", true)]
		public ArgumentExpression Arg( int arg ) {
			if( arg < 0 || arg >= _paramTypes.Length )
				throw new ArgumentOutOfRangeException( "arg" );

			if( IsStatic )
				return new ArgumentExpression( _paramTypes[arg], arg, _paramNames[arg] );
			else
				return new ArgumentExpression( _paramTypes[arg], arg + 1, _paramNames[arg] );
		}
		
		public Param Param( int arg ) {
			if( IsStatic )
				return new Param( arg, _paramTypes[arg], _paramNames[arg] );
			else
				return new Param( arg + 1, _paramTypes[arg], _paramNames[arg] );
		}
		
		/// <summary>
		/// Adds an expression to the body of the method.
		/// </summary>
		/// <param name="exp">Expression to add.</param>
		/// <exception cref='ArgumentNullException'><paramref name='exp'/> is <see langword='null'/>.</exception>
		public void AddExpression( Expression exp ) {
			_body.Add( exp );
		}
		
		/// <summary>
		/// Adds a list of expressions to the body of the method.
		/// </summary>
		/// <param name="exps">List of expressions to add.</param>
		/// <exception cref='ArgumentNullException'><paramref name='exps'/> is <see langword='null'/>.</exception>
		public void AddExpressionRange( params Expression[] exps ) {
			_body.AddRange( exps );
		}

		/// <summary>
		/// Adds a list of expressions to the body of the method.
		/// </summary>
		/// <param name="list">List of expressions to add.</param>
		/// <exception cref='ArgumentNullException'><paramref name='list'/> is <see langword='null'/>.</exception>
		public void AddExpressionRange( ListExpression list ) {
			_body.AddRange( list );
		}

		/// <summary>
		/// Gets a reference to the method represented by this context.
		/// </summary>
		/// <value>Reference to a <see cref="MethodBuilder"/> for the method represented by this context.</value>
		public MethodBuilder Method {
			get {
				return _method;
			}
		}

		private void WriteMethodStart() {
			if( _writer == null )
				return;
				
			if( _method.IsPublic )
				_writer.Write( "public " );
			if( _method.IsFamily )
				_writer.Write( "protected " );
			if( _method.IsFamilyAndAssembly )
				_writer.Write( "protected internal /* FamAndAssem */" );
			if( _method.IsFamilyOrAssembly )
				_writer.Write( "protected internal " );
			if( _method.IsAssembly )
				_writer.Write( "internal " );
			if( _method.IsPrivate )
				_writer.Write( "private " );
			if( _method.IsStatic )
				_writer.Write( "static " );
			if( _method.IsVirtual && !_method.DeclaringType.IsInterface )
				_writer.Write( "virtual " );
/*			if( _method.IsFinal )
				_writer.Write( "final " );*/
			if( _method.IsAbstract && !_method.DeclaringType.IsInterface )
				_writer.Write( "abstract " );
			
			_writer.Write( Expression.GetSimpleTypeName( _method.ReturnType ) + " " + _method.Name + "(" );
			
			string[] paramList = new string[_paramTypes.Length];
			
			for( int i = 0; i < paramList.Length; i++ ) {
				if( _paramNames[i] == null )
					paramList[i] = Expression.GetSimpleTypeName( _paramTypes[i] ) + " arg" + i;
				else
					paramList[i] = Expression.GetSimpleTypeName( _paramTypes[i] ) + " " + _paramNames[i];
			}
			
			if( paramList.Length != 0 )
				_writer.Write( " " + string.Join( ", ", paramList ) + " " );
				
			_writer.Write( ")" );
			
			if( !_method.IsAbstract ) {
				_writer.WriteLine( " {" );
				_writer.Indent++;
			}
		}
		
		private void WriteMethodEnd( bool writeReturn ) {
			if( !_method.IsAbstract ) {
				if( _writer != null )
					_writer.Indent--;

				ILGenerator.WriteLineMarkedCode( "}" );

				if( writeReturn ) {
					ILGenerator.Generator.Emit( OpCodes.Ret );
				}
				
				ILGenerator.EmitReturnEpilogue();
			}
			else {
				if( _writer != null )
					_writer.WriteLine( ";" );
			}
			
			_writer.WriteLine();
		}
		
		/// <summary>
		/// Emits the method body to the IL stream and writes the method's code to the text writer, if any.
		/// </summary>
		/// <exception cref="InvalidOperationException">The member has already been emitted.</exception>
		public override void EmitBody() {
			base.EmitBody();
		
			bool writeReturn = false;
			WriteMethodStart();
			
			if( !_method.IsAbstract ) {
				using( ILGenerator.BeginScope() ) {
					_body.Emit( ILGenerator );
				}
				
				if( !(_body.LastExpression is ReturnExpression) ) {
					if( _method.ReturnType == typeof(void) ) {
						// The user just probably forgot the final return statement;
						// we'll emit it silently without generating code text
						writeReturn = true;
					}
					else {
						// The user forgot to enter a required return statement
						throw new Exception( "Return statement required for non-void method." );
					}
				}
			}
			
			WriteMethodEnd( writeReturn );
		}
		
		/// <summary>
		/// Gets a value that represents whether this method is static.
		/// </summary>
		/// <value>True if the method represented by this context is a static method, false otherwise.</value>
		public override bool IsStatic {
			get { return _method.IsStatic; }
		}
	}
}
