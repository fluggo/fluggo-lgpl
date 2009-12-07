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
	/// Represents a static or instance field in a type.
	/// </summary>
	public class Field : ObjectProxy {
		Expression _baseExpr;
		FieldInfo _field;

		/// <summary>
		/// Creates a new instance of the <see cref='Field'/> class.
		/// </summary>
		/// <param name="baseExpression">An expression for the field's instance, or <see langword='null'/> if the field is static.</param>
		/// <param name="field">A <see cref="FieldInfo"/> value representing the field to access.</param>
		/// <exception cref='ArgumentNullException'><paramref name='field'/> is <see langword='null'/>.</exception>
		public Field( Expression baseExpression, FieldInfo field )
			: base() {
			if( field == null )
				throw new ArgumentNullException( "field" );

			_baseExpr = baseExpression;
			_field = field;
		}

		/// <summary>
		/// Gets the name of the local.
		/// </summary>
		/// <value>The name of the local.</value>
		public string Name {
			get {
				return _field.Name;
			}
		}

		/// <summary>
		/// Gets the type of the storage slot.
		/// </summary>
		/// <value>The type of the storage slot.</value>
		public override Type Type {
			get {
				return _field.FieldType;
			}
		}

		/// <summary>
		/// Gets an expression for the value of the store.
		/// </summary>
		/// <returns>An <see cref="Expression"/> for the value of the store.</returns>
		public override Expression Get() {
			if( _baseExpr == null )
				return new FieldExpression( _field );
			else
				return new FieldExpression( _baseExpr, _field );
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
			if( _baseExpr == null )
				return new StoreFieldExpression( _field, ILCodeBuilder.ToExpression( value ) );
			else
				return new StoreFieldExpression( _baseExpr, _field, ILCodeBuilder.ToExpression( value ) );
		}
	}
	
	public class FieldGeneratorContext : Field {
		FieldBuilder _builder;
		
		public FieldGeneratorContext( Expression baseExpression, FieldBuilder builder ) : base( baseExpression, builder ) {
			_builder = builder;
		}

		public void AddCustomAttribute( ConstructorInfo ctor ) {
			_builder.SetCustomAttribute( new CustomAttributeBuilder( ctor, new object[0] ) );
		}
		
		public void AddCustomAttribute( ConstructorInfo ctor, params object[] ctorParams ) {
			_builder.SetCustomAttribute( new CustomAttributeBuilder( ctor, ctorParams ) );
		}
		
		public void AddCustomAttribute( ConstructorInfo ctor, object[] ctorParams, PropertyInfo[] properties, object[] propertyValues ) {
			_builder.SetCustomAttribute( new CustomAttributeBuilder( ctor, ctorParams, properties, propertyValues ) );
		}

		public void AddCustomAttribute( ConstructorInfo ctor, object[] ctorParams, FieldInfo[] fields, object[] fieldValues ) {
			_builder.SetCustomAttribute( new CustomAttributeBuilder( ctor, ctorParams, fields, fieldValues ) );
		}
	}
}