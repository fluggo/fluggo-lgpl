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

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a context for generating a type member containing code, such as a method, constructor, or property accessor.
	/// </summary>
	public abstract class CodeMemberGeneratorContext {
		TypeGeneratorContext _typeCxt;
		ILGeneratorContext _ilCxt;
		Type _returnType;

		/// <summary>
		/// Creates a new instance of the <see cref='CodeMemberGeneratorContext'/> class.
		/// </summary>
		/// <param name="typeContext"><see cref="TypeGeneratorContext"/> for the member's containing type.</param>
		/// <param name="ilGen"><see cref="ILGenerator"/> for the member.</param>
		/// <param name="returnType">Return type of the member, or <see cref='Void'/> if the member does not return a value.</param>
		/// <param name="writer"><see cref="CodeTextWriter"/> to which any code text is written, or <see langword='null'/> if
		///   a writer is unavailable.</param>
		/// <exception cref='ArgumentNullException'><paramref name='typeContext'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='ilGen'/> is <see langword='null'/>.</para>
		///   <para>— OR —</para>
		///   <para><paramref name='returnType'/> is <see langword='null'/>.</para></exception>
		protected CodeMemberGeneratorContext( TypeGeneratorContext typeContext, ILGenerator ilGen, Type returnType, CodeTextWriter writer ) {
			if( typeContext == null )
				throw new ArgumentNullException( "typeContext" );

			if( ilGen == null )
				throw new ArgumentNullException( "ilGen" );

			if( returnType == null )
				throw new ArgumentNullException( "returnType" );

			_typeCxt = typeContext;
			_ilCxt = new ILGeneratorContext( this, ilGen, writer );
			_returnType = returnType;
		}

		/// <summary>
		/// Gets an expression referring to the current object.
		/// </summary>
		/// <value>A <see cref="ThisExpression"/> that evaluates to a reference to the current object.</value>
		/// <exception cref="InvalidOperationException">This context represents a static member.</exception>
		public ThisExpression This {
			get {
				if( IsStatic )
					throw new InvalidOperationException( "The containing method is static and does not have a this reference." );

				return _typeCxt.This;
			}
		}

		/// <summary>
		/// Gets the generator context of the member's module.
		/// </summary>
		/// <value>The <see cref="ModuleGeneratorContext"/> of the member's module.</value>
		public ModuleGeneratorContext Module {
			get {
				return _typeCxt.Module;
			}
		}

		/// <summary>
		/// Gets the generator context for the member's containing type.
		/// </summary>
		/// <value>The generator context for the member's containing type.</value>
		public TypeGeneratorContext Type {
			get {
				return _typeCxt;
			}
		}

		/// <summary>
		/// Gets the <see cref="ILGeneratorContext"/> for the member's body.
		/// </summary>
		/// <value>The <see cref="ILGeneratorContext"/> for the member's body.</value>
		protected ILGeneratorContext ILGenerator {
			get {
				return _ilCxt;
			}
		}

		/// <summary>
		/// Gets the return type of the member, if any.
		/// </summary>
		/// <value>The return type of the member, if any, or the <see cref='Void'/> type if the member does not return a value.</value>
		public Type ReturnType {
			get {
				return _returnType;
			}
		}

		/// <summary>
		/// Gets a value that represents whether the member is static.
		/// </summary>
		/// <value>True if the member is static, false otherwise.</value>
		public abstract bool IsStatic { get; }

		private bool _hasBeenEmitted = false;

		/// <summary>
		/// Gets a value that represents whether this member has been generated and emitted.
		/// </summary>
		/// <value>True if this member has been generated and emitted, false otherwise.</value>
		public bool HasBeenEmitted { get { return _hasBeenEmitted; } }
		
		/// <summary>
		/// Emits the member body to the IL stream and writes the member's code to the text writer, if any.
		/// </summary>
		/// <exception cref="InvalidOperationException">The member has already been emitted.</exception>
		/// <remarks>This method is called by the <see cref="TypeGeneratorContext"/> when the type is created and finalized.</remarks>
		public virtual void EmitBody() {
			if( _hasBeenEmitted )
				throw new InvalidOperationException( "This member has already been emitted." );

			_hasBeenEmitted = true;
		}
	}
}