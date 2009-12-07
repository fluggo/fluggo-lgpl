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
using System.CodeDom;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Base class for IL expressions.
	/// </summary>
	public abstract class Expression {
		/// <include file='Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		/// <remarks>You should override this method in your class.</remarks>
		public abstract void Emit( ILGeneratorContext cxt );
		
		/// <summary>
		/// Evaluates the address of the expression and stores the resulting IL in the given context.
		/// </summary>
		/// <param name="cxt"><see cref="ILGeneratorContext"/> to which the IL should be written.</param>
		/// <exception cref='ArgumentNullException'><paramref name='cxt'/> is <see langword='null'/>.</exception>
		/// <exception cref="NotSupportedException">Finding the address of this expression is not supported.</exception>
		/// <remarks>The base implementation always throws a <see cref="NotSupportedException"/>.
		///   <para>Use the <see cref="CanTakeAddress"/> property to determine whether you can take the address of an expression.</para></remarks>
		public virtual void EmitAddress( ILGeneratorContext cxt ) {
			throw new NotSupportedException( "Finding the address of this expression is not supported." );
		}

		/// <summary>
		/// Gets a value that represents whether the address of the expression can be taken.
		/// </summary>
		/// <value>True if the address of the expression can be taken, false otherwise. The default is false.</value>
		public virtual bool CanTakeAddress {
			get {
				return false;
			}
		}
		
		/// <summary>
		/// Gets the type of the result of evaluating this expression.
		/// </summary>
		/// <value>The <see cref="Type"/> of the result left at the top of the stack when the expression is evaluated.
		///   If the expression leaves nothing on the stack, the value is the <see cref="Type"/> for <see cref="Void"/>.</value>
		/// <remarks>The default value is <see cref="Void"/>.</remarks>
		public virtual Type ResultType {
			get {
				return typeof(void);
			}
		}
		
		/// <summary>
		/// Gets a value that represents whether the expression marks its own sequence points in the IL stream.
		/// </summary>
		/// <value>True if the expression marks its own sequence points in the IL stream, false otherwise.
		///   The default value is false.</value>
		/// <remarks>An expression that marks its own sequence points also writes its own code text. If this
		///   property returns false for an expression, use the <see cref='Object.ToString()'/> method to get
		///   the expression's code text and write it to the stream yourself.</remarks>
		public virtual bool MarksOwnSequence {
			get {
				return false;
			}
		}
		
		/// <summary>
		/// Converts a range of expressions into their string representations.
		/// </summary>
		/// <param name="exprs">Array of expressions to convert.</param>
		/// <param name="start">Index of the first expression to convert.</param>
		/// <param name="length">Number of expressions to convert, beginning at <paramref name="start"/>.</param>
		/// <returns>An array of strings for the given range of expressions.</returns>
		public static string[] ToStringArray( Expression[] exprs, int start, int length ) {
			string[] result = new string[length];
			
			for( int i = start; i < (start + length); i++ )
				result[i - start] = exprs[i].ToString();
				
			return result;
		}
		
		/// <summary>
		/// Gets the simple C# name of a type.
		/// </summary>
		/// <param name="type">Type for which to get the simple name.</param>
		/// <returns>The built-in keyword for the type, if any, or the original type's name.</returns>
		public static string GetSimpleTypeName( Type type ) {
			switch( Type.GetTypeCode( type ) ) {
				case TypeCode.Boolean:
					return "bool";
				case TypeCode.Byte:
					return "byte";
				case TypeCode.Char:
					return "char";
				case TypeCode.Decimal:
					return "decimal";
				case TypeCode.Double:
					return "double";
				case TypeCode.Int16:
					return "short";
				case TypeCode.Int32:
					return "int";
				case TypeCode.Int64:
					return "long";
				case TypeCode.SByte:
					return "sbyte";
				case TypeCode.Single:
					return "float";
				case TypeCode.String:
					return "string";
				case TypeCode.UInt16:
					return "ushort";
				case TypeCode.UInt32:
					return "uint";
				case TypeCode.UInt64:
					return "ulong";
				case TypeCode.DBNull:
				case TypeCode.DateTime:
				case TypeCode.Empty:
				case TypeCode.Object:
				default:
					if( type.IsArray ) {
						Type elementType = type.GetElementType();
						return GetSimpleTypeName( elementType ) + "[]";
					}
					
					if( type.IsPointer ) {
						Type elementType = type.GetElementType();
						return GetSimpleTypeName( elementType ) + "*";
					}
					
					if( type == typeof( void ) )
						return "void";
						
					if( type == typeof( object ) )
						return "object";

					return type.Name;
			}
		}
	}
}