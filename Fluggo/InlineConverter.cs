using System;
using System.Collections.Generic;
using System.Text;

namespace Fluggo {
	/// <summary>
	/// Allows you to define an inline conversion.
	/// </summary>
	/// <typeparam name="TInput">Type of the objects being enumerated.</typeparam>
	/// <typeparam name="TOutput">Destination type of the objects being enumerated.</typeparam>
	public sealed class InlineConverter<TInput,TOutput> : IEnumerator<TOutput>, IEnumerable<TOutput> {
		IEnumerator<TInput> _root;
		Converter<TInput,TOutput> _converter;
		TOutput _current;

		/// <summary>
		/// Creates a new instance of the <see cref='InlineConverter{TInput,TOutput}'/> class.
		/// </summary>
		/// <param name="root">Base enumerator with objects to filter.</param>
		/// <param name="converter">Reference to a <see cref="Converter{TInput,TOutput}"/> method that returns converted objects.</param>
		public InlineConverter( IEnumerator<TInput> root, Converter<TInput,TOutput> converter ) {
			if( root == null )
				throw new ArgumentNullException( "root" );

			if( converter == null )
				throw new ArgumentNullException( "converter" );

			_root = root;
			_converter = converter;
		}

		/// <summary>
		/// Gets the current item from the enumerator.
		/// </summary>
		/// <value>The current item from the enumerator.</value>
		public TOutput Current {
			get { return _current; }
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
			get { return _current; }
		}

		/// <summary>
		/// Advances the enumerator.
		/// </summary>
		/// <returns>True if there were more items to read, or false if there are no more items.</returns>
		public bool MoveNext() {
			if( _root.MoveNext() ) {
				_current = _converter( _root.Current );
				return true;
			}
			
			_current = default(TOutput);
			return false;
		}

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
		public void Reset() {
			_root.Reset();
		}

		IEnumerator<TOutput> System.Collections.Generic.IEnumerable<TOutput>.GetEnumerator() {
			return this;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this;
		}
	}
}
