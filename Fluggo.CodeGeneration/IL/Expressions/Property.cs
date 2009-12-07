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
	/// Represents a static or instance property on an object.
	/// </summary>
	public class Property : ObjectProxy {
		Expression _baseExpr;
		PropertyInfo _prop;
		MethodInfo _get;
		MethodInfo _set;

		/// <summary>
		/// Creates a new instance of the <see cref='Property'/> class.
		/// </summary>
		/// <param name="baseExpression">Expression for the instance on which the property appears, or <see langword="null"/> if the property is static.</param>
		/// <param name="property"><see cref="PropertyInfo"/> describing the property to access.</param>
		/// <exception cref='ArgumentNullException'><paramref name='property'/> is <see langword='null'/>.</exception>
		public Property( Expression baseExpression, PropertyInfo property )
			: base() {
			if( property == null )
				throw new ArgumentNullException( "property" );

			_get = property.GetGetMethod();
			_set = property.GetSetMethod();

			_baseExpr = baseExpression;
			_prop = property;
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
			return new PropGetExpression( this );
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
			return new PropSetExpression( this, ILCodeBuilder.ToExpression( value ) );
		}
		
		class PropGetExpression : Expression {
			Property _prop;
			
			public PropGetExpression( Property prop ) {
				if( prop._get == null )
					throw new NotSupportedException( "This property has no public get accessor." );
					
				if( !prop._get.IsStatic && prop._baseExpr == null )
					throw new InvalidOperationException( "A base expression is needed for this instance property." );
					
				_prop = prop;
			}
			
			public override Type ResultType {
				get {
					return _prop._prop.PropertyType;
				}
			}

			public override string ToString() {
				if( _prop._get.IsStatic )
					return GetSimpleTypeName( _prop._prop.DeclaringType ) + "." + _prop._prop.Name;
				else if( _prop._baseExpr is ThisExpression )
					return _prop._prop.Name;
				else
					return _prop._baseExpr.ToString() + "." + _prop._prop.Name;
			}

			public override void Emit( ILGeneratorContext cxt ) {
				if( cxt == null )
					throw new ArgumentNullException( "cxt" );

				if( _prop._get.IsStatic ) {
					cxt.Generator.Emit( OpCodes.Call, _prop._get );
				}
				else {
					_prop._baseExpr.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Callvirt, _prop._get );
				}
			}
		}

		class PropSetExpression : Expression {
			Property _prop;
			Expression _value;

			public PropSetExpression( Property prop, Expression value ) {
				if( prop._set == null )
					throw new NotSupportedException( "This property has no public set accessor." );

				if( !_prop._set.IsStatic && _prop._baseExpr == null )
					throw new InvalidOperationException( "A base expression is needed for this instance property." );

				_prop = prop;
				_value = value;
			}

			public override string ToString() {
				if( _prop._set.IsStatic )
					return GetSimpleTypeName( _prop._prop.DeclaringType ) + "." + _prop._prop.Name + " = " + _value.ToString();
				else if( _prop._baseExpr is ThisExpression )
					return _prop._prop.Name + " = " + _value.ToString();
				else
					return _prop._baseExpr.ToString() + "." + _prop._prop.Name + " = " + _value.ToString();
			}

			public override void Emit( ILGeneratorContext cxt ) {
				if( cxt == null )
					throw new ArgumentNullException( "cxt" );

				if( _prop._get.IsStatic ) {
					_value.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Call, _prop._set );
				}
				else {
					_prop._baseExpr.Emit( cxt );
					_value.Emit( cxt );
					cxt.Generator.Emit( OpCodes.Callvirt, _prop._get );
				}
			}
		}
	}
	
	public class PropertyGeneratorContext : Property {
		PropertyBuilder _builder;
		MethodGeneratorContext _getMethod, _setMethod;
		
		public PropertyGeneratorContext( Expression baseExpression, PropertyBuilder builder, MethodGeneratorContext getMethod, MethodGeneratorContext setMethod ) : base( baseExpression, builder ) {
			_builder = builder;
			_getMethod = getMethod;
			_setMethod = setMethod;
		}
		
		public MethodGeneratorContext GetMethod {
			get { return _getMethod; }
		}
		
		public MethodGeneratorContext SetMethod {
			get { return _setMethod; }
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
	}
}