using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo {
	/// <summary>
	/// Allows you to define an inline filter using a predicate.
	/// </summary>
	/// <typeparam name="T">Type of the objects being enumerated.</typeparam>
	public sealed class InlineFilter<T> : IEnumerator<T> {
		IEnumerator<T> _root;
		Predicate<T> _predicate;

		/// <summary>
		/// Creates a new instance of the <see cref='InlineFilter{T}'/> class.
		/// </summary>
		/// <param name="root">Base enumerator with objects to filter.</param>
		/// <param name="predicate">Reference to a <see cref="Predicate{T}"/> method that returns true for
		///		acceptable objects.</param>
		public InlineFilter( IEnumerator<T> root, Predicate<T> predicate ) {
			if( root == null )
				throw new ArgumentNullException( "root" );

			if( predicate == null )
				throw new ArgumentNullException( "predicate" );

			_root = root;
			_predicate = predicate;
		}

		/// <summary>
		/// Gets the current item from the enumerator.
		/// </summary>
		/// <value>The current item from the enumerator.</value>
		public T Current {
			get { return _root.Current; }
		}

		/// <summary>
		/// Disposes the enumerator.
		/// </summary>
		public void Dispose() {
			_root.Dispose();
		}

		/// <summary>
		/// Gets the current item from the enumerator.
		/// </summary>
		/// <value>The current item from the enumerator.</value>
		object System.Collections.IEnumerator.Current {
			get { return _root.Current; }
		}

		/// <summary>
		/// Advances the enumerator.
		/// </summary>
		/// <returns>True if there were more items to read, or false if there are no more items.</returns>
		public bool MoveNext() {
			while( _root.MoveNext() ) {
				if( _predicate( Current ) )
					return true;
			}
			
			return false;
		}

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
		public void Reset() {
			_root.Reset();
		}
	}
}
