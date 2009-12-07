using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo {
	/// <summary>
	/// A queue implementation that is fast for short lists or where most of the operations take place at the beginning of the queue.
	/// </summary>
	/// <typeparam name="T">Type of value to store in the queue.</typeparam>
	public class LinkedSortedQueue<T> where T : class, IComparable<T> {
		class Link {
			public Link( T value, Link next ) {
				Value = value;
				Next = next;
			}
			
			public readonly T Value;
			public Link Next;
		}
		
		Link _head;
		int count;
		
		public void Enqueue( T value ) {
			if( value == null )
				throw new ArgumentNullException( "value" );
				
			count++;
			
			if( _head == null ) {
				_head = new Link( value, null );
				return;
			}
			
			// Compare to head
			int result = value.CompareTo( _head.Value );
			
			if( result < 0 ) {
				_head = new Link( value, _head );
				return;
			}
			
			Link current = _head, last = null;
			
			while( result >= 0 ) {
				// Appending to the end of the list:
				if( current.Next == null ) {
					current.Next = new Link( value, null );
					return;
				}
				
				last = current;
				current = current.Next;
				
				result = value.CompareTo( current.Value );
			}
			
			// Insert into the middle of the list
			last.Next = new Link( value, current );
		}
		
		public bool TryDequeue( out T value ) {
			// Really, really cheap dequeue:
			if( _head == null ) {
				value = null;
				return false;
			}
			
			value = _head.Value;
			_head = _head.Next;
			count--;
			return true;
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
			if( _head == null)
				throw new InvalidOperationException( "The queue is empty." );
				
			return _head.Value;
		}

		/// <summary>
		/// Removes all objects from the queue.
		/// </summary>
		public void Clear() {
			_head = null;
			count = 0;
		}
		
		/// <summary>
		/// Gets the number of elements contained in the queue.
		/// </summary>
		/// <value>The number of elements contained in the queue.</value>
		public int Count {
			get {
				return count;
			}
		}
	}
}
