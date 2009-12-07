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
	/// Represents a try-catch-filter-finally block.
	/// </summary>
	public class TryBlockExpression : Expression {
		Expression _tryBlock, _finallyBlock;
		List<CatchBlock> _catchBlocks = new List<CatchBlock>();

		/// <summary>
		/// Creates a new instance of the <see cref='TryBlockExpression'/> class.
		/// </summary>
		/// <param name="tryBlock">Expression that is guarded by catch or finally clauses.</param>
		/// <param name="catchBlocks">Optional catch blocks for the exception block.</param>
		/// <param name="finallyBlock">Optional expression to execute regardless of exceptions.</param>
		/// <exception cref='ArgumentNullException'><paramref name='tryBlock'/> is <see langword='null'/>.</exception>
		/// <exception cref="StackArgumentException"><paramref name="tryBlock"/> or <paramref name="finallyBlock"/> has a <see cref="Expression.ResultType"/> that is not void.</exception>
		public TryBlockExpression( Expression tryBlock, CatchBlock[] catchBlocks, Expression finallyBlock ) {
			if( tryBlock == null )
				throw new ArgumentNullException( "tryBlock" );
				
			if( tryBlock.ResultType != typeof(void) )
				throw new StackArgumentException( "tryBlock" );
				
			if( finallyBlock != null && finallyBlock.ResultType != typeof(void) )
				throw new StackArgumentException( "finallyBlock" );

			if( catchBlocks != null )
				_catchBlocks.AddRange( catchBlocks );
				
			_tryBlock = tryBlock;
			_finallyBlock = finallyBlock;
		}

		/// <include file='../Common.xml' path='/root/Expression/property[@name="MarksOwnSequence"]/*'/>
		public override bool MarksOwnSequence {
			get {
				return true;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			// This one's a tricky one, to say the least
			_catchBlocks.Sort();
			
			using( cxt.BeginExceptionBlock() ) {
				// Write try block
				using( cxt.BeginScope() ) {
					cxt.WriteCodeLine( "try {" );
					using( cxt.Indentation ) {
						// Write code for try block
						if( !_tryBlock.MarksOwnSequence )
							cxt.WriteLineMarkedCode( _tryBlock.ToString() + ";" );
						
						_tryBlock.Emit( cxt );
					}
					cxt.WriteCodeLine( "}" );
				} // endscope
					
				#region Write catch blocks
				foreach( CatchBlock block in _catchBlocks ) {
					string catchStmt = "catch";
					
					if( block.ExceptionType != typeof(object) ) {
						catchStmt += "( " + block.ExceptionType.Name + " ";
						
						if( block.ExceptionLocal != null )
							catchStmt += block.ExceptionLocal.Name + " ";
							
						catchStmt += ")";
					}
						
					cxt.Generator.BeginCatchBlock( block.ExceptionType );

					cxt.WriteMarkedCode( catchStmt );
					cxt.WriteCodeLine( " {" );

					using( cxt.BeginScope() ) {
						using( cxt.Indentation ) {
							// Write code for catch block
							block.EmitHandlerBody( cxt );
						}
					}
					
					cxt.WriteCodeLine( "}" );
				}
				#endregion
				
				#region Write finally block
				if( _finallyBlock != null ) {
					cxt.Generator.BeginFinallyBlock();
					
					cxt.WriteCodeLine( "finally {" );
					
					using( cxt.BeginScope() ) {
						using( cxt.Indentation ) {
							if( !_finallyBlock.MarksOwnSequence )
								cxt.WriteLineMarkedCode( _finallyBlock.ToString() + ";" );
								
							_finallyBlock.Emit( cxt );
						}
					}
					
					cxt.WriteCodeLine( "}" );
				}
				#endregion
			} // endexceptionregion
		}
	}
	
	/// <summary>
	/// Represents an exception catch block.
	/// </summary>
	public class CatchBlock : IComparable {
		Type _exceptionType;
		ListExpression _handler = new ListExpression( true );
		Local _exceptionLocal;
		DeclareLocalExpression _declLocal;

		/// <summary>
		/// Creates a new instance of the <see cref='CatchBlock'/> class.
		/// </summary>
		/// <param name="exceptionType">Type of the exception to catch in this block.</param>
		/// <param name="exceptionLocal">On return, a local to contain a reference to the caught exception.</param>
		/// <exception cref='ArgumentNullException'><paramref name='exceptionType'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='handler'/> is <see langword='null'/>.</para></exception>
		/// <exception cref="ArgumentException"><paramref name="exceptionLocal"/> is not the same type as <paramref name="exceptionType"/>.</exception>
		/// <exception cref="StackArgumentException"><paramref name="handler"/> has a <see cref="Expression.ResultType"/> that is not void.</exception>
		public CatchBlock( Type exceptionType, out Local exceptionLocal ) {
			if( exceptionType == null )
				throw new ArgumentNullException( "exceptionType" );

			_exceptionType = exceptionType;
			_declLocal = new DeclareLocalExpression( _exceptionType, "ex", out exceptionLocal );
			_exceptionLocal = exceptionLocal;
		}

		/// <summary>
		/// Creates a new instance of the <see cref='CatchBlock'/> class.
		/// </summary>
		/// <param name="exceptionType">Type of the exception to catch in this block. If this is <see langword='null'/>,
		///	  the catch block handles all exceptions.</param>
		/// <exception cref='ArgumentNullException'><paramref name='handler'/> is <see langword='null'/>.</exception>
		/// <exception cref="StackArgumentException"><paramref name="handler"/> has a <see cref="Expression.ResultType"/> that is not void.</exception>
		public CatchBlock( Type exceptionType ) {
			if( exceptionType == null )
				exceptionType = typeof(object);
			
			_exceptionType = exceptionType;
		}

		/// <summary>
		/// Gets the type of exception caught by this exception block.
		/// </summary>
		/// <value>The type of exception caught by this exception block, or <see langword="null"/> if the block will catch all exceptions.</value>
		public Type ExceptionType {
			get {
				return _exceptionType;
			}
		}
		
		/// <summary>
		/// Adds an expression to the catch handler inline.
		/// </summary>
		/// <param name="exp">Expression to add.</param>
		/// <returns>A reference to the current catch block so that the result of the call can be passed
		///   directly to the <see cref="TryBlockExpression"/> constructor.</returns>
		public CatchBlock Add( Expression exp ) {
			_handler.Add( exp );
			return this;
		}

		/// <summary>
		/// Adds a list of expressions to the catch handler inline.
		/// </summary>
		/// <param name="expList">List of expressions to add.</param>
		/// <returns>A reference to the current catch block so that the result of the call can be passed
		///   directly to the <see cref="TryBlockExpression"/> constructor.</returns>
		public CatchBlock AddRange( params Expression[] expList ) {
			_handler.AddRange( expList );
			return this;
		}

		/// <summary>
		/// Gets the local variable used to store the caught exception.
		/// </summary>
		/// <value>The local variable used to store the caught exception, or <see langword="null"/> if a local will not be used.</value>
		public Local ExceptionLocal {
			get {
				return _exceptionLocal;
			}
		}
		
		/// <summary>
		/// Emits the body of the catch handler.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> into which to emit the catch handler body.</param>
		/// <exception cref='ArgumentNullException'><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public void EmitHandlerBody( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
				
			if( _declLocal != null ) {
				// If we have a local, fill it
				_declLocal.Emit( cxt );
				_exceptionLocal.Set( new StackTop( _exceptionType ) ).Emit( cxt );
			}
			else {
				// Otherwise, pop the exception off the stack
				cxt.Generator.Emit( OpCodes.Pop );
			}

			_handler.Emit( cxt );
		}
		
		/// <summary>
		/// Compares this catch block with another catch block.
		/// </summary>
		/// <param name="obj">Catch block to compare to.</param>
		/// <returns>Less than zero if this block's exception type inherits from the other catch block's exception type,
		///   greater than zero if the reverse is true, and zero if neither is true.</returns>
		public int CompareTo( object obj ) {
			CatchBlock block = obj as CatchBlock;
			
			if( obj == null )
				return 0;
			
			// If they have a null exception type, they go last
			if( _exceptionType == null ) {
				if( block._exceptionType == null )
					return 0;
				else
					return 1;
			}
			else if( block._exceptionType == null )
				return -1;
			
			// If they subclass us, we are greater than (go after) them
			if( block._exceptionType.IsSubclassOf( _exceptionType ) )
				return 1;
			else if( _exceptionType.IsSubclassOf( block._exceptionType ) )
				return -1;
				
			return 0;
		}

		class StackTop : Expression
		{
			Type _type;

			public StackTop( Type type ) {
				_type = type;
			}

			public override Type ResultType {
				get {
					return _type;
				}
			}

			public override void Emit( ILGeneratorContext cxt ) {
			}
		}
	}
}