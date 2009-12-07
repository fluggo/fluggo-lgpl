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
	/// Represents a context for generating a static or instance constructor.
	/// </summary>
	public class ConstructorGeneratorContext : CodeMemberGeneratorContext {
		ConstructorBuilder _ctor;
		Type[] _paramTypes;
		string[] _paramNames;
		CodeTextWriter _writer;
		ListExpression _body = new ListExpression( true );

		/// <summary>
		/// Creates a new instance of the <see cref="ConstructorGeneratorContext"/> class.
		/// </summary>
		/// <param name="typeContext"><see cref="TypeGeneratorContext"/> representing the type and generator context in which this constructor appears.</param>
		/// <param name="ctor"><see cref="ConstructorBuilder"/> for the constructor that this context will represent.</param>
		/// <param name="params">Array of types for the parameters to the constructor.</param>
		/// <param name="writer">Optional <see cref="CodeTextWriter"/> to which to write generated code text for the constructor.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeContext"/> is <see langword='null'/>.
		///   <para>- OR -</para>
		///   <para><paramref name="ctor"/> is <see langword="null"/>.</para></exception>
		public ConstructorGeneratorContext( TypeGeneratorContext typeContext, ConstructorBuilder ctor, Type[] @params, CodeTextWriter writer )
			: base( typeContext, ctor.GetILGenerator(), typeof(void), writer ) {
			if( ctor == null )
				throw new ArgumentNullException( "ctor" );

			_ctor = ctor;
			_paramTypes = @params;
			_paramNames = new string[@params.Length];
			_writer = writer;
		}

		/// <summary>
		/// Gets a reference to the constructor represented by this context.
		/// </summary>
		/// <value>Reference to a <see cref="ConstructorBuilder"/> for the constructor represented by this context.</value>
		public ConstructorBuilder Constructor {
			get {
				return _ctor;
			}
		}

		/// <summary>
		/// Defines additional data about a parameter.
		/// </summary>
		/// <param name="arg">Zero-based index of the parameter, where zero is the first declared argument.</param>
		/// <param name="name">Name of the parameter.</param>
		/// <remarks>The parameter is declared as an <see cref="ParameterAttributes.In"/> parameter.</remarks>
		/// <returns>A <see cref="Param"/> object describing the parameter.</returns>
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

			_ctor.DefineParameter( arg + 1, attributes, name );
			return new Param( arg + 1, _paramTypes[arg], name );
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
		/// <exception cref="InvalidOperationException">The constructor is static.</exception>
		[Obsolete( "Use the value returned from DefineParameter instead.", true )]
		public ArgumentExpression Arg( int arg ) {
			if( IsStatic )
				throw new InvalidOperationException( "Static constructors have no arguments." );

			if( arg < 0 || arg >= _paramTypes.Length )
				throw new ArgumentOutOfRangeException( "arg" );

			return new ArgumentExpression( _paramTypes[arg], arg + 1, _paramNames[arg] );
		}

		public Param Param( int arg ) {
			if( IsStatic )
				return new Param( arg, _paramTypes[arg], _paramNames[arg] );
			else
				return new Param( arg + 1, _paramTypes[arg], _paramNames[arg] );
		}

		/// <summary>
		/// Gets a value that represents whether this constructor is static.
		/// </summary>
		/// <value>True if the constructor represented by this context is the static constructor for its type, false otherwise.</value>
		public override bool IsStatic {
			get { return _ctor.IsStatic; }
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

		private void WriteConstructorStart() {
			if( _writer == null )
				return;

			if( _ctor.IsPublic )
				_writer.Write( "public " );
			if( _ctor.IsFamily )
				_writer.Write( "protected " );
			if( _ctor.IsFamilyAndAssembly )
				_writer.Write( "protected internal /* FamAndAssem */" );
			if( _ctor.IsFamilyOrAssembly )
				_writer.Write( "protected internal " );
			if( _ctor.IsAssembly )
				_writer.Write( "internal " );
			if( _ctor.IsPrivate )
				_writer.Write( "private " );
			if( _ctor.IsStatic )
				_writer.Write( "static " );

			_writer.Write( _ctor.DeclaringType.Name + "(" );

			string[] paramList = new string[_paramTypes.Length];

			for( int i = 0; i < paramList.Length; i++ ) {
				if( _paramNames[i] == null )
					paramList[i] = Expression.GetSimpleTypeName( _paramTypes[i] ) + " arg" + i;
				else
					paramList[i] = Expression.GetSimpleTypeName( _paramTypes[i] ) + " " + _paramNames[i];
			}

			if( paramList.Length != 0 )
				_writer.Write( " " + string.Join( ", ", paramList ) + " " );

			_writer.WriteLine( ") {" );
			_writer.Indent++;
		}

		private void WriteConstructorEnd( bool writeReturn ) {
			if( _writer == null )
				return;

			_writer.Indent--;

			if( writeReturn ) {
				ILGenerator.WriteLineMarkedCode( "}" );
				ILGenerator.Generator.Emit( OpCodes.Ret );
			}
			else {
				_writer.WriteLine( "}" );
			}
			
			ILGenerator.EmitReturnEpilogue();

			_writer.WriteLine();
		}

		/// <summary>
		/// Emits the constructor body to the IL stream and writes the constructor's code to the text writer, if any.
		/// </summary>
		/// <exception cref="InvalidOperationException">The member has already been emitted.</exception>
		public override void EmitBody() {
			base.EmitBody();
			
			WriteConstructorStart();

			_body.Emit( ILGenerator );

			bool writeReturn = false;

			if( !(_body.LastExpression is ReturnExpression) ) {
				// The user just probably forgot the final return statement;
				// we'll emit it silently without generating code text
				writeReturn = true;
			}

			WriteConstructorEnd( writeReturn );
		}
	}
}
