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

#include "GraphicsDevice.h"
#include "Shaders.h"

NS_OPEN
	/************* VertexShader implementation ******************************/
	VertexShader::VertexShader( _GD^ device, IDirect3DVertexShader9 *shader ) {
		if( device == nullptr )
			throw gcnew ArgumentNullException( "device" );
			
		if( shader == NULL )
			throw gcnew ArgumentNullException( "shader" );
			
		_device = device;
		_shaderW = shader;
	}
	
	VertexShader::VertexShader( _GD^ graphicsDevice, array<unsigned char>^ shaderCode ) {
		if( graphicsDevice == nullptr )
			throw gcnew ArgumentNullException( "graphicsDevice" );
			
		if( shaderCode == nullptr )
			throw gcnew ArgumentNullException( "shaderCode" );
		
		pin_ptr<DWORD> shaderCodePtr = (interior_ptr<DWORD>) &shaderCode[0];
		IDirect3DVertexShader9 *shader;

		Utility::CheckResult( graphicsDevice->Ptr->CreateVertexShader( shaderCodePtr, &shader ) );
		
		_device = graphicsDevice;
		_shaderW = shader;
	}
	
	/************* PixelShader implementation ******************************/
	PixelShader::PixelShader( _GD^ device, IDirect3DPixelShader9 *shader ) {
		if( device == nullptr )
			throw gcnew ArgumentNullException( "device" );
			
		if( shader == NULL )
			throw gcnew ArgumentNullException( "shader" );
			
		_device = device;
		_shaderW = shader;
	}
	
	PixelShader::PixelShader( _GD^ graphicsDevice, array<unsigned char>^ shaderCode ) {
		if( graphicsDevice == nullptr )
			throw gcnew ArgumentNullException( "graphicsDevice" );
			
		if( shaderCode == nullptr )
			throw gcnew ArgumentNullException( "shaderCode" );
		
		pin_ptr<DWORD> shaderCodePtr = (interior_ptr<DWORD>) &shaderCode[0];
		IDirect3DPixelShader9 *shader;

		Utility::CheckResult( graphicsDevice->Ptr->CreatePixelShader( shaderCodePtr, &shader ) );
		
		_device = graphicsDevice;
		_shaderW = shader;
	}
NS_CLOSE
