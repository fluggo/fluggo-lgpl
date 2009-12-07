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

using namespace System::Collections::ObjectModel;

NS_OPEN
	/// <summary>
	/// Represents a strongly-typed weak reference.
	/// </summary>
	/// <typeparam name="T">Type of the object referenced.</typeparam>
	generic <typename T> where T : ref class
	ref class WeakReference {
	private:
		System::WeakReference ^_ref;

	public:
		/// <summary>
		/// Creates a new instance of the <see cref='WeakReference{T}'/> class.
		/// </summary>
		/// <param name='value'>Target of the weak reference.</param>
		/// <exception cref='ArgumentNullException'><paramref name='value'/> is <see langword='null'/>.</exception>
		WeakReference( T value ) {
			if( value == nullptr )
				throw gcnew ArgumentNullException( "value" );

			_ref = gcnew System::WeakReference( value, false );
		}

		/// <summary>
		/// Gets or sets the target of the reference.
		/// </summary>
		/// <value>The target of the reference, or <see langword='null'/> if the target has already been garbage-collected.</value>
		/// <exception cref='ArgumentNullException'>The property is set to <see langword='null'/>.</exception>
		property T Target {
			T get() {
				return safe_cast<T>(_ref->Target);
			}
			void set( T value ) {
				if( value == nullptr )
					throw gcnew ArgumentNullException( "value" );

				_ref->Target = value;
			}
		}

		/// <summary>
		/// Gets a value that represents whether the target is alive.
		/// </summary>
		/// <value>True if the target is alive, or false if it has been garbage-collected.</value>
		/// <remarks>Do not use this property to determine whether you can retrieve the target object. The target
		///   could be garbage-collected between the time you call this property and the time you call <see cref="Target"/>.
		///   Get the value of <see cref="Target"/> and test it against <see langword='null'/> instead.</remarks>
		property bool IsAlive {
			bool get() {
				return _ref->IsAlive;
			}
		}
	};
	
	public ref class GraphicsAdapter {
	private:
		static WeakReference<Direct3D^> ^_d3dRef;
		static Object ^_d3dLock = gcnew Object();
		String ^_driver, ^_description, ^_deviceName;
		long _driverVersion;
		int _vendorID, _deviceID, _subSysID, _revision;
		Guid _deviceIdentifier;
		DateTime _whqlCertificationDate;
		Direct3D ^_d3d;
		unsigned int _adapter;
		bool _certified;
		
		//AdapterIdentifier( IDirect3D9 *d3d, unsigned int adapter, bool getWhql );
	
		static property Direct3D ^D3D {
			Direct3D ^get();
		}
		
		GraphicsAdapter( Direct3D ^d3d, unsigned int adapter );
		
	internal:
		IDirect3DDevice9 *CreateDevice( DeviceType deviceType, IntPtr focusWindow, CreateOptions behaviorFlags, PresentationParameters ^presentationParameters );
		
	public:
		static property ReadOnlyCollection<GraphicsAdapter^>^ Adapters {
			ReadOnlyCollection<GraphicsAdapter^>^ get();
		}
		
		GraphicsDeviceCapabilities ^GetCapabilities( DeviceType deviceType );

		SIMPLE_PROPERTY_GET(String^,_description,Description)
		SIMPLE_PROPERTY_GET(int,_deviceID,DeviceId)
		SIMPLE_PROPERTY_GET(int,_vendorID,VendorId)
		SIMPLE_PROPERTY_GET(int,_subSysID,SubSystemId)
		SIMPLE_PROPERTY_GET(int,_revision,Revision)
		SIMPLE_PROPERTY_GET(Guid,_deviceIdentifier,DeviceIdentifier)
		SIMPLE_PROPERTY_GET(String^,_deviceName,DeviceName)
		SIMPLE_PROPERTY_GET(String^,_driver,DriverDll)
	};
NS_CLOSE