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

#pragma once
#include "Common.h"

NS_OPEN
	public ref class VertexShader {
	private:
		GraphicsDevice ^_device;
		IDirect3DVertexShader9 *_shaderW;
		
	internal:
		VertexShader( GraphicsDevice^ device, IDirect3DVertexShader9 *shader );
		
		~VertexShader() { this->!VertexShader(); }
		!VertexShader() {
			SAFE_RELEASE( _shaderW );
		}
		
		property IDirect3DVertexShader9 *Ptr {
			IDirect3DVertexShader9 *get() {
				if( _shaderW == NULL )
					throw gcnew ObjectDisposedException( nullptr );
					
				return _shaderW;
			}
		}
		
	public:
		// XNA-1.0-compatible
		VertexShader( GraphicsDevice^ graphicsDevice, array<unsigned char>^ shaderCode );
		
		// XNA-1.0-compatible
		property bool IsDisposed {
			bool get() {
				return _shaderW == NULL;
			}
		}
		
		// XNA-1.0-compatible
		property _GD ^GraphicsDevice {
			_GD ^get() {
				return _device;
			}
		}
	};

	public ref class PixelShader {
	private:
		GraphicsDevice ^_device;
		IDirect3DPixelShader9 *_shaderW;
		
	internal:
		PixelShader( GraphicsDevice^ device, IDirect3DPixelShader9 *shader );
		
		~PixelShader() { this->!PixelShader(); }
		!PixelShader() {
			SAFE_RELEASE( _shaderW );
		}
		
		property IDirect3DPixelShader9 *Ptr {
			IDirect3DPixelShader9 *get() {
				if( _shaderW == NULL )
					throw gcnew ObjectDisposedException( nullptr );
					
				return _shaderW;
			}
		}
		
	public:
		// XNA-1.0-compatible
		PixelShader( GraphicsDevice^ graphicsDevice, array<unsigned char>^ shaderCode );

		// XNA-1.0-compatible
		property bool IsDisposed {
			bool get() {
				return _shaderW == NULL;
			}
		}
		
		// XNA-1.0-compatible
		property _GD ^GraphicsDevice {
			_GD ^get() {
				return _device;
			}
		}
	};
NS_CLOSE