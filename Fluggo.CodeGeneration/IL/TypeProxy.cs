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

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents the static methods and properties of a type.
	/// </summary>
	/// <remarks>Use this class in your generator code to access the static members of a type more easily.</remarks>
	public class TypeProxy : ILCodeBuilder {
		Type _type;

		/// <summary>
		/// Creates a new instance of the <see cref='TypeProxy'/> class.
		/// </summary>
		/// <param name="type">Type to proxy.</param>
		/// <exception cref='ArgumentNullException'><paramref name='type'/> is <see langword='null'/>.</exception>
		public TypeProxy( Type type ) {
			_type = type;
		}
		
		/// <summary>
		/// Creates a new object of the proxied type.
		/// </summary>
		/// <param name="params">Arguments to the constructor.</param>
		/// <returns>A <see cref="NewObjectExpression"/> for the new object.</returns>
		public NewObjectExpression New( params object[] @params ) {
			Expression[] exprs = ToExpressions( @params );
			Type[] paramTypes = new Type[exprs.Length];

			for( int i = 0; i < exprs.Length; i++ )
				paramTypes[i] = exprs[i].ResultType;
				
			if( paramTypes.Length == 0 && _type.IsValueType )
				throw new Exception( "Default value types cannot be created this way." );

			ConstructorInfo ctor = _type.GetConstructor( paramTypes );
			
			if( ctor == null )
				throw new Exception( "The type " + _type.FullName + " does not have a public constructor that matches the given parameters." );
			
			return new NewObjectExpression( ctor, exprs );
		}
		
		/// <summary>
		/// Calls a static method on the proxied type.
		/// </summary>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="params">Arguments to the method.</param>
		/// <returns>A <see cref="CallExpression"/> representing the method call.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='methodName'/> is <see langword='null'/>.</exception>
		public CallExpression Call( string methodName, params object[] @params ) {
			if( methodName == null )
				throw new ArgumentNullException( "methodName" );
				
			Expression[] exprs = ToExpressions( @params );
			Type[] paramTypes = new Type[exprs.Length];
			
			for( int i = 0; i < exprs.Length; i++ )
				paramTypes[i] = exprs[i].ResultType;
			
			MethodInfo method = _type.GetMethod( methodName, BindingFlags.Public | BindingFlags.Static, null, paramTypes, null );
			
			return new CallExpression( method, exprs );
		}
	}
}