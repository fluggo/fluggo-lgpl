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
using System.Runtime.Serialization;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents common properties and methods for a data store, such as a parameter, field, or local variable.
	/// </summary>
	public interface IDataStore {
		/// <summary>
		/// Gets the type of the storage slot.
		/// </summary>
		/// <value>The type of the storage slot.</value>
		Type Type { get; }
		
		/// <summary>
		/// Gets an expression for the value of the store.
		/// </summary>
		/// <returns>An <see cref="Expression"/> for the value of the store.</returns>
		Expression Get();

		/// <summary>
		/// Gets an expression for setting the value of the store.
		/// </summary>
		/// <param name="value">An object or <see cref="Expression"/> for the value of the store.</param>
		/// <returns>An <see cref="Expression"/> that sets the value of the store to the given value.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='value'/> is <see langword='null'/>.</exception>
		/// <exception cref='StackArgumentException'><paramref name='value'/> is a void expression.</exception>
		/// <exception cref="NotSupportedException">Setting this store is not supported.</exception>
		Expression Set( object value );
	}
	
	/// <summary>
	/// Represents the error that occurs if an expression has the wrong <see cref="Expression.ResultType"/> for the context.
	/// </summary>
	[Serializable]
	public class StackArgumentException : ArgumentException {
		/// <summary>
		/// Creates a new instance of the <see cref='StackArgumentException'/> class.
		/// </summary>
		/// <param name="paramName">Name of the argument that caused the exception.</param>
		public StackArgumentException( string paramName )
			: base( "One of the given expressions was the wrong type for this context.", paramName ) {
		}

		public StackArgumentException() {
		}
		
		public StackArgumentException( string paramName, Exception innerException )
			: base( "One of the given expressions was the wrong type for this context.", innerException ) {
		}

		protected StackArgumentException( SerializationInfo info, StreamingContext context )
			: base( info, context ) {
		}
	}
	
	[Obsolete]
	public sealed class AddressOfFieldExpression : Expression {
		Expression _expr;
		FieldInfo _field;
		
		public AddressOfFieldExpression( Expression baseExpression, FieldInfo field ) {
			if( baseExpression == null )
				throw new ArgumentNullException( "baseExpression" );

			if( field == null )
				throw new ArgumentNullException( "field" );

			if( !field.DeclaringType.IsAssignableFrom( baseExpression.ResultType ) )
				throw new ArgumentException( string.Format( "{0} does not contain a definition for {1}.", baseExpression.ResultType,
					field.Name ), "baseExpression" );

			_expr = baseExpression;
			_field = field;
		}

		/// <include file='Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return "&" + _expr.ToString() + "." + _field.Name;
		}

		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of a managed reference to the type of the field referenced in this expression.</value>
		public override Type ResultType {
			get {
				return _field.FieldType.MakeByRefType();
			}
		}

		/// <include file='Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			_expr.Emit( cxt );
			cxt.Generator.Emit( OpCodes.Ldflda, _field );
		}
	}


#if false
	[Obsolete]
	public sealed class CallVirtExpression : Expression {
		MethodInfo _method;
		Expression[] _args;
		
		public CallVirtExpression( MethodInfo method, Expression[] args ) {
			if( method == null )
				throw new ArgumentNullException( "method" );

			_method = method;
			
			ParameterInfo[] @params;
			
			try {
				@params = method.GetParameters();

				if( _method.IsStatic ) {
					if( @params.Length != args.Length )
						throw new ArgumentException( "Incorrect number of arguments for this call." );
				}
				else {
					if( @params.Length + 1 != args.Length )
						throw new ArgumentException( "Incorrect number of arguments for this call." );
				}
			}
			catch( NotSupportedException ) {
			}

			if( args == null )
				args = new Expression[0];
				

			foreach( Expression arg in args ) {
				if( arg == null )
					throw new ArgumentException( "One of the given argument expressions is null." );

				if( arg.ResultType == typeof(void) )
					throw new StackArgumentException( "args" );
			}

			_args = args;
		}

		/// <include file='Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return _args[0].ToString() + "." + _method.Name +
				"( " + string.Join( ", ", ToStringArray( _args, 1, _args.Length - 1 ) ) + " )";
		}
		
		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The return type of the method, unless the return value has been suppressed.</value>
		public override Type ResultType {
			get {
				return _method.ReturnType;
			}
		}

		/// <include file='Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			foreach( Expression exp in _args )
				exp.Emit( cxt );
				
			cxt.Generator.Emit( OpCodes.Callvirt, _method );
		}
	}
#endif

	

	
	
	
}
