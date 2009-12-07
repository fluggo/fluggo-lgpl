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
using System.IO;
using System.Collections.Generic;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a context for generating IL in a method, property, or constructor.
	/// </summary>
	public class ILGeneratorContext {
		CodeMemberGeneratorContext _codeCxt;
		CodeTextWriter _writer;
		ILGenerator _gen;
		Scope _currentScope = null;
		TryBlock _currentTryBlock = null;
		LocalBuilder _returnLocal = null;
		Label _returnEpilogueLabel;
		bool _returnEpilogueNeeded = false;

		/// <summary>
		/// Creates a new instance of the <see cref='ILGeneratorContext'/> class.
		/// </summary>
		/// <param name="codeCxt"><see cref="CodeMemberGeneratorContext"/> for the member in which the code is being generated.</param>
		/// <param name="gen"><see cref="ILGenerator"/> for generating IL in the code member.</param>
		/// <param name="writer">Optional <see cref="CodeTextWriter"/> for writing debuggable code text.</param>
		/// <exception cref='ArgumentNullException'><paramref name='codeCxt'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='gen'/> is <see langword='null'/>.</para></exception>
		public ILGeneratorContext( CodeMemberGeneratorContext codeCxt, ILGenerator gen, CodeTextWriter writer ) {
			if( codeCxt == null )
				throw new ArgumentNullException( "codeCxt" );

			if( gen == null )
				throw new ArgumentNullException( "gen" );

			_codeCxt = codeCxt;
			_gen = gen;
			_writer = writer;
		}

		public ILGenerator Generator {
			get {
				return _gen;
			}
		}

		public CodeMemberGeneratorContext CodeMember {
			get {
				return _codeCxt;
			}
		}

		public ModuleGeneratorContext Module {
			get {
				if( _codeCxt == null )
					return null;

				return _codeCxt.Module;
			}
		}

		/// <summary>
		/// Defines a new label.
		/// </summary>
		/// <returns>A <see cref="Label"/> value that can be used to later mark a target point in the IL stream.</returns>
		public Label DefineLabel() {
			return _gen.DefineLabel();
		}

		/// <summary>
		/// Declares a local variable in the current scope.
		/// </summary>
		/// <param name="type">Type of the local variable.</param>
		/// <param name="name">Name of the local variable.</param>
		/// <returns>A <see cref="ILGeneratorContext.Local"/> representing the local variable.</returns>
		public Local DeclareLocal( Type type, string name ) {
			return DeclareLocal( type, name, false );
		}

		/// <summary>
		/// Declares a local variable in the current scope.
		/// </summary>
		/// <param name="type">Type of the local variable.</param>
		/// <param name="name">Name of the local variable.</param>
		/// <param name="pinned">True if the local should pin the object to which it is set.</param>
		/// <returns>A <see cref="ILGeneratorContext.Local"/> representing the local variable.</returns>
		public Local DeclareLocal( Type type, string name, bool pinned ) {
			if( _currentScope != null )
				return _currentScope.DeclareLocal( type, name );

			LocalBuilder builder = _gen.DeclareLocal( type, pinned );
			builder.SetLocalSymInfo( name );
			return new Local( builder, name );
		}

		/// <summary>
		/// Emits text to the code text writer.
		/// </summary>
		/// <param name="value">String to write.</param>
		public void WriteCode( string value ) {
			if( _writer == null )
				return;

			_writer.Write( value );
		}

		/// <summary>
		/// Emits formatted text to the code text writer.
		/// </summary>
		/// <param name="format">Format string for the code to write.</param>
		/// <param name="args">Arguments to insert in the format string.</param>
		public void WriteCode( string format, params object[] args ) {
			if( _writer == null )
				return;

			_writer.Write( format, args );
		}

		/// <summary>
		/// Emits formatted text to the code text writer and marks a sequence point for that code.
		/// </summary>
		/// <param name="format">Format string for the code to write.</param>
		/// <param name="args">Arguments to insert in the format string.</param>
		/// <remarks>IL emitted between this call and the next sequence point will be attributed to this code in
		///	  the debugger.</remarks>
		public void WriteMarkedCode( string format, params object[] args ) {
			IMarkable markable = WriteMarkableCode( format, args );
			markable.Mark();
		}

		/// <summary>
		/// Emits formatted text to the code text writer and returns a markable sequence point for that code.
		/// </summary>
		/// <param name="format">Format string for the code to write.</param>
		/// <param name="args">Arguments to insert in the format string.</param>
		/// <returns>An <see cref="IMarkable"/> that can be used to mark the IL associated with this text.</returns>
		/// <remarks>Use this override if code will be executed out of sequence with its appearance in the code text.</remarks>
		public IMarkable WriteMarkableCode( string format, params object[] args ) {
			if( _writer == null )
				return new NullMarkable();

			// Advance to the starting position
			_writer.WriteIndent();

			// Mark beginning and end
			FilePosition startPosition = _writer.Position;
			_writer.Write( format, args );
			FilePosition endPosition = _writer.Position;

			ISymbolDocumentWriter doc = Module.GetSymbolWriterForSource( _writer.Filename );
			return new CodeMarkable( _gen, doc, startPosition, endPosition );
		}

		private class NullMarkable : IMarkable {
			public NullMarkable() { }
			public void Mark() { }
		}

		private class CodeMarkable : IMarkable {
			FilePosition _start, _end;
			ISymbolDocumentWriter _doc;
			ILGenerator _gen;

			public CodeMarkable( ILGenerator gen, ISymbolDocumentWriter doc, FilePosition start, FilePosition end ) {
				_gen = gen;
				_doc = doc;
				_start = start;
				_end = end;
			}

			public void Mark() {
				_gen.MarkSequencePoint( _doc, _start.Line, _start.Column, _end.Line, _end.Column );
			}
		}

		/// <summary>
		/// Emits a new line to the code text writer, if any.
		/// </summary>
		public void WriteCodeLine() {
			if( _writer == null )
				return;

			_writer.WriteLine( string.Empty );
		}

		/// <summary>
		/// Emits a line to the code text writer, if any.
		/// </summary>
		/// <param name="value">Code text to write.</param>
		public void WriteCodeLine( string value ) {
			if( _writer == null )
				return;

			_writer.WriteLine( value );
		}

		/// <summary>
		/// Emits a formatted line to the code text writer, if any.
		/// </summary>
		/// <param name="format">Format string for the code to write.</param>
		/// <param name="args">Arguments to insert in the format string.</param>
		public void WriteCodeLine( string format, params object[] args ) {
			if( _writer == null )
				return;

			_writer.WriteLine( format, args );
		}

		/// <summary>
		/// Emits a formatted line to the code text writer and marks a sequence point for that line.
		/// </summary>
		/// <param name="format">Format string for the code to write.</param>
		/// <param name="args">Arguments to insert in the format string.</param>
		/// <remarks>IL emitted between this call and the next sequence point will be attributed to this line in
		///	  the debugger.</remarks>
		public void WriteLineMarkedCode( string format, params object[] args ) {
			if( _writer == null )
				return;

			// Advance to the starting position
			_writer.WriteIndent();

			// Mark beginning and end
			FilePosition startPosition = _writer.Position;
			_writer.Write( format, args );
			FilePosition endPosition = _writer.Position;

			_writer.WriteLine();

			ISymbolDocumentWriter doc = Module.GetSymbolWriterForSource( _writer.Filename );
			_gen.MarkSequencePoint( doc, startPosition.Line, startPosition.Column, endPosition.Line, endPosition.Column );
		}

		/// <summary>
		/// Emits a line to the code text writer and marks a sequence point for that line.
		/// </summary>
		/// <param name="value">String for the code to write.</param>
		/// <remarks>IL emitted between this call and the next sequence point will be attributed to this line in
		///	  the debugger.</remarks>
		public void WriteLineMarkedCode( string value ) {
			if( _writer == null )
				return;

			// Advance to the starting position
			_writer.WriteIndent();

			// Mark beginning and end
			FilePosition startPosition = _writer.Position;
			_writer.Write( value );
			FilePosition endPosition = _writer.Position;

			_writer.WriteLine();

			ISymbolDocumentWriter doc = Module.GetSymbolWriterForSource( _writer.Filename );
			_gen.MarkSequencePoint( doc, startPosition.Line, startPosition.Column, endPosition.Line, endPosition.Column );
		}

		/// <summary>
		/// Gets or sets the number of tabs used for indenting code text.
		/// </summary>
		/// <value>The number of tabs used for indenting code text.</value>
		public int Indent {
			get {
				if( _writer != null )
					return _writer.Indent;
				else
					return 0;
			}
			set {
				if( _writer != null )
					_writer.Indent = value;
			}
		}

		/// <summary>
		/// Gets an object that indents emitted code text until it is disposed.
		/// </summary>
		/// <value>An object that indents emitted code text until it is disposed.</value>
		/// <remarks>This is a convenience property for use with the C# using keyword.</remarks>
		public IDisposable Indentation {
			get {
				return new DisposableIndent( this );
			}
		}

		class DisposableIndent : IDisposable {
			ILGeneratorContext _cxt;

			public DisposableIndent( ILGeneratorContext cxt ) {
				if( cxt == null )
					throw new ArgumentNullException( "cxt" );

				_cxt = cxt;
				_cxt.Indent++;
			}

			public void Dispose() {
				_cxt.Indent--;
			}
		}

		/// <summary>
		/// Represents a local variable.
		/// </summary>
		public class Local : IDisposable {
			LocalBuilder _local;
			string _name;

			/// <summary>
			/// Creates a new instance of the <see cref='Local'/> class.
			/// </summary>
			/// <param name="local"><see cref="LocalBuilder"/> that represents the local variable.</param>
			/// <param name="name">Name of the local variable.</param>
			/// <exception cref='ArgumentNullException'><paramref name='local'/> is <see langword='null'/>.</exception>
			public Local( LocalBuilder local, string name ) {
				if( local == null )
					throw new ArgumentNullException( "local" );

				_local = local;
				_name = name;
			}

			/// <summary>
			/// Returns an expression that gets the value of the local.
			/// </summary>
			/// <returns>A <see cref="LocalExpression"/> that evaluates to the value of the local variable.</returns>
			/// <exception cref="ObjectDisposedException">The local's scope has ended.</exception>
			public LocalExpression Get() {
				if( _local == null )
					throw new ObjectDisposedException( _name );

				return new LocalExpression( _local, _name );
			}

			/// <summary>
			/// Returns an expression that stores a value into the local.
			/// </summary>
			/// <param name="value">Expression for the new value of the local.</param>
			/// <returns>A <see cref="StoreLocalExpression"/> that stores the given expression into the local.</returns>
			/// <exception cref="ObjectDisposedException">The local's scope has ended.</exception>
			public StoreLocalExpression Set( Expression value ) {
				if( _local == null )
					throw new ObjectDisposedException( _name );

				return new StoreLocalExpression( _local, value, _name );
			}

			/// <summary>
			/// Disposes this object.
			/// </summary>
			/// <remarks>This method marks the variable as unusable or out of scope.</remarks>
			public void Dispose() {
				_local = null;
			}
		}

		class Scope : IDisposable {
			ILGeneratorContext _gen;
			Scope _parentScope;
			List<Local> _locals = new List<Local>();

			public Scope( ILGeneratorContext gen, Scope parentScope ) {
				if( gen == null )
					throw new ArgumentNullException( "gen" );

				_gen = gen;
				_gen.Generator.BeginScope();
				_parentScope = parentScope;
			}

			/// <summary>
			/// Gets the parent to this scope.
			/// </summary>
			/// <value>The parent to this scope, or <see langword='null'/> if the parent scope is the method.</value>
			public Scope ParentScope {
				get {
					return _parentScope;
				}
			}

			/// <summary>
			/// Declares a local in this scope.
			/// </summary>
			/// <param name="type">Type of the local.</param>
			/// <param name="name">Optional name of the local.</param>
			/// <returns>A <see cref="Local"/> that represents the local.</returns>
			/// <exception cref="ObjectDisposedException">This scope has ended.</exception>
			public Local DeclareLocal( Type type, string name ) {
				if( _locals == null )
					throw new ObjectDisposedException( null );

				LocalBuilder builder = _gen.Generator.DeclareLocal( type );
				builder.SetLocalSymInfo( name );
				Local local = new Local( builder, name );

				_locals.Add( local );
				return local;
			}

			/// <summary>
			/// Ends this scope.
			/// </summary>
			public void Dispose() {
				if( _locals == null )
					return;

				_gen.Generator.EndScope();
				_gen.PopScope();

				foreach( Local local in _locals )
					local.Dispose();

				_locals = null;
			}
		}
		
		class TryBlock : IDisposable {
			ILGeneratorContext _ilCxt;
			TryBlock _parentBlock;
			Label _endBlockLabel;
			
			public TryBlock( ILGeneratorContext ilCxt, TryBlock parentBlock ) {
				if( ilCxt == null )
					throw new ArgumentNullException( "ilCxt" );
			
				_ilCxt = ilCxt;
				_parentBlock = parentBlock;
				
				_endBlockLabel = _ilCxt.Generator.BeginExceptionBlock();
			}
			
			public void EmitLeave() {
				_ilCxt.Generator.Emit( OpCodes.Leave, _endBlockLabel );
			}
			
			public void EmitLeave( Label label ) {
				_ilCxt.Generator.Emit( OpCodes.Leave, label );
			}
			
			public void BeginCatchBlock( Type exceptionType ) {
				_ilCxt.Generator.BeginCatchBlock( exceptionType );
			}
			
			public TryBlock ParentBlock {
				get {
					return _parentBlock;
				}
			}
			
			public void Dispose() {
				if( _ilCxt == null )
					return;
					
				_ilCxt.Generator.EndExceptionBlock();
				_ilCxt.PopExceptionBlock();
				_ilCxt = null;
			}
		}

		/// <summary>
		/// Begins a new local scope.
		/// </summary>
		/// <returns>An object that represents the new current scope. Dispose the object to end the scope.</returns>
		/// <remarks>Calls to <see cref="DeclareLocal(Type,string)"/> made after this call will create locals in the new scope.
		///   When the scope object is disposed, these locals will be marked as inaccessible.</remarks>
		public IDisposable BeginScope() {
			return (_currentScope = new Scope( this, _currentScope ));
		}

		private void PopScope() {
			if( _currentScope == null )
				throw new InvalidOperationException( "The current scope cannot be ended." );

			_currentScope = _currentScope.ParentScope;
		}
		
		/// <summary>
		/// Begins a new exception block.
		/// </summary>
		/// <returns>An object that represents the new current exception block. Dispose the object to end the exception block.</returns>
		public IDisposable BeginExceptionBlock() {
			return (_currentTryBlock = new TryBlock( this, _currentTryBlock ));
		}
		
		private void PopExceptionBlock() {
			if( _currentTryBlock == null )
				throw new InvalidOperationException( "There is no current exception block to end." );
				
			_currentTryBlock = _currentTryBlock.ParentBlock;
		}
		
		/// <summary>
		/// Emits a return sequence for the member, including exception block unwinds.
		/// </summary>
		/// <param name="returnExpression">Expression to return from the method, or <see langword='null'/> if the member does not return a value.</param>
		/// <exception cref='ArgumentException'><paramref name='returnExpression'/> is <see langword='null'/> and the member does not return void.
		///   <para>— OR —</para>
		///   <para><paramref name='returnExpression'/> is not <see langword='null'/> and the member returns void.</para></exception>
		public void EmitReturn( Expression returnExpression ) {
			if( returnExpression != null && _codeCxt.ReturnType == typeof(void) )
				throw new ArgumentException( "Return value supplied for void member.", "returnExpression" );
			
			if( returnExpression == null && _codeCxt.ReturnType != typeof(void) )
				throw new ArgumentException( "Return value required for non-void member.", "returnExpression" );
			
			if( _currentTryBlock == null ) {
				if( returnExpression != null )
					returnExpression.Emit( this );

				Generator.Emit( OpCodes.Ret );
			}
			else {
				if( !_returnEpilogueNeeded ) {
					_returnEpilogueNeeded = true;
					_returnEpilogueLabel = DefineLabel();
					
					if( _codeCxt.ReturnType != typeof(void) ) {
						_returnLocal = _gen.DeclareLocal( _codeCxt.ReturnType );
						_returnLocal.SetLocalSymInfo( "(return value)" );
					}
				}
				
				if( returnExpression != null ) {
					EmitStoreLocal( _returnLocal, returnExpression );
					_gen.Emit( OpCodes.Leave, _returnEpilogueLabel );
				}
			}
		}
		
		/// <summary>
		/// Emits an expression followed by a stloc instruction.
		/// </summary>
		/// <param name="builder"><see cref="LocalBuilder"/> for the local.</param>
		/// <param name="value"><see cref="Expression"/> for the value to store.</param>
		/// <exception cref='ArgumentNullException'><paramref name='value'/> is <see langword='null'/>.</exception>
		/// <exception cref='StackArgumentException'><paramref name='value'/> is has the wrong <see cref='Expression.ResultType'/>.</exception>
		public void EmitStoreLocal( LocalBuilder builder, Expression value ) {
			if( builder == null )
				throw new ArgumentNullException( "builder" );

			if( value == null )
				throw new ArgumentNullException( "value" );

			if( !builder.LocalType.IsAssignableFrom( value.ResultType ) )
				throw new StackArgumentException( "value" );

			value.Emit( this );

			switch( builder.LocalIndex ) {
				case 0:
					_gen.Emit( OpCodes.Stloc_0 );
					break;

				case 1:
					_gen.Emit( OpCodes.Stloc_1 );
					break;

				case 2:
					_gen.Emit( OpCodes.Stloc_2 );
					break;

				case 3:
					_gen.Emit( OpCodes.Stloc_3 );
					break;

				default:
					if( builder.LocalIndex < 256 )
						_gen.Emit( OpCodes.Stloc_S, builder );
					else
						_gen.Emit( OpCodes.Stloc, builder );

					break;
			}
		}

		/// <summary>
		/// Emits a ldloc instruction.
		/// </summary>
		/// <param name="builder"><see cref="LocalBuilder"/> for the local.</param>
		public void EmitLoadLocal( LocalBuilder builder ) {
			if( builder == null )
				throw new ArgumentNullException( "builder" );

			switch( builder.LocalIndex ) {
				case 0:
					_gen.Emit( OpCodes.Ldloc_0 );
					break;

				case 1:
					_gen.Emit( OpCodes.Ldloc_1 );
					break;

				case 2:
					_gen.Emit( OpCodes.Ldloc_2 );
					break;

				case 3:
					_gen.Emit( OpCodes.Ldloc_3 );
					break;

				default:
					if( builder.LocalIndex < 256 )
						_gen.Emit( OpCodes.Ldloc_S, builder );
					else
						_gen.Emit( OpCodes.Ldloc, builder );

					break;
			}
		}

		/// <summary>
		/// Emits a ldloc instruction.
		/// </summary>
		/// <param name="builder"><see cref="LocalBuilder"/> for the local.</param>
		public void EmitLoadLocalAddress( LocalBuilder builder ) {
			if( builder == null )
				throw new ArgumentNullException( "builder" );

			if( builder.LocalIndex < 256 )
				_gen.Emit( OpCodes.Ldloca_S, builder );
			else
				_gen.Emit( OpCodes.Ldloca, builder );
		}

		/// <summary>
		/// Emits a return epilogue, if needed.
		/// </summary>
		/// <remarks>This method emits code needed to support returns in try-catch-finally blocks.
		///   Be sure to emit the final return for your member before calling this method.</remarks>
		public void EmitReturnEpilogue() {
			if( _returnEpilogueNeeded ) {
				_gen.MarkLabel( _returnEpilogueLabel );
			
				if( _codeCxt.ReturnType != typeof(void) )
					EmitLoadLocal( _returnLocal );
			
				_gen.Emit( OpCodes.Ret );
			}
		}
	}
}