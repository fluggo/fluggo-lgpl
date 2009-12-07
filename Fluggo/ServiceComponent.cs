/*
	Fluggo Common Library
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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections.Generic;

namespace Fluggo {
	/// <summary>
	/// A component that provides services to a service domain.
	/// </summary>
	public class ServiceComponent : Component {
		/// <summary>
		/// Overrides the <see cref='Component.Site'/> property.
		/// </summary>
		/// <value>The <see cref="ISite"/> for this service component.</value>
		/// <remarks>Setting this property can cause the <see cref="AddServices"/>, <see cref="PublishServices"/>, <see cref="RevokeServices"/>, or <see cref="RemoveServices"/> methods
		///	  to be invoked for the new site or the component's original site.</remarks>
		public override ISite Site {
			get {
				return base.Site;
			}
			set {
				if( base.Site != null ) {
					IServicePublisher publisher = (IServicePublisher) Site.GetService( typeof(IServicePublisher) );

					if( publisher != null )
						RevokeServices( publisher );

					IServiceContainer container = (IServiceContainer) Site.GetService( typeof(IServiceContainer) );
					
					if( container != null )
						RemoveServices( container );
				}
				
				base.Site = value;
				
				if( value != null ) {
					IServiceContainer container = (IServiceContainer) Site.GetService( typeof(IServiceContainer) );
					
					if( container != null )
						AddServices( container );

					IServicePublisher publisher = (IServicePublisher) Site.GetService( typeof(IServicePublisher) );

					if( publisher != null )
						PublishServices( publisher );
				}
			}
		}
		
		/// <summary>
		/// Adds this component's services to the given container.
		/// </summary>
		/// <param name="container">Service container to which to add services.</param>
		/// <remarks>This method is called when the component is sited (see the <see cref="Site"/> property). Override this method in your own class to
		///     add additional services to the parent service domain. Be sure to call the base <see cref="AddServices"/> implementation from your code.
		///   <para>Services identified with the <see cref="ProvidesAttribute"/> or the <see cref="PublishesAttribute"/> on the component's
		///     class are automatically added and removed from the service domain. Override this method if you need to add a service not
		///     implemented directly by the class, such as a service provided by a private nested type.</para></remarks>
		/// <exception cref='ArgumentNullException'><paramref name='container'/> is <see langword='null'/>.</exception>
		protected virtual void AddServices( IServiceContainer container ) {
			Set<Type> _list = new Set<Type>();
			
			// Collect both "provides" and "publishes" types
			foreach( ProvidesAttribute att in GetType().GetCustomAttributes( typeof(ProvidesAttribute), true ) ) {
				_list[att.ServiceType] = true;
			}
			
			foreach( PublishesAttribute att in GetType().GetCustomAttributes( typeof(PublishesAttribute), true ) ) {
				_list[att.ServiceType] = true;
			}

			// Add service
			foreach( Type type in _list ) {
				container.AddService( type, this );
			}
		}

		/// <summary>
		/// Adds this component's services to the given publisher.
		/// </summary>
		/// <param name="publisher">Service publisher to which to add services.</param>
		/// <remarks>This method is called when the component is sited (see the <see cref="Site"/> property). Override this method in your own class to
		///     publish additional services beyond the parent service domain. Be sure to call the base <see cref="AddServices"/> implementation from your code.
		///   <para>Services identified with the <see cref="PublishesAttribute"/> on the component's
		///     class are automatically published to and revoked from the service domain. Override this method if you need to publish a service not
		///     implemented directly by the class, such as a service provided by a private nested type.</para></remarks>
		/// <exception cref='ArgumentNullException'><paramref name='publisher'/> is <see langword='null'/>.</exception>
		protected virtual void PublishServices( IServicePublisher publisher ) {
			foreach( PublishesAttribute att in GetType().GetCustomAttributes( typeof( PublishesAttribute ), false ) ) {
				publisher.PublishService( att.ServiceType );
			}
		}

		/// <summary>
		/// Removes this component's services from the given publisher.
		/// </summary>
		/// <param name="publisher">Service publisher from which to remove services.</param>
		/// <remarks>This method is called when the component is un-sited (see the <see cref="Site"/> property). Override this method in your own class to
		///     publish additional services beyond the parent service domain. Be sure to call the base <see cref="AddServices"/> implementation from your code.
		///   <para>Services identified with the <see cref="PublishesAttribute"/> on the component's
		///     class are automatically published to and revoked from the service domain. Override this method if you need to publish a service not
		///     implemented directly by the class, such as a service provided by a private nested type.</para></remarks>
		/// <exception cref='ArgumentNullException'><paramref name='publisher'/> is <see langword='null'/>.</exception>
		protected virtual void RevokeServices( IServicePublisher publisher ) {
			foreach( PublishesAttribute att in GetType().GetCustomAttributes( typeof( PublishesAttribute ), false ) ) {
				publisher.RevokeService( att.ServiceType );
			}
		}

		/// <summary>
		/// Removes this component's services from the given container.
		/// </summary>
		/// <param name="container">Service container from which to remove services.</param>
		/// <remarks>This method is called when the component is un-sited (see the <see cref="Site"/> property). Override this method in your own class to
		///     add additional services to the parent service domain. Be sure to call the base <see cref="AddServices"/> implementation from your code.
		///   <para>Services identified with the <see cref="ProvidesAttribute"/> or the <see cref="PublishesAttribute"/> on the component's
		///     class are automatically added and removed from the service domain. Override this method if you need to add a service not
		///     implemented directly by the class, such as a service provided by a private nested type.</para></remarks>
		/// <exception cref='ArgumentNullException'><paramref name='container'/> is <see langword='null'/>.</exception>
		protected virtual void RemoveServices( IServiceContainer container ) {
			if( container == null )
				throw new ArgumentNullException( "container" );
			
			Set<Type> _list = new Set<Type>();

			// Collect both "provides" and "publishes" types
			foreach( ProvidesAttribute att in GetType().GetCustomAttributes( typeof( ProvidesAttribute ), false ) ) {
				_list[att.ServiceType] = true;
			}

			foreach( PublishesAttribute att in GetType().GetCustomAttributes( typeof( PublishesAttribute ), false ) ) {
				_list[att.ServiceType] = true;
			}

			// Add service
			foreach( Type type in _list ) {
				container.RemoveService( type );
			}
		}
		
		/// <summary>
		/// Retrieves the services that this component requires from the given service provider.
		/// </summary>
		/// <param name="provider">Provider from which to retrieve services.</param>
		/// <exception cref='ArgumentNullException'><paramref name='provider'/> is <see langword='null'/>.</exception>
		/// <remarks>Override this method in your own class to retrieve services from the service domain. Use the
		///   <see cref="LoadService"/> or <see cref="LoadService{T}"/> methods to retrieve services from your current site.</remarks>
		public virtual void LoadServices( IServiceProvider provider ) {
		}
		
		/// <summary>
		/// Retrieves a service from the component's site.
		/// </summary>
		/// <param name="serviceType">Type of service to retrieve.</param>
		/// <param name="required">True if the service is required, false otherwise. If true, and the requested service was not found, an exception is thrown.</param>
		/// <returns>A reference to the requested service, if found, or <see langword='null'/> if the service was not found and <paramref name="required"/> is false.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='serviceType'/> is <see langword='null'/>.</exception>
		/// <exception cref="Exception"><paramref name="required"/> is true and <see cref="Site"/> is set to <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name="required"/> is true and the requested service was not found in the service domain.</para></exception>
		protected object LoadService( Type serviceType, bool required ) {
			if( serviceType == null )
				throw new ArgumentNullException( "serviceType" );
				
			if( Site == null ) {
				if( required )
					throw new Exception( "This component requires a site to retrieve services. Set the Site property before calling this method." );
					
				return null;
			}
			
			object service = Site.GetService( serviceType );
			
			if( required && service == null )
				throw new Exception( "Required service " + serviceType.Name + " not found." );
				
			return service;
		}

		/// <summary>
		/// Retrieves a service from the component's site.
		/// </summary>
		/// <typeparam name="T">Type of service to retrieve.</typeparam>
		/// <param name="required">True if the service is required, false otherwise. If true, and the requested service was not found, an exception is thrown.</param>
		/// <returns>A reference to the requested service, if found, or <see langword='null'/> if the service was not found and <paramref name="required"/> is false.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='serviceType'/> is <see langword='null'/>.</exception>
		/// <exception cref="Exception"><paramref name="required"/> is true and <see cref="Site"/> is set to <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name="required"/> is true and the requested service was not found in the service domain.</para></exception>
		protected T LoadService<T>( bool required ) where T : class {
			if( Site == null ) {
				if( required )
					throw new Exception( "This component requires a site to retrieve services. Set the Site property before calling this method." );

				return null;
			}

			object service = Site.GetService( typeof(T) );
			
			if( required && service == null )
				throw new Exception( "Required service " + typeof(T).Name + " not found." );
				
			return (T) service;
		}
	}
	
	public class ServiceHost : Container, IServiceProvider, IPublishedServiceEnumeratorService {
		ServiceContainer _serviceContainer = new ServiceContainer();
		Set<Type> _publishedServices = new Set<Type>();
		List<IServiceProvider> _externalProviders = new List<IServiceProvider>();
		
		class ServicePublisher : IServicePublisher {
			ServiceHost _owner;
			
			public ServicePublisher( ServiceHost owner ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );

				_owner = owner;
			}

			public void PublishService( Type serviceType ) {
				_owner.PublishService( serviceType );
			}

			public void RevokeService( Type serviceType ) {
				_owner.RevokeService( serviceType );
			}
		}
		
		public ServiceHost() {
			_serviceContainer.AddService( typeof(IServiceContainer), _serviceContainer );
			_serviceContainer.AddService( typeof(IServicePublisher), new ServicePublisher( this ) );
			_serviceContainer.AddService( typeof(IPublishedServiceEnumeratorService), this );
			_serviceContainer.AddService( typeof(IContainer), this );
		}
		
		public void AddService( Type serviceType, object provider ) {
			_serviceContainer.AddService( serviceType, provider );
		}
		
		public void LateLoadServices() {
			foreach( IComponent component in Components ) {
				ServiceComponent service = component as ServiceComponent;
				
				if( service != null )
					service.LoadServices( this );
			}
		}
		
		public void AddExternalServiceProvider( IServiceProvider provider ) {
			if( provider == null )
				throw new ArgumentNullException( "provider" );

			_externalProviders.Add( provider );
		}
		
/*		public void AddService( Type serviceType, object service ) {
			_serviceContainer.AddService( serviceType, service );
		}*/
		
		object IServiceProvider.GetService( Type serviceType ) {
			if( serviceType == typeof(IContainer) )
				return this;
			
			return GetService( serviceType );
		}
		
		protected override object GetService(Type service) {
			if( service == typeof(IContainer) )
				return this;
			
			object result = _serviceContainer.GetService( service );
			
			if( result != null )
				return result;
			
			foreach( IServiceProvider provider in _externalProviders ) {
				result = provider.GetService( service );
				
				if( result != null )
					return result;
			}
			
			return null;
		}
		
		public void PublishService( Type serviceType ) {
			// Ensure it's in the service container
			if( _serviceContainer.GetService( serviceType ) == null )
				throw new ArgumentException( "The type " + serviceType.Name + " is not in the service container.", "serviceType" );
				
			// Publish it
			_publishedServices[serviceType] = true;
			OnServicePublished( serviceType );
		}
		
		public void RevokeService( Type serviceType ) {
			// Ensure it's in the service container
			if( _serviceContainer.GetService( serviceType ) == null )
				throw new ArgumentException( "The type " + serviceType.Name + " is not in the service container.", "serviceType" );

			// Unpublish it
			_publishedServices[serviceType] = false;
			OnServiceRevoked( serviceType );
		}
		
	#region Events
		private void OnServicePublished( Type serviceType ) {
			EventHandler<PublishedServiceEventArgs> handler = ServicePublished;
			
			if( handler != null )
				handler( this, new PublishedServiceEventArgs( serviceType ) );
		}

		private void OnServiceRevoked( Type serviceType ) {
			EventHandler<PublishedServiceEventArgs> handler = ServiceRevoked;

			if( handler != null )
				handler( this, new PublishedServiceEventArgs( serviceType ) );
		}

		public event EventHandler<PublishedServiceEventArgs> ServicePublished;

		public event EventHandler<PublishedServiceEventArgs> ServiceRevoked;
	#endregion

		public Type[] GetPublishedServices() {
			Type[] keys = new Type[_publishedServices.Count];
			_publishedServices.CopyTo( keys, 0 );
			
			return keys;
		}
	}

	public class PublishedServiceEventArgs : EventArgs {
		Type _serviceType;

		public PublishedServiceEventArgs( Type serviceType ) {
			if( serviceType == null )
				throw new ArgumentNullException( "serviceType" );

			_serviceType = serviceType;
		}

		public Type ServiceType { get { return _serviceType; } }
	}

	[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
	public sealed class ProvidesAttribute : Attribute {
		Type _serviceType;

		public ProvidesAttribute( Type serviceType ) {
			_serviceType = serviceType;
		}

		public Type ServiceType {
			get {
				return _serviceType;
			}
		}
	}

	[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
	public sealed class PublishesAttribute : Attribute {
		Type _serviceType;
		
		public PublishesAttribute( Type serviceType ) {
			_serviceType = serviceType;
		}
		
		public Type ServiceType {
			get {
				return _serviceType;
			}
		}
	}

	/// <summary>Provides a method and events for determining the services available in a service domain.</summary>
	// BJC: Dealing with what-services-are-available may be tricky. We've always just assumed in the past that
	// services do *not* change, and maybe that's the best interpretation for now, and we can deal with
	// whether they change later.
	public interface IPublishedServiceEnumeratorService {
		event EventHandler<PublishedServiceEventArgs> ServicePublished;
		event EventHandler<PublishedServiceEventArgs> ServiceRevoked;

		Type[] GetPublishedServices();
	}
	
	public interface IServicePublisher {
		void PublishService( Type serviceType );
		void RevokeService( Type serviceType );
	}
	
	public class ServiceNotFoundException : Exception {
		Type _serviceType;
		
		public ServiceNotFoundException( Type serviceType ) : base( serviceType == null ? string.Empty : serviceType.ToString() ) {
			if( serviceType == null )
				throw new ArgumentNullException( "serviceType" );

			_serviceType = serviceType;
		}
		
		public Type ServiceType {
			get {
				return _serviceType;
			}
		}
	}
}