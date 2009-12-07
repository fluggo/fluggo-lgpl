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
#include "GraphicsResource.h"

NS_OPEN
	public ref class Texture abstract : public GraphicsResource {
	internal:
		Nullable<ResourceUsage> _usage;
	
		property IDirect3DBaseTexture9 *Ptr {
			IDirect3DBaseTexture9 *get() new { return (IDirect3DBaseTexture9*) GraphicsResource::Ptr; }
		}
		
		Texture( _GD ^device, IDirect3DBaseTexture9 *baseTexture ) : GraphicsResource( device, baseTexture ) {
		}

		Texture( _GD ^device, IDirect3DBaseTexture9 *baseTexture, ResourceUsage usage ) : GraphicsResource( device, baseTexture ), _usage( usage ) {
		}
		
	public:
		void GenerateMipSubLevels() {
			Ptr->GenerateMipSubLevels();
		}
		
		property TextureFilterType AutoGenFilterType {
			void set( TextureFilterType value );
			TextureFilterType get();
		}
		
		property unsigned int LevelOfDetail {
			void set( unsigned int value );
			unsigned int get();
		}
		
		property unsigned int LevelCount {
			unsigned int get();
		}
	};
	
	public ref class Texture2D sealed : public Texture {
	private:
		SurfaceFormat _format;
		int _height, _width;
		ResourceManagementMode _rmm;
		ResourceUsage _usage;
		
		IDirect3DTexture9 *CreateTexture( _GD^ graphicsDevice,
			int width, int height, int numberLevels, ResourceUsage usage, SurfaceFormat format,
			ResourceManagementMode resourceManagementMode );
			
		void PrepareDesc();
	
	internal:
		property IDirect3DTexture9 *Ptr {
			IDirect3DTexture9 *get() new { return (IDirect3DTexture9*) GraphicsResource::Ptr; }
		}
		
		Texture2D( _GD^ graphicsDevice, IDirect3DTexture9 *texture, NS(ResourceUsage) usage ) :
			Texture( graphicsDevice, texture, usage ) {}
		
		Texture2D( _GD^ graphicsDevice, IDirect3DTexture9 *texture ) :
			Texture( graphicsDevice, texture ) {}

	public:
		Texture2D( _GD^ graphicsDevice,
				int width, int height, int numberLevels, ResourceUsage usage, SurfaceFormat format,
				ResourceManagementMode resourceManagementMode )
				: Texture( graphicsDevice, CreateTexture( graphicsDevice, width, height, numberLevels, usage, format, resourceManagementMode ), usage ) {
			PrepareDesc();
		}

		Texture2D( _GD^ graphicsDevice,
				int width, int height, int numberLevels, ResourceUsage usage, SurfaceFormat format )
				: Texture( graphicsDevice, CreateTexture( graphicsDevice, width, height, numberLevels, usage, format, NS(ResourceManagementMode)::Manual ), usage ) {
			PrepareDesc();
		}
			
		generic <typename T> where T : value class
		void SetData( int level, Nullable<Rectangle> rect, array<T>^ data, int startIndex, int elementCount, SetDataOptions options );
		
		static Texture2D ^FromFile( _GD ^graphicsDevice, String ^filename );
		
		SIMPLE_PROPERTY_GET(NS(ResourceUsage),_usage,ResourceUsage)
		SIMPLE_PROPERTY_GET(NS(ResourceManagementMode),_rmm,ResourceManagementMode)
		SIMPLE_PROPERTY_GET(int,_width,Width)
		SIMPLE_PROPERTY_GET(int,_height,Height)
		SIMPLE_PROPERTY_GET(SurfaceFormat,_format,Format)
	};
	
	public ref class TextureCube sealed : public Texture {
	internal:
		property IDirect3DCubeTexture9 *Ptr {
			IDirect3DCubeTexture9 *get() new { return (IDirect3DCubeTexture9*) GraphicsResource::Ptr; }
		}
		
		TextureCube( _GD^ device, IDirect3DCubeTexture9 *cubeTexture ) : Texture( device, cubeTexture ) {
		}
		
		TextureCube( _GD^ device, IDirect3DCubeTexture9 *cubeTexture, ResourceUsage usage ) : Texture( device, cubeTexture, usage ) {
		}
		
	public:
	};

	public ref class Texture3D sealed : public Texture {
	internal:
		property IDirect3DVolumeTexture9 *Ptr {
			IDirect3DVolumeTexture9 *get() new { return (IDirect3DVolumeTexture9*) GraphicsResource::Ptr; }
		}
		
		Texture3D( _GD^ device, IDirect3DVolumeTexture9 *volumeTexture ) : Texture( device, volumeTexture ) {
		}
		
		Texture3D( _GD^ device, IDirect3DVolumeTexture9 *volumeTexture, ResourceUsage usage ) : Texture( device, volumeTexture, usage ) {
		}
		
	public:
	};
NS_CLOSE
