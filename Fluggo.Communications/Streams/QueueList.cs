/*
	Fluggo Communications Library
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

namespace Fluggo.Communications
{
/*	public interface IQueueReader<T> {
		T Dequeue();
		T Peek();
	}
	*/
	/// <summary>
	/// Represents a list of interdependent queues.
	/// </summary>
	/// <typeparam name="T">Type of item stored in the queues.</typeparam>
	public class QueueList<T> {
		class QueueItem {
			public QueueItem( int queue, T value ) {
				Queue = queue;
				Value = value;
			}
			
			public T Value;
			public int Queue;
			public QueueItem Previous;
			public QueueItem Next;
		}
		
		QueueItem _head, _tail;
		QueueItem[] _headList;
		int _count;
		int[] _queueCounts;

		/// <summary>
		/// Creates a new instance of the <see cref='QueueList'/> class.
		/// </summary>
		/// <param name="queueCount">Number of queues in the new list.</param>
		public QueueList( int queueCount ) {
			if( queueCount < 0 )
				throw new ArgumentOutOfRangeException( "queueCount" );
			
			_headList = new QueueItem[queueCount];
			_queueCounts = new int[queueCount];
		}
		
		/// <summary>
		/// Adds an item to the end of a queue.
		/// </summary>
		/// <param name="queue">Zero-based index of the queue to which to add the item.</param>
		/// <param name="value">Value of the item to add to the queue.</param>
		public virtual void Enqueue( int queue, T value ) {
			if( queue < 0 || queue >= _headList.Length )
				throw new ArgumentOutOfRangeException( "queue" );
			
			QueueItem item = new QueueItem( queue, value );
			
			// Add to main queue
			if( _tail == null ) {
				_head = item;
				_tail = item;
			}
			else {
				item.Previous = _tail;
				_tail.Next = item;
				_tail = item;
			}
			
			// Index in head list 
			if( _headList[queue] == null )
				_headList[queue] = item;
				
			_count++;
			_queueCounts[queue]++;
		}
		
		/// <summary>
		/// Removes and returns the item at the head of all queues.
		/// </summary>
		/// <returns>Returns the item at the head of all the queues.</returns>
		/// <exception cref="InvalidOperationException">All queues are empty.</exception>
		public virtual T Dequeue() {
			if( _head == null )
				throw new InvalidOperationException();
			
			try {
				return _head.Value;
			}
			finally {
				Remove( _head );
			}
		}
		
		/// <summary>
		/// Removes and returns the item at the head of the given queue.
		/// </summary>
		/// <param name="queue">Queue from which to retrieve the item.</param>
		/// <returns>Returns the item at the head of the queue.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="queue"/> does to refer to a valid queue.</exception>
		/// <exception cref="InvalidOperationException">The given queue is empty.</exception>
		public virtual T Dequeue( int queue ) {
			if( queue < 0 || queue >= _headList.Length )
				throw new ArgumentOutOfRangeException( "queue" );
				
			if( _headList[queue] == null )
				throw new InvalidOperationException();
			
			try {
				return _headList[queue].Value;
			}
			finally {
				Remove( _headList[queue] );
			}
		}
		
		/// <summary>
		/// Returns the item at the head of all queues.
		/// </summary>
		/// <returns>Returns the item at the head of all the queues.</returns>
		/// <exception cref="InvalidOperationException">All queues are empty.</exception>
		public virtual T Peek() {
			if( _head == null )
				throw new InvalidOperationException();

			return _head.Value;
		}

		/// <summary>
		/// Returns the item at the head of the given queue.
		/// </summary>
		/// <param name="queue">Queue from which to retrieve the item.</param>
		/// <returns>Returns the item at the head of the queue.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="queue"/> does to refer to a valid queue.</exception>
		/// <exception cref="InvalidOperationException">The given queue is empty.</exception>
		public virtual T Peek( int queue ) {
			if( queue < 0 || queue >= _headList.Length )
				throw new ArgumentOutOfRangeException( "queue" );

			if( _headList[queue] == null )
				throw new InvalidOperationException();

			return _headList[queue].Value;
		}
		
		private void Remove( QueueItem item ) {
			// Patch the hole
			if( item.Previous != null ) {
				if( item.Next != null ) {
					// Middle of the list; tie the two neighbors together
					item.Previous.Next = item.Next;
					item.Next.Previous = item.Previous;
				}
				else {
					// End of the list
					item.Previous.Next = null;
					_tail = item.Previous;
				}
			}
			else {
				if( item.Next != null ) {
					// Head of the list
					item.Next.Previous = null;
					_head = _head.Next;
				}
				else {
					// Only item in the list
					_head = null;
					_tail = null;
				}
			}

			// Update the individual queue
			QueueItem current = _headList[item.Queue].Next;

			// Walk until we find the next one
			while( current != null ) {
				if( current.Queue == item.Queue ) {
					_headList[item.Queue] = current;
					break;
				}

				current = current.Next;
			}

			if( current == null )
				_headList[item.Queue] = null;

			_count--;
			_queueCounts[item.Queue]--;
		}

		/// <summary>
		/// Gets the number of items in all queues.
		/// </summary>
		/// <value>The number of items in all queues.</value>
		public int Count {
			get {
				return _count;
			}
		}

		/// <summary>
		/// Gets the number of items in the given queue.
		/// </summary>
		/// <returns>Returns the number of items in the given queue.</returns>
		public int GetQueueItemCount( int queue ) {
			return _queueCounts[queue];
		}

		/// <summary>
		/// Gets the number of queues in this list.
		/// </summary>
		/// <value>The number of queues in this list.</value>
		public int QueueCount {
			get {
				return _queueCounts.Length;
			}
		}
	}
}
