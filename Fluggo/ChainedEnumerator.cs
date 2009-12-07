using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo {
	/// <summary>
	/// Chains several <see cref='IEnumerable{T}'/> lists together.
	/// </summary>
	public class ChainedEnumerator<T> : IEnumerable<T>, IEnumerator<T> {
		IEnumerable<T>[] _list;
		IEnumerator<T> _current;
		int _position = 0;
		
		/// <summary>
		/// Creates a new instance of the <see cref="ChainedEnumerator{T}"/> class.
		/// </summary>
		/// <param name="enumerables">List of <see cref="IEnumerable{T}"/> instances to chain together.
		///   The items will appear in the output in the order the enumerables appear in this list.</param>
		public ChainedEnumerator( params IEnumerable<T>[] enumerables ) {
			_list = enumerables;
		}
		
		public IEnumerator<T> GetEnumerator() {
			return this;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this;
		}

		/// <summary>
		/// Gets the value of the current item in the enumerator.
		/// </summary>
		/// <value>The current item in the enumerator.</value>
		/// <exception cref="InvalidOperationException">The enumerator is not positioned on a valid element.</exception>
		public T Current {
			get {
				if( _current == null )
					throw new InvalidOperationException( "The enumerator is not positioned on a valid element." );
			
				return _current.Current;
			}
		}

		/// <summary>
		/// Releases managed and unmanaged resources for this enumerator.
		/// </summary>
		public void Dispose() {
			if( _current != null ) {
				_current.Dispose();
				_current = null;
			}
		}

		object System.Collections.IEnumerator.Current {
			get { return Current; }
		}

		/// <summary>
		/// Moves to the next position.
		/// </summary>
		/// <returns>True if there is another item to be processed, or false if there is none.</returns>
		public bool MoveNext() {
			for( ;; ) {
				if( _current != null ) {
					// Return the next item from the current enumerator, if available
					if( _current.MoveNext() )
						return true;
						
					// We've run to the end of the current enumerator, time to get rid of it
					_current.Dispose();
					_current = null;
					_position++;
				}
				
				// If we've no more enumerables to get through, return false
				if( _position >= _list.Length )
					return false;
					
				// Get a new enumerator
				_current = _list[_position].GetEnumerator();
			}
		}

		/// <summary>
		/// Resets the enumerator to the beginning of the list.
		/// </summary>
		public void Reset() {
			if( _current != null ) {
				_current.Dispose();
				_current = null;
			}
		}
	}
}
