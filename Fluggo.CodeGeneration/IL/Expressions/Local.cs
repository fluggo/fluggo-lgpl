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
	/// Represents a local variable in a method.
	/// </summary>
	public class Local : ObjectProxy, IDataStore {
		Type _type;
		string _name;
		ILGeneratorContext.Local _local;

		/// <summary>
		/// Creates a new instance of the <see cref='Local'/> class.
		/// </summary>
		/// <param name="type">Type of the local.</param>
		/// <param name="name">Name of the local.</param>
		/// <exception cref='ArgumentNullException'><paramref name='type'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='name'/> is <see langword='null'/>.</para></exception>
		/// <remarks>This type does not declare the local or even reference it until the
		///	  <see cref="Declare(ILGeneratorContext)"/> method is called. This allows a <see cref="Local"/> instance to
		///	  be created before the variable is declared or in scope. For example, the <see cref="DeclareLocalExpression"/>
		///	  uses this overload to create the local and expressions for it before the declaration is reached
		///	  in IL code.</remarks>
		public Local( Type type, string name )
			: base() {
			if( type == null )
				throw new ArgumentNullException( "type" );

			if( name == null )
				throw new ArgumentNullException( "name" );

			_type = type;
			_name = name;
		}

		/// <summary>
		/// Gets the name of the local.
		/// </summary>
		/// <value>The name of the local.</value>
		public string Name {
			get {
				return _name;
			}
		}

		/// <summary>
		/// Gets the type of the local.
		/// </summary>
		/// <value>The type of the local.</value>
		public override Type Type {
			get {
				return _type;
			}
		}

		/// <summary>
		/// Gets an expression for the value of the store.
		/// </summary>
		/// <returns>An <see cref="Expression"/> for the value of the store.</returns>
		public override Expression Get() {
			return new ScopedLocalExpression( this );
		}
		
		private class ScopedLocalExpression : Expression {
			Local _local;
			
			public ScopedLocalExpression( Local local ) {
				_local = local;
			}

			public override string ToString() {
				return _local.Name;
			}

			public override Type ResultType {
				get {
					return _local.Type;
				}
			}

			public override bool CanTakeAddress {
				get {
					return true;
				}
			}

			public override void Emit( ILGeneratorContext cxt ) {
				if( _local._local == null )
					throw new InvalidOperationException( "This local is not yet in scope." );
					
				_local._local.Get().Emit( cxt );
			}

			public override void EmitAddress( ILGeneratorContext cxt ) {
				if( _local._local == null )
					throw new InvalidOperationException( "This local is not yet in scope." );

				_local._local.Get().EmitAddress( cxt );
			}
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
			return new ScopedSetLocalExpression( this, ILCodeBuilder.ToExpression( value ) );
		}

		private class ScopedSetLocalExpression : Expression
		{
			Local _local;
			Expression _value;

			public ScopedSetLocalExpression( Local local, Expression value ) {
				_local = local;
				_value = value;
			}

			public override string ToString() {
				return _local.Name + " = " + _value.ToString();
			}

			public override void Emit( ILGeneratorContext cxt ) {
				if( _local._local == null )
					throw new InvalidOperationException( "This local is not yet in scope." );

				_local._local.Set( _value ).Emit( cxt );
			}
		}

		/// <summary>
		/// Declares the variable at the current point in IL code.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> into which the declaration should be emitted.</param>
		/// <exception cref='ArgumentNullException'><paramref name='cxt'/> is <see langword='null'/>.</exception>
		/// <exception cref="InvalidOperationException">The local has already been declared.</exception>
		/// <remarks>The local is declared in the current scope.</remarks>
		public void Declare( ILGeneratorContext cxt ) {
			Declare( cxt, false );
		}

		/// <summary>
		/// Declares the variable at the current point in IL code.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> into which the declaration should be emitted.</param>
		/// <param name="pinned">True if the local should pin its value.</param>
		/// <exception cref='ArgumentNullException'><paramref name='cxt'/> is <see langword='null'/>.</exception>
		/// <exception cref="InvalidOperationException">The local has already been declared.</exception>
		/// <remarks>The local is declared in the current scope.</remarks>
		public void Declare( ILGeneratorContext cxt, bool pinned ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( _local != null )
				throw new InvalidOperationException( "This local has already been declared." );

			_local = cxt.DeclareLocal( _type, _name );
		}
	}
}