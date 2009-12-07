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

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a parameter to a method.
	/// </summary>
	public class Param : ObjectProxy {
		int _arg;
		Type _type;
		string _name;

		/// <summary>
		/// Creates a new instance of the <see cref='Param'/> class.
		/// </summary>
		/// <param name="arg">Index of the argument that this instance represents.</param>
		/// <param name="type">Type of the argument.</param>
		/// <param name="name">Name of the argument.</param>
		/// <exception cref='ArgumentNullException'><paramref name='type'/> is <see langword='null'/>.</exception>
		public Param( int arg, Type type, string name ) {
			if( type == null )
				throw new ArgumentNullException( "type" );

			_arg = arg;
			_type = type;
			_name = name;
		}

		/// <summary>
		/// Gets the type of the parameter.
		/// </summary>
		/// <value>The type of the parameter.</value>
		public override Type Type {
			get {
				return _type;
			}
		}

		/// <summary>
		/// Returns an expression that gets the value of the parameter.
		/// </summary>
		/// <returns>An <see cref="Expression"/> that gets the value of the parameter.</returns>
		public override Expression Get() {
			return new ArgumentExpression( _type, _arg, _name );
		}

		/// <summary>
		/// Returns an expression that stores a value into the parameter.
		/// </summary>
		/// <param name="value">Expression for the new value of the parameter.</param>
		/// <returns>An <see cref="Expression"/> that stores the given expression into the parameter.</returns>
		public override Expression Set( object value ) {
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Gets an expression that checks the argument for a null value.
		/// </summary>
		/// <returns>An expression that throws an <see cref="ArgumentNullException"/> if the underlying parameter is <see langword='null'/>.</returns>
		/// <exception cref="InvalidOperationException">The parameter is not a reference type.</exception>
		public Expression ThrowExceptionIfNull() {
			TypeProxy argNull = new TypeProxy( typeof(ArgumentNullException) );
			
			return new ConditionalExpression( IsNull,
				new ThrowExpression( argNull.New( _name ) ),
				null
			);
		}
		
		/// <summary>
		/// Gets an expression that throws an <see cref="ArgumentOutOfRangeException"/> for this parameter.
		/// </summary>
		/// <returns>An expression that unconditionally throws an <see cref="ArgumentOutOfRangeException"/>.</returns>
		public Expression ThrowArgumentOutOfRange() {
			TypeProxy argRange = new TypeProxy( typeof( ArgumentOutOfRangeException ) );
			return new ThrowExpression( argRange.New( _name ) );
		}

		/// <summary>
		/// Gets an expression that throws an <see cref="ArgumentOutOfRangeException"/> for this parameter.
		/// </summary>
		/// <param name="message">Message to add to the exception.</param>
		/// <returns>An expression that unconditionally throws an <see cref="ArgumentOutOfRangeException"/>.</returns>
		public Expression ThrowArgumentOutOfRange( string message ) {
			TypeProxy argRange = new TypeProxy( typeof( ArgumentOutOfRangeException ) );
			return new ThrowExpression( argRange.New( _name, message ) );
		}
	}
}