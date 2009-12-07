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
	/// Represents an expression that is syncrhonized through a monitor lock.
	/// </summary>
	public class LockExpression : Expression {
		Expression _lockObj, _body;

		/// <summary>
		/// Creates a new instance of the <see cref='LockExpression'/> class.
		/// </summary>
		/// <param name="lockObject">Expression for the object on which the expression will lock.</param>
		/// <param name="body">Expression that will be synchronized with the lock.</param>
		/// <exception cref='ArgumentNullException'><paramref name='lockObject'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='body'/> is <see langword='null'/>.</para></exception>
		/// <exception cref='StackArgumentException'><paramref name='lockObject'/> is a value type or void.
		///   <para>— OR —</para>
		///   <para><paramref name='body'/> is not void.</para></exception>
		public LockExpression( Expression lockObject, Expression body ) {
			if( lockObject == null )
				throw new ArgumentNullException( "lockObject" );

			if( body == null )
				throw new ArgumentNullException( "body" );

			if( lockObject.ResultType.IsValueType || (lockObject.ResultType == typeof(void)) )
				throw new StackArgumentException( "lockObject" );
				
			if( body.ResultType != typeof(void) )
				throw new StackArgumentException( "body" );
				
			_lockObj = lockObject;
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

			cxt.WriteMarkedCode( "lock( " + _lockObj.ToString() + " )" );

			TypeProxy monitor = new TypeProxy( typeof(System.Threading.Monitor) );
			monitor.Call( "Enter", _lockObj ).Emit( cxt );

			using( cxt.BeginExceptionBlock() ) {
				cxt.WriteCodeLine( " {" );
				
				using( cxt.Indentation ) {
					if( !_body.MarksOwnSequence )
						cxt.WriteLineMarkedCode( _body.ToString() + ";" );
						
					_body.Emit( cxt );
				}
				
				cxt.WriteCodeLine( "}" );
				
				cxt.Generator.BeginFinallyBlock();
				monitor.Call( "Exit", _lockObj ).Emit( cxt );
			}
		}
	}
}