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

namespace Fluggo {
	/// <summary>
	/// Represents an unordered collection of distinct items suitable for quick lookup.
	/// </summary>
	/// <typeparam name="T">Type of item to store.</typeparam>
	public class Set<T> : ICollection<T> {
		Dictionary<T, bool> _list = new Dictionary<T,bool>();

		/// <summary>
		/// Creates a new instance of the <see cref='Set{T}'/> class.
		/// </summary>
		public Set() {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='Set{T}'/> class.
		/// </summary>
		/// <param name="itemList">Array of items to include in the set. Duplicate items are ignored.</param>
		/// <exception cref='ArgumentNullException'><paramref name='itemList'/> is <see langword='null'/>.</exception>
		public Set( T[] itemList ) {
			AddRange( itemList );
		}
		
		/// <summary>
		/// Adds an item to the set.
		/// </summary>
		/// <param name="item">Item to add.</param>
		/// <remarks>If the item is already in the set, this method does nothing.
		///   <para>This method approaches an O(1) operation.</para></remarks>
		public void Add( T item ) {
			this[item] = true;
		}
		
		/// <summary>
		/// Adds the given items to the set.
		/// </summary>
		/// <param name="itemList">Array of items to add. Duplicate items are ignored.</param>
		/// <exception cref='ArgumentNullException'><paramref name='itemList'/> is <see langword='null'/>.</exception>
		public void AddRange( T[] itemList ) {
			if( itemList == null )
				throw new ArgumentNullException( "itemList" );
				
			for( int i = 0; i < itemList.Length; i++ )
				Add( itemList[i] );
		}

		/// <summary>
		/// Adds the given items to the set.
		/// </summary>
		/// <param name="itemList">Enumerable list of items to add. Duplicate items are ignored.</param>
		/// <exception cref='ArgumentNullException'><paramref name='itemList'/> is <see langword='null'/>.</exception>
		public void AddRange( IEnumerable<T> itemList ) {
			if( itemList == null )
				throw new ArgumentNullException( "itemList" );
				
			foreach( T item in itemList )
				Add( item );
		}

		/// <summary>
		/// Empties the set.
		/// </summary>
		/// <remarks>This method is an O(n) operation.</remarks>
		public void Clear() {
			_list.Clear();
		}

		/// <summary>
		/// Determines whether the given item is in the set.
		/// </summary>
		/// <param name="item">Item to find in the set.</param>
		/// <returns>This method approaches an O(1) operation.</returns>
		public bool Contains( T item ) {
			return _list.ContainsKey( item );
		}

		/// <summary>
		/// Copies the elements of the set to the given array.
		/// </summary>
		/// <param name="array">Array into which to copy the values in the set.</param>
		/// <param name="arrayIndex">Index in <paramref name="array"/> where copying should begin.</param>
		/// <exception cref='ArgumentNullException'><paramref name='array'/> is <see langword='null'/>.</exception>
		public void CopyTo( T[] array, int arrayIndex ) {
			_list.Keys.CopyTo( array, arrayIndex );
		}

		/// <summary>
		/// Gets the number of distinct items in the set.
		/// </summary>
		/// <value>The number of distinct items in the set.</value>
		public int Count {
			get { return _list.Count; }
		}

		/// <summary>
		/// Gets a value that represents whether the set is read-only.
		/// </summary>
		/// <value>This property always returns false.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Removes an item from the set.
		/// </summary>
		/// <param name="item">Item to remove.</param>
		/// <returns>True if the item was removed, or false if it was not found.</returns>
		/// <remarks>This method does not throw an exception if the item was not found in the set.
		///   <para>This method approaches an O(1) operation.</para></remarks>
		public bool Remove( T item ) {
			return _list.Remove( item );
		}

		/// <summary>
		/// Returns an enumerator that enumerates through the set.
		/// </summary>
		/// <returns>An enumerator for the set.</returns>
		public IEnumerator<T> GetEnumerator() {
			return _list.Keys.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that enumerates through the set.
		/// </summary>
		/// <returns>An enumerator for the set.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _list.Keys.GetEnumerator();
		}
		
		/// <summary>
		/// Gets or sets whether the set contains a given item.
		/// </summary>
		/// <param name="item">Item for which to get or set membership.</param>
		/// <returns>True if the item is in the set, false otherwise.</returns>
		/// <remarks>Set this property to true to ensure the item is in the set. Set it to false to remove it.</remarks>
		public bool this[T item] {
			get {
				return _list.ContainsKey( item );
			}
			set {
				if( value )
					_list[item] = true;
				else
					_list.Remove( item );
			}
		}
		
		/// <summary>
		/// Returns the set as an array.
		/// </summary>
		/// <returns>An array containing the members of the set.</returns>
		public T[] ToArray() {
			T[] result = new T[_list.Count];
			_list.Keys.CopyTo( result, 0 );
			
			return result;
		}
		
		public static Set<T> operator - ( Set<T> s1, Set<T> s2 ) {
			Set<T> result = new Set<T>();
			
			foreach( T item in s1 ) {
				if( !s2.Contains( item ) )
					result.Add( item );
			}
			
			return result;
		}
		
		public static Set<T> operator + ( Set<T> s1, Set<T> s2 ) {
			Set<T> result = new Set<T>();
			
			foreach( T item in s1 )
				result.Add( item );
				
			foreach( T item in s2 )
				result.Add( item );
				
			return result;
		}
	}
}