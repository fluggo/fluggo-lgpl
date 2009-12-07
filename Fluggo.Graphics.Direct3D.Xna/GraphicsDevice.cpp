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
#include "GraphicsAdapter.h"
#include "Texture.h"
#include "VertexBuffer.h"
#include "IndexBuffer.h"
#include "Shaders.h"

using Fluggo::Graphics::Rectangle;

NS_OPEN
	GraphicsDevice::GraphicsDevice( GraphicsAdapter ^adapter, DeviceType deviceType, IntPtr renderWindowHandle, CreateOptions creationOptions, PresentationParameters ^presentationParameters ) {
		_deviceW = adapter->CreateDevice( deviceType, renderWindowHandle, creationOptions, presentationParameters );
		_textures = gcnew TextureCollection( this, 0 );
		_vertexTextures = gcnew TextureCollection( this, D3DVERTEXTEXTURESAMPLER0 );
	}
	
	void GraphicsDevice::Clear( array<Rectangle>^ rectangles, ClearTargets targets, unsigned int color, float z, unsigned int stencil ) {
		pin_ptr<D3DRECT> rectptr = nullptr;
		
		if( rectangles != nullptr ) {
			if( rectangles->Length == 0 )
				throw gcnew ArgumentException( "The given array of rectangles was empty. Specify null to clear the entire device." );

			rectptr = (interior_ptr<D3DRECT>) &rectangles[0];
		}
	
		Utility::CheckResult( Ptr->Clear( (rectangles == nullptr) ? 0u : (unsigned int) rectangles->Length,
			rectptr, (unsigned int) targets, (D3DCOLOR) color, z, stencil ) );
	}

	void GraphicsDevice::Present( Nullable<Rectangle> sourceRect, Nullable<Rectangle> destRect, IntPtr destWindowOverride ) {
		pin_ptr<RECT> psrc = nullptr;
		pin_ptr<RECT> pdest = nullptr;
		Rectangle ssrc, sdest;
		
		if( sourceRect.HasValue ) {
			ssrc = sourceRect.Value;
			psrc = (interior_ptr<RECT>) &ssrc;
		}
			
		if( destRect.HasValue ) {
			sdest = destRect.Value;
			pdest = (interior_ptr<RECT>) &sdest;
		}

		if( _beginSceneCalled )
			EndScene();

		Utility::CheckResult( Ptr->Present( psrc, pdest, (HWND)(void*) destWindowOverride, NULL ) );
	}

	void GraphicsDevice::BeginScene() {
		Utility::CheckResult( Ptr->BeginScene() );
		_beginSceneCalled = true;
	}

	void GraphicsDevice::EndScene() {
		Utility::CheckResult( Ptr->EndScene() );
		_beginSceneCalled = false;
	}
	
	void GraphicsDevice::DrawPrimitives( NS(PrimitiveType) primitiveType, int startVertex, int primitiveCount ) {
		if( !_beginSceneCalled )
			BeginScene();
			
		if( startVertex < 0 )
			throw gcnew ArgumentOutOfRangeException( "startVertex" );
			
		if( primitiveCount < 1 )
			throw gcnew ArgumentOutOfRangeException( "primitiveCount" );
		
		Utility::CheckResult( Ptr->DrawPrimitive( (D3DPRIMITIVETYPE) primitiveType, startVertex, primitiveCount ) );
	}
	
	void GraphicsDevice::DrawIndexedPrimitives(NS(PrimitiveType) primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount) {
		if( !_beginSceneCalled )
			BeginScene();
			
		if( minVertexIndex < 0 )
			throw gcnew ArgumentOutOfRangeException( "minVertexIndex" );
			
		if( primitiveCount < 1 )
			throw gcnew ArgumentOutOfRangeException( "primitiveCount" );
		
/*		if( GetVertexCount( primitiveType, primitiveCount ) > numVertices )
			throw gcnew ArgumentException( "Not enough vertices were supplied." );*/
		
		Utility::CheckResult( Ptr->DrawIndexedPrimitive( (D3DPRIMITIVETYPE) primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount ) );
	}

	generic <typename T> where T : value class
	void GraphicsDevice::DrawUserPrimitives( PrimitiveType primitiveType, array<T>^ vertexData, int vertexOffset, int primitiveCount ) {
		if( !_beginSceneCalled )
			BeginScene();
		
		if( vertexData == nullptr )
			throw gcnew ArgumentNullException( "vertexData" );
			
		if( vertexOffset < 0 || vertexOffset >= vertexData->Length )
			throw gcnew ArgumentOutOfRangeException( "vertexOffset" );
	
		if( primitiveCount < 0 )
			throw gcnew ArgumentOutOfRangeException( "primitiveCount" );
			
		if( sizeof(T) == 0 )
			throw gcnew NotSupportedException( "The given value type contains no data." );
		
		if( !Utility::IsSimpleValueType( T::typeid ) )
			throw gcnew NotSupportedException( "The given value type must not contain references to the garbage-collected heap." );
			
		int vertexCount = vertexData->Length - vertexOffset;
		
		if( GetVertexCount( primitiveType, primitiveCount ) > vertexCount )
			throw gcnew ArgumentException( "Not enough vertices were supplied." );
		
		pin_ptr<T> ptr = &vertexData[vertexOffset];
		
		Utility::CheckResult( Ptr->DrawPrimitiveUP( (D3DPRIMITIVETYPE) primitiveType, primitiveCount, (void *) ptr, sizeof(T) ) );
	}
	
/*	DeviceCaps^ GraphicsDevice::GetDeviceCaps() {
		EnsureDevice();
		DeviceCaps^ caps = gcnew DeviceCaps();
		pin_ptr<D3DCAPS9> pcaps = pC(*caps);
		
		Direct3D::CheckResult( _device->GetDeviceCaps( pcaps ) );
		
		return caps;
	}*/
	
	DisplayMode GraphicsDevice::GetDisplayMode( int swapChain ) {
		DisplayMode mode;
		Utility::CheckResult( Ptr->GetDisplayMode( (unsigned int) swapChain, (D3DDISPLAYMODE*) &mode ) );
		
		return mode;
	}

	NS(FVF) GraphicsDevice::FVF::get() {
		DWORD fvf;
		Utility::CheckResult( Ptr->GetFVF( &fvf ) );
		return (NS(FVF)) fvf;
	}
	
	void GraphicsDevice::FVF::set( NS(FVF) value ) {
		Utility::CheckResult( Ptr->SetFVF( (DWORD) value ) );
	}
	
/*	VertexBuffer^ GraphicsDevice::CreateVertexBuffer( unsigned int size, BufferUsage usage, Fluggo::Graphics::Direct3D::FVF fvf, Pool pool ) {
		EnsureDevice();
		
		IDirect3DVertexBuffer9 *result;
		Direct3D::CheckResult( _device->CreateVertexBuffer( size, (unsigned int) usage, (unsigned int) fvf, (D3DPOOL) pool, &result, NULL ) );
		
		return gcnew VertexBuffer( this, result, size, usage );
	}
	
	IndexBuffer^ GraphicsDevice::CreateIndexBuffer( unsigned int sizeInBytes, BufferUsage usage, Pool pool, bool useLongIndexes ) {
		EnsureDevice();
		
		IDirect3DIndexBuffer9 *result;
		Direct3D::CheckResult( _device->CreateIndexBuffer( sizeInBytes, (DWORD) usage,
			useLongIndexes ? D3DFMT_INDEX32 : D3DFMT_INDEX16, (D3DPOOL) pool, &result, NULL ) );
		
		return gcnew IndexBuffer( this, result );
	}*/

	VertexDeclaration::VertexDeclaration( _GD ^graphicsDevice, array<VertexElement> ^elements ) {
		if( graphicsDevice == nullptr )
			throw gcnew ArgumentNullException( "graphicsDevice" );
		
		if( elements == nullptr )
			throw gcnew ArgumentNullException( "elements" );
		
		_device = graphicsDevice;

		array<VertexElement> ^newElements = gcnew array<VertexElement>( elements->Length + 1 );
		elements->CopyTo( newElements, 0 );
		newElements[elements->Length] = VertexElement::End;

		bool transformed;
		bool multiStream;
		
		for( int i = 0; i < elements->Length; i++ ) {
			transformed |= (elements[i].Usage == VertexDeclarationUsage::TransformedPosition);
			multiStream |= (elements[i].Stream != 0);
		}
		
		if( transformed && multiStream )
			throw gcnew ArgumentException( "Transformed vertices must only use stream zero.", "vertexElements" );
			
		pin_ptr<VertexElement> v = &newElements[0];
		IDirect3DVertexDeclaration9 *decl;
		
		Utility::CheckResult( _device->Ptr->CreateVertexDeclaration( (D3DVERTEXELEMENT9*) v, &decl ) );
		
		_decl = decl;
	}
	
	void GraphicsDevice::VertexDeclaration::set( NS(VertexDeclaration) ^value ) {
		if( value == nullptr )
			throw gcnew ArgumentNullException( "value" );
			
		if( value->_decl == NULL )
			throw gcnew ObjectDisposedException( "value" );
			
		Utility::CheckResult( Ptr->SetVertexDeclaration( value->_decl ) );
	}
	
	NS(VertexDeclaration) ^GraphicsDevice::VertexDeclaration::get() {
		IDirect3DVertexDeclaration9 *decl;
		Utility::CheckResult( Ptr->GetVertexDeclaration( &decl ) );
		
		return gcnew NS(VertexDeclaration)( this, decl );
	}
	
/*	Surface^ GraphicsDevice::CreateOffscreenPlainSurface( unsigned int width, unsigned int height, SurfaceFormat format, Pool pool ) {
		IDirect3DSurface9 *surface;
		Utility::CheckResult( Ptr->CreateOffscreenPlainSurface( width, height, (D3DFORMAT) format, (D3DPOOL) pool, &surface, NULL ) );
		
		return gcnew Surface( this, surface );
	}*/
	
	VertexShader ^GraphicsDevice::VertexShader::get() {
		IDirect3DVertexShader9 *shader;
		Utility::CheckResult( Ptr->GetVertexShader( &shader ) );
		
		if( shader == NULL )
			return nullptr;
		
		return gcnew NS(VertexShader)( this, shader );
	}
	
	void GraphicsDevice::VertexShader::set( NS(VertexShader) ^value ) {
		if( value == nullptr ) {
			Utility::CheckResult( Ptr->SetVertexShader( NULL ) );
		}
		else {
			Utility::CheckResult( Ptr->SetVertexShader( value->Ptr ) );
		}
	}
	
	PixelShader ^GraphicsDevice::PixelShader::get() {
		IDirect3DPixelShader9 *shader;
		Utility::CheckResult( Ptr->GetPixelShader( &shader ) );
		
		if( shader == NULL )
			return nullptr;
		
		return gcnew NS(PixelShader)( this, shader );
	}

	void GraphicsDevice::PixelShader::set( NS(PixelShader)^ value ) {
		if( value == nullptr ) {
			Utility::CheckResult( Ptr->SetPixelShader( NULL ) );
		}
		else {
			Utility::CheckResult( Ptr->SetPixelShader( value->Ptr ) );
		}
	}
	
	IndexBuffer^ GraphicsDevice::Indices::get() {
		IDirect3DIndexBuffer9 *buffer;
		Utility::CheckResult( Ptr->GetIndices( &buffer ) );
		
		if( buffer == NULL )
			return nullptr;
		
		return gcnew IndexBuffer( this, buffer );
	}
	
	void GraphicsDevice::Indices::set( IndexBuffer^ value ) {
		if( value == nullptr ) {
			Utility::CheckResult( Ptr->SetIndices( NULL ) );
		}
		else {
			Utility::CheckResult( Ptr->SetIndices( value->Ptr ) );
		}
	}
	
	NS(RenderState) ^GraphicsDevice::RenderState::get() {
		return gcnew NS(RenderState)( this );
	}

	/************************* Texture stages ***/
	SamplerState ^SamplerStateCollection::default::get( int index )
		{ return gcnew SamplerState( _device, index ); }
	VertexStream ^VertexStreamCollection::default::get( int index )
		{ return gcnew VertexStream( _device, index ); }

	Texture ^TextureCollection::default::get( int index ) {
		if( index < 0 || index >= 256 )
			throw gcnew ArgumentOutOfRangeException( "index" );
		
		// We store the texture in a dictionary because it carries the Name and Tag properties on it.
		// Besides, the texture couldn't possibly have a reference outside of this instance and the D3D runtime;
		// as long as we keep up with it (including in state blocks) we should be fine.
		Texture^ result;
		if( _dict->TryGetValue( index, result ) )
			return result;
		
		IDirect3DBaseTexture9 *tex;
		Utility::CheckResult( _device->Ptr->GetTexture( index + _offset, &tex ) );
		
		if( tex == NULL )
			return nullptr;
			
		IDirect3DTexture9 *tex2d;
		IDirect3DVolumeTexture9 *tex3d;
		IDirect3DCubeTexture9 *texCube;
		
		if( !FAILED( tex->QueryInterface( IID_IDirect3DTexture9, (void**) &tex2d ) ) ) {
			tex->Release();
			return gcnew NS(Texture2D)( _device, tex2d );
		}

		if( !FAILED( tex->QueryInterface( IID_IDirect3DVolumeTexture9, (void**) &tex3d ) ) ) {
			tex->Release();
			return gcnew Texture3D( _device, tex3d );
		}

		if( !FAILED( tex->QueryInterface( IID_IDirect3DCubeTexture9, (void**) &texCube ) ) ) {
			tex->Release();
			return gcnew TextureCube( _device, texCube );
		}
		
		// It's a weird situation, but I suppose it could happen
		//return gcnew BaseTexture( _device, tex );
		tex->Release();
		throw gcnew Exception( "Resource of an unknown type was returned." );
	}
	
	void TextureCollection::default::set( int index, Texture^ value ) {
		if( index < 0 || index >= 256 )
			throw gcnew ArgumentOutOfRangeException( "index" );
		
		if( value == nullptr ) {
			Utility::CheckResult( _device->Ptr->SetTexture( index + _offset, NULL ) );
			_dict->Remove( index );
		}
		else {
			Utility::CheckResult( _device->Ptr->SetTexture( index + _offset, value->Ptr ) );
			_dict[index] = value;
		}
	}

	/******** VertexStream **/
	void VertexStream::SetFrequency( int frequency ) {
		Utility::CheckResult( _device->Ptr->SetStreamSourceFreq( _index, frequency ) );
	}
	
	void VertexStream::SetFrequencyOfIndexData( int frequency ) {
		Utility::CheckResult( _device->Ptr->SetStreamSourceFreq( _index, D3DSTREAMSOURCE_INDEXEDDATA | frequency ) );
	}
	
	void VertexStream::SetFrequencyOfInstanceData( int frequency ) {
		Utility::CheckResult( _device->Ptr->SetStreamSourceFreq( _index, D3DSTREAMSOURCE_INSTANCEDATA | frequency ) );
	}
	
	void VertexStream::SetSource( NS(VertexBuffer) ^vb, int offsetInBytes, int vertexStride ) {
		if( vb == nullptr )
			throw gcnew ArgumentNullException( "vb" );
			
		if( offsetInBytes < 0 )
			throw gcnew ArgumentOutOfRangeException( "offsetInBytes" );
			
		if( vertexStride < 0 )
			throw gcnew ArgumentOutOfRangeException( "vertexStride" );
			
		Utility::CheckResult( _device->Ptr->SetStreamSource( _index, vb->Ptr, offsetInBytes, vertexStride ) );
	}
	
	int VertexStream::OffsetInBytes::get() {
		IDirect3DVertexBuffer9 *buffer;
		unsigned int offsetInBytes, stride;
		
		Utility::CheckResult( _device->Ptr->GetStreamSource( _index, &buffer, &offsetInBytes, &stride ) );
		
		return offsetInBytes;
	}
	
	NS(VertexBuffer) ^VertexStream::VertexBuffer::get() {
		IDirect3DVertexBuffer9 *buffer;
		unsigned int offsetInBytes, stride;
		
		Utility::CheckResult( _device->Ptr->GetStreamSource( _index, &buffer, &offsetInBytes, &stride ) );
		
		if( buffer == NULL )
			return nullptr;
			
		return gcnew NS(VertexBuffer)( _device, buffer );
	}
	
	int VertexStream::VertexStride::get() {
		IDirect3DVertexBuffer9 *buffer;
		unsigned int offsetInBytes, stride;
		
		Utility::CheckResult( _device->Ptr->GetStreamSource( _index, &buffer, &offsetInBytes, &stride ) );
		
		return stride;
	}
NS_CLOSE