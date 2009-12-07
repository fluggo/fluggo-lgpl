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
	/// Represents an expression for the value of a method argument.
	/// </summary>
	public sealed class ArgumentExpression : Expression {
		Type _argType;
		int _local;
		string _argName;

		/// <summary>
		/// Creates a new instance of the <see cref="ArgumentExpression"/> class.
		/// </summary>
		/// <param name="argType">Type of the argument referred to.</param>
		/// <param name="arg">Index of the argument to retrieve, where zero is the first argument.
		///   For instance methods, argument zero is the "this" reference, and argument one is the first declared argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arg"/> is less than zero or greater than 65534.</exception>
		public ArgumentExpression( Type argType, int arg, string argName ) {
			if( argType == null )
				throw new ArgumentNullException( "argType" );

			if( arg < 0 || arg > 65534 )
				throw new ArgumentOutOfRangeException( "arg" );

			_argType = argType;
			_local = arg;
			_argName = argName;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ArgumentExpression"/> class.
		/// </summary>
		/// <param name="argType">Type of the argument referred to.</param>
		/// <param name="arg">Index of the argument to retrieve, where zero is the first argument.
		///   For instance methods, argument zero is the "this" reference, and argument one is the first declared argument.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arg"/> is less than zero or greater than 65534.</exception>
		public ArgumentExpression( Type argType, int arg )
			: this( argType, arg, null ) {
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _argName == null )
				return "arg" + (_local - 1).ToString();
			else
				return _argName;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the argument, as supplied in the constructor.</value>
		public override Type ResultType {
			get {
				return _argType;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			switch( _local ) {
				case 0:
					cxt.Generator.Emit( OpCodes.Ldarg_0 );
					break;

				case 1:
					cxt.Generator.Emit( OpCodes.Ldarg_1 );
					break;

				case 2:
					cxt.Generator.Emit( OpCodes.Ldarg_2 );
					break;

				case 3:
					cxt.Generator.Emit( OpCodes.Ldarg_3 );
					break;

				default:
					if( _local < 256 )
						cxt.Generator.Emit( OpCodes.Ldarg_S, (byte) _local );
					else
						cxt.Generator.Emit( OpCodes.Ldarg, (short) _local );

					break;
			}
		}

		/// <summary>
		/// Evaluates the address of the expression and stores the resulting IL in the given context.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <exception cref='ArgumentNullException'><paramref name='cxt'/> is <see langword='null'/>.</exception>
		public override void EmitAddress( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( _local < 256 )
				cxt.Generator.Emit( OpCodes.Ldarga_S, (byte) _local );
			else
				cxt.Generator.Emit( OpCodes.Ldarga, (short) _local );
		}

		/// <summary>
		/// Gets a value that represents whether the address of the expression can be taken.
		/// </summary>
		/// <value>This property always returns true.</value>
		public override bool CanTakeAddress {
			get {
				return true;
			}
		}
	}
}