/*
	Fluggo Common Library
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

namespace Fluggo {
	/// <summary>
	/// Represents a strongly-typed weak reference.
	/// </summary>
	/// <typeparam name="T">Type of the object referenced.</typeparam>
	public class WeakReference<T> where T : class {
		WeakReference _ref;

		/// <summary>
		/// Creates a new instance of the <see cref='WeakReference{T}'/> class.
		/// </summary>
		/// <param name='value'>Target of the weak reference.</param>
		/// <exception cref='ArgumentNullException'><paramref name='value'/> is <see langword='null'/>.</exception>
		public WeakReference( T value ) {
			if( value == null )
				throw new ArgumentNullException( "value" );

			_ref = new WeakReference( value, false );
		}

		/// <summary>
		/// Gets or sets the target of the reference.
		/// </summary>
		/// <value>The target of the reference, or <see langword='null'/> if the target has already been garbage-collected.</value>
		/// <exception cref='ArgumentNullException'>The property is set to <see langword='null'/>.</exception>
		public T Target {
			get {
				return (T) _ref.Target;
			}
			set {
				if( value == null )
					throw new ArgumentNullException( "value" );

				_ref.Target = value;
			}
		}

		/// <summary>
		/// Gets a value that represents whether the target is alive.
		/// </summary>
		/// <value>True if the target is alive, or false if it has been garbage-collected.</value>
		/// <remarks>Do not use this property to determine whether you can retrieve the target object. The target
		///   could be garbage-collected between the time you call this property and the time you call <see cref="Target"/>.
		///   Get the value of <see cref="Target"/> and test it against <see langword='null'/> instead.</remarks>
		public bool IsAlive {
			get {
				return _ref.IsAlive;
			}
		}
	}
}