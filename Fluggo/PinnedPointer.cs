using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Fluggo {
	/// <summary>
	/// Represents a pinned object on the garbage-collected heap.
	/// </summary>
	/// <remarks>
	///		This class is a wrapper around <see cref="GCHandle"/>, similar to the <see cref="WeakReference"/> class.
	///	  <para>You can pin an object to obtain a direct pointer to it on the heap.</para>
	///   <para>The object specified in the constructor remains pinned for as long as this class lives,
	///		preventing the garbage collector from performing effectively. As soon as you are finished using
	///		the pinned object, call <see cref="Dispose()"/> immediately.</para></remarks>
	public sealed class PinnedObject : IDisposable {
		GCHandle _handle;
		bool _disposed;

		/// <summary>
		/// Creates a new instance of the <see cref='PinnedObject'/> class.
		/// </summary>
		/// <param name="value">Object to pin. This can be a simple reference type, an array, or even a value type.</param>
		public PinnedObject( object value ) {
			_handle = GCHandle.Alloc( value, GCHandleType.Pinned );
		}
		
		/// <summary>
		/// Un-pins the pinned object.
		/// </summary>
		public void Dispose() {
			Dispose( true );
		}
		
		private void Dispose( bool disposing ) {
			if( _disposed )
				return;
				
			if( disposing )
				GC.SuppressFinalize( this );
				
			_handle.Free();
			_disposed = true;
		}
		
		/// <summary>
		/// Releases the pinned object.
		/// </summary>
		/// <remarks>Always call <see cref="Dispose()"/> immediately after you have finished using the pinned object.
		///   Waiting for the finalizer to run may severely impact the performance of your application.</remarks>
		~PinnedObject() {
			Dispose( false );
		}

		/// <summary>
		/// Gets the pointer to the pinned object in memory.
		/// </summary>
		/// <value>The pointer to the pinned object in memory.</value>
		/// <exception cref="ObjectDisposedException">The pinned object has been un-pinned with a call to <see cref="Dispose()"/>.</exception>
		public IntPtr Pointer {
			get {
				if( _disposed )
					throw new ObjectDisposedException( null );
					
				return _handle.AddrOfPinnedObject();
			}
		}
		
		/// <summary>
		/// Provides a convenient way to retrieve the <see cref="Pointer"/> property.
		/// </summary>
		/// <param name="pinned"><see cref="PinnedObject"/> value to convert to a pointer.</param>
		/// <returns>Pointer to the pinned object.</returns>
		public static implicit operator IntPtr( PinnedObject pinned ) {
			return pinned.Pointer;
		}
	}
}
