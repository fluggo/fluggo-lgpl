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

#include "VertexBuffer.h"
#include "GraphicsDevice.h"

NS_OPEN
	/**************************************
		VertexBuffer
	*/
	VertexBuffer::VertexBuffer( _GD ^device, IDirect3DVertexBuffer9 *vbuf ) : GraphicsResource( device, vbuf ) {
		D3DVERTEXBUFFER_DESC desc;
		Utility::CheckResult( vbuf->GetDesc( &desc ) );
		
		_pool = (NS(ResourceManagementMode))(int) desc.Pool;
		_usage = (NS(ResourceUsage))(int) desc.Usage;
		_length = (int) desc.Size;
	}
	
	IDirect3DVertexBuffer9 *VertexBuffer::CreateBuffer(_GD ^device, System::Type ^vertexType, int elementCount, NS(ResourceUsage) usage, NS(ResourceManagementMode) resourceManagementMode) {
		if( device == nullptr )
			throw gcnew ArgumentNullException( "device" );
			
		if( vertexType == nullptr )
			throw gcnew ArgumentNullException( "vertexType" );
			
		if( elementCount <= 0 )
			throw gcnew ArgumentOutOfRangeException( "elementCount" );
		
		IDirect3DVertexBuffer9 *vbuf;
		
		Utility::CheckResult( device->Ptr->CreateVertexBuffer( Marshal::SizeOf( vertexType ) * elementCount,
			(unsigned int) usage, 0, (D3DPOOL)(int) resourceManagementMode, &vbuf, NULL ) );
		
		return vbuf;
	}

	IDirect3DVertexBuffer9 *VertexBuffer::CreateBuffer( _GD ^device, int sizeInBytes, ResourceUsage usage, ResourceManagementMode resourceManagementMode ) {
		if( device == nullptr )
			throw gcnew ArgumentNullException( "device" );
			
		if( sizeInBytes <= 0 )
			throw gcnew ArgumentOutOfRangeException( "sizeInBytes" );
		
		IDirect3DVertexBuffer9 *vbuf;
		
		Utility::CheckResult( device->Ptr->CreateVertexBuffer( sizeInBytes,
			(unsigned int) usage, 0, (D3DPOOL)(int) resourceManagementMode, &vbuf, NULL ) );
		
		return vbuf;
	}

	generic <typename T> where T : value class
	void VertexBuffer::SetData( int offsetInBytes, array<T>^ data, int startIndex, int elementCount, SetDataOptions options ) {
		if( data == nullptr )
			throw gcnew ArgumentNullException( "data" );
			
		if( startIndex < 0 || startIndex >= data->Length )
			throw gcnew ArgumentOutOfRangeException( "startIndex" );
			
		if( offsetInBytes < 0 || offsetInBytes >= _length )
			throw gcnew ArgumentOutOfRangeException( "offsetInBytes" );
			
		if( elementCount < 0 || (offsetInBytes + (elementCount * (int) sizeof(T))) > _length )
			throw gcnew ArgumentOutOfRangeException( "elementCount" );
	
		if( sizeof(T) == 0 )
			throw gcnew NotSupportedException( "The given value type contains no data." );
		
		/*if( !Direct3D::IsSimpleValueType( T::typeid ) )
			throw gcnew NotSupportedException( "The given value type must not contain references to the garbage-collected heap." );*/
			
		void *targetPtr;
		DWORD flags = 0;
		
		if( (options & SetDataOptions::Discard) == SetDataOptions::Discard ) {
			if( (_usage & ResourceUsage::Dynamic) != ResourceUsage::Dynamic )
				throw gcnew ArgumentException( "The SetDataOptions.Discard flag can only be used on dynamic buffers." );
		
			flags |= D3DLOCK_DISCARD;
		}
			
		if( (options & SetDataOptions::NoOverwrite) == SetDataOptions::NoOverwrite ) {
			if( (_usage & ResourceUsage::Dynamic) != ResourceUsage::Dynamic )
				throw gcnew ArgumentException( "The SetDataOptions.NoOverwrite flag can only be used on dynamic buffers." );
		
			flags |= D3DLOCK_NOOVERWRITE;
		}
		
		Utility::CheckResult( Ptr->Lock( offsetInBytes, ((unsigned int) elementCount) * sizeof(T), &targetPtr, flags ) );
		
		pin_ptr<T> ptr = &data[startIndex];
		memcpy( targetPtr, ptr, ((unsigned int) elementCount) * sizeof(T) );
		
		Utility::CheckResult( Ptr->Unlock() );
	}
NS_CLOSE