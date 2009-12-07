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

#include "CompiledShader.h"
#include "GraphicsDevice.h"
#include <d3dx9.h>
#include <vcclr.h>

NS_OPEN
	CompiledShader ShaderCompiler::CompileFromSource( String^ shaderSourceCode, array<CompilerMacro>^ preprocessorDefines,
			CompilerIncludeHandler ^includeHandler, CompilerOptions options, String ^functionName, ShaderProfile profile,
			TargetPlatform platform ) {
		if( shaderSourceCode == nullptr )
			throw gcnew ArgumentNullException( "shaderSourceCode" );
			
		if( functionName == nullptr )
			throw gcnew ArgumentNullException( "functionName" );
			
		if( platform != TargetPlatform::Windows )
			throw gcnew NotSupportedException( "Platforms other than Windows are not supported." );
			
		if( includeHandler != nullptr )
			throw gcnew NotImplementedException( "Include handlers are not yet implemented." );
		
		const char *profileName;
		
		switch( profile ) {
			case ShaderProfile::PS_1_1:		profileName = "ps_1_1"; break;
			case ShaderProfile::PS_1_2:		profileName = "ps_1_2"; break;
			case ShaderProfile::PS_1_3:		profileName = "ps_1_3"; break;
			case ShaderProfile::PS_1_4:		profileName = "ps_1_4"; break;
			case ShaderProfile::PS_2_0:		profileName = "ps_2_0"; break;
			case ShaderProfile::PS_2_A:		profileName = "ps_2_a"; break;
			case ShaderProfile::PS_2_B:		profileName = "ps_2_b"; break;
			case ShaderProfile::PS_2_SW:	profileName = "ps_2_sw"; break;
			case ShaderProfile::PS_3_0: 	profileName = "ps_3_0"; break;
			case ShaderProfile::XPS_3_0:	profileName = "xps_3_0"; break;
			case ShaderProfile::VS_1_1:		profileName = "vs_1_1"; break;
			case ShaderProfile::VS_2_0:		profileName = "vs_2_0"; break;
			case ShaderProfile::VS_2_A:		profileName = "vs_2_a"; break;
			case ShaderProfile::VS_2_SW:	profileName = "vs_2_sw"; break;
			case ShaderProfile::VS_3_0:		profileName = "vs_3_0"; break;
			case ShaderProfile::XVS_3_0:	profileName = "xvs_3_0"; break;
			default:
				throw gcnew NotSupportedException( "The given shader profile is not supported." );
		}
		
		IntPtr sourcePtr = Marshal::StringToHGlobalAnsi( shaderSourceCode );
		IntPtr entryPointPtr = Marshal::StringToHGlobalAnsi( functionName );
		ID3DXBuffer *shaderBuffer = NULL, *errorMessagesBuffer = NULL;
		//ID3DXConstantTable *constantTablePtr = NULL;
		
		array<IntPtr>^ macros;
		pin_ptr<IntPtr> macroPtr = nullptr;
		
		if( preprocessorDefines != nullptr ) {
			macros = gcnew array<IntPtr>( (preprocessorDefines->Length + 1) * 2 );
			
			for( int i = 0; i < preprocessorDefines->Length; i++ ) {
				macros[i * 2] = Marshal::StringToHGlobalUni( preprocessorDefines[i].Name );
				macros[i * 2 + 1] = Marshal::StringToHGlobalUni( preprocessorDefines[i].Definition );
			}
			
			macroPtr = &macros[0];
		}
		
		try {
			HRESULT result = D3DXCompileShader(
				(LPCSTR)(void *) sourcePtr, shaderSourceCode->Length + 1, (D3DXMACRO*) macroPtr, NULL, (LPCSTR)(void *) entryPointPtr,
				profileName, (DWORD) options, &shaderBuffer, &errorMessagesBuffer, NULL );
				
			if( FAILED( result ) && errorMessagesBuffer != NULL ) {
				String^ errorMessages = Marshal::PtrToStringAnsi( (IntPtr) errorMessagesBuffer->GetBufferPointer(), errorMessagesBuffer->GetBufferSize() );
				
				throw gcnew Exception( errorMessages );
			}
			
			Utility::CheckResult( result );
		}
		catch( Exception^ ex ) {
			if( shaderBuffer != NULL )
				shaderBuffer->Release();
				
			if( errorMessagesBuffer != NULL )
				errorMessagesBuffer->Release();
				
			//if( constantTablePtr != NULL )
			//	constantTablePtr->Release();
				
			throw ex;
		}
		finally {
			Marshal::FreeHGlobal( sourcePtr );
			Marshal::FreeHGlobal( entryPointPtr );
			
			if( macros != nullptr ) {
				for( int i = 0; i < macros->Length; i++ ) {
					if( macros[i] != IntPtr::Zero )
						Marshal::FreeHGlobal( macros[i] );
				}
			}
		}
		
		DWORD *shaderPtr = (DWORD*) shaderBuffer->GetBufferPointer();
		array<unsigned char>^ shader = gcnew array<unsigned char>( shaderBuffer->GetBufferSize() );
		pin_ptr<unsigned char> shaderArrayPtr = &shader[0];
		
		memcpy( shaderArrayPtr, shaderPtr, shader->Length );
		
		shaderArrayPtr = nullptr;
		shaderBuffer->Release();
		
		String^ errorMessages = nullptr;
		
		if( errorMessagesBuffer != NULL ) {
			errorMessages = Marshal::PtrToStringAnsi( (IntPtr) errorMessagesBuffer->GetBufferPointer(), errorMessagesBuffer->GetBufferSize() );
			errorMessagesBuffer->Release();
		}
		
		//ShaderConstantTable^ table;
		
		//if( constantTablePtr != NULL )
		//	table = gcnew ShaderConstantTable( constantTablePtr );
			
		return CompiledShader( shader, errorMessages /*, table */ );
	}
	
	/* ShaderConstant */
	ShaderConstant::ShaderConstant( ShaderConstantTable^ table, IntPtr handle ) {
		if( table == nullptr )
			throw gcnew ArgumentNullException( "table" );
			
		_table = table;
		_handle = handle;
	}
	
	void ShaderConstant::EnsureDescription() {
		if( _desc != nullptr )
			return;
			
		_desc = gcnew ShaderConstantDescription( _table->Ptr, _handle );
	}

	generic <typename T> where T : value class
	void ShaderConstant::SetValue( GraphicsDevice ^device, T value ) {
		if( device == nullptr )
			throw gcnew ArgumentNullException( "device" );
			
		pin_ptr<T> pvalue = &value;
		Utility::CheckResult( _table->Ptr->SetValue( device->Ptr, (D3DXHANDLE)(void *) _handle, (void*) pvalue, sizeof(T) ) );
	}
	
	/* ShaderConstantCollection */
	ShaderConstant^ ShaderConstantCollection::default::get( unsigned int index ) {
		if( index >= _table->ConstantCount )
			throw gcnew ArgumentOutOfRangeException( "index" );
			
		IntPtr handle = (IntPtr)(void*) _table->Ptr->GetConstant( NULL, index );
		
		if( handle == IntPtr::Zero )
			throw gcnew Exception( "Invalid handle returned from GetConstant." );
			
		return gcnew ShaderConstant( _table, handle );
	}
	
	ShaderConstant^ ShaderConstantCollection::default::get( String^ name ) {
		if( name == nullptr )
			throw gcnew ArgumentNullException( "name" );
			
		IntPtr namePtr = Marshal::StringToHGlobalAnsi( name );
		IntPtr handle;
		
		try {
			handle = (IntPtr)(void*) _table->Ptr->GetConstantByName( NULL, (LPCSTR)(void*) namePtr );
		}
		finally {
			Marshal::FreeHGlobal( namePtr );
		}
		
		if( handle == IntPtr::Zero )
			throw gcnew KeyNotFoundException( "\"" + name + "\" not found in shader constant table." );
			
		return gcnew ShaderConstant( _table, handle );
	}
	
	unsigned int ShaderConstantCollection::Count::get() {
		return _table->ConstantCount;
	}
	
	/* ShaderConstantTable */
	/*ShaderConstantTable::ShaderConstantTable( ID3DXConstantTable *table ) {
		if( table == NULL )
			throw gcnew ArgumentNullException( "table" );
			
		_tableW = table;
		
		D3DXCONSTANTTABLE_DESC desc;
		Utility::CheckResult( table->GetDesc( &desc ) );
		
		_creator = Marshal::PtrToStringAnsi( (IntPtr)(void *) desc.Creator );
		_version = desc.Version;
		_constants = desc.Constants;
	}*/
	ShaderConstantTable::ShaderConstantTable( array<unsigned char> ^code ) {
		if( code == nullptr )
			throw gcnew ArgumentNullException( "code" );
			
		pin_ptr<unsigned char> pCode = &code[0];
		ID3DXConstantTable *table;
		
		Utility::CheckResult( D3DXGetShaderConstantTable( (DWORD*) pCode, &table ) );

		_tableW = table;
		
		D3DXCONSTANTTABLE_DESC desc;
		Utility::CheckResult( table->GetDesc( &desc ) );
		
		_creator = Marshal::PtrToStringAnsi( (IntPtr)(void *) desc.Creator );
		_version = desc.Version;
		_constants = desc.Constants;
	}
	
	/* ShaderConstantDescription */
	ShaderConstantDescription::ShaderConstantDescription( ID3DXConstantTable *table, IntPtr constantHandle ) {
		if( table == NULL )
			throw gcnew ArgumentNullException( "table" );
			
		D3DXCONSTANT_DESC desc;
		unsigned int count = 1;

		Utility::CheckResult( table->GetConstantDesc( (D3DXHANDLE)(void*) constantHandle, &desc, &count ) );
		
		_name = Marshal::PtrToStringAnsi( (IntPtr)(void*) desc.Name );
		_registerSet = (ShaderRegisterSet) desc.RegisterSet;
		_paramClass = (ShaderParameterClass) desc.Class;
		_paramType = (ShaderParameterType) desc.Type;
		_registerIndex = desc.RegisterIndex;
		_registerCount = desc.RegisterCount;
		_rows = desc.Rows;
		_columns = desc.Columns;
		_elements = desc.Elements;
		_structMembers = desc.StructMembers;
		_bytes = desc.Bytes;
		_defaultValue = desc.DefaultValue;
	}
	
NS_CLOSE
