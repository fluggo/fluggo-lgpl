/*
	Fluggo Direct3D Interop Library
	Copyright (C) 2005-7  Brian J. Crowell <brian@fluggo.com>

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

#include "Common.h"

NS_OPEN
	public enum class ShaderParameterClass : unsigned int {
		Scalar = D3DXPC_SCALAR,
		Vector = D3DXPC_VECTOR,
		RowMajorMatrix = D3DXPC_MATRIX_ROWS,
		ColumnMajorMatrix = D3DXPC_MATRIX_COLUMNS,
		Object = D3DXPC_OBJECT,
		Struct = D3DXPC_STRUCT
	};
	
	public enum class ShaderProfile : int {
		PS_1_1 = 0,
		PS_1_2 = 1,
		PS_1_3 = 2,
		PS_1_4 = 3,
		PS_2_0 = 4,
		PS_2_A = 5,
		PS_2_B = 6,
		PS_2_SW = 7,
		PS_3_0 = 8,
		XPS_3_0 = 9,
		VS_1_1 = 10,
		VS_2_0 = 11,
		VS_2_A = 12,
		VS_2_SW = 13,
		VS_3_0 = 14,
		XVS_3_0 = 15,
		Unknown = 16,
	};
	
	public enum class ShaderParameterType : unsigned int {
		VoidPointer = D3DXPT_VOID,
		Boolean = D3DXPT_BOOL,
		Integer = D3DXPT_INT,
		Float = D3DXPT_FLOAT,
		String = D3DXPT_STRING,
		Texture = D3DXPT_TEXTURE,
		Texture1D = D3DXPT_TEXTURE1D,
		Texture2D = D3DXPT_TEXTURE2D,
		Texture3D = D3DXPT_TEXTURE3D,
		TextureCube = D3DXPT_TEXTURECUBE,
		Sampler = D3DXPT_SAMPLER,
		Sampler1D = D3DXPT_SAMPLER1D,
		Sampler2D = D3DXPT_SAMPLER2D,
		Sampler3D = D3DXPT_SAMPLER3D,
		SamplerCube = D3DXPT_SAMPLERCUBE,
		PixelShader = D3DXPT_PIXELSHADER,
		VertexShader = D3DXPT_VERTEXSHADER,
		PixelFragment = D3DXPT_PIXELFRAGMENT,
		VertexFragment = D3DXPT_VERTEXFRAGMENT
	};
	
	public enum class ShaderRegisterSet : unsigned int {
		Boolean = D3DXRS_BOOL,
		Integer4D = D3DXRS_INT4,
		Float4D = D3DXRS_FLOAT4,
		Sampler4D = D3DXRS_SAMPLER
	};
	
	ref class ShaderConstantTable;
	ref class ShaderConstant;
	
	ref class ShaderConstantDescription {
	private:
		String^ _name;
		ShaderRegisterSet _registerSet;
		ShaderParameterClass _paramClass;
		ShaderParameterType _paramType;
		unsigned int _registerIndex, _registerCount,
			_rows, _columns, _elements, _structMembers, _bytes;
		void const *_defaultValue;
		
	public:
		ShaderConstantDescription( ID3DXConstantTable *table, IntPtr constantHandle );
		
		SIMPLE_PROPERTY_GET(String^,_name,Name)
		SIMPLE_PROPERTY_GET(ShaderRegisterSet,_registerSet,RegisterSet)
		SIMPLE_PROPERTY_GET(ShaderParameterClass,_paramClass,ParameterClass)
		SIMPLE_PROPERTY_GET(ShaderParameterType,_paramType,ParameterType)
		SIMPLE_PROPERTY_GET(unsigned int,_registerIndex,RegisterIndex)
		SIMPLE_PROPERTY_GET(unsigned int,_registerCount,RegisterCount)
		SIMPLE_PROPERTY_GET(unsigned int,_rows,Rows)
		SIMPLE_PROPERTY_GET(unsigned int,_columns,Columns)
		SIMPLE_PROPERTY_GET(unsigned int,_elements,Elements)
		SIMPLE_PROPERTY_GET(unsigned int,_structMembers,StructMembers)
		//SIMPLE_PROPERTY_GET(unsigned int,_bytes,Bytes)
	};
	
	public ref class ShaderConstant {
	internal:
		ShaderConstantTable^ _table;
		ShaderConstantDescription^ _desc;
		IntPtr _handle;
		
		ShaderConstant( ShaderConstantTable ^table, IntPtr handle );
		void EnsureDescription();
		
	public:
#define CONST_PROP_GET(t,pn) \
		property t pn {	\
			t get() { \
				EnsureDescription(); \
				return _desc->pn; \
			} \
		}
		
		CONST_PROP_GET(String^,Name)
		CONST_PROP_GET(ShaderRegisterSet,RegisterSet)
		CONST_PROP_GET(ShaderParameterClass,ParameterClass)
		CONST_PROP_GET(ShaderParameterType,ParameterType)
		CONST_PROP_GET(unsigned int,RegisterIndex)
		CONST_PROP_GET(unsigned int,RegisterCount)
		CONST_PROP_GET(unsigned int,Rows)
		CONST_PROP_GET(unsigned int,Columns)
		CONST_PROP_GET(unsigned int,Elements)
		CONST_PROP_GET(unsigned int,StructMembers)
		
		generic <typename T> where T : value class
		void SetValue( GraphicsDevice ^device, T value );
	};
	
	public ref class ShaderConstantCollection {
	internal:
		ShaderConstantTable^ _table;
		
		ShaderConstantCollection( ShaderConstantTable^ table ) {
			if( table == nullptr )
				throw gcnew ArgumentNullException( "table" );
				
			_table = table;
		}
	
	public:
		property ShaderConstant^ default[unsigned int] {
			ShaderConstant^ get(unsigned int index);
			ShaderConstant^ get(String^ name);
		}

		property unsigned int Count {
			unsigned int get();
		}
	};
	
	public ref class ShaderConstantTable {
	private:
		ID3DXConstantTable *_tableW;
		String^ _creator;
		unsigned int _version, _constants;
	
	internal:
		~ShaderConstantTable() { this->!ShaderConstantTable(); }
		!ShaderConstantTable() {
			if( _tableW != NULL ) {
				_tableW->Release();
				_tableW = NULL;
			}
		}
		
//		ShaderConstantTable( ID3DXConstantTable *table );
		
		property ID3DXConstantTable *Ptr {
			ID3DXConstantTable *get() {
				if( _tableW == NULL )
					throw gcnew ObjectDisposedException( nullptr );
					
				return _tableW;
			}
		}
		
		property unsigned int ConstantCount {
			unsigned int get() { return _constants; }
		}
		
	public:
		ShaderConstantTable( array<unsigned char> ^code );
	
		property String^ Creator {
			String^ get() {
				return _creator;
			}
		}
		
		property unsigned int Version {
			unsigned int get() {
				return _version;
			}
		}
		
		property ShaderConstantCollection^ Constants {
			ShaderConstantCollection^ get() {
				return gcnew ShaderConstantCollection( this );
			}
		}
		
/*		IntPtr GetConstant( IntPtr constantHandle, unsigned int index );
		IntPtr GetConstantByName( IntPtr constantHandle, String^ name );
		IntPtr GetConstantElement( IntPtr constantHandle, unsigned int index );*/
	};
	
	public value class CompiledShader {
	private:
		array<unsigned char>^ _shader;
		String^ _errorMessages;
		//ShaderConstantTable^ _constantTable;
		
/*		CompiledShader( array<unsigned char>^ shader, String^ errorMessages, ShaderConstantTable^ constantTable ) {
			if( shader == nullptr )
				throw gcnew ArgumentNullException( "shader" );
				
			_shader = shader;
			_errorMessages = errorMessages;
			_constantTable = constantTable;
		}*/
		
	public:
		
		// XNA-1.0-compatible
		CompiledShader( array<unsigned char> ^shader, String ^errorMessages ) {
			if( shader == nullptr )
				throw gcnew ArgumentNullException( "shader" );
				
			_shader = shader;
			_errorMessages = errorMessages;
		}
		
		// XNA-1.0-compatible
		array<unsigned char>^ GetShaderCode() {
			return _shader;
		}
		
		// XNA-1.0-compatible
		property int ShaderSize {
			int get() {
				return _shader->Length;
			}
		}
		
		// XNA-1.0-compatible
		property String^ ErrorsAndWarnings {
			String^ get() {
				return _errorMessages;
			}
		}
		
/*		property ShaderConstantTable^ ConstantTable {
			ShaderConstantTable^ get() {
				return _constantTable;
			}
		}*/
	};
	
	public ref class ShaderCompiler {
	private:
		ShaderCompiler() {}
		
	public:
		static CompiledShader CompileFromSource( String^ shaderSourceCode, array<CompilerMacro>^ preprocessorDefines,
			CompilerIncludeHandler ^includeHandler, CompilerOptions options, String ^functionName, ShaderProfile profile,
			TargetPlatform platform );
	};
NS_CLOSE