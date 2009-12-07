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

using System.Collections.Generic;
using System;
using Fluggo.Resources;

namespace Fluggo {
	/// <summary>
	/// Chains the given service providers to overlay new services on existing service domains.
	/// </summary>
	public class ChainedServiceProvider : IServiceProvider {
		LinkedList<IServiceProvider> _providers = new LinkedList<IServiceProvider>();

		/// <summary>
		/// Creates a new instance of the <see cref='ChainedServiceProvider'/> class.
		/// </summary>
		public ChainedServiceProvider() {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='ChainedServiceProvider'/> class.
		/// </summary>
		/// <param name="provider">The base service provider for this chain.</param>
		public ChainedServiceProvider( IServiceProvider provider ) {
			Add( provider );
		}

		/// <summary>
		/// Adds a new service provider to the chain.
		/// </summary>
		/// <param name="provider">Service provider to add. The services in this provider will override services in existing providers.</param>
		/// <exception cref='ArgumentNullException'><paramref name='provider'/> is <see langword='null'/>.</exception>
		public void Add( IServiceProvider provider ) {
			if( provider == null )
				throw new ArgumentNullException( "provider" );

			_providers.AddFirst( provider );
		}

		/// <summary>
		/// Gets a service from the root service provider.
		/// </summary>
		/// <param name="serviceType"><see cref="Type"/> of the requested service.</param>
		/// <returns>The requested service, if found, or <see langword="null"/> if not found.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='serviceType'/> is <see langword='null'/>.</exception>
		/// <remarks>If the <see cref="IResourceProvider"/> service is requested, a proxy is returned to ensure that
		///   the root represents the same service domain.</remarks>
		public object GetService( Type serviceType ) {
			bool isResourceProvider = (serviceType == typeof( IResourceProvider ));

			foreach( IServiceProvider provider in _providers ) {
				object service = provider.GetService( serviceType );

				if( service != null )
					return isResourceProvider ? new InternalResourceProvider( this, (IResourceProvider) service ) : service;
			}

			return null;
		}

		/// <summary>
		/// Ensures that the root path of an <see cref="IResourceProvider"/> refers to the given service provider.
		/// </summary>
		class InternalResourceProvider : IResourceProvider {
			IServiceProvider _owner;
			IResourceProvider _root;

			/// <summary>
			/// Creates a new instance of the <see cref='InternalResourceProvider'/> class.
			/// </summary>
			/// <param name="owner">Reference to the master service provider.</param>
			/// <param name="root">Reference to the <see cref="IResourceProvider"/> to emulate.</param>
			/// <exception cref='ArgumentNullException'><paramref name='owner'/> is <see langword='null'/>.
			///   <para>— OR —</para>
			///   <para><paramref name='root'/> is <see langword='null'/>.</para></exception>
			public InternalResourceProvider( IServiceProvider owner, IResourceProvider root ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );

				if( root == null )
					throw new ArgumentNullException( "root" );

				_owner = owner;
				_root = root;
			}

			/// <summary>
			/// Gets the service provider at the given path.
			/// </summary>
			/// <param name="path">Path to the service provider.</param>
			/// <returns>The <see cref="IServiceProvider"/> at the given path, the master service provider for the root,
			///   or <see langword="null"/> if the requested service provider doesn't exist.</returns>
			/// <exception cref='ArgumentNullException'><paramref name='path'/> is <see langword='null'/>.</exception>
			public IServiceProvider GetResource( string path ) {
				if( path == ResourceTable.RootPath )
					return _owner;

				return _root.GetResource( path );
			}

			/// <summary>
			/// Gets a service from the root service provider.
			/// </summary>
			/// <param name="serviceType"><see cref="Type"/> of the requested service.</param>
			/// <returns>The requested service, if found, or <see langword="null"/> if not found.</returns>
			/// <exception cref='ArgumentNullException'><paramref name='serviceType'/> is <see langword='null'/>.</exception>
			public object GetService( Type serviceType ) {
				return _owner.GetService( serviceType );
			}
		}
	}
}