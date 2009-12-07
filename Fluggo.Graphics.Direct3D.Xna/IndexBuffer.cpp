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

#include "IndexBuffer.h"
#include "GraphicsDevice.h"

NS_OPEN
	IndexBuffer::IndexBuffer( _GD ^device, IDirect3DIndexBuffer9 *ibuf ) : GraphicsResource( device, ibuf ) {
		D3DINDEXBUFFER_DESC desc;
		Utility::CheckResult( ibuf->GetDesc( &desc ) );
		
		_pool = (NS(ResourceManagementMode))(int) desc.Pool;
		_usage = (NS(ResourceUsage))(int) desc.Usage;
		_size = (int) desc.Size;
	}

	IDirect3DIndexBuffer9 *IndexBuffer::CreateBuffer( _GD ^device, Type ^indexType, int elementCount, NS(ResourceUsage) usage, NS(ResourceManagementMode) resourceManagementMode ) {
		if( device == nullptr )
			throw gcnew ArgumentNullException( "device" );
			
		if( indexType == nullptr )
			throw gcnew ArgumentNullException( "indexType" );
			
		if( elementCount <= 0 )
			throw gcnew ArgumentOutOfRangeException( "elementCount" );
		
		IDirect3DIndexBuffer9 *ibuf;
		
		// XNA rejects anything that's not 16 or 32 bits; I think that's totally wrong, as you could have a 6-byte type
		// with three 16-bit integers.
		
		int typeSize = Marshal::SizeOf( indexType );
		
		if( typeSize != 2 && typeSize != 4 )
			throw gcnew ArgumentException( "The given type must be either two or four bytes long.", "indexType" );
		
		Utility::CheckResult( device->Ptr->CreateIndexBuffer( Marshal::SizeOf( indexType ) * elementCount,
			(unsigned int) usage, (typeSize == 2) ? D3DFMT_INDEX16 : D3DFMT_INDEX32, (D3DPOOL)(int) resourceManagementMode, &ibuf, NULL ) );
		
		return ibuf;
	}

	IDirect3DIndexBuffer9 *IndexBuffer::CreateBuffer( _GD ^device, int sizeInBytes, NS(ResourceUsage) usage, IndexElementSize elementSize, NS(ResourceManagementMode) resourceManagementMode ) {
		if( device == nullptr )
			throw gcnew ArgumentNullException( "device" );
			
		if( sizeInBytes <= 0 )
			throw gcnew ArgumentOutOfRangeException( "sizeInBytes" );
		
		IDirect3DIndexBuffer9 *ibuf;
		
		// XNA rejects anything that's not 16 or 32 bits; I think that's totally wrong, as you could have a 6-byte type
		// with three 16-bit integers.
		
		Utility::CheckResult( device->Ptr->CreateIndexBuffer( sizeInBytes,
			(unsigned int) usage, (elementSize == IndexElementSize::SixteenBits) ? D3DFMT_INDEX16 : D3DFMT_INDEX32,
			(D3DPOOL)(int) resourceManagementMode, &ibuf, NULL ) );
		
		return ibuf;
	}

	generic <typename T> where T : value class
	void IndexBuffer::SetData( int offsetInBytes, array<T>^ data, int startIndex, int elementCount, SetDataOptions options ) {
		if( data == nullptr )
			throw gcnew ArgumentNullException( "data" );
			
		if( startIndex >= data->Length )
			throw gcnew ArgumentOutOfRangeException( "startIndex" );
			
		if( offsetInBytes >= _size )
			throw gcnew ArgumentOutOfRangeException( "offset" );
			
		if( (offsetInBytes + (elementCount * (int) sizeof(T))) > _size )
			throw gcnew ArgumentOutOfRangeException( "count" );
	
		void *targetPtr;
		DWORD flags = 0;
		
		if( (options & SetDataOptions::Discard) == SetDataOptions::Discard ) {
			if( (_usage & NS(ResourceUsage)::Dynamic) != NS(ResourceUsage)::Dynamic )
				throw gcnew ArgumentException( "The SetDataOptions.Discard flag can only be used on dynamic buffers." );
		
			flags |= D3DLOCK_DISCARD;
		}
			
		if( (options & SetDataOptions::NoOverwrite) == SetDataOptions::NoOverwrite ) {
			if( (_usage & NS(ResourceUsage)::Dynamic) != NS(ResourceUsage)::Dynamic )
				throw gcnew ArgumentException( "The SetDataOptions.NoOverwrite flag can only be used on dynamic buffers." );
		
			flags |= D3DLOCK_NOOVERWRITE;
		}
		
		Utility::CheckResult( Ptr->Lock( offsetInBytes << 1, elementCount << 1, &targetPtr, flags ) );
		
		pin_ptr<T> ptr = &data[startIndex];
		memcpy( targetPtr, (void*) ptr, sizeof(T) * elementCount );
		
		Utility::CheckResult( Ptr->Unlock() );
	}
NS_CLOSE