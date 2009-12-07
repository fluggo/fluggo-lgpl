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

#include "GraphicsResource.h"

NS_OPEN
	/*********************************************
		Resource
	*/
	void GraphicsResource::FreePrivateData( Guid guid ) {
		pin_ptr<GUID> pguid = (interior_ptr<GUID>) &guid;
		Utility::CheckResult( Ptr->FreePrivateData( *pguid ) );
	}
	
	void GraphicsResource::Priority::set( int value ) {
		Ptr->SetPriority( value );
	}

	int GraphicsResource::Priority::get() {
		return Ptr->GetPriority();
	}
	
	ResourceType GraphicsResource::ResourceType::get() {
		return (NS(ResourceType)) Ptr->GetType();
	}
	
	void GraphicsResource::PreLoad() {
		Ptr->PreLoad();
	}
NS_CLOSE