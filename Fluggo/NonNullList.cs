using System.Collections.Generic;
using System;

namespace Fluggo {
	/// <summary>
	/// Represents a generic list of non-null items.
	/// </summary>
	/// <typeparam name="T">Reference type for the list.</typeparam>
	/// <remarks>This type wraps the <see cref="List{T}"/> type.</remarks>
	public class NonNullList<T> : IList<T> where T : class {
		List<T> _list;

		/// <summary>
		/// Creates a new instance of the <see cref='NonNullList{T}'/> class.
		/// </summary>
		public NonNullList() {
			_list = new List<T>();
		}

		/// <summary>
		/// Creates a new instance of the <see cref='NonNullList{T}'/> class with the given initial members.
		/// </summary>
		/// <param name="array">Array of items initially in the list.</param>
		public NonNullList( T[] array ) : this() {
			AddRange( array );
		}

		/// <summary>
		/// Determines the index of a specific item in the list.
		/// </summary>
		/// <param name="item">The object to locate in the list.</param>
		/// <returns>The index of <paramref name="item"/> if found in the list; otherwise, -1.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='item'/> is <see langword='null'/>.</exception>
		/// <remarks>If an object occurs multiple times in the list, the <see cref="IndexOf"/> method always returns the first instance found.</remarks>
		public int IndexOf( T item ) {
			if( item == null )
				throw new ArgumentNullException( "item" );

			return _list.IndexOf( item );
		}

		/// <summary>
		/// Inserts an item to the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert into the list.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the list.</exception>
		/// <exception cref='ArgumentNullException'><paramref name='item'/> is <see langword='null'/>.</exception>
		public void Insert( int index, T item ) {
			if( item == null )
				throw new ArgumentNullException( "item" );

			_list.Insert( index, item );
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the list.</exception>
		public void RemoveAt( int index ) {
			_list.RemoveAt( index );
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name='index'>The zero-based index of the element to get or set.</param>
		/// <value>The element at the specified index.</value>
		public T this[int index] {
			get {
				return _list[index];
			}
			set {
				if( value == null )
					throw new ArgumentNullException( "value" );

				_list[index] = value;
			}
		}

		/// <summary>
		/// Adds an item to the list.
		/// </summary>
		/// <param name="item">The object to add to the list.</param>
		/// <exception cref='ArgumentNullException'><paramref name='item'/> is <see langword='null'/>.</exception>
		public void Add( T item ) {
			if( item == null )
				throw new ArgumentNullException( "item" );

			_list.Add( item );
		}
		
		public void AddRange( IEnumerable<T> itemList ) {
			if( itemList == null )
				throw new ArgumentNullException( "itemList" );

			foreach( T item in itemList ) {
				if( item == null )
					throw new ArgumentException( "One of the items in the given list was null." );
				
				_list.Add( item );
			}
		}
		
		public void AddRange( IList<T> itemArray ) {
			if( itemArray == null )
				throw new ArgumentNullException( "itemArray" );
				
			for( int i = 0; i < itemArray.Count; i++ ) {
				if( itemArray[i] == null )
					throw new ArgumentException( "One of the given items was null." );
			}

			_list.AddRange( itemArray );
		}

		/// <summary>
		/// Removes all items from the list.
		/// </summary>
		public void Clear() {
			_list.Clear();
		}

		/// <summary>
		/// Determines whether the list contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the list.</param>
		/// <returns>True if <paramref name="item"/> is found in the list; otherwise, false.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='item'/> is <see langword='null'/>.</exception>
		public bool Contains( T item ) {
			if( item == null )
				throw new ArgumentNullException( "item" );

			return _list.Contains( item );
		}

		/// <summary>
		/// Copies the elements of the list to an array, starting at a particular array index.
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the list.
		///   The array must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref='ArgumentNullException'><paramref name='array'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than zero.</exception>
		/// <exception cref="ArgumentException"><paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
		///   <para>- or -</para>
		///   <para>The number of elements in the source list is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination array.</para></exception>
		public void CopyTo( T[] array, int arrayIndex ) {
			_list.CopyTo( array, arrayIndex );
		}

		/// <summary>
		/// Gets the number of elements contained in the list.
		/// </summary>
		/// <value>The number of elements contained in the list.</value>
		public int Count {
			get { return _list.Count; }
		}

		/// <summary>
		/// Gets a value that represents whether the list is read-only.
		/// </summary>
		/// <value>True if the list is read-only, false otherwise. This implementation always returns false.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the list.
		/// </summary>
		/// <param name="item">The object to remove from the list.</param>
		/// <returns>True if <paramref name="item"/> was successfully removed from the list; otherwise, false.
		///   This method also returns false if <paramref name="item"/> is not found in the original list.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='item'/> is <see langword='null'/>.</exception>
		public bool Remove( T item ) {
			if( item == null )
				throw new ArgumentNullException( "item" );

			return _list.Remove( item );
		}

		public IEnumerator<T> GetEnumerator() {
			return _list.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _list.GetEnumerator();
		}
		
		public T[] ToArray() {
			return _list.ToArray();
		}
	}
}