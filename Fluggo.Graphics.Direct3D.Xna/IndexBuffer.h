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
	public enum class IndexElementSize {
		SixteenBits = 0,
		ThirtyTwoBits = 1
	};
	
	public ref class IndexBuffer : public GraphicsResource {
	private:
		NS(ResourceUsage) _usage;
		NS(ResourceManagementMode) _pool;
		int _size;
	
		static IDirect3DIndexBuffer9 *CreateBuffer( _GD ^device, int sizeInBytes, ResourceUsage usage, IndexElementSize elementSize,
			ResourceManagementMode resourceManagementMode );
		static IDirect3DIndexBuffer9 *CreateBuffer( _GD ^device, Type ^indexType, int elementCount, NS(ResourceUsage) usage,
			NS(ResourceManagementMode) resourceManagementMode );

	internal:
		property IDirect3DIndexBuffer9 *Ptr {
			IDirect3DIndexBuffer9 *get() new { return (IDirect3DIndexBuffer9*) GraphicsResource::Ptr; }
		}
		
		IndexBuffer( _GD ^device, IDirect3DIndexBuffer9 *indexBuffer );
		
	public:
		IndexBuffer( _GD ^device, Type ^indexType, int elementCount, NS(ResourceUsage) usage, NS(ResourceManagementMode) resourceManagementMode )
			: GraphicsResource( device, CreateBuffer( device, indexType, elementCount, usage, resourceManagementMode ) ), _usage( usage ), _pool( resourceManagementMode ) {
			_size = Marshal::SizeOf( indexType ) * elementCount;
		}
	
		IndexBuffer( _GD ^device, Type ^indexType, int elementCount, NS(ResourceUsage) usage )
			: GraphicsResource( device, CreateBuffer( device, indexType, elementCount, usage, NS(ResourceManagementMode)::Automatic ) ), _usage( usage ), _pool( NS(ResourceManagementMode)::Automatic ) {
			_size = Marshal::SizeOf( indexType ) * elementCount;
		}

		IndexBuffer( _GD ^device, int sizeInBytes, ResourceUsage usage, IndexElementSize elementSize, ResourceManagementMode resourceManagementMode )
			: GraphicsResource( device, CreateBuffer( device, sizeInBytes, usage, elementSize, resourceManagementMode ) ), _usage( usage ), _size( sizeInBytes ), _pool( resourceManagementMode ) {
		}
	
		IndexBuffer( _GD ^device, int sizeInBytes, ResourceUsage usage, IndexElementSize elementSize )
			: GraphicsResource( device, CreateBuffer( device, sizeInBytes, usage, elementSize, NS(ResourceManagementMode)::Automatic ) ), _usage( usage ), _size( sizeInBytes ), _pool( NS(ResourceManagementMode)::Automatic ) {
		}

//		SIMPLE_PROPERTY_GET(SurfaceFormat,_desc->_format,Format)
		SIMPLE_PROPERTY_GET(NS(ResourceUsage),_usage,ResourceUsage)
		SIMPLE_PROPERTY_GET(NS(ResourceManagementMode),_pool,ResourceManagementMode)
		SIMPLE_PROPERTY_GET(int,_size,SizeInBytes)

		generic <typename T> where T : value class
		void SetData( int offsetInBytes, array<T>^ data, int startIndex, int elementCount, SetDataOptions options );
	};
NS_CLOSE
