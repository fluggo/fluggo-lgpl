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

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a context for generating a type.
	/// </summary>
	public class TypeGeneratorContext {
		ModuleGeneratorContext _moduleCxt;
		TypeBuilder _type;
		CodeTextWriter _writer;
		bool _isNested, _startWritten = false;
		List<Type> _ifs = new List<Type>();
		//List<TypeGeneratorContext> _nestedTypes = new List<TypeGeneratorContext>();
		List<CodeMemberGeneratorContext> _members = new List<CodeMemberGeneratorContext>();
		
		/// <summary>
		/// Creates a new instance of the <see cref="TypeGeneratorContext"/> class.
		/// </summary>
		/// <param name="moduleContext"><see cref="ModuleGeneratorContext"/> representing the module and generator context in which the type appears.</param>
		/// <param name="typeBuilder"><see cref="TypeBuilder"/> for the type this context will represent.</param>
		/// <param name="writeToFile">True to write source code to a file, false otherwise. If true, a file will be created in the current directory
		///   based on the type's name.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleContext"/> is <see langword='null'/>.
		///   <para>- OR -</para>
		///   <para><paramref name="typeBuilder"/> is <see langword="null"/>.</para></exception>
		public TypeGeneratorContext( ModuleGeneratorContext moduleContext, TypeBuilder typeBuilder, bool writeToFile ) {
			if( moduleContext == null )
				throw new NotImplementedException( "moduleContext" );
				
			if( typeBuilder == null )
				throw new NotImplementedException( "typeBuilder" );
			
			_moduleCxt = moduleContext;
			_type = typeBuilder;
			
			if( writeToFile ) {
				_writer = new CodeTextWriter( typeBuilder.FullName.Replace( "\\+", "+" ) + ".cs" );
				WriteTypeStart();
			}
		}

		private TypeGeneratorContext( ModuleGeneratorContext moduleContext, TypeBuilder typeBuilder, CodeTextWriter writer ) {
			if( moduleContext == null )
				throw new NotImplementedException( "moduleContext" );

			if( typeBuilder == null )
				throw new NotImplementedException( "typeBuilder" );

			_moduleCxt = moduleContext;
			_type = typeBuilder;
			_writer = writer;
			_isNested = true;
			
			if( _writer != null )
				WriteTypeStart();
		}

		/// <summary>
		/// Gets a reference to the <see cref="ModuleGeneratorContext"/> for this type.
		/// </summary>
		/// <value>A reference to the <see cref="ModuleGeneratorContext"/> in which this type appears.</value>
		public ModuleGeneratorContext Module {
			get {
				return _moduleCxt;
			}
		}
		
		/// <summary>
		/// Gets an expression representing the current object.
		/// </summary>
		/// <value>A <see cref="ThisExpression"/> representing the current object.</value>
		public ThisExpression This {
			get {
				return new ThisExpression( _type );
			}
		}

		/// <summary>
		/// Gets a reference to the type represented by this context.
		/// </summary>
		/// <value>Reference to a <see cref="TypeBuilder"/> for the type represented by this context.</value>
		public TypeBuilder Type {
			get {
				return _type;
			}
		}
		
		public MethodGeneratorContext DefineMethod( string name, MethodAttributes attributes, Type returnType, Type[] paramTypes ) {
			MethodGeneratorContext cxt = new MethodGeneratorContext( this, Type.DefineMethod( name, attributes, returnType, paramTypes ), paramTypes, _writer );
			_members.Add( cxt );
			
			return cxt;
		}
		
		public PropertyGeneratorContext DefineProperty( string name, PropertyAttributes attributes, Type returnType, MethodAttributes? getMethodAttributes, MethodAttributes? setMethodAttributes, Type[] parameterTypes ) {
			PropertyBuilder prop = Type.DefineProperty( name, attributes, returnType, parameterTypes );
			MethodGeneratorContext getMethodCxt = null, setMethodCxt = null;
			
			if( getMethodAttributes.HasValue ) {
				getMethodCxt = DefineMethod( name, getMethodAttributes.Value, returnType, parameterTypes );
				prop.SetGetMethod( getMethodCxt.Method );
			}
			
			if( setMethodAttributes.HasValue ) {
				List<Type> @params = new List<Type>( parameterTypes );
				@params.Insert( 0, returnType );
				
				setMethodCxt = DefineMethod( name, setMethodAttributes.Value, typeof(void), @params.ToArray() );
				prop.SetSetMethod( setMethodCxt.Method );
			}
			
			return new PropertyGeneratorContext( This, prop, getMethodCxt, setMethodCxt );
		}
		
		public PropertyGeneratorContext DefineOverrideProperty( PropertyInfo overriddenProperty ) {
			PropertyBuilder prop = Type.DefineProperty( overriddenProperty.Name, overriddenProperty.Attributes, overriddenProperty.PropertyType, Array.ConvertAll<ParameterInfo,Type>(
				overriddenProperty.GetIndexParameters(), delegate( ParameterInfo info ) { return info.ParameterType; } ) );
			
			MethodGeneratorContext getMethodCxt = null, setMethodCxt = null;
			
			if( overriddenProperty.CanRead ) {
				getMethodCxt = DefineOverrideMethod( overriddenProperty.GetGetMethod( true ) );
				prop.SetGetMethod( getMethodCxt.Method );
			}
			
			if( overriddenProperty.CanWrite ) {
				setMethodCxt = DefineOverrideMethod( overriddenProperty.GetSetMethod( true ) );
				prop.SetSetMethod( setMethodCxt.Method );
			}
			
			return new PropertyGeneratorContext( This, prop, getMethodCxt, setMethodCxt );
		}
		
		public FieldGeneratorContext DefineField( string fieldName, Type type, FieldAttributes attributes ) {
			FieldBuilder builder = Type.DefineField( fieldName, type, attributes );
			return new FieldGeneratorContext( This, builder );
		}
		
		public MethodGeneratorContext DefineOverrideMethod( MethodInfo overriddenMethod ) {
			ParameterInfo[] info = overriddenMethod.GetParameters();
			Type[] paramTypes = new Type[info.Length];
			
			for( int i = 0; i < info.Length; i++ )
				paramTypes[i] = info[i].ParameterType;
				
			MethodAttributes attrs = (overriddenMethod.Attributes | MethodAttributes.Virtual) & ~MethodAttributes.Abstract;
			
			if( (_type.Attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed )
				attrs = (attrs | MethodAttributes.Final);
			
			MethodBuilder builder = Type.DefineMethod( overriddenMethod.Name,
				attrs, overriddenMethod.ReturnType, paramTypes );
			
			MethodGeneratorContext cxt = new MethodGeneratorContext( this, builder, paramTypes, _writer );
			_members.Add( cxt );
			
			for( int i = 0; i < info.Length; i++ )
				cxt.DefineParameter( i, info[i].Name, info[i].Attributes );

			Type.DefineMethodOverride( builder, overriddenMethod );
			
			return cxt;
		}
		
		public ConstructorGeneratorContext DefineConstructor( MethodAttributes attributes, Type[] paramTypes ) {
			ConstructorGeneratorContext cxt = new ConstructorGeneratorContext( this,
				Type.DefineConstructor( attributes, CallingConventions.Standard, paramTypes ), paramTypes, _writer );
			_members.Add( cxt );
			
			return cxt;
		}

		public void AddCustomAttribute( ConstructorInfo ctor ) {
			Type.SetCustomAttribute( new CustomAttributeBuilder( ctor, new object[0] ) );
		}
		
		public void AddCustomAttribute( ConstructorInfo ctor, params object[] ctorParams ) {
			Type.SetCustomAttribute( new CustomAttributeBuilder( ctor, ctorParams ) );
		}
		
		public void AddCustomAttribute( ConstructorInfo ctor, object[] ctorParams, PropertyInfo[] properties, object[] propertyValues ) {
			Type.SetCustomAttribute( new CustomAttributeBuilder( ctor, ctorParams, properties, propertyValues ) );
		}
		
		public void AddCustomAttribute( ConstructorInfo ctor, object[] ctorParams, FieldInfo[] fields, object[] fieldValues ) {
			Type.SetCustomAttribute( new CustomAttributeBuilder( ctor, ctorParams, fields, fieldValues ) );
		}

		public void AddInterface( Type interfaceType ) {
			Type.AddInterfaceImplementation( interfaceType );
			_ifs.Add( interfaceType );
		}
		
		private void WriteTypeStart() {
			if( _startWritten || _writer == null )
				return;
			
			if( !_isNested && _type.Namespace.Length != 0 ) {
				_writer.WriteLine( "namespace " + _type.Namespace + " {" );
				_writer.Indent++;
			}

			if( _type.IsPublic )
				_writer.Write( "public " );
			if( _type.IsSealed )
				_writer.Write( "sealed " );
			if( _type.IsAbstract && !_type.IsInterface )
				_writer.Write( "abstract " );

			if( _type.IsValueType )
				_writer.Write( "struct " );
			if( _type.IsInterface )
				_writer.Write( "interface " );
			if( _type.IsEnum )
				_writer.Write( "enum " );
			if( _type.IsClass )
				_writer.Write( "class " );

			_writer.Write( _type.Name );
			
			Type[] ifs = _ifs.ToArray();
			string[] ifNames = new string[ifs.Length];
			
			for( int i = 0; i < ifs.Length; i++ )
				ifNames[i] = ifs[i].Name;
				
			if( ifNames.Length != 0 )
				_writer.Write( " : " + string.Join( ", ", ifNames ) );
				
			_writer.WriteLine( " {" );
			_writer.Indent++;
			
			_startWritten = true;
		}
		
		private void WriteTypeEnd() {
			if( _writer == null )
				return;

			_writer.Indent--;
			_writer.WriteLine( "}" );
			
			if( !_isNested && _type.Namespace.Length != 0 ) {
				_writer.Indent--;
				_writer.WriteLine( "}" );
			}

			_writer.Close();
		}

		public TypeGeneratorContext DefineNestedType( string name ) {
			return AddToNested( new TypeGeneratorContext( _moduleCxt, _type.DefineNestedType( name ), _writer ) );
		}
		
		private TypeGeneratorContext AddToNested( TypeGeneratorContext cxt ) {
			WriteTypeStart();
			return cxt;
		}

		public TypeGeneratorContext DefineNestedType( string name, TypeAttributes attr ) {
			return AddToNested( new TypeGeneratorContext( _moduleCxt, _type.DefineNestedType( name, attr ), _writer ) );
		}

		public TypeGeneratorContext DefineNestedType( string name, TypeAttributes attr, Type parent ) {
			if( parent.IsInterface ) {
				TypeGeneratorContext cxt = DefineNestedType( name, attr );
				cxt.AddInterface( parent );
				return AddToNested( cxt );
			}
			else {
				return AddToNested( new TypeGeneratorContext( _moduleCxt, _type.DefineNestedType( name, attr, parent ), _writer ) );
			}
		}

		/// <summary>
		/// Finalizes and generates the type.
		/// </summary>
		/// <returns>The generated <see cref="Type"/>.</returns>
		/// <remarks>The type's code, as well as the code of its members, is written to the code text writer if one is available.</remarks>
		public Type CreateType() {
			WriteTypeStart();
			
			for( int i = 0; i < _members.Count; i++ ) {
				if( !_members[i].HasBeenEmitted )
					_members[i].EmitBody();
//				else
//					_writer.WriteLine( "// A member has already been emitted" );
				
/*				if( i != (_members.Count - 1) )
					_writer.WriteLine();*/
			}
				
			WriteTypeEnd();
			
			return Type.CreateType();
		}
	}
}
