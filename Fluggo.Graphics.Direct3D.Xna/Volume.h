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
	public ref class Volume {
	internal:
		Fluggo::Interop::Direct3D9::Device^ _device;
		IDirect3DVolume9 *_volume;
		
		Volume( Fluggo::Interop::Direct3D9::Device^ device, IDirect3DVolume9 *volume ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			if( volume == NULL )
				throw gcnew ArgumentNullException( "volume" );
				
			_device = device;
			_volume = volume;
		}
		
		~Volume() {
			this->!Volume();
		}
		!Volume() {
			if( _volume != NULL ) {
				_volume->Release();
				_volume = NULL;
			}
		}
		
	public:
		property Fluggo::Interop::Direct3D9::Device^ Device {
			Fluggo::Interop::Direct3D9::Device^ get() {
				return _device;
			}
		}
	};
NS_CLOSE
