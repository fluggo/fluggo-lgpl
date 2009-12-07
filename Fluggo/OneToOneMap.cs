using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo {
	/// <summary>
	/// Represents a one-to-one mapping from a set of unique items to a set of unique items.
	/// </summary>
	/// <typeparam name="K">Type of the key.</typeparam>
	/// <typeparam name="V">Type of the value.</typeparam>
	public class OneToOneMap<K,V> : IDictionary<K,V> where V : IEquatable<V> {
		Dictionary<K,V> _keyDictionary;
		Dictionary<V,K> _valueDictionary;
		
		/// <summary>
		/// Creates a new instance of the <see cref="OneToOneMap{K,V}"/> class.
		/// </summary>
		public OneToOneMap() {
			_keyDictionary = new Dictionary<K,V>();
			_valueDictionary = new Dictionary<V,K>();
		}
		
		/// <summary>
		/// Adds a new key-value pair to the map.
		/// </summary>
		/// <param name="key">Key to add.</param>
		/// <param name="value">Value to add.</param>
		/// <exception cref="ArgumentException">An element with the same key or the same value is already in the map.</exception>
		public void Add( K key, V value ) {
			if( _keyDictionary.ContainsKey( key ) )
				throw new ArgumentException( "An element with the same key is already in the map." );
				
			if( _valueDictionary.ContainsKey( value ) )
				throw new ArgumentException( "An element with the same value is already in the map." );
				
			_keyDictionary.Add( key, value );
			_valueDictionary.Add( value, key );
		}

		/// <summary>
		/// Determines whether the map contains the given key.
		/// </summary>
		/// <param name="key">Key to test.</param>
		/// <returns>True if the map contains the given key, false otherwise.</returns>
		public bool ContainsKey( K key ) {
			return _keyDictionary.ContainsKey( key );
		}
		
		/// <summary>
		/// Determines whether the map contains the given value.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns>True if the map contains the given value, false otherwise.</returns>
		public bool ContainsValue( V value ) {
			return _valueDictionary.ContainsKey( value );
		}

		/// <summary>
		/// Gets a collection of keys in the map.
		/// </summary>
		/// <value>A collection of keys in the map.</value>
		public ICollection<K> Keys {
			get { return _keyDictionary.Keys; }
		}

		/// <summary>
		/// Removes a key from the map.
		/// </summary>
		/// <param name="key">Key to remove.</param>
		/// <returns>True if the key was in the map, false otherwise.</returns>
		public bool Remove( K key ) {
			V value;
			
			if( !_keyDictionary.TryGetValue( key, out value ) )
				return false;
				
			_keyDictionary.Remove( key );
			return _valueDictionary.Remove( value );
		}
		
		/// <summary>
		/// Removes a value from the map.
		/// </summary>
		/// <param name="value">Value to remove.</param>
		/// <returns>True if the value was in the map, false otherwise.</returns>
		public bool RemoveValue( V value ) {
			K key;
			
			if( !_valueDictionary.TryGetValue( value, out key ) )
				return false;
				
			_valueDictionary.Remove( value );
			return _keyDictionary.Remove( key );
		}

		/// <summary>
		/// Tries to get the value for the given key.
		/// </summary>
		/// <param name="key">Key to use to retrieve the value.</param>
		/// <param name="value">Reference to a value. On returning true, contains the value in the map for the given key.</param>
		/// <returns>True if the key was found, false otherwise.</returns>
		public bool TryGetValue( K key, out V value ) {
			return _keyDictionary.TryGetValue( key, out value );
		}
		
		/// <summary>
		/// Tries to get the key for the given value.
		/// </summary>
		/// <param name="value">Value to use to retrieve the key.</param>
		/// <param name="key">Reference to a key. On returning true, contains the key in the map for the given value.</param>
		/// <returns>True if the value was found, false otherwise.</returns>
		public bool TryGetKey( V value, out K key ) {
			return _valueDictionary.TryGetValue( value, out key );
		}

		/// <summary>
		/// Gets a collection of values in the map.
		/// </summary>
		/// <value>A collection of values in the map.</value>
		public ICollection<V> Values {
			get { return _keyDictionary.Values; }
		}

		/// <summary>
		/// Gets or sets the value for a given key.
		/// </summary>
		/// <param name="key">Key used to get or set a value.</param>
		/// <returns>The value for the given key.</returns>
		/// <exception cref="ArgumentException">The operation would have led to a duplicate value in the map.</exception>
		public V this[K key] {
			get {
				return _keyDictionary[key];
			}
			set {
				// Find out the old value, if any
				V oldValue;
				
				if( _keyDictionary.TryGetValue( key, out oldValue ) ) {
					// The old value entry must be removed and a new one created.
					// But first, we have to make sure the operation doesn't create
					// a value collision.
					
					if( value.Equals( oldValue ) )
						return;
						
					if( _valueDictionary.ContainsKey( value ) )
						throw new ArgumentException( "An element with the same value already exists." );
						
					_valueDictionary.Remove( oldValue );
				}

				_valueDictionary.Add( value, key );
				_keyDictionary[key] = value;
			}
		}

		void ICollection<KeyValuePair<K,V>>.Add( KeyValuePair<K, V> item ) {
			Add( item.Key, item.Value );
		}

		/// <summary>
		/// Clears all elements from the map.
		/// </summary>
		public void Clear() {
			_keyDictionary.Clear();
			_valueDictionary.Clear();
		}

		bool ICollection<KeyValuePair<K,V>>.Contains( KeyValuePair<K, V> item ) {
			throw new Exception( "The method or operation is not implemented." );
		}

		void ICollection<KeyValuePair<K,V>>.CopyTo( KeyValuePair<K, V>[] array, int arrayIndex ) {
			((ICollection<KeyValuePair<K,V>>) _keyDictionary).CopyTo( array, arrayIndex );
		}

		/// <summary>
		/// Gets the number of elements in the map.
		/// </summary>
		/// <value>The number of elements in the map.</value>
		public int Count {
			get { return _keyDictionary.Count; }
		}

		/// <summary>
		/// Gets a value that represents whether this map is read-only.
		/// </summary>
		/// <value>This property always returns false.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		bool ICollection<KeyValuePair<K,V>>.Remove( KeyValuePair<K, V> item ) {
			return Remove( item.Key );
		}

		/// <summary>
		/// Gets an object that allows enumerating through the elements in the map.
		/// </summary>
		/// <returns>An object that allows enumerating through the elements in the map.</returns>
		public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
			return _keyDictionary.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _keyDictionary.GetEnumerator();
		}
	}
}
