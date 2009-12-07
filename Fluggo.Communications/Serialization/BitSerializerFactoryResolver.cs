/*
	Fluggo Communications Library
	Copyright (C) 2005-6  Brian J. Crowell

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

using System;
using System.Collections.Generic;

namespace Fluggo.Communications.Serialization {
	/// <summary>
	/// Provides factories for classes that handle requests using the <see cref="BitSerializer"/> class.
	/// </summary>
	public class BitSerializerFactoryResolver : IRequestFactoryProvider {
		BitSerializer _ser;
		Dictionary<Guid, Type> _types = new Dictionary<Guid,Type>();

		/// <summary>
		/// Creates a new instance of the <see cref='BitSerializerFactoryResolver'/> class.
		/// </summary>
		/// <param name="serializer"><see cref="BitSerializer"/> to be used to generate request factories.</param>
		/// <exception cref='ArgumentNullException'><paramref name='serializer'/> is <see langword='null'/>.</exception>
		public BitSerializerFactoryResolver( BitSerializer serializer ) {
			if( serializer == null )
				throw new ArgumentNullException( "serializer" );
				
			_ser = serializer;
		}
		
		/// <summary>
		/// Adds an interface to the resolver type map.
		/// </summary>
		/// <param name="interfaceType">Type of the interface for which to handle requests.</param>
		/// <exception cref='ArgumentNullException'><paramref name='interfaceType'/> is <see langword='null'/>.</exception>
		/// <exception cref='ArgumentException'><paramref name='interfaceType'/> does not represent an interface.</exception>
		/// <remarks>The <see cref="BitSerializerFactoryResolver"/> only creates factories for interfaces
		///   that have been added using this method. Once the interface has been added, you can retrieve
		///   a factory for sending or receiving requests using the <see cref="GetRequestSenderFactory"/>
		///   and <see cref="GetRequestReceiverFactory"/> methods respectively. These methods accept the interface's
		///   <see cref="Guid"/>, which can be retrieved from the type's <see cref="Type.GUID"/> property.</remarks>
		public void AddInterface( Type interfaceType ) {
			if( interfaceType == null )
				throw new ArgumentNullException( "interfaceType" );

			if( !interfaceType.IsInterface )
				throw new ArgumentException( "The given type is not an interface.", "interfaceType" );
				
			_types.Add( interfaceType.GUID, interfaceType );
		}
		
		public void RemoveInterface( Type interfaceType ) {
			if( interfaceType == null )
				throw new ArgumentNullException( "interfaceType" );

			if( !interfaceType.IsInterface )
				throw new ArgumentException( "The given type is not an interface.", "interfaceType" );

			_types.Remove( interfaceType.GUID );
		}
		
		public void RemoveInterface( Guid iid ) {
			_types.Remove( iid );
		}
		
/*		public void Register( RequestChannel channel ) {
			channel.RequestSenderResolve += delegate( object sender, ResolveRequestHandlerEventArgs e ) {
				Type ifType;
				
				if( !_types.TryGetValue( e.InterfaceID, out ifType ) )
					return null;
					
				return _ser.GenerateRequestSenderFactory( ifType );
			};
			channel.RequestReceiverResolve += delegate( object sender, ResolveRequestHandlerEventArgs e ) {
				Type ifType;
				
				if( !_types.TryGetValue( e.InterfaceID, out ifType ) )
					return null;
					
				return _ser.GenerateRequestReceiverFactory( ifType );
			};
		}*/

		public RequestReceiverFactory GetRequestReceiverFactory( Guid iid ) {
			Type ifType;
			
			if( !_types.TryGetValue( iid, out ifType ) )
				return null;
			
			return _ser.GenerateRequestReceiverFactory( ifType );
		}

		public RequestSenderFactory GetRequestSenderFactory( Guid iid ) {
			Type ifType;

			if( !_types.TryGetValue( iid, out ifType ) )
				return null;

			return _ser.GenerateRequestSenderFactory( ifType );
		}
	}
}