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
	// XNA-beta2-compatible
	public enum class ResourceManagementMode : int {
		Manual = D3DPOOL_DEFAULT,
		Automatic = D3DPOOL_MANAGED,
//		SystemMemory = D3DPOOL_SYSTEMMEM,
//		Scratch = D3DPOOL_SCRATCH
	};
	
	// XNA-beta2-compatible
	public enum class ResourceUsage : int {
		None = 0,
		AutoGenMipMap = D3DUSAGE_AUTOGENMIPMAP,
		DoNotClip = D3DUSAGE_DONOTCLIP,
		Dynamic = D3DUSAGE_DYNAMIC,
		// Linear (XBox 360 only)
		Points = D3DUSAGE_POINTS,
		ResolveTarget = D3DUSAGE_RENDERTARGET,
		SoftwareProcessing = D3DUSAGE_SOFTWAREPROCESSING,
		// Tiled (XBox 360 only)
		WriteOnly = D3DUSAGE_WRITEONLY,

/*		Patches = D3DUSAGE_RTPATCHES,
		NPatches = D3DUSAGE_NPATCHES,
		DepthStencil = D3DUSAGE_DEPTHSTENCIL,
		DisplacementMap = D3DUSAGE_DMAP,*/
	};
	
	public ref class GraphicsResource abstract {
	private:
		GraphicsDevice ^_device;
		IDirect3DResource9 *_resource;
		String ^_name;
		Object ^_tag;
		
	protected:
		property GraphicsDevice ^Device {
			GraphicsDevice ^get() { return _device; }
		}
		
	internal:
		GraphicsResource( GraphicsDevice ^device, IDirect3DResource9 *resource ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
			
			if( resource == NULL )
				throw gcnew ArgumentNullException( "resource" );
				
			_device = device;
			_resource = resource;
		}
		
		property IDirect3DResource9 *Ptr {
			IDirect3DResource9 *get() {
				if( _resource == NULL )
					throw gcnew ObjectDisposedException( nullptr );
					
				return _resource;
			}
		}
		
		~GraphicsResource() {
			this->!GraphicsResource();
		}
		!GraphicsResource() {
			if( _resource != NULL ) {
				_resource->Release();
				_resource = NULL;
				
				Disposing( this, EventArgs::Empty );
			}
		}
		
	public:
		void FreePrivateData( Guid guid );
		
		property int Priority {
			void set( int value );
			int get();
		}
		
		property Object ^Tag {
			void set( Object ^tag ) { _tag = tag; }
			Object ^get() { return _tag; }
		}
		
		property String ^Name {
			void set( String ^name ) { _name = name; }
			String ^get() { return _name; }
		}
		
		property bool IsDisposed {
			bool get() { return _resource == NULL; }
		}
		
		property _ResType ResourceType {
			virtual _ResType get();
		}
		
		void PreLoad();
		
		property _GD^ GraphicsDevice {
			_GD^ get() {
				return _device;
			}
		}
		
		event EventHandler ^Disposing;
	};
NS_CLOSE