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
	public ref class VertexBuffer sealed : public GraphicsResource {
	private:
		int _length;
		ResourceUsage _usage;
		ResourceManagementMode _pool;
		
		static IDirect3DVertexBuffer9 *CreateBuffer( _GD ^device, int sizeInBytes, ResourceUsage usage, ResourceManagementMode resourceManagementMode );
		static IDirect3DVertexBuffer9 *CreateBuffer( _GD ^device, System::Type ^vertexType, int elementCount, ResourceUsage usage, ResourceManagementMode resourceManagementMode );
	
	internal:
		property IDirect3DVertexBuffer9 *Ptr {
			IDirect3DVertexBuffer9 *get() new { return (IDirect3DVertexBuffer9*) GraphicsResource::Ptr; }
		}
		
		VertexBuffer( _GD ^device, IDirect3DVertexBuffer9 *vbuf );
		
	public:
		VertexBuffer( _GD ^device, System::Type ^vertexType, int elementCount, ResourceUsage usage, ResourceManagementMode resourceManagementMode )
			: GraphicsResource( device, CreateBuffer( device, vertexType, elementCount, usage, resourceManagementMode ) ), _usage( usage ), _pool( resourceManagementMode ) {
			_length = Marshal::SizeOf( vertexType ) * elementCount;
		}

		VertexBuffer( _GD ^device, System::Type ^vertexType, int elementCount, ResourceUsage usage )
			: GraphicsResource( device, CreateBuffer( device, vertexType, elementCount, usage, ResourceManagementMode::Automatic ) ), _usage( usage ), _pool( ResourceManagementMode::Automatic ) {
			_length = Marshal::SizeOf( vertexType ) * elementCount;
		}
		
		VertexBuffer( _GD ^device, int sizeInBytes, ResourceUsage usage, ResourceManagementMode resourceManagementMode )
			: GraphicsResource( device, CreateBuffer( device, sizeInBytes, usage, resourceManagementMode ) ), _usage( usage ), _length( sizeInBytes ), _pool( resourceManagementMode ) {
		}
	
		VertexBuffer( _GD ^device, int sizeInBytes, ResourceUsage usage )
			: GraphicsResource( device, CreateBuffer( device, sizeInBytes, usage, ResourceManagementMode::Automatic ) ), _usage( usage ), _length( sizeInBytes ), _pool( ResourceManagementMode::Automatic ) {
		}

		generic <typename T> where T : value class
		void SetData( int offsetInBytes, array<T>^ data, int startIndex, int elementCount, SetDataOptions options );
		
		property unsigned int Length {
			unsigned int get() { return _length; }
		}
		
		static unsigned int GetStride( System::Type^ type ) {
			if( !Utility::IsSimpleValueType( type ) )
				throw gcnew ArgumentException( "Type is not a simple value type.", "type" );
				
			return (unsigned int) _Marshal::SizeOf( type );
		}
		
		generic <typename T> where T : value class
		static unsigned int GetStride() {
			if( !Utility::IsSimpleValueType( T::typeid ) )
				throw gcnew NotSupportedException( "Type is not a simple value type." );
				
			return sizeof(T);
		}
	};
NS_CLOSE
