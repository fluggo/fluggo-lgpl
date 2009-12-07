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
	/// Allows easy access to the properties and methods of a typed object.
	/// </summary>
	/// <remarks>Use one of the classes that derive from <see cref="ObjectProxy"/> to write more natural
	///   generation code.</remarks>
	public abstract class ObjectProxy : IDataStore {
		/// <summary>
		/// Creates a new instance of the <see cref='ObjectProxy'/> class.
		/// </summary>
		protected ObjectProxy() {
		}

		/// <summary>
		/// Calls a static method on the proxied type.
		/// </summary>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="params">Arguments to the method.</param>
		/// <returns>A <see cref="CallExpression"/> representing the method call.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='methodName'/> is <see langword='null'/>.</exception>
		/// <exception cref="StackArgumentException">One of the <paramref name="params"/> is <see cref="Void"/>.</exception>
		public CallExpression Call( string methodName, params object[] @params ) {
			if( methodName == null )
				throw new ArgumentNullException( "methodName" );

			Expression[] exprs = ILCodeBuilder.ToExpressions( @params );
			Type[] paramTypes = new Type[exprs.Length];

			for( int i = 0; i < exprs.Length; i++ ) {
				paramTypes[i] = exprs[i].ResultType;
				
				if( paramTypes[i] == typeof(void) )
					throw new StackArgumentException( "params" );
			}

			MethodInfo method = Type.GetMethod( methodName, BindingFlags.Public | BindingFlags.Instance, null, paramTypes, null );
			
			if( method == null )
				throw new ArgumentException( string.Format( "The method {0} could not be found on the {1} type.", methodName, Type.Name ), "methodName" );

			Expression[] callExprs = new Expression[exprs.Length + 1];
			exprs.CopyTo( callExprs, 1 );
			callExprs[0] = Get();

			return new CallExpression( method, callExprs );
		}

		/// <summary>
		/// Gets a null reference of the same type as the underlying object.
		/// </summary>
		/// <value>A null reference of the same type as the underlying object.</value>
		public NullExpression Null {
			get {
				Type type = Type;
				
				if( type.IsValueType )
					throw new InvalidOperationException( "Null values only apply reference types." );
					
				return new NullExpression( type );
			}
		}

		/// <summary>
		/// Gets a boolean expression representing whether the underlying value is <see langword='null'/>.
		/// </summary>
		/// <value>A boolean expression that is true if the underlying value is <see langword='null'/> and false otherwise.</value>
		public BooleanExpression IsNull {
			get {
				return new CompareExpression( Get(), CompareOperator.Equals, Null );
			}
		}

		/// <summary>
		/// Gets a boolean expression representing whether the underlying value is not <see langword='null'/>.
		/// </summary>
		/// <value>A boolean expression that is true if the underlying value is not <see langword='null'/> and false otherwise.</value>
		public BooleanExpression IsNotNull {
			get {
				return new CompareExpression( Get(), CompareOperator.NotEquals, Null );
			}
		}

		/// <summary>
		/// Gets the type of the storage slot.
		/// </summary>
		/// <value>The type of the storage slot.</value>
		public abstract Type Type { get; }
		
		/// <summary>
		/// Gets an expression for the value of the store.
		/// </summary>
		/// <returns>An <see cref="Expression"/> for the value of the store.</returns>
		public abstract Expression Get();
		
		/// <summary>
		/// Gets an expression for setting the value of the store.
		/// </summary>
		/// <param name="value">An object or <see cref="Expression"/> for the value of the store.</param>
		/// <returns>An <see cref="Expression"/> that sets the value of the store to the given value.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='value'/> is <see langword='null'/>.</exception>
		/// <exception cref='StackArgumentException'><paramref name='value'/> is a void expression.</exception>
		/// <exception cref="NotSupportedException">Setting this store is not supported.</exception>
		/// <remarks>The base version of this method throws the <see cref="NotSupportedException"/>.
		///   Override this method in your own class to provide a set expression for your store.</remarks>
		public virtual Expression Set( object value ) {
			throw new NotSupportedException();
		}
		
		/// <summary>
		/// Gets an expression that sets the store to contain a new object.
		/// </summary>
		/// <param name="params">Parameters to the constructor of the store's type.</param>
		/// <returns>An expression that creates a new object and stores it in this store.</returns>
		public Expression SetNew( params object[] @params ) {
			return Set( new TypeProxy( Type ).New( @params ) );
		}

		/// <summary>
		/// Gets an expression that sets the store to contain a new array.
		/// </summary>
		/// <param name="elementCount">Expression or value for the number of elements in the new array.</param>
		/// <returns>An expression that creates a new array and stores it in this store.</returns>
		/// <exception cref="InvalidOperationException">The underlying type is not an array.</exception>
		public Expression SetNewArray( object elementCount ) {
			if( !Type.IsArray )
				throw new InvalidOperationException( "The underlying type of this store is not an array type." );
			
			return Set( new NewArrayExpression( Type.GetElementType(), ILCodeBuilder.ToExpression( elementCount ) ) );
		}

		/// <summary>
		/// Implicitly retrieves the value of the store.
		/// </summary>
		/// <param name="proxy"><see cref="ObjectProxy"/> for the store.</param>
		/// <returns>The value returned from the <see cref="Get"/> method.</returns>
		public static implicit operator Expression( ObjectProxy proxy ) {
			if( proxy == null )
				return null;
			
			return proxy.Get();
		}
		
		/// <summary>
		/// Gets an <see cref="ObjectProxy"/> based on the object's indexer.
		/// </summary>
		/// <param name="index">Parameter to the object's indexer or index into the array.</param>
		/// <returns>An <see cref="ObjectProxy"/> for the given element, if successful.</returns>
		/// <exception cref="InvalidOperationException">The underlying object is not an array and does not have an indexer property.</exception>
		public ObjectProxy this[object index] {
			get {
				return new ArrayElement( Get(), ILCodeBuilder.ToExpression( index ) );
			}
		}

		/// <summary>
		/// Gets an expression for incrementing the value of the expression.
		/// </summary>
		/// <returns>An <see cref="ArithmeticAssignmentExpression"/> representing an increment operation.</returns>
		public ArithmeticAssignmentExpression Increment()
			{ return new ArithmeticAssignmentExpression( this, ArithmeticOperator.Add, new Int32ConstantExpression( 1 ), true ); }

		/// <summary>
		/// Gets the property with the given name.
		/// </summary>
		/// <value>The property with the given name.</value>
		public Property Prop( string name ) {
			PropertyInfo info = Type.GetProperty( name );

			if( info == null )
				throw new ArgumentOutOfRangeException( "name", "The given property was not found." );

			return new Property( Get(), info );
		}

		/// <summary>
		/// Gets the field with the given name.
		/// </summary>
		/// <value>The field with the given name.</value>
		public Field Field( string name ) {
			FieldInfo info = Type.GetField( name );

			if( info == null )
				throw new ArgumentOutOfRangeException( "name", "The given field was not found." );

			return new Field( Get(), info );
		}

		/// <summary>
		/// Wraps an expression and allows easy access to the properties and methods of the object it represents.
		/// </summary>
		sealed class WrapperProxy : ObjectProxy {
			Expression _expr;

			/// <summary>
			/// Creates a new instance of the <see cref='WrapperProxy'/> class.
			/// </summary>
			/// <param name="expr">Expression to wrap.</param>
			/// <exception cref='ArgumentNullException'><paramref name='expr'/> is <see langword='null'/>.</exception>
			/// <exception cref='StackArgumentException'><paramref name='expr'/> is void.</exception>
			public WrapperProxy( Expression expr ) {
				if( expr == null )
					throw new ArgumentNullException( "expr" );

				if( expr.ResultType == typeof( void ) )
					throw new StackArgumentException( "expr" );

				_expr = expr;
			}

			/// <summary>
			/// Gets an expression for the value of the store.
			/// </summary>
			/// <returns>An <see cref="Expression"/> for the value of the store.</returns>
			public override Expression Get() {
				return _expr;
			}

			/// <summary>
			/// Gets the type of the storage slot.
			/// </summary>
			/// <value>The type of the storage slot.</value>
			public override Type Type {
				get { return _expr.ResultType; }
			}
		}
		
		sealed class StoreWrapperProxy : ObjectProxy {
			IDataStore _store;
			
			public StoreWrapperProxy( IDataStore store ) {
				if( store == null )
					throw new ArgumentNullException( "store" );
				
				if( store.Type == null || store.Type == typeof(void) )
					throw new StackArgumentException( "store" );

				_store = store;
			}

			public override Expression Get() {
				return _store.Get();
			}

			public override Expression Set( object value ) {
				return _store.Set( value );
			}

			public override Type Type {
				get { return _store.Type; }
			}
		}

		/// <summary>
		/// Wraps an expression as an <see cref="ObjectProxy"/>.
		/// </summary>
		/// <param name="expr">Expression to wrap.</param>
		/// <returns>An <see cref="ObjectProxy"/> that wraps the given expression.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='expr'/> is <see langword='null'/>.</exception>
		/// <exception cref='StackArgumentException'><paramref name='expr'/> is void.</exception>
		public static ObjectProxy Wrap( Expression expr ) {
			if( expr == null )
				throw new ArgumentNullException( "expr" );

			if( expr.ResultType == typeof( void ) )
				throw new StackArgumentException( "expr" );

			return new WrapperProxy( expr );
		}

		/// <summary>
		/// Wraps a data store as an <see cref="ObjectProxy"/>.
		/// </summary>
		/// <param name="store"><see cref="IDataStore"/> to wrap.</param>
		/// <returns>An <see cref="ObjectProxy"/> that wraps the given expression.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='store'/> is <see langword='null'/>.</exception>
		/// <exception cref='StackArgumentException'><paramref name='store'/> is void.</exception>
		public static ObjectProxy Wrap( IDataStore store ) {
			if( store == null )
				throw new ArgumentNullException( "store" );

			if( store.Type == null || store.Type == typeof( void ) )
				throw new StackArgumentException( "store" );

			return new StoreWrapperProxy( store );
		}
	}
	
}