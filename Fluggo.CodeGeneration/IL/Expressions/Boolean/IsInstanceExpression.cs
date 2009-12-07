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

namespace Fluggo.CodeGeneration.IL
{
	/// <summary>
	/// Represents an expression that tests whether an object is an instance of a given class.
	/// </summary>
	public class IsInstanceExpression : BooleanExpression {
		Expression _baseExp;
		Type _targetType;

		/// <summary>
		/// Creates a new instance of the <see cref='IsInstanceExpression'/> class.
		/// </summary>
		/// <param name="baseExp">Expression to test.</param>
		/// <param name="targetType">Target type of the cast.</param>
		/// <exception cref='ArgumentNullException'><paramref name='baseExp'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='targetType'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='StackArgumentException'><paramref name='baseExp'/> is void.</exception>
		public IsInstanceExpression( Expression baseExp, Type targetType ) {
			if( baseExp == null )
				throw new ArgumentNullException( "baseExp" );

			if( targetType == null )
				throw new ArgumentNullException( "targetType" );

			if( baseExp.ResultType == typeof( void ) )
				throw new StackArgumentException( "baseExp" );

			_baseExp = baseExp;
			_targetType = targetType;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return _baseExp.ToString() + " is " + GetSimpleTypeName( _targetType );
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
				
			if( _baseExp.ResultType.IsValueType ) {
				cxt.Generator.Emit( _targetType.IsAssignableFrom( _baseExp.ResultType ) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0 );
			}

			_baseExp.Emit( cxt );
			cxt.Generator.Emit( OpCodes.Isinst, _targetType );
		}

		public override void EmitTrueBranch( ILGeneratorContext cxt, Label target ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( _baseExp.ResultType.IsValueType && _targetType.IsAssignableFrom( _baseExp.ResultType ) ) {
				cxt.Generator.Emit( OpCodes.Br, target );
			}

			_baseExp.Emit( cxt );
			cxt.Generator.Emit( OpCodes.Isinst, _targetType );
			cxt.Generator.Emit( OpCodes.Brtrue, target );
		}

		public override void EmitFalseBranch( ILGeneratorContext cxt, Label target ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );

			if( _baseExp.ResultType.IsValueType && !_targetType.IsAssignableFrom( _baseExp.ResultType ) ) {
				cxt.Generator.Emit( OpCodes.Br, target );
			}

			_baseExp.Emit( cxt );
			cxt.Generator.Emit( OpCodes.Isinst, _targetType );
			cxt.Generator.Emit( OpCodes.Brfalse, target );
		}
	}
}