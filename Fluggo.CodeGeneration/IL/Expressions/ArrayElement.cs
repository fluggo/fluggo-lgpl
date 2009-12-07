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
	/// Represents an element in a native CLR vector.
	/// </summary>
	public class ArrayElement : ObjectProxy {
		Expression _baseExpr;
		Expression _index;

		/// <summary>
		/// Creates a new instance of the <see cref='ArrayElement'/> class.
		/// </summary>
		/// <param name="baseExpression">Expression for the array element to access.</param>
		/// <param name="index">Expression for the index of the array element.</param>
		/// <exception cref='ArgumentNullException'><paramref name='baseExpression'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='index'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='StackArgumentException'><paramref name='baseExpression'/> does not represent an array.</exception>
		public ArrayElement( Expression baseExpression, Expression index )
			: base() {
			if( baseExpression == null )
				throw new ArgumentNullException( "baseExpression" );

			if( index == null )
				throw new ArgumentNullException( "index" );
				
			if( !baseExpression.ResultType.IsArray )
				throw new StackArgumentException( "baseExpression" );

			_baseExpr = baseExpression;
			_index = index;
		}

		/// <summary>
		/// Gets the type of the storage slot.
		/// </summary>
		/// <value>The type of the storage slot.</value>
		public override Type Type {
			get {
				return _baseExpr.ResultType.GetElementType();
			}
		}

		/// <summary>
		/// Gets an expression for the value of the store.
		/// </summary>
		/// <returns>An <see cref="Expression"/> for the value of the store.</returns>
		public override Expression Get() {
			return new ArrayElementExpression( _baseExpr, _index );
		}

		/// <summary>
		/// Gets an expression for setting the value of the store.
		/// </summary>
		/// <param name="value">An object or <see cref="Expression"/> for the value of the store.</param>
		/// <returns>An <see cref="Expression"/> that sets the value of the store to the given value.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='value'/> is <see langword='null'/>.</exception>
		/// <exception cref='StackArgumentException'><paramref name='value'/> is a void expression.</exception>
		/// <exception cref="NotSupportedException">Setting this store is not supported.</exception>
		public override Expression Set( object value ) {
			return new SetArrayElementExpression( _baseExpr, _index, ILCodeBuilder.ToExpression( value ) );
		}
	}
}