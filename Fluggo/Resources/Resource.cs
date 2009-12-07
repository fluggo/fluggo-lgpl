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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Fluggo.Resources {
	public class ResourceTable : IResourceProvider {
		IServiceProvider _root;
		SortedDictionary<string,IResourceProvider> _parsePoints;
		
		public readonly static string RootPath = "/";
		
		private ResourceTable() {
			_parsePoints = new SortedDictionary<string,IResourceProvider>( new ParsePointComparer() );
		}
		
		public ResourceTable( IServiceProvider root ) {
			if( root == null )
				throw new ArgumentNullException( "root" );
				
			_root = root;
			_parsePoints = new SortedDictionary<string,IResourceProvider>( new ParsePointComparer() );
		}
		
		class ParsePointComparer : IComparer<string> {
			public int Compare( string x, string y ) {
				if( x.StartsWith( y ) ) {
					// "/foo/rah/" belongs before "/foo/", so x is less
					return -1;
				}
				else if( y.StartsWith( x ) ) {
					// "/foo/" belongs after "/foo/rah/", so x is more
					return 1;
				}
				
				return 0;
			}
		}
		
		private string FixupPath( string path ) {
			if( path == null )
				throw new ArgumentNullException( "path" );

			if( !path.StartsWith( "/" ) )
				path = "/" + path;
				
			if( !path.EndsWith( "/" ) )
				path += "/";
				
			if( path.Contains( "//" ) )
				throw new ArgumentException( "Path contains an empty segment.", "path" );
				
			return path;
		}
		
		public void Mount( string path, IResourceProvider provider ) {
			if( provider == null )
				throw new ArgumentNullException( "provider" );
				
			_parsePoints.Add( FixupPath( path ), provider );
		}
		
		public void Unmount( string path ) {
			path = FixupPath( path );
			
			if( !_parsePoints.ContainsKey( path ) )
				throw new ArgumentException( "The parse point was not found.", "path" );
				
			_parsePoints.Remove( path );
		}
		
		public IServiceProvider Root { get { return _root; } }
		
		public object GetService( Type serviceType ) {
			if( serviceType == typeof(IResourceProvider) )
				return this;
			
			return _root.GetService( serviceType );
		}
		
		public IServiceProvider GetResource( string path ) {
			if( path == null )
				throw new ArgumentNullException( "path" );
				
			if( path.Length == 0 )
				throw new ArgumentException( "The path string was empty.", "path" );
				
			if( path == RootPath )
				return _root;
			
			if( !path.StartsWith( "/" ) )
				path = "/" + path;
				
			foreach( KeyValuePair<string,IResourceProvider> pair in _parsePoints ) {
				if( path.StartsWith( pair.Key ) )
					return pair.Value.GetResource( path.Substring( pair.Key.Length ) );
			}
			
			return null;
		}
	}
	
	/// <summary>
	/// Represents a set of resources that can be found by path.
	/// </summary>
	public interface IResourceProvider : IServiceProvider {
		/// <summary>
		/// Retrieves the service provider that represents the resource at the given path.
		/// </summary>
		/// <param name="path">Path to the resource.</param>
		/// <returns>The service provider representing the resource at the given path.</returns>
		/// <exception cref="Exception">The resource could not be found. The specific type of error depends on the resource provider.</exception>
		IServiceProvider GetResource( string path );
	}
	
	/// <summary>
	/// Represents the set of data or parameters necessary to create or load a specific kind of <see cref="Resource"/>.
	/// </summary>
	public abstract class ResourceDescriptor {
		public abstract Resource Instantiate( IServiceProvider provider );
		
		// BJC: Add convenience methods for finding another resource from the service provider
	}
	
	/// <summary>
	/// Represents a set of services that operate on one logical object.
	/// </summary>
	public abstract class Resource : IServiceProvider {
		IServiceProvider _rootProvider;

		/// <summary>
		/// Creates a new instance of the <see cref='Resource'/> class.
		/// </summary>
		/// <param name="rootProvider">Optional root service provider.</param>
		protected Resource( IServiceProvider rootProvider ) {
			_rootProvider = rootProvider;
		}
		
		/// <summary>
		/// Gets a service from the root service provider.
		/// </summary>
		/// <typeparam name="T">Type of the service to retrieve.</typeparam>
		/// <returns>A reference to the service, if found, or <see langword='null'/> if the service was not found.</returns>
		protected T GetRootService<T>() where T : class {
			if( _rootProvider == null )
				return null;
				
			return _rootProvider.GetService( typeof(T) ) as T;
		}
		
		/// <summary>
		/// Gets a service provided by this resource.
		/// </summary>
		/// <param name="serviceType">Type of service to retrieve.</param>
		/// <returns>A reference to the service if this resource provides it, or <see langword='null'/> if it does not.</returns>
		/// <remarks>The base implementation will return a reference to the resource object if it can be
		///   cast to <paramref name="serviceType"/>.</remarks>
		/// <exception cref='ArgumentNullException'><paramref name='serviceType'/> is <see langword='null'/>.</exception>
		public virtual object GetService( Type serviceType ) {
			if( serviceType == null )
				throw new ArgumentNullException( "serviceType" );
			
			if( serviceType.IsAssignableFrom( this.GetType() ) )
				return this;
				
			return null;
		}
	}
	
	
}
