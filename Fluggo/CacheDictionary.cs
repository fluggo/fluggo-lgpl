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
using System.Collections.ObjectModel;

namespace Fluggo {
	/// <summary>
	/// Represents a dictionary of garbage-collectible objects.
	/// </summary>
	/// <typeparam name="TKey">Type of the key.</typeparam>
	/// <typeparam name="TValue">Type of the value, which must be a reference type.</typeparam>
	/// <remarks>Values stored in this dictionary are unrooted. This means that if the only reference to an object is
	///   inside this dictionary, it is eligible for garbage collection. This also means that keys may disappear from
	///   the dictionary at any time. Ensure that you do not count on a previously added entry to exist at a later time.</remarks>
	public class CacheDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TValue : class {
		Dictionary<TKey, WeakReference<TValue>> _dict = new Dictionary<TKey, WeakReference<TValue>>();

		/// <summary>
		/// Adds the given key-value pair to the dictionary.
		/// </summary>
		/// <param name="key">Key of the element to add.</param>
		/// <param name="value">Value of the element to add. This cannot be <see langword='null'/>.</param>
		/// <exception cref='ArgumentNullException'><paramref name='value'/> is <see langword='null'/>.</exception>
		/// <remarks>There is no guarantee that a value added with this method can be retrieved later, unless a reference
		///   to the object exists outside the cache. A key, on the other hand, might stay in the cache for a long time.
		///   You should ensure that your keys are small.</remarks>
		public void Add( TKey key, TValue value ) {
			WeakReference<TValue> @ref;

			if( _dict.TryGetValue( key, out @ref ) && !@ref.IsAlive )
				_dict[key] = new WeakReference<TValue>( value );
			else
				_dict.Add( key, new WeakReference<TValue>( value ) );
		}

		/// <summary>
		/// Determines whether the given key is in this cache.
		/// </summary>
		/// <param name="key">Key to find in the cache.</param>
		/// <returns>True if the key is in the cache, or false if it is not.</returns>
		/// <remarks>This method is a good way to determine whether a given object has been garbage-collected.</remarks>
		public bool ContainsKey( TKey key ) {
			WeakReference<TValue> @ref;
			
			if( !_dict.TryGetValue( key, out @ref ) )
				return false;
				
			if( !@ref.IsAlive ) {
				_dict.Remove( key );
				return false;
			}
			
			return true;
		}

		/// <summary>
		/// Gets the collection of keys in the dictionary.
		/// </summary>
		/// <value>The collection of keys in the dictionary.</value>
		/// <remarks>This property is an O(n) operation.</remarks>
		public ICollection<TKey> Keys {
			get { ClearDeadObjects(); return _dict.Keys; }
		}

		/// <summary>
		/// Removes a key-value pair from the dictionary.
		/// </summary>
		/// <param name="key">Key of the pair to remove.</param>
		/// <returns>True if the key was found and removed, or false if the key was not found.</returns>
		/// <remarks>The key might not be found if the value was garbage-collected.</remarks>
		public bool Remove( TKey key ) {
			if( !ContainsKey( key ) )
				return false;
			
			return _dict.Remove( key );
		}

		/// <summary>
		/// Gets the value associated with the given key.
		/// </summary>
		/// <param name="key">Key of the value to get.</param>
		/// <param name="value">Reference to a value. On return, this contains the value associated with the key, if the key was found.
		///   Otherwise, this contains <see langword='null'/>.</param>
		/// <returns>True if the key was found and <paramref name="value"/> contains the value associated with the key, or false if the key
		///   was not found and <paramref name="value"/> contains <see langword='null'/>.</returns>
		public bool TryGetValue( TKey key, out TValue value ) {
			WeakReference<TValue> @ref;

			if( !_dict.TryGetValue( key, out @ref ) ) {
				value = null;
				return false;
			}

			value = @ref.Target;
			
			if( value == null ) {
				_dict.Remove( key );
				return false;
			}

			return true;
		}
		
		/// <summary>
		/// Gets the collection of all live values in the cache.
		/// </summary>
		/// <value>The collection of live values in the cache.</value>
		/// <remarks>Retrieving this collection is an O(n) operation.</remarks>
		public ICollection<TValue> Values {
			get {
				// Convert to a live array
				ClearDeadObjects();
				List<TValue> list = new List<TValue>( _dict.Count );

				foreach( WeakReference<TValue> @ref in _dict.Values ) {
					TValue value = @ref.Target;

					if( value == null )
						continue;

					list.Add( value );
				}

				return new ReadOnlyCollection<TValue>( list );
			}
		}

		/// <summary>
		/// Gets or sets the value associated with the given key.
		/// </summary>
		/// <param name="key">Key of the value to get or set.</param>
		/// <value>Value associated with the specified key.</value>
		/// <exception cref='ArgumentNullException'><paramref name='key'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para>The item is being set to <see langword='null'/>.</para></exception>
		/// <exception cref="KeyNotFoundException">The property was retrieved and <paramref name="key"/> was not in the cache.</exception>
		public TValue this[TKey key] {
			get {
				TValue value;
				
				if( !TryGetValue( key, out value ) )
					throw new KeyNotFoundException();
					
				return value;
			}
			set {
				_dict[key] = new WeakReference<TValue>( value );
			}
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add( KeyValuePair<TKey, TValue> item ) {
			Add( item.Key, item.Value );
		}

		/// <summary>
		/// Removes all items from the cache.
		/// </summary>
		/// <remarks>This method is an O(n) operation.</remarks>
		public void Clear() {
			_dict.Clear();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains( KeyValuePair<TKey, TValue> item ) {
			return ContainsKey( item.Key );
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex ) {
			throw new Exception( "The method or operation is not implemented." );
		}

		/// <summary>
		/// Gets the number of objects in the collection.
		/// </summary>
		/// <value>The number of objects in the collection.</value>
		/// <remarks>Retrieving this property is an O(n) operation.</remarks>
		public int Count {
			get { ClearDeadObjects(); return _dict.Count; }
		}

		/// <summary>
		/// Gets a value that represents whether the cache is read-only.
		/// </summary>
		/// <value>This property is always false.</value>
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
			get { return false; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove( KeyValuePair<TKey, TValue> item ) {
			return Remove( item.Key );
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
			ClearDeadObjects();

			foreach( KeyValuePair<TKey, WeakReference<TValue>> pair in _dict ) {
				TValue value = pair.Value.Target;
				
				if( value == null )
					continue;
					
				yield return new KeyValuePair<TKey, TValue>( pair.Key, value );
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			throw new Exception( "The method or operation is not implemented." );
		}
		
		/// <summary>
		/// Removes dead objects from the cache.
		/// </summary>
		/// <remarks>Dead objects are removed from the cache automatically when calling certain methods. This method will
		///   ensure that all pairs where the value has been garbage-collected are removed from the cache.</remarks>
		public void ClearDeadObjects() {
			List<TKey> deadKeys = new List<TKey>();

			foreach( KeyValuePair<TKey, WeakReference<TValue>> pair in _dict ) {
				if( !pair.Value.IsAlive )
					deadKeys.Add( pair.Key );
			}
			
			foreach( TKey key in deadKeys )
				Remove( key );
		}
	}
}