using System;
using System.Collections.Generic;

namespace Fluggo {
	/// <summary>
	/// Represents a list sorted by the elements it contains.
	/// </summary>
	/// <typeparam name="T">Type of element stored in this list.</typeparam>
	public class SortedList<T> : ICollection<T> where T : IComparable<T> {
		List<T> _list = new List<T>();

		/// <summary>
		/// Creates a new instance of the <see cref='SortedList{T}'/> class.
		/// </summary>
		public SortedList() {
		}
		
		public int IndexOf( T item ) {
			int index = _list.BinarySearch( item );
			return (index < 0) ? -1 : index;
		}

		public void RemoveAt( int index ) {
			_list.RemoveAt( index );
		}

		public T this[int index] {
			get {
				return _list[index];
			}
		}

		public void Add( T item ) {
			int index = _list.BinarySearch( item );
			
			if( index < 0 )
				_list.Insert( ~index, item );
			else
				_list.Insert( index, item );
		}

		public void Clear() {
			_list.Clear();
		}

		public bool Contains( T item ) {
			return _list.BinarySearch( item ) >= 0;
		}

		public void CopyTo( T[] array, int arrayIndex ) {
			_list.CopyTo( array, arrayIndex );
		}

		public int Count {
			get { return _list.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool Remove( T item ) {
			int index = _list.BinarySearch( item );
			
			if( index < 0 )
				return false;
			
			_list.RemoveAt( index );
			return true;
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
		
		public void TrimExcess() {
			_list.TrimExcess();
		}
		
		public List<TOutput> ConvertAll<TOutput>( Converter<T,TOutput> converter ) {
			return _list.ConvertAll<TOutput>( converter );
		}
	}
}