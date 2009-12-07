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
	/// Represents a method call on an object or type.
	/// </summary>
	public sealed class CallExpression : Expression {
		MethodInfo _method;
		ConstructorInfo _ctor;
		Expression[] _args;

		/// <summary>
		/// Creates a new instance of the <see cref='CallExpression'/> class.
		/// </summary>
		/// <param name="args">Arguments to the call.</param>
		private CallExpression( Expression[] args ) {
			if( args == null )
				args = new Expression[0];

			foreach( Expression arg in args ) {
				if( arg == null )
					throw new ArgumentException( "One of the given argument expressions is null." );

				if( arg.ResultType == typeof( void ) )
					throw new StackArgumentException( "args" );
			}

			_args = args;
		}

		/// <summary>
		/// Creates a new instance of the <see cref='CallExpression'/> class.
		/// </summary>
		/// <param name="method"><type cref="MethodInfo"/> describing the method to call.</param>
		/// <param name="args">Arguments to the call. If the method is an instance method, the "this" parameter goes in
		///   the first array element.</param>
		/// <exception cref='ArgumentNullException'><paramref name='method'/> is <see langword='null'/>.</exception>
		public CallExpression( MethodInfo method, Expression[] args )
			: this( args ) {
			if( method == null )
				throw new ArgumentNullException( "method" );

			_method = method;
		}

		/// <summary>
		/// Creates a new instance of the <see cref='CallExpression'/> class.
		/// </summary>
		/// <param name="ctor"><type cref="ConstructorInfo"/> describing the constructor to call.</param>
		/// <param name="args">Arguments to the call. If the method is an instance method, the "this" parameter goes in
		///   the first array element.</param>
		/// <exception cref='ArgumentNullException'><paramref name='ctor'/> is <see langword='null'/>.</exception>
		/// <remarks>This overload is suitable for calling chained or base constructors.</remarks>
		public CallExpression( ConstructorInfo ctor, Expression[] args )
			: this( args ) {
			if( ctor == null )
				throw new ArgumentNullException( "ctor" );

			_ctor = ctor;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _method != null ) {
				if( _method.IsStatic ) {
					string @base = _method.DeclaringType.Name + "." + _method.Name;

					if( _args.Length == 0 )
						return @base + "()";
					else
						return @base + "( " + string.Join( ", ", ToStringArray( _args, 0, _args.Length ) ) + " )";
				}
				else {
					string @base = _args[0].ToString() + "." + _method.Name;

					if( _args[0] is ThisExpression )
						@base = _method.Name;

					if( _args.Length == 1 )
						return @base + "()";
					else
						return @base + "( " + string.Join( ", ", ToStringArray( _args, 1, _args.Length - 1 ) ) + " )";
				}
			}
			else {
				string @base = _args[0].ToString() + "." + _ctor.DeclaringType.Name;

				if( _args[0] is ThisExpression )
					@base = _ctor.DeclaringType.Name;

				@base = "/* chained ctor call */ " + @base;

				if( _args.Length == 1 )
					return @base + "()";
				else
					return @base + "( " + string.Join( ", ", ToStringArray( _args, 1, _args.Length - 1 ) ) + " )";
			}
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The return type of the method, unless the return value has been suppressed.</value>
		public override Type ResultType {
			get {
				if( _ctor != null )
					return typeof( void );

				return _method.ReturnType;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			bool firstExp = true;
			
			foreach( Expression exp in _args ) {
				if( firstExp && exp is ArrayElementExpression ) {
					exp.EmitAddress( cxt );
				}
				else {
					exp.Emit( cxt );
				}
				
				firstExp = false;
			}

			if( _method != null ) {
				if( _method.IsAbstract || _method.IsVirtual )
					cxt.Generator.Emit( OpCodes.Callvirt, _method );
				else
					cxt.Generator.Emit( OpCodes.Call, _method );
			}
			else
				cxt.Generator.Emit( OpCodes.Call, _ctor );
		}
	}
}