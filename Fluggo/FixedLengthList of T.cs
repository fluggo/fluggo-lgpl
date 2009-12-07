using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo {
	public class FixedLengthList<T> : IList<T> {
		IList<T> _root;
		
		public FixedLengthList( IList<T> root ) {
			if( root == null )
				throw new ArgumentNullException( "root" );

			_root = root;
		}
		
		public int IndexOf( T item ) {
			return _root.IndexOf( item );
		}

		public void Insert( int index, T item ) {
			throw new NotSupportedException();
		}

		public void RemoveAt( int index ) {
			throw new NotSupportedException();
		}

		public T this[int index] {
			get {
				return _root[index];
			}
			set {
				_root[index] = value;
			}
		}

		public void Add( T item ) {
			throw new NotSupportedException();
		}

		public void Clear() {
			throw new NotSupportedException();
		}

		public bool Contains( T item ) {
			return _root.Contains( item );
		}

		public void CopyTo( T[] array, int arrayIndex ) {
			_root.CopyTo( array, arrayIndex );
		}

		public int Count {
			get { return _root.Count; }
		}

		public bool IsReadOnly {
			get { return _root.IsReadOnly; }
		}

		public bool Remove( T item ) {
			throw new NotSupportedException();
		}

		public IEnumerator<T> GetEnumerator() {
			return _root.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _root.GetEnumerator();
		}
	}
}
