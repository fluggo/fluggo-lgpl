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

#define INITGUID
#include "GraphicsAdapter.h"
#include "GraphicsDeviceCapabilities.h"
#include "GraphicsDevice.h"

NS_OPEN
	// IDirect3D9 holder class
	ref class Direct3D sealed {
	private:
		IDirect3D9 *_d3d9;
		
	public:
		property IDirect3D9 *Ptr {
			IDirect3D9 *get() {
				if( _d3d9 == NULL )
					throw gcnew ObjectDisposedException( "nullptr" );
				
				return _d3d9;
			}
		}
	
		Direct3D() {
			_d3d9 = Direct3DCreate9( D3D_SDK_VERSION );
			
			if( _d3d9 == NULL ) {
				throw gcnew Exception( "Could not create a Direct3D object." );
			}
		}
		
		~Direct3D() { this->!Direct3D(); }
		!Direct3D() {
			if( _d3d9 != NULL ) {
				_d3d9->Release();
				_d3d9 = NULL;
			}
		}
	};

	/****************************************************
		Helper class implementation
	*/

	int Utility::GetBytesPerElement( SurfaceFormat format ) {
		switch( format ) {
			default:
				throw gcnew NotSupportedException( "The given format is either unrecognized, unimplemented, or not supported." );

			case SurfaceFormat::Alpha8:
			case SurfaceFormat::Bgr233:
			case SurfaceFormat::Palette8:
			case SurfaceFormat::Luminance8:
			case SurfaceFormat::LuminanceAlpha8:
				return 1;
			
			case SurfaceFormat::Bgr24:
				return 3;
			
			case SurfaceFormat::Bgr32:
			case SurfaceFormat::Bgra1010102:
			case SurfaceFormat::Color:
			case SurfaceFormat::Depth24:
			case SurfaceFormat::Depth24Stencil4:
			case SurfaceFormat::Depth24Stencil8:
			case SurfaceFormat::Depth24Stencil8Single:
			case SurfaceFormat::Depth32:
//			case SurfaceFormat::Depth32SingleLockable:
			case SurfaceFormat::Rgba32:
			case SurfaceFormat::Rgb32:
			case SurfaceFormat::Rg32:
			case SurfaceFormat::Rgba1010102:
			case SurfaceFormat::NormalizedLuminance32:
			case SurfaceFormat::NormalizedByte4:
			case SurfaceFormat::NormalizedShort2:
			case SurfaceFormat::NormalizedAlpha1010102:
			case SurfaceFormat::Single:
			case SurfaceFormat::HalfVector2:
				return 4;
				
			case SurfaceFormat::Bgr444:
			case SurfaceFormat::Bgr555:
			case SurfaceFormat::Bgr565:
			case SurfaceFormat::Bgra2338:
			case SurfaceFormat::Bgra5551:
			case SurfaceFormat::Bgra4444:
			case SurfaceFormat::Depth15Stencil1:
			case SurfaceFormat::Depth16:
//			case SurfaceFormat::Depth16Lockable:
			case SurfaceFormat::PaletteAlpha16:
			case SurfaceFormat::LuminanceAlpha16:
			case SurfaceFormat::Luminance16:
			case SurfaceFormat::NormalizedByte2:
			case SurfaceFormat::NormalizedByte2Computed:
			case SurfaceFormat::NormalizedLuminance16:
			case SurfaceFormat::VideoRgBg:
//			case SurfaceFormat::VideoYuYv:
//			case SurfaceFormat::VideoUyVy:
			case SurfaceFormat::VideoGrGb:
			case SurfaceFormat::HalfSingle:
				return 2;
			
			case SurfaceFormat::Rgba64:
			case SurfaceFormat::NormalizedShort4:
			case SurfaceFormat::Vector2:
			case SurfaceFormat::HalfVector4:
				return 8;
			
/*			case SurfaceFormat::Dxt1:
			case SurfaceFormat::Dxt2:
			case SurfaceFormat::Dxt3:
			case SurfaceFormat::Dxt4:
			case SurfaceFormat::Dxt5:
			case SurfaceFormat::Multi2Brga32:*/
			case SurfaceFormat::Vector4:
				return 16;
		}
	}

	void Utility::CheckResult( HRESULT result ) {
		if( result == D3DERR_INVALIDCALL )
			throw gcnew ArgumentException();
		
		_Marshal::ThrowExceptionForHR( (int) result );
	}
	
	bool Utility::IsSimpleValueType( Type^ type ) {
		if( type == nullptr )
			throw gcnew ArgumentNullException( "type" );
			
		if( !type->IsValueType )
			return false;
		
		if( type->IsPrimitive )
			return true;
			
		if( type->IsEnum )
			return true;
	
		// Can cache these results
		for each( System::Reflection::FieldInfo^ field in type->GetFields( System::Reflection::BindingFlags::Instance | System::Reflection::BindingFlags::Public ) ) {
			if( !IsSimpleValueType( field->FieldType ) )
				return false;
		}
		
		return true;
	}

	/****************************************************
		GraphicsAdapter implementation
	*/

/*	unsigned int Direct3D::GetAdapterCount() {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		return _d3d9->GetAdapterCount();
	}*/
		
/*	AdapterIdentifier ^Direct3D::GetAdapterIdentifier( unsigned int adapter, bool getWhqlLevel ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
			
		return gcnew AdapterIdentifier( _d3d9, adapter, getWhqlLevel );
	}*/
		
	Direct3D ^GraphicsAdapter::D3D::get() {
		System::Threading::Monitor::Enter( _d3dLock );
		
		try {
			NS(Direct3D) ^d3d = (_d3dRef == nullptr) ? nullptr : _d3dRef->Target;
			
			if( d3d == nullptr ) {
				d3d = gcnew NS(Direct3D)();
				_d3dRef = gcnew WeakReference<NS(Direct3D)^>( d3d );
			}
			
			return d3d;
		}
		finally {
			System::Threading::Monitor::Exit( _d3dLock );
		}
	}
	
	GraphicsAdapter::GraphicsAdapter(Direct3D ^d3d, unsigned int adapter) {
		if( d3d == nullptr )
			throw gcnew ArgumentNullException( "d3d" );
			
		D3DADAPTER_IDENTIFIER9 id;
		Utility::CheckResult( d3d->Ptr->GetAdapterIdentifier( adapter, 0, &id ) );
		
		_driver = Marshal::PtrToStringAnsi( (IntPtr)(void*) id.Driver, MAX_DEVICE_IDENTIFIER_STRING );
		_description = Marshal::PtrToStringAnsi( (IntPtr)(void*) id.Description, MAX_DEVICE_IDENTIFIER_STRING );
		_deviceName = Marshal::PtrToStringAnsi( (IntPtr)(void*) id.DeviceName, 32 );
		_driverVersion = (long) id.DriverVersion.QuadPart;
		_vendorID = id.VendorId;
		_deviceID = id.DeviceId;
		_subSysID = id.SubSysId;
		_revision = id.Revision;
		_deviceIdentifier = Guid( id.DeviceIdentifier.Data1, id.DeviceIdentifier.Data2, id.DeviceIdentifier.Data3,
			id.DeviceIdentifier.Data4[0], id.DeviceIdentifier.Data4[1], id.DeviceIdentifier.Data4[2],
			id.DeviceIdentifier.Data4[3], id.DeviceIdentifier.Data4[4], id.DeviceIdentifier.Data4[5],
			id.DeviceIdentifier.Data4[6], id.DeviceIdentifier.Data4[7] );
		_d3d = d3d;
		_adapter = adapter;
		
#if 0
		if( id.WHQLLevel == 0 ) {
			_certified = false;
			_whqlCertificationDate = DateTime::MinValue;
		}
		else if( id.WHQLLevel == 1 ) {
			_certified = true;
			_whqlCertificationDate = DateTime::MinValue;
		}
		else {
			_certified = true;
			_whqlCertificationDate = DateTime( id.WHQLLevel >> 16, (id.WHQLLevel >> 8) & 0xFF, id.WHQLLevel & 0xFF );
		}
#endif
	}
	
	ReadOnlyCollection<GraphicsAdapter^> ^GraphicsAdapter::Adapters::get() {
		Direct3D ^d3d = GraphicsAdapter::D3D;
		array<GraphicsAdapter^>^ adapters = gcnew array<GraphicsAdapter^>( d3d->Ptr->GetAdapterCount() );
		
		for( int i = 0; i < adapters->Length; i++ )
			adapters[i] = gcnew GraphicsAdapter( d3d, (unsigned int) i );
			
		return Array::AsReadOnly<GraphicsAdapter^>( adapters );
	}
	
	GraphicsDeviceCapabilities ^GraphicsAdapter::GetCapabilities(NS(DeviceType) deviceType) {
		D3DCAPS9 caps;
		pin_ptr<D3DCAPS9> pcaps = &caps;
		
		_d3d->Ptr->GetDeviceCaps( _adapter, (D3DDEVTYPE) deviceType, pcaps );
		return gcnew GraphicsDeviceCapabilities( pcaps );
	}

	IDirect3DDevice9 *GraphicsAdapter::CreateDevice( DeviceType deviceType, IntPtr focusWindow, CreateOptions behaviorFlags, PresentationParameters ^presentationParameters ) {
		IDirect3DDevice9 *device;
		unsigned int flags = (unsigned int) behaviorFlags;
		
		if( (behaviorFlags & CreateOptions::SingleThreaded) == CreateOptions::SingleThreaded )
			flags &= ~((unsigned int) CreateOptions::SingleThreaded);
		else
			flags |= D3DCREATE_MULTITHREADED;
			
		pin_ptr<D3DPRESENT_PARAMETERS> pp = presentationParameters->Ptr;
		
		Utility::CheckResult( _d3d->Ptr->CreateDevice( _adapter, (D3DDEVTYPE) deviceType, (HWND)(void*) focusWindow, flags, pp, &device ) );
		return device;
	}

/*	void Direct3D::RegisterSoftwareDevice( IntPtr initializeFunction ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		CheckResult( _d3d9->RegisterSoftwareDevice( (void *) initializeFunction ) );
	}
		
	unsigned int Direct3D::GetAdapterModeCount( unsigned int adapter, Format format ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		return _d3d9->GetAdapterModeCount( adapter, (D3DFORMAT) format );
	}
	
	DisplayMode Direct3D::EnumAdapterModes( unsigned int adapter, Format format, unsigned int mode ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		DisplayMode result;
		
		CheckResult( _d3d9->EnumAdapterModes( adapter, (D3DFORMAT) format, mode, (D3DDISPLAYMODE*) &result ) );
		
		return result;
	}
	
	DisplayMode Direct3D::GetAdapterDisplayMode( unsigned int adapter ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		DisplayMode result;
		CheckResult( _d3d9->GetAdapterDisplayMode( adapter, (D3DDISPLAYMODE*) &result ) );
		
		return result;
	}

	bool Direct3D::CheckDeviceType( unsigned int adapter, DeviceType deviceType, Format displayFormat, Format backBufferFormat, bool windowed ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		HRESULT result = _d3d9->CheckDeviceType( adapter, (D3DDEVTYPE) deviceType, (D3DFORMAT) displayFormat, (D3DFORMAT) backBufferFormat, windowed );
		
		if( result == D3DERR_NOTAVAILABLE )
			return false;
		
		CheckResult( result );
		return true;
	}
	
	bool Direct3D::CheckDeviceFormat( unsigned int adapter, DeviceType deviceType, Format adapterFormat, unsigned int usage, ResourceType resourceType, Format checkFormat ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		HRESULT result = _d3d9->CheckDeviceFormat( adapter, (D3DDEVTYPE) deviceType, (D3DFORMAT) adapterFormat, usage,
			(D3DRESOURCETYPE) resourceType, (D3DFORMAT) checkFormat );
		
		if( result == D3DERR_NOTAVAILABLE )
			return false;
		
		CheckResult( result );
		return true;
	}
	
	bool Direct3D::CheckDeviceMultiSampleType( unsigned int adapter, DeviceType deviceType, Format surfaceFormat, bool windowed, MultiSampleType type, [Out] unsigned int %qualityLevels ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		pin_ptr<unsigned int> pQualityLevels = &qualityLevels;
		
		HRESULT result = _d3d9->CheckDeviceMultiSampleType( adapter, (D3DDEVTYPE) deviceType, (D3DFORMAT) surfaceFormat, windowed,
			(D3DMULTISAMPLE_TYPE) type, (DWORD *) pQualityLevels );
		
		if( result == D3DERR_NOTAVAILABLE )
			return false;
		
		CheckResult( result );
		return true;
	}
		
	bool Direct3D::CheckDepthStencilMatch( unsigned int adapter, DeviceType deviceType, Format adapterFormat, Format renderTargetFormat, Format depthStencilFormat ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		HRESULT result = _d3d9->CheckDepthStencilMatch( adapter, (D3DDEVTYPE) deviceType, (D3DFORMAT) adapterFormat,
			(D3DFORMAT) renderTargetFormat, (D3DFORMAT) depthStencilFormat );
		
		if( result == D3DERR_NOTAVAILABLE )
			return false;
		
		CheckResult( result );
		return true;
	}
	
	bool Direct3D::CheckDeviceFormatConversion( unsigned int adapter, DeviceType deviceType, Format sourceFormat, Format targetFormat ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		HRESULT result = _d3d9->CheckDeviceFormatConversion( adapter, (D3DDEVTYPE) deviceType, (D3DFORMAT) sourceFormat,
			(D3DFORMAT) targetFormat );
		
		if( result == D3DERR_NOTAVAILABLE )
			return false;
		
		CheckResult( result );
		return true;
	}
	
	DeviceCaps^ Direct3D::GetDeviceCaps( unsigned int adapter, DeviceType deviceType ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		DeviceCaps^ result = gcnew DeviceCaps();
		pin_ptr<D3DCAPS9> pResult = pC(*result);
		
		CheckResult( _d3d9->GetDeviceCaps( adapter, (D3DDEVTYPE) deviceType, pResult ) );
		
		return result;
	}
	
	IntPtr Direct3D::GetAdapterMonitor( unsigned int adapter ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		return (IntPtr)(void*) _d3d9->GetAdapterMonitor( adapter );
	}
	
	GraphicsDevice^ Direct3D::CreateDevice( unsigned int adapter, DeviceType deviceType, IntPtr focusWindow, CreateFlags behaviorFlags, PresentParameters %presentationParameters ) {
		if( _d3d9 == NULL )
			throw gcnew ObjectDisposedException( nullptr );
		
		if( (behaviorFlags & CreateFlags::HardwareVertexProcessing) != CreateFlags::HardwareVertexProcessing &&
				(behaviorFlags & CreateFlags::SoftwareVertexProcessing) != CreateFlags::SoftwareVertexProcessing &&
				(behaviorFlags & CreateFlags::MixedVertexProcessing) != CreateFlags::MixedVertexProcessing ) {
			throw gcnew ArgumentException( "Must include either the CreateFlags.HardwareVertexProcessing, CreateFlags.SoftwareVertexProcessing, or CreateFlags.MixedVertexProcessing flags.", "behaviorFlags" );
		}
		
		if( !presentationParameters.Windowed ) {
			if( presentationParameters.BackBufferFormat == Format::Unknown )
				throw gcnew ArgumentException( "In full-screen mode, you must specify a valid value for BackBufferFormat.", "presentationParameters" );
				
			if( presentationParameters.BackBufferWidth == 0 || presentationParameters.BackBufferHeight == 0 )
				throw gcnew ArgumentException( "In full-screen mode, you must specify the dimensions of the back buffer.", "presentationParameters" );
		}
		
		IDirect3DDevice9 *device;
		pin_ptr<D3DPRESENT_PARAMETERS> pp = pPP( presentationParameters );
		
		CheckResult( _d3d9->CreateDevice( adapter, (D3DDEVTYPE) deviceType, (HWND)(void*) focusWindow, (unsigned int) behaviorFlags, pp, &device ) );
		
		return gcnew GraphicsDevice( device );
	}*/
	
NS_CLOSE
