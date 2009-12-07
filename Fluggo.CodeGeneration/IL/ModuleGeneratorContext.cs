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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Security;
using System.Security.Permissions;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a context for generating a dynamic assembly.
	/// </summary>
	/// <remarks>This class wraps the <see cref="AssemblyBuilder"/> class and provides extended services. Use the <see cref="CreateAssembly"/>
	///   method to easily create a new dynamic assembly.
	///   <para>Modules, types, and methods can be added to an assembly while methods are being executed from the assembly.
	///     Use this to your advantage to reduce the number of assemblies you generate. Remember that there is no support for
	///     unloading an assembly from memory without creating a new <see cref="AppDomain"/>.</para></remarks>
	public class AssemblyGeneratorContext
	{
		AssemblyBuilder _assembly;
		string _filename;
		bool _debugMode;

		/// <summary>
		/// Creates a new instance of the <see cref='AssemblyGeneratorContext'/> class.
		/// </summary>
		/// <param name="assembly"><see cref="AssemblyBuilder"/> to be wrapped.</param>
		/// <param name="filename">Optional filename for the assembly. The filename can be used to save the assembly to disk.</param>
		/// <param name="debugMode">True if the context should emit source code for the assembly, false otherwise.</param>
		/// <exception cref='ArgumentNullException'><paramref name='assembly'/> is <see langword='null'/>.</exception>
		public AssemblyGeneratorContext( AssemblyBuilder assembly, string filename, bool debugMode ) {
			if( assembly == null )
				throw new ArgumentNullException( "assembly" );
			
			_assembly = assembly;
			_filename = filename;
			_debugMode = debugMode;
		}
		
		/// <summary>
		/// Creates a new dynamic assembly.
		/// </summary>
		/// <param name="name">Name of the new assembly.</param>
		/// <param name="filename">Optional filename for the new assembly. This filename is used if you call the <see cref="Save"/> method.</param>
		/// <param name="debugMode">Specify true to disable optimizations and emit source code for the new assembly, or false to enable optimizations
		///   and disable source code generation.</param>
		/// <returns>An <see cref="AssemblyGeneratorContext"/> for the new assembly.</returns>
		public static AssemblyGeneratorContext CreateAssembly( string name, string filename, bool debugMode ) {
			AssemblyName aname = new AssemblyName();
			aname.Name = name;

			PermissionSet deniedPerms = new PermissionSet( null );
			deniedPerms.AddPermission( new SecurityPermission( SecurityPermissionFlag.SkipVerification ) );

            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly( aname, AssemblyBuilderAccess.RunAndSave, (string) null,
				new PermissionSet( null ), new PermissionSet( null ), deniedPerms );
				
			if( debugMode ) {
				// See also:
				//		http://blogs.msdn.com/jmstall/archive/2005/2/3.aspx
				//		http://blogs.msdn.com/rmbyers/archive/2005/06/26/432922.aspx
				ConstructorInfo daCtor = typeof(DebuggableAttribute).GetConstructor( new Type[] { typeof( DebuggableAttribute.DebuggingModes ) } );
				CustomAttributeBuilder daBuilder = new CustomAttributeBuilder( daCtor, new object[] { 
					DebuggableAttribute.DebuggingModes.DisableOptimizations | 
					DebuggableAttribute.DebuggingModes.Default } ); 
				
				assembly.SetCustomAttribute( daBuilder );
			}
			
			return new AssemblyGeneratorContext( assembly, filename, debugMode );
		}
		
		public ModuleGeneratorContext DefineModule( string name ) {
			return new ModuleGeneratorContext( _assembly.DefineDynamicModule( name, _filename, _debugMode ), _debugMode );
		}
		
		/// <summary>
		/// Saves the module to the file specified when this context was created.
		/// </summary>
		/// <exception cref="InvalidOperationException">A filename was not specified when this assembly context was created.</exception>
		public void Save() {
			if( _filename == null )
				throw new InvalidOperationException( "A filename was not specified for this assembly." );
			
			_assembly.Save( _filename );
		}
	}
	
	/// <summary>
	/// Represents a context for generating a module.
	/// </summary>
	public class ModuleGeneratorContext {
		Dictionary<string,ISymbolDocumentWriter> _sourceTable = new Dictionary<string,ISymbolDocumentWriter>();
		ModuleBuilder _module;
		bool _writeDebug;

		/// <summary>
		/// Creates a new instance of the <see cref="ModuleGeneratorContext"/> class.
		/// </summary>
		/// <param name="module"><see cref="ModuleBuilder"/> for the module this instance will represent.</param>
		/// <param name="writeDebug">True to generate source code for this module, false otherwise.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword='null'/>.</exception>
		public ModuleGeneratorContext( ModuleBuilder module, bool writeDebug ) {
			if( module == null )
				throw new ArgumentNullException( "module" );

			_module = module;
			_writeDebug = writeDebug;
		}

		/// <summary>
		/// Gets a reference to the module represented by this context.
		/// </summary>
		/// <value>Reference to a <see cref="ModuleBuilder"/> for the module represented by this context.</value>
		public ModuleBuilder Module {
			get {
				return _module;
			}
		}

		internal ISymbolDocumentWriter GetSymbolWriterForSource( string filename ) {
			if( filename == null )
				throw new ArgumentNullException( "filename" );

			ISymbolDocumentWriter doc;
			
			if( !_sourceTable.TryGetValue( filename, out doc ) ) {
				doc = _module.DefineDocument( filename, SymLanguageType.CSharp, SymLanguageVendor.Microsoft, SymDocumentType.Text );
				_sourceTable[filename] = doc;
			}

			return doc;
		}

		/// <summary>
		/// Defines a new type.
		/// </summary>
		/// <param name="name">Name of the new type.</param>
		/// <returns>A <see cref="TypeGeneratorContext"/> for adding fields, properties, and methods to the new type.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='name'/> is <see langword='null'/>.</exception>
		public TypeGeneratorContext DefineType( string name ) {
			return new TypeGeneratorContext( this, _module.DefineType( name ), _writeDebug );
		}

		/// <summary>
		/// Defines a new type.
		/// </summary>
		/// <param name="name">Name of the new type.</param>
		/// <param name="attr">Bitwise combination of <see cref="TypeAttributes"/> that apply to the type.</param>
		/// <returns>A <see cref="TypeGeneratorContext"/> for adding fields, properties, and methods to the new type.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='name'/> is <see langword='null'/>.</exception>
		public TypeGeneratorContext DefineType( string name, TypeAttributes attr ) {
			return new TypeGeneratorContext( this, _module.DefineType( name, attr ), _writeDebug );
		}

		/// <summary>
		/// Defines a new type.
		/// </summary>
		/// <param name="name">Name of the new type.</param>
		/// <param name="attr">Bitwise combination of <see cref="TypeAttributes"/> that apply to the type.</param>
		/// <param name="parent">Parent of the type or an interface to implement.</param>
		/// <returns>A <see cref="TypeGeneratorContext"/> for adding fields, properties, and methods to the new type.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='name'/> is <see langword='null'/>.</exception>
		public TypeGeneratorContext DefineType( string name, TypeAttributes attr, Type parent ) {
			if( parent == null )
				return DefineType( name, attr );
			
			if( parent.IsInterface ) {
				TypeGeneratorContext cxt = new TypeGeneratorContext( this, _module.DefineType( name, attr ), _writeDebug );
				cxt.AddInterface( parent );
				return cxt;
			}
			else {
				return new TypeGeneratorContext( this, _module.DefineType( name, attr, parent ), _writeDebug );
			}
		}
	}
}