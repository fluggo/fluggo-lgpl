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
	/// Represents an expression that declares a local variable.
	/// </summary>
	public class DeclareLocalExpression : Expression {
		Local _local;
		Expression _initExpr;

		/// <summary>
		/// Creates a new instance of the <see cref='DeclareLocalExpression'/> class.
		/// </summary>
		/// <param name="type">Type of the local.</param>
		/// <param name="name">Name of the local.</param>
		/// <param name="local">On return, contains a reference to a <see cref="Local"/> instance that
		///   allows access to the local. Expressions returned by this value should not be evaluated outside
		///   the local's scope.</param>
		/// <exception cref='ArgumentNullException'><paramref name='type'/> is <see langword='null'/>.</exception>
		/// <exception cref="StackArgumentException"><paramref name="initExpression"/> was supplied and is not of type <paramref name='type'/>.</exception>
		public DeclareLocalExpression( Type type, string name, out Local local )
			: this( type, name, null, out local ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='DeclareLocalExpression'/> class.
		/// </summary>
		/// <param name="type">Type of the local.</param>
		/// <param name="name">Name of the local.</param>
		/// <param name="initExpression">Optional expression for the initial value of the local.</param>
		/// <param name="local">On return, contains a reference to a <see cref="Local"/> instance that
		///   allows access to the local. Expressions returned by this value should not be evaluated outside
		///   the local's scope.</param>
		/// <exception cref='ArgumentNullException'><paramref name='type'/> is <see langword='null'/>.</exception>
		/// <exception cref="StackArgumentException"><paramref name="initExpression"/> was supplied and is not of type <paramref name='type'/>.</exception>
		public DeclareLocalExpression( Type type, string name, Expression initExpression, out Local local ) {
			if( type == null )
				throw new ArgumentNullException( "type" );

			if( initExpression != null && initExpression.ResultType != type )
				throw new StackArgumentException( "initExpression" );

			_local = new Local( type, name );
			_initExpr = initExpression;
			local = _local;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			if( _initExpr != null )
				return GetSimpleTypeName( _local.Type ) + " " + _local.Name + " = " + _initExpr.ToString();
			else
				return GetSimpleTypeName( _local.Type ) + " " + _local.Name;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			_local.Declare( cxt );
			
			if( _local.Type.IsValueType ) {
//				cxt.Generator.Emit( OpCodes.Initobj, _local.Type );
			}

			if( _initExpr != null )
				_local.Set( _initExpr ).Emit( cxt );
		}
	}
}