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

#include "RenderTarget.h"
#include <vcclr.h>

NS_OPEN
#if 0
	generic <typename T> where T : value class
	void Surface::WriteData( Nullable<Fluggo::Primitives::Rectangle> rectangle, array<T>^ data, unsigned int startIndex, unsigned int count, WriteOptions options ) {
		using Fluggo::Primitives::Rectangle;
		
		if( data == nullptr )
			throw gcnew ArgumentNullException( "data" );
			
		if( startIndex >= (unsigned int) data->Length )
			throw gcnew ArgumentOutOfRangeException( "startIndex" );
			
		if( (startIndex + count) > (unsigned int) data->Length )
			throw gcnew ArgumentOutOfRangeException( "count" );
			
/*		if( !Direct3D::IsSimpleValueType( T::typeid ) )
			throw gcnew NotSupportedException( "The given value type must not contain references to the garbage-collected heap." );*/
			
		if( sizeof(T) != (unsigned int) Utility::GetBytesPerElement( _desc->_format ) )
			throw gcnew ArgumentException( "The given value type is not the right size for the format of the surface." );
			
		if( _desc->_pool == Fluggo::Graphics::Direct3D::Pool::Default && (_desc->_usage & ImageUsage::Dynamic) != ImageUsage::Dynamic )
			throw gcnew InvalidOperationException( "Surfaces in the default pool without the ImageUsage.Dynamic flag cannot be written to using this method." );

		pin_ptr<Rectangle> prect = nullptr;
		Rectangle rect;
		unsigned int lineWidth;
			
		if( rectangle.HasValue ) {
			rect = rectangle.Value;
			lineWidth = (unsigned int)(rect.Right - rect.Left);

			if( rect.Top >= rect.Bottom || rect.Left >= rect.Right ) {
				throw gcnew ArgumentException( "The given rectangle is degenerate.", "rectangle" );
			}

			if( rect.Top < 0 || rect.Left < 0 || (unsigned int) rect.Bottom > _desc->_height || (unsigned int) rect.Right > _desc->_width ) {
				throw gcnew ArgumentException( "The given rectangle extends beyond the boundaries of the surface.", "rectangle" ); 
			}
			
			if( count > ((unsigned int)(rect.Bottom - rect.Top) * lineWidth) )
				throw gcnew ArgumentOutOfRangeException( "count" );

			prect = &rect;
		}
		else {
			lineWidth = _desc->_width;
			
			if( count > (_desc->_width * _desc->_height) )
				throw gcnew ArgumentOutOfRangeException( "count" );
		}
			
		DWORD flags = 0;
		
		if( (options & WriteOptions::Discard) == WriteOptions::Discard ) {
			if( (_desc->_usage & ImageUsage::Dynamic) != ImageUsage::Dynamic )
				throw gcnew ArgumentException( "The WriteOptions.Discard flag can only be used on dynamic surfaces or textures." );
				
			if( rectangle.HasValue )
				throw gcnew ArgumentException( "Cannot use WriteOptions.Discard with a rectangle.", "options" );
		
			flags |= D3DLOCK_DISCARD;
		}
			
		D3DLOCKED_RECT lockedRect;
		Utility::CheckResult( Ptr->LockRect( &lockedRect, (RECT*) prect, flags ) );
		
		void *target = lockedRect.pBits;
		unsigned int copied = 0;
		
		while( copied < count ) { 
			pin_ptr<T> ptr = &data[startIndex + copied];
			
			unsigned int copyLength = min(lineWidth,count - copied);
			memcpy( target, ptr, copyLength * sizeof(T) );
			
			copied += copyLength;
			target = (void*)(((unsigned char *)target) + lockedRect.Pitch);
		}
	
		Utility::CheckResult( Ptr->UnlockRect() );
	}
	
	ImageFileInfo Surface::FillFromFile( array<ColorRGBA>^ targetPalette, Nullable<Fluggo::Primitives::Rectangle> targetRect, String^ sourceFile,
			Nullable<Fluggo::Primitives::Rectangle> sourceRect, FilterOptions filter, ColorARGB colorKey ) {
		if( sourceFile == nullptr )
			throw gcnew ArgumentNullException( "sourceFile" );
		
		Fluggo::Primitives::Rectangle lsourceRect, ltargetRect;
		
		pin_ptr<ColorRGBA> ppalette = (targetPalette != nullptr) ? &targetPalette[0] : nullptr;
		pin_ptr<Fluggo::Primitives::Rectangle> psourceRect = nullptr;
		pin_ptr<Fluggo::Primitives::Rectangle> ptargetRect = nullptr;
		pin_ptr<const Char> psourceFile = PtrToStringChars( sourceFile );
		
		if( sourceRect.HasValue ) {
			lsourceRect = sourceRect.Value;
			psourceRect = &lsourceRect;
		}
		
		if( targetRect.HasValue ) {
			ltargetRect = targetRect.Value;
			ptargetRect = &ltargetRect;
		}
		
		ImageFileInfo info;
		pin_ptr<ImageFileInfo> pinfo = &info;
		
		Utility::CheckResult( ::D3DXLoadSurfaceFromFileW( Ptr, (PALETTEENTRY*) ppalette, (RECT*) ptargetRect,
			(wchar_t*) psourceFile, (RECT*) psourceRect, (DWORD) filter, (D3DCOLOR) colorKey.PackedColor,
			(D3DXIMAGE_INFO*) pinfo ) );
			
		return info;
	}
#endif
NS_CLOSE
