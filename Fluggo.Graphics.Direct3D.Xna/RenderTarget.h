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
#include <d3dx9.h>

NS_OPEN
	[Flags]
	public enum class FilterOptions : unsigned int {
		None = D3DX_FILTER_NONE,
		Point = D3DX_FILTER_POINT,
		Linear = D3DX_FILTER_LINEAR,
		Triangle = D3DX_FILTER_TRIANGLE,
		Box = D3DX_FILTER_BOX,

		MirrorU = D3DX_FILTER_MIRROR_U,
		MirrorV = D3DX_FILTER_MIRROR_V,
		MirrorW = D3DX_FILTER_MIRROR_W,
		Mirror = D3DX_FILTER_MIRROR,

		Dither = D3DX_FILTER_DITHER,
		DitherDiffusion = D3DX_FILTER_DITHER_DIFFUSION,

		sRGBIn = D3DX_FILTER_SRGB_IN,
		sRGBOut = D3DX_FILTER_SRGB_OUT,
		sRGB = D3DX_FILTER_SRGB
	};
	
	public enum ImageFileFormat {
		WindowsBitmap = D3DXIFF_BMP,
		Jpeg = D3DXIFF_JPG,
		Targa = D3DXIFF_TGA,
		Png = D3DXIFF_PNG,
		DirectDrawSurface = D3DXIFF_DDS,
		Ppm = D3DXIFF_PPM,
		Dib = D3DXIFF_DIB,
		HighDynamicRange = D3DXIFF_HDR,
		PortableFloatMap = D3DXIFF_PFM
	};
	
	public value class ImageFileInfo {
	private:
		unsigned int _width, _height, _depth, _mipLevels;
		SurfaceFormat _format;
		ResourceType _resourceType;
		ImageFileFormat _fileFormat;
		
	public:
		SIMPLE_PROPERTY_GET(unsigned int,_width,Width)
		SIMPLE_PROPERTY_GET(unsigned int,_height,Height)
		SIMPLE_PROPERTY_GET(unsigned int,_depth,Depth)
		SIMPLE_PROPERTY_GET(unsigned int,_mipLevels,MipLevels)
		SIMPLE_PROPERTY_GET(NS(SurfaceFormat),_format,Format)
		SIMPLE_PROPERTY_GET(NS(ResourceType),_resourceType,ResourceType)
		SIMPLE_PROPERTY_GET(NS(ImageFileFormat),_fileFormat,FileFormat)
	};

	ref class SurfaceDescription {
	internal:
		SurfaceFormat _format;
		ResourceType _type;
		ResourceUsage _usage;
		ResourceManagementMode _pool;
		MultiSampleType _multiSampleType;
		int _multiSampleQuality, _width, _height;
		
		SurfaceDescription() {}
		
	public:
		SIMPLE_PROPERTY_GET(NS(SurfaceFormat),_format,Format)
		SIMPLE_PROPERTY_GET(NS(ResourceType),_type,Type)
		SIMPLE_PROPERTY_GET(NS(ResourceUsage),_usage,Usage)
		SIMPLE_PROPERTY_GET(NS(ResourceManagementMode),_pool,Pool)
		SIMPLE_PROPERTY_GET(NS(MultiSampleType),_multiSampleType,MultiSampleType)
		SIMPLE_PROPERTY_GET(int,_multiSampleQuality,MultiSampleQuality)
		SIMPLE_PROPERTY_GET(int,_width,Width)
		SIMPLE_PROPERTY_GET(int,_height,Height)
	};
	
	public ref class RenderTarget {
	private:
		IDirect3DSurface9 *_surface;
		SurfaceDescription^ _desc;
	
	internal:
		property IDirect3DSurface9 *Ptr {
			IDirect3DSurface9 *get() {
				if( _surface == NULL )
					throw gcnew ObjectDisposedException( nullptr );
				
				return _surface;
			}
		}
		
		~RenderTarget() {
			this->!RenderTarget();
		}
		!RenderTarget() {
			if( _surface != NULL ) {
				_surface->Release();
				_surface = NULL;
			}
		}

	public:
		SIMPLE_PROPERTY_GET(int,_desc->_width,Width)
		SIMPLE_PROPERTY_GET(int,_desc->_height,Height)
		SIMPLE_PROPERTY_GET(NS(SurfaceFormat),_desc->_format,Format)
		//SIMPLE_PROPERTY_GET(Fluggo::Graphics::Direct3D::ResourceUsage,_desc->_usage,Usage)
		SIMPLE_PROPERTY_GET(NS(ResourceManagementMode),_desc->_pool,ResourceManagementMode)
		SIMPLE_PROPERTY_GET(NS(MultiSampleType),_desc->_multiSampleType,MultiSampleType)
		SIMPLE_PROPERTY_GET(int,_desc->_multiSampleQuality,MultiSampleQuality)
		
		property Object ^Tag {
			Object ^get() {
				void *ptr;
				DWORD size = IntPtr::Size;
				HRESULT result = Ptr->GetPrivateData( UDATA_TAG, &ptr, &size );
				
				if( result == D3DERR_NOTFOUND )
					return nullptr;
				
				Utility::CheckResult( result );
				return Marshal::GetObjectForIUnknown( (IntPtr) ptr );
			}
			void set( Object ^value ) {
				if( value == nullptr ) {
					HRESULT result = Ptr->FreePrivateData( UDATA_TAG );
					
					if( result != D3DERR_NOTFOUND )
						Utility::CheckResult( result );
				}
				else {
					void *ptr = (void *) Marshal::GetIUnknownForObject( value );
					Utility::CheckResult( Ptr->SetPrivateData( UDATA_TAG, ptr, IntPtr::Size, D3DSPD_IUNKNOWN ) );
				}
			}
		}
		
/*		generic <typename T> where T : value class
		void WriteData( Nullable<Fluggo::Primitives::Rectangle> rectangle, array<T>^ data, unsigned int startIndex, unsigned int count, WriteOptions options );
		
		ImageFileInfo FillFromFile( array<ColorRGBA>^ targetPalette, Nullable<Fluggo::Primitives::Rectangle> targetRect, String^ sourceFile,
			Nullable<Fluggo::Primitives::Rectangle> sourceRect, FilterOptions filter, ColorARGB colorKey );*/
	};
NS_CLOSE
