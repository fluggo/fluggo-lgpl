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

#include "Texture.h"
#include "GraphicsDevice.h"

using Fluggo::Graphics::Rectangle;

typedef NS(ResourceManagementMode) _RMM;
typedef NS(ResourceUsage) _RU;

NS_OPEN
	/*********************************************
		Texture
	*/
	
	unsigned int Texture::LevelOfDetail::get() {
		return Ptr->GetLOD();
	}
	
	void Texture::LevelOfDetail::set( unsigned int value ) {
		Ptr->SetLOD( value );
	}
	
	void Texture::AutoGenFilterType::set( TextureFilterType value ) {
		Utility::CheckResult( Ptr->SetAutoGenFilterType( (D3DTEXTUREFILTERTYPE) value ) );
	}
	
	TextureFilterType Texture::AutoGenFilterType::get() {
		return (TextureFilterType) Ptr->GetAutoGenFilterType();
	}
	
	unsigned int Texture::LevelCount::get() {
		return Ptr->GetLevelCount();
	}
	
	/*******************************************
		Texture2D
	*/
	
	IDirect3DTexture9 *Texture2D::CreateTexture( _GD^ graphicsDevice, int width, int height, int numberLevels,
			NS(ResourceUsage) usage, SurfaceFormat format, NS(ResourceManagementMode) resourceManagementMode ) {
		
		if( resourceManagementMode == NS(ResourceManagementMode)::Automatic && ((usage & NS(ResourceUsage)::Dynamic) == _RU::Dynamic) ) {
			throw gcnew ArgumentException( "Textures with the automatic resource management mode cannot be dynamic." );
		}
		
		D3DPOOL pool = (resourceManagementMode == NS(ResourceManagementMode)::Automatic) ? D3DPOOL_MANAGED : D3DPOOL_DEFAULT;
		
		IDirect3DTexture9 *tex;
		Utility::CheckResult( graphicsDevice->Ptr->CreateTexture( width, height, numberLevels, (DWORD) usage, (D3DFORMAT) format, pool, &tex, NULL ) );

		return tex;
	}
	
	void Texture2D::PrepareDesc() {
		D3DSURFACE_DESC desc;
		Utility::CheckResult( Ptr->GetLevelDesc( 0, &desc ) );
		
		_format = (SurfaceFormat) desc.Format;
		_height = (int) desc.Height;
		_width = (int) desc.Width;
		
		_usage = (_RU) desc.Usage;
		_rmm = (desc.Pool == D3DPOOL_MANAGED) ? NS(ResourceManagementMode)::Automatic : NS(ResourceManagementMode)::Manual;
	}

	generic <typename T> where T : value class
	void Texture2D::SetData( int level, Nullable<Rectangle> rectangle, array<T>^ data, int startIndex, int elementCount, SetDataOptions options ) {
		
		if( data == nullptr )
			throw gcnew ArgumentNullException( "data" );
			
		if( startIndex < 0 || startIndex >= data->Length )
			throw gcnew ArgumentOutOfRangeException( "startIndex" );
			
		if( (elementCount < 0) || (startIndex + elementCount) > data->Length )
			throw gcnew ArgumentOutOfRangeException( "elementCount" );
			
/*		if( !Direct3D::IsSimpleValueType( T::typeid ) )
			throw gcnew NotSupportedException( "The given value type must not contain references to the garbage-collected heap." );*/
			
		if( sizeof(T) != (unsigned int) Utility::GetBytesPerElement( _format ) )
			throw gcnew ArgumentException( "The given value type is not the right size for the format of the surface." );
			
		if( _rmm == NS(ResourceManagementMode)::Manual && (_usage & NS(ResourceUsage)::Dynamic) != NS(ResourceUsage)::Dynamic )
			throw gcnew InvalidOperationException( "Textures with manual resource management without the ResourceUsage.Dynamic flag cannot be written to using this method." );

		int width, height;
		
		if( level == 0 ) {
			width = _width;
			height = _height;
		}
		else {
			D3DSURFACE_DESC desc;
			Utility::CheckResult( Ptr->GetLevelDesc( level, &desc ) );
			width = desc.Width;
			height = desc.Height;
		}
		
		pin_ptr<Rectangle> prect = nullptr;
		Rectangle rect;
		int lineWidth;
			
		if( rectangle.HasValue ) {
			rect = rectangle.Value;
			lineWidth = rect.Width;
			
			if( rect.Top >= rect.Bottom || rect.Left >= rect.Right ) {
				throw gcnew ArgumentException( "The given rectangle is degenerate.", "rectangle" );
			}

			if( rect.Top < 0 || rect.Left < 0 || rect.Bottom > height || rect.Right > width ) {
				throw gcnew ArgumentException( "The given rectangle extends beyond the boundaries of the surface.", "rectangle" ); 
			}
			
			if( elementCount > ((rect.Bottom - rect.Top) * lineWidth) )
				throw gcnew ArgumentOutOfRangeException( "elementCount" );

			prect = &rect;
		}
		else {
			lineWidth = _width;
			
			if( elementCount > (width * height) )
				throw gcnew ArgumentOutOfRangeException( "elementCount" );
		}
			
		DWORD flags = 0;
		
		if( (options & SetDataOptions::Discard) == SetDataOptions::Discard ) {
			if( (_usage & NS(ResourceUsage)::Dynamic) != NS(ResourceUsage)::Dynamic )
				throw gcnew ArgumentException( "The SetDataOptions.Discard flag can only be used on dynamic textures." );
				
			if( rectangle.HasValue )
				throw gcnew ArgumentException( "Cannot use SetDataOptions.Discard with a rectangle.", "options" );
		
			flags |= D3DLOCK_DISCARD;
		}
			
		D3DLOCKED_RECT lockedRect;
		Utility::CheckResult( Ptr->LockRect( (unsigned int) level, &lockedRect, (RECT*) prect, flags ) );
		
		void *target = lockedRect.pBits;
		int copied = 0;
		
		while( copied < elementCount ) { 
			pin_ptr<T> ptr = &data[startIndex + copied];
			
			unsigned int copyLength = min(lineWidth,elementCount - copied);
			memcpy( target, ptr, copyLength * sizeof(T) );
			
			copied += copyLength;
			target = (void*)(((unsigned char *)target) + lockedRect.Pitch);
		}
	
		Utility::CheckResult( Ptr->UnlockRect( level ) );
	}
	
	Texture2D ^Texture2D::FromFile( _GD ^graphicsDevice, String ^filename ) {
		if( graphicsDevice == nullptr )
			throw gcnew ArgumentNullException( "graphicsDevice" );
		
		if( filename == nullptr )
			throw gcnew ArgumentNullException( "filename" );
		
		IntPtr stringPtr = Marshal::StringToHGlobalUni( filename );
		IDirect3DTexture9 *texture;
		
		try {
			Utility::CheckResult( D3DXCreateTextureFromFile( graphicsDevice->Ptr, (LPCWSTR)(void*) stringPtr, &texture ) );
		}
		finally {
			Marshal::FreeHGlobal( stringPtr );
		}
		
		return gcnew Texture2D( graphicsDevice, texture );
	}
NS_CLOSE