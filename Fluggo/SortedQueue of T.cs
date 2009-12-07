using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo {
	public class SortedQueue<T> where T : IComparable<T> {
		List<T> _list = new List<T>();
		
		class ReverseComparer : IComparer<T> {
			public int Compare( T x, T y ) {
				return -x.CompareTo( y );
			}
		}
		
		/// <summary>
		/// Creates a new instance of the <see cref="SortedQueue{T}"/> class.
		/// </summary>
		public SortedQueue() {
		}
		
		public void Enqueue( T value ) {
			int index = _list.BinarySearch( value, new ReverseComparer() );
			
			if( index < 0 )
				_list.Insert( ~index, value );
			else
				_list.Insert( index, value );
		}
		
		/// <summary>
		/// Removes and returnes the object at the beginning of the queue.
		/// </summary>
		/// <returns>The object that is removed from the beginning of the queue.</returns>
		/// <exception cref="InvalidOperationException">The queue is empty.</exception>
		public T Dequeue() {
			T value;
			
			if( !TryDequeue( out value ) )
				throw new InvalidOperationException( "The queue is empty." );
				
			return value;
		}
		
		public T Peek() {
			if( _list.Count == 0 )
				throw new InvalidOperationException( "The queue is empty." );
				
			return _list[_list.Count - 1];
		}
		
		public void TrimExcess() {
			_list.TrimExcess();
		}
		
		/// <summary>
		/// Removes all objects from the queue.
		/// </summary>
		public void Clear() {
			_list.Clear();
		}
		
		/// <summary>
		/// Gets the number of elements contained in the queue.
		/// </summary>
		/// <value>The number of elements contained in the queue.</value>
		public int Count {
			get {
				return _list.Count;
			}
		}
		
		public bool TryDequeue( out T value ) {
			if( _list.Count == 0 ) {
				value = default(T);
				return false;
			}
			
			int lastIndex = _list.Count - 1;
			value = _list[_list.Count - 1];
			_list.RemoveAt( lastIndex );
			
			return true;
		}
	}
}
