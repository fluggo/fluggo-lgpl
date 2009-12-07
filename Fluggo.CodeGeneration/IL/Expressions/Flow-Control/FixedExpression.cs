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
	/// Represents an expression that pins and takes the address of a value.
	/// </summary>
	public class FixedExpression : Expression {
		Expression _value, _body;
		Local _fixedLocal;

		/// <summary>
		/// Creates a new instance of the <see cref='FixedExpression'/> class.
		/// </summary>
		/// <param name="fixedLocal">A <see cref="Local"/> to take on the address of the value. The local is scoped to the body of the statement.</param>
		/// <param name="value">Expression for the value to pin. You must be able to take the address of the expression.</param>
		/// <param name="body">Expression for the body of the fixed statement.</param>
		/// <exception cref='ArgumentNullException'><paramref name='value'/>, <paramref name="fixedLocal"/>, or <paramref name="body"/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException">The address of <paramref name="value"/> cannot be taken.
		///   <para>— OR —</para>
		///   <para>The type of <paramref name="local"/> is not a managed reference to the result type of <paramref name="value"/>.</para></exception>
		/// <exception cref="StackArgumentException"><paramref name="body"/> has a result type other than <see cref="Void"/>.</exception>
		public FixedExpression( Local fixedLocal, Expression value, Expression body ) {
			if( value == null )
				throw new ArgumentNullException( "value" );
				
			if( !value.CanTakeAddress )
				throw new ArgumentException( "The address of the expression could not be taken.", "value" );

			if( fixedLocal == null )
				throw new ArgumentNullException( "fixedLocal" );
				
			if( value.ResultType.MakeByRefType() != fixedLocal.Type )
				throw new ArgumentException( "The type of the local must be a managed reference to the type of the value.", "local" );

			if( body == null )
				throw new ArgumentNullException( "body" );
				
			if( body.ResultType != typeof(void) )
				throw new StackArgumentException( "body" );
				
			_value = value;
			_fixedLocal = fixedLocal;
			_body = body;
		}

		/// <include file='../Common.xml' path='/root/Expression/property[@name="MarksOwnSequence"]/*'/>
		public override bool MarksOwnSequence {
			get {
				return true;
			}
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			Expression addrExpr = new AddressOfExpression( _value );
			cxt.WriteMarkedCode( "fixed( " + GetSimpleTypeName( _value.ResultType ) + " *" + _fixedLocal.Name + " = " + addrExpr.ToString() + " )" );
			
			using( cxt.BeginScope() ) {
				_fixedLocal.Declare( cxt, true );
				_fixedLocal.Set( addrExpr ).Emit( cxt );
				
				cxt.WriteCodeLine( " {" );
				
				using( cxt.Indentation ) {
					if( !_body.MarksOwnSequence )
						cxt.WriteLineMarkedCode( _body.ToString() + ";" );
					
					_body.Emit( cxt );
				}
				
				cxt.WriteCodeLine( "}" );
				_fixedLocal.Set( new NullExpression( _fixedLocal.Type ) ).Emit( cxt );
			}
		}
	}
}