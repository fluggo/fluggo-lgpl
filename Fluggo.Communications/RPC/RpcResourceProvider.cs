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

using Fluggo.Resources;
using System;
using System.IO;
using System.Collections.Generic;
using Fluggo.Communications.Serialization;
using System.Runtime.InteropServices;

namespace Fluggo.Communications {
	/// <summary>
	/// What is the <see cref="RpcResourceProvider"/>, you may ask? Well, I'll tell you: It's the most POWERFUL
	/// of ALL the communication classes in this library! It does everything, baby.
	/// </summary>
	/// <remarks>A more useful description would be: This class provides much of the intricate wiring necessary to convey
	///   service types and publishing records across an RPC channel.</remarks>
	public class RpcResourceProvider : IResourceProvider, IPublishedServiceEnumeratorService, IDisposable {
		Dictionary<Guid, Type> _knownServiceTypes = new Dictionary<Guid,Type>();
		Set<Guid> _unknownServiceTypes = new Set<Guid>();
		Set<Type> _remoteKnownServices = new Set<Type>();
		Set<Guid> _remoteUnknownServices = new Set<Guid>();
		EventHandler<PublishedServiceEventArgs> _localPublishedHandler, _localRevokedHandler;
		object _remoteServiceLock = new object(), _knownServiceLock = new object();
		IPublishedServiceEnumeratorService _localEnumService;
		IServiceDeclarationService _remoteDeclService;
		MessageChannelOverStream _baseChannel;
		IServiceProvider _localProvider;
		IResourceProvider _localResourceProvider;
		BitSerializer _serializer;
		ChannelMultiplexer _mux;
		RequestChannel _rpc;
		
		// BJC: Here's how I think we can solve the service-declaration problem on sub-paths.
		// Just create a new RpcResourceProvider-ish instance for it. Let it expire on garbage-collection.
		
	#region FactoryProvider
		class FactoryProvider : IRequestFactoryProvider {
			RpcResourceProvider _owner;
			
			public FactoryProvider( RpcResourceProvider owner ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );
					
				_owner = owner;
			}
		
			public RequestReceiverFactory GetRequestReceiverFactory( Guid iid ) {
				Type ifType;
				
				if( !_owner._knownServiceTypes.TryGetValue( iid, out ifType ) )
					return null;
					
				return _owner._serializer.GenerateRequestReceiverFactory( ifType );
			}

			public RequestSenderFactory GetRequestSenderFactory( Guid iid ) {
				Type ifType;

				if( !_owner._knownServiceTypes.TryGetValue( iid, out ifType ) )
					return null;

				return _owner._serializer.GenerateRequestSenderFactory( ifType );
			}
		}
	#endregion
	
	#region ServiceDeclarationService
		class ServiceDeclarationService : IServiceDeclarationService {
			RpcResourceProvider _owner;
			
			public ServiceDeclarationService( RpcResourceProvider owner ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );
					
				_owner = owner;
			}
		
			public void DeclareServices( Guid[] serviceIIDList ) {
				if( serviceIIDList == null )
					throw new ArgumentNullException( "serviceIIDList" );
				
				lock( _owner._remoteServiceLock ) {
					foreach( Guid iid in serviceIIDList ) {
						Type serviceType;
						
						if( _owner._knownServiceTypes.TryGetValue( iid, out serviceType ) )
							_owner._remoteKnownServices[serviceType] = true;
						else
							_owner._remoteUnknownServices[iid] = true;
					}
				}
			}

			public void RemoveServices( Guid[] serviceIIDList ) {
				if( serviceIIDList == null )
					throw new ArgumentNullException( "serviceIIDList" );
					
				lock( _owner._remoteServiceLock ) {
					foreach( Guid iid in serviceIIDList ) {
						Type serviceType;

						if( _owner._knownServiceTypes.TryGetValue( iid, out serviceType ) )
							_owner._remoteKnownServices[serviceType] = false;
						else
							_owner._remoteUnknownServices[iid] = false;
					}
				}
			}
		}
	#endregion
	
	#region LocalServiceProvider
		class LocalServiceProvider : IServiceProvider {
			RpcResourceProvider _owner;
			IServiceProvider _root;
			
			public LocalServiceProvider( RpcResourceProvider owner, IServiceProvider root ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );

				if( root == null )
					throw new ArgumentNullException( "root" );

				_owner = owner;
				_root = root;
			}
			
			public object GetService( Type serviceType ) {
				if( serviceType == typeof(IServiceDeclarationService) )
					return new ServiceDeclarationService( _owner );
				
				return _root.GetService( serviceType );
			}
		}
	#endregion
	
	#region RemoteServiceProvider
		class RemoteServiceProvider : IServiceProvider {
			RpcResourceProvider _owner;
			string _path;
			
			public RemoteServiceProvider( RpcResourceProvider owner, string path ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );

				if( path == null )
					throw new ArgumentNullException( "path" );

				_owner = owner;
				_path = path;
			}
			
			public object GetService( Type serviceType ) {
				return _owner.GetService( serviceType, _path );
			}
		}
	#endregion
		
		public RpcResourceProvider( BitSerializer serializer, Stream stream, IServiceProvider localProvider ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );

			if( serializer == null )
				throw new ArgumentNullException( "serializer" );

			_baseChannel = new MessageChannelOverStream( stream );
			_localProvider = localProvider;
			_serializer = serializer;
			_localPublishedHandler = HandleLocalServiceDeclared;
			_localRevokedHandler = HandleLocalServiceRevoked;
			
			if( _localProvider != null ) {
				_localResourceProvider = (IResourceProvider) _localProvider.GetService( typeof(IResourceProvider) );
				AddKnownServiceTypes( _localProvider );
			}
			
			AddKnownServiceType( typeof(IServiceDeclarationService) );
			
			_mux = new ChannelMultiplexer( _baseChannel, 2, 2, 2048 );
			_rpc = new RequestChannel( new LocalServiceProvider( this, _localProvider ), new FactoryProvider( this ), _mux, 0, 2 );
			
			_remoteDeclService = GetService<IServiceDeclarationService>();
			_localEnumService = (IPublishedServiceEnumeratorService) _localProvider.GetService( typeof( IPublishedServiceEnumeratorService ) );

			if( _localEnumService != null ) {
				_remoteDeclService.DeclareServices(
					Array.ConvertAll<Type, Guid>( _localEnumService.GetPublishedServices(), delegate( Type type ) { return type.GUID; } ) );
				_localEnumService.ServicePublished += _localPublishedHandler;
				_localEnumService.ServiceRevoked += _localRevokedHandler;
			}
		}
		
		private void HandleLocalServiceDeclared( object sender, PublishedServiceEventArgs e ) {
			_remoteDeclService.DeclareServices( new Guid[] { e.ServiceType.GUID } );
		}
		
		private void HandleLocalServiceRevoked( object sender, PublishedServiceEventArgs e ) {
			_remoteDeclService.RemoveServices( new Guid[] { e.ServiceType.GUID } );
		}
		
		private void AddKnownServiceTypes( IServiceProvider provider ) {
			IPublishedServiceEnumeratorService service = (IPublishedServiceEnumeratorService) provider.GetService( typeof(IPublishedServiceEnumeratorService) );
			
			if( service == null )
				return;
				
			foreach( Type type in service.GetPublishedServices() )
				AddKnownServiceType( type );
		}
		
		private void AddKnownServiceType( Type serviceType ) {
			Type existingType;

			lock( _knownServiceLock ) {
				if( !_knownServiceTypes.TryGetValue( serviceType.GUID, out existingType ) ) {
					// We don't know about it
					_unknownServiceTypes[serviceType.GUID] = false;
					_knownServiceTypes[serviceType.GUID] = serviceType;
				}
				else if( serviceType != existingType ) {
					// We know a different type with the same GUID... BAD!!
					throw new Exception( "Both " + serviceType.FullName + " and " + existingType.FullName + " have the same GUID " + serviceType.GUID.ToString() + "." );
				}
			}
			
			lock( _remoteServiceLock ) {
				if( _remoteUnknownServices[serviceType.GUID] ) {
					_remoteUnknownServices[serviceType.GUID] = false;
					_remoteKnownServices[serviceType] = true;
				}
			}
		}
		
		/// <summary>
		/// Adds an unknown service type to this resource provider.
		/// </summary>
		/// <param name="iid"><see cref="Guid"/> of the unknown service.</param>
		private void AddUnknownServiceType( Guid iid ) {
			lock( _knownServiceLock ) {
				if( _knownServiceTypes.ContainsKey( iid ) )
					return;
					
				_unknownServiceTypes[iid] = true;
			}
		}

		/// <summary>
		/// Gets a service from the root service provider.
		/// </summary>
		/// <param name="serviceType"><see cref="Type"/> of the requested service.</param>
		/// <returns>The requested service, if found, or <see langword="null"/> if not found.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='serviceType'/> is <see langword='null'/>.</exception>
		public object GetService( Type serviceType ) {
			return GetService( serviceType, ResourceTable.RootPath );
		}
		
		public object GetService( Type serviceType, string path ) {
			AddKnownServiceType( serviceType );
			
			if( !_remoteKnownServices.Contains( serviceType ) )
				return null;

			return _rpc.GetRequestSender( path, serviceType.GUID );
		}
		
		public T GetService<T>() {
			return GetService<T>( ResourceTable.RootPath );
		}
		
		public T GetService<T>( string path ) {
			AddKnownServiceType( typeof( T ) );
			return (T) _rpc.GetRequestSender( path, typeof( T ).GUID );
		}
		
		public IServiceProvider GetResource( string path ) {
			if( path == null )
				throw new ArgumentNullException( "path" );

			if( path.Length == 0 )
				throw new ArgumentException( "The path was empty.", "path" );
				
			return new RemoteServiceProvider( this, path );
		}
		
		public void Dispose() {
			if( _localEnumService != null ) {
				_localEnumService.ServicePublished -= _localPublishedHandler;
				_localEnumService.ServiceRevoked -= _localRevokedHandler;
				_localEnumService = null;
			}
		}

		/// <summary>
		/// Provides a service for declaring published services to another listener.
		/// </summary>
		[Guid( "EE6AA504-D6EC-4b41-8D21-61A77BDA4DF2" )]
		public interface IServiceDeclarationService {
			/// <summary>
			/// Declares new root services to the far end of the channel.
			/// </summary>
			/// <param name="serviceIIDList">Types of the new services to declare.</param>
			/// <exception cref='ArgumentNullException'><paramref name='serviceIIDList'/> is <see langword='null'/>.</exception>
			void DeclareServices( Guid[] serviceIIDList );
			void RemoveServices( Guid[] serviceIIDList );
		}

		public event EventHandler<PublishedServiceEventArgs> ServicePublished;
		
		private void OnServicePublished( Type serviceType ) {
			EventHandler<PublishedServiceEventArgs> handler = ServicePublished;
			
			if( handler != null )
				handler( this, new PublishedServiceEventArgs( serviceType ) );
		}

		public event EventHandler<PublishedServiceEventArgs> ServiceRevoked;
		
		private void OnServiceRevoked( Type serviceType ) {
			EventHandler<PublishedServiceEventArgs> handler = ServiceRevoked;
			
			if( handler != null )
				handler( this, new PublishedServiceEventArgs( serviceType ) );
		}

		/// <summary>
		/// Gets a list of services published by the remote service provider.
		/// </summary>
		/// <returns>An array of <see cref="Type"/> values for services offered by the remote service provider.
		///   This may not be a complete list if the <see cref="RpcResourceProvider"/> cannot translate all of the
		///   remote service <see cref="Guid"/> values to known types.</returns>
		public Type[] GetPublishedServices() {
			return _remoteKnownServices.ToArray();
		}
	}
	
}