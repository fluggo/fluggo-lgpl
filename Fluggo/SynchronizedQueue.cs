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
using System.Threading;

namespace Fluggo
{
	/// <summary>
	/// Represents a synchronized double-lock queue.
	/// </summary>
	/// <typeparam name="T">Type of item stored in the queue.</typeparam>
	/// <remarks>In a double-lock queue, adding an item to the queue will not stall an operation
	///     trying to remove an item from the queue. This makes this kind of queue ideal for passing items
	///     between threads, where one thread produces items and the other consumes them.
	///   <para>This queue only blocks when two <see cref="Enqueue"/> or two <see cref="Dequeue"/> operations
	///     are attempted at the same time. For a queue that blocks until an item appears, see <see cref="AsynchronousQueue{T}"/>.</para></remarks>
	public class SynchronizedQueue<T> {
		class Node {
			public Node( T value ) {
				Value = value;
			}
			
			public T Value;
			public Node Next;
		}
		
		Node _head, _tail;
		object _headLock = new object(), _tailLock = new object();
		int _queueCount;

		/// <summary>
		/// Creates a new instance of the <see cref='SynchronizedQueue{T}'/> class.
		/// </summary>
		public SynchronizedQueue() {
			_head = _tail = new Node( default(T) );
		}
		
		/// <summary>
		/// Adds an item to the end of the queue.
		/// </summary>
		/// <param name="value">Value of the item to add to the queue.</param>
		public void Enqueue( T value ) {
			Node node = new Node( value );
			
			lock( _tailLock ) {
				_tail.Next = node;
				_tail = node;
				Interlocked.Increment( ref _queueCount );
			}
		}
		
		/// <summary>
		/// Removes an item from the beginning of the queue and returns it.
		/// </summary>
		/// <param name="value">Reference to a variable. On return, if the queue was not empty, this will contain the item found
		///   at the beginning of the queue.</param>
		/// <returns>True if an item was found, or false if the queue was empty.</returns>
		/// <remarks>This method returns immediately if an item is not found. For a queue that waits for an item,
		///   see <see cref="AsynchronousQueue{T}"/>.</remarks>
		public bool Dequeue( out T value ) {
			lock( _headLock ) {
				Node node = _head, newHead = node.Next;
				
				if( newHead == null ) {
					value = default(T);
					return false;
				}
					
				value = newHead.Value;
				_head = newHead;
				Interlocked.Decrement( ref _queueCount );
				return true;
			}
		}

		/// <summary>
		/// Gets the number of items in the queue.
		/// </summary>
		/// <value>The number of items in the queue.</value>
		/// <remarks>In a multithreaded situation, this value can change quickly. Use the return value from <see cref="Dequeue"/>
		///   to determine whether the queue is empty.</remarks>
		public int Count {
			get {
				return Thread.VolatileRead( ref _queueCount );
			}
		}
	}
}
