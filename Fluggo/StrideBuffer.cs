using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Fluggo {
	//public interface IStrideBuffer {
	//    /// <summary>
	//    /// Sets data in the buffer.
	//    /// </summary>
	//    /// <typeparam name="T">Type of data to place in the buffer.</typeparam>
	//    /// <param name="data">Array containing elements to copy into the buffer.</param>
	//    /// <param name="index">Index into <paramref name="data"/> at which copying should begin.</param>
	//    /// <param name="count">Number of elements to copy.</param>
	//    /// <param name="byteOffset">Offset into the buffer, in bytes, at which copying should begin.</param>
	//    /// <param name="stride">The offset between the start of one element and the start of the next. This must be at
	//    ///   least the size of <typeparamref name="T"/>.</param>
	//    void SetData<T>( T[] data, int index, int count, int byteOffset, int stride ) where T : struct;

	//    void GetData<T>( T[] destination, int index, int count, int byteOffset, int stride ) where T : struct;
	//}
	
	/// <summary>
	/// Constructs a byte buffer from arrays of primitives or simple types.
	/// </summary>
	/// <remarks>A stride buffer is useful for constructing a buffer which contains a mixed array of elements
	///     at different offsets. One example is a vertex buffer. A vertex buffer usually contains data for each
	///     vertex in a mesh, and each vertex may contain attributes such as position, color, or texture coordinates.
	///     Many combinations of attribute types are possible, and it is not always practical to construct a new type
	///     just to hold a given set of attributes for a vertex. The <see cref="StrideBuffer"/> can be used in this
	///     scenario to interleave elements of separate arrays, each containing a single type of vertex attribute.
	///   <para>This class accesses memory directly, and requires unmanaged permissions for the assembly in order to run.</para></remarks>
	public static class StrideBuffer {
		public static void SetData<T>( byte[] buffer, int byteOffset, T[] source, int stride ) where T : struct {
			if( source == null )
				throw new ArgumentNullException( "source" );

			SetData<T>( buffer, byteOffset, source, 0, source.Length, stride );
		}
		
		/// <summary>
		/// Sets data in a buffer.
		/// </summary>
		/// <typeparam name="T">Type of data to place in the buffer.</typeparam>
		/// <param name="buffer">Buffer into which to copy the data.</param>
		/// <param name="byteOffset">Offset into the buffer, in bytes, at which copying should begin.</param>
		/// <param name="source">Array containing elements to copy into the buffer.</param>
		/// <param name="sourceIndex">Index into <paramref name="source"/> at which copying should begin.</param>
		/// <param name="count">Number of elements to copy.</param>
		/// <param name="stride">The offset between the start of one element and the start of the next. This must be at
		///   least the size of <typeparamref name="T"/>.</param>
		public static void SetData<T>( byte[] buffer, int byteOffset, T[] source, int sourceIndex, int count, int stride ) where T : struct {
			if( count == 0 )
				return;

			using( PinnedObject pinData = new PinnedObject( source ) ) {
				int sizeOfType = (int) (Marshal.UnsafeAddrOfPinnedArrayElement( source, 1 ).ToInt64() - Marshal.UnsafeAddrOfPinnedArrayElement( source, 0 ).ToInt64());

				if( stride < sizeOfType )
					throw new ArgumentException( "The stride is less than the size of the given elements." );

				new ArraySegment<T>( source, sourceIndex, count );
				new ArraySegment<byte>( buffer, byteOffset, (count - 1) * stride + sizeOfType );
			
				if( stride == sizeOfType ) {
					// Direct copy: fastest method
					Marshal.Copy( (IntPtr) (pinData.Pointer.ToInt64() + sourceIndex * sizeOfType), buffer, byteOffset, count * stride );
				}
				else {
#if ALLOW_UNSAFE
					unsafe {
						// Copy individual elements: slower, but not necessarily by much
						fixed( byte *pinDest = &buffer[0] ) {
							for( int i = 0; i < count; i++ ) {
								byte *dest = pinDest + byteOffset + stride * i;
								byte *src = (byte*)(void*) pinData.Pointer + (sourceIndex + i) * sizeOfType;
								int byteCount = sizeOfType;

								while( byteCount-- != 0 )
									*dest++ = *src++;
							}
						}
					}
#else
					// Copy individual elements: slower, but not necessarily by much
					for( int i = 0; i < count; i++ ) {
						Marshal.Copy( (IntPtr) (pinData.Pointer.ToInt64() + (sourceIndex + i) * sizeOfType), buffer, byteOffset + stride * i, sizeOfType );
					}
#endif
				}
			}
		}

		public static void GetData<T>( byte[] buffer, int byteOffset, T[] destination, int destinationIndex, int count, int stride ) where T : struct {
			if( count == 0 )
				return;

			using( PinnedObject pinData = new PinnedObject( destination ) ) {
				int sizeOfType = (int) (Marshal.UnsafeAddrOfPinnedArrayElement( destination, 1 ).ToInt64() - Marshal.UnsafeAddrOfPinnedArrayElement( destination, 0 ).ToInt64());

				if( stride < sizeOfType )
					throw new ArgumentException( "The stride is less than the size of the given elements." );

				new ArraySegment<T>( destination, destinationIndex, count );
				new ArraySegment<byte>( buffer, byteOffset, (count - 1) * stride + sizeOfType );

				if( stride == sizeOfType ) {
					// Direct copy: fastest method
					Marshal.Copy( buffer, byteOffset, (IntPtr) (pinData.Pointer.ToInt64() + destinationIndex * sizeOfType), count * stride );
				}
				else {
#if ALLOW_UNSAFE
					unsafe {
						// Copy individual elements: slower, but not necessarily by much
						fixed( byte *pinSrc = &buffer[0] ) {
							for( int i = 0; i < count; i++ ) {
								byte *src = pinSrc + byteOffset + stride * i;
								byte *dest = (byte*)(void*) pinData.Pointer + (destinationIndex + i) * sizeOfType;
								int byteCount = sizeOfType;

								while( byteCount-- != 0 )
									*dest++ = *src++;
							}
						}
					}
#else
					// Copy individual elements: slower, but not necessarily by much
					for( int i = 0; i < count; i++ ) {
						Marshal.Copy( buffer, byteOffset + stride * i, (IntPtr) (pinData.Pointer.ToInt64() + (destinationIndex + i) * sizeOfType), sizeOfType );
					}
#endif
				}
			}
		}
		
		public static void Copy( byte[] sourceBuffer, int sourceOffset, int sourceStride, byte[] destinationBuffer, int destinationOffset, int destinationStride, int count ) {
			new ArraySegment<byte>( sourceBuffer, sourceOffset, sourceStride * count );
			new ArraySegment<byte>( destinationBuffer, destinationOffset, destinationStride * count );
			
			int smallerStride = Math.Min( sourceStride, destinationStride );
			
			for( int i = 0; i < count; i++ ) {
				for( int j = 0; j < smallerStride; j++ )
					destinationBuffer[destinationOffset + i * destinationStride + j] = sourceBuffer[sourceOffset + i * sourceStride + j];
			}
		}
	}
	
	/// <summary>
	/// Represents a subset of the values in a <see cref="StrideBuffer"/> or similar interleaved buffer.
	/// </summary>
	/// <typeparam name="T">Type of the data in the buffer.</typeparam>
	/// <remarks>Where the <see cref="StrideBuffer"/> class focuses on manipulating large blocks of data at once, this class
	///     allows you to access a subset of buffer data like an array. Accessing the buffer this way may be slower than using
	///     the <see cref="StrideBuffer"/> methods.</remarks>
	/// <example>This example, using the Fluggo graphics primitives library, stores and retrieves several floating-point values,
	///     and demonstrates how the <see cref="StrideBuffer"/> and <see cref="SubStrideBuffer{T}"/> classes make it easy to interpret
	///     the same data several different ways.<code>
	/// static void Main( string[] args ) {
	///     Vector3f[] positions = new Vector3f[] { new Vector3f( 1.0f, 2.0f, 3.0f ),
	///         new Vector3f( 4.0f, 5.0f, 6.0f ),
	///         new Vector3f( 7.0f, 8.0f, 9.0f ) };
	///	    		
	///     byte[] buffer = new byte[4 * 9];
	///     StrideBuffer.SetData&lt;Vector3f&gt;( buffer, 0, positions, 0, 3, 12 );
	///	
	///     foreach( Vector3f vector in new SubStrideBuffer&lt;Vector3f&gt;( buffer, 0, 3, 12 ) ) {
	///         Console.WriteLine( vector );
	///     }
	///	    
	///     Console.WriteLine();
	///	    
	///     foreach( float value in new SubStrideBuffer&lt;float&gt;( buffer, 0, 9, 4 ) ) {
	///         Console.WriteLine( value );
	///     }
	///	    
	///     Console.WriteLine();
	///	    
	///     foreach( byte value in new SubStrideBuffer&lt;byte&gt;( buffer, 0, 4 * 9, 1 ) ) {
	///         Console.Write( "{0:X2}", value );
	///     }
	///	    
	///     Console.WriteLine();
	///     Console.ReadKey();
	/// }
	/// </code></example>
	public class SubStrideBuffer<T> : IList<T> where T : struct, IEquatable<T> {
		byte[] _buffer;
		int _byteOffset, _stride, _elementCount, _elementSize;

		public SubStrideBuffer( byte[] buffer, int byteOffset, int elementCount, int stride ) {
			if( buffer == null )
				throw new ArgumentNullException( "buffer" );
				
			new ArraySegment<byte>( buffer, byteOffset, elementCount * stride );
				
			T[] test = new T[2];
			
			using( PinnedObject pin = new PinnedObject( test ) ) {
				_elementSize = (int) (Marshal.UnsafeAddrOfPinnedArrayElement( test, 1 ).ToInt64() - Marshal.UnsafeAddrOfPinnedArrayElement( test, 0 ).ToInt64());
			}
			
			if( stride < _elementSize )
				throw new ArgumentException( "The given stride was less than the element size." );
				
			_buffer = buffer;
			_byteOffset = byteOffset;
			_stride = stride;
			_elementCount = elementCount;
		}
		
		public int IndexOf( T item ) {
			T[] destination = new T[1];

			using( PinnedObject pinData = new PinnedObject( destination ) ) {
				for( int i = 0; i < _elementCount; i++ ) {
					Marshal.Copy( _buffer, _byteOffset + _stride * i, pinData.Pointer, _elementSize );

					if( destination[0].Equals( item ) )
						return i;
				}
			}
			
			return -1;
		}

		public void Insert( int index, T item ) {
			throw new NotSupportedException();
		}

		public void RemoveAt( int index ) {
			throw new NotSupportedException();
		}

#if ALLOW_UNSAFE
		public unsafe T this[int index] {
			get {
				if( index < 0 || index >= _elementCount )
					throw new ArgumentOutOfRangeException( "index" );

				T[] values = new T[1];

				using( PinnedObject pin = new PinnedObject( values ) ) {
					fixed( byte* pinSrc = &_buffer[0] ) {
						byte* src = pinSrc + _byteOffset + _stride * index;
						byte* dest = (byte*)(void*) pin.Pointer;
						int byteCount = _elementSize;
						
						while( byteCount-- != 0 )
							*dest++ = *src++;
					}
				}

				return values[0];
			}
			set {
				if( index < 0 || index >= _elementCount )
					throw new ArgumentOutOfRangeException( "index" );

				T[] values = new T[1] { value };

				using( PinnedObject pin = new PinnedObject( values ) ) {
					fixed( byte* pinDest = &_buffer[0] ) {
						byte* dest = pinDest + _byteOffset + _stride * index;
						byte* src = (byte*) (void*) pin.Pointer;
						int byteCount = _elementSize;

						while( byteCount-- != 0 )
							*dest++ = *src++;
					}
				}
			}
		}
#else
		public T this[int index] {
			get {
				if( index < 0 || index >= _elementCount )
					throw new ArgumentOutOfRangeException( "index" );
				
				T[] values = new T[1];
				
				using( PinnedObject pin = new PinnedObject( values ) ) {
					Marshal.Copy( _buffer, _byteOffset + _stride * index, pin.Pointer, _elementSize );
				}
				
				return values[0];
			}
			set {
				if( index < 0 || index >= _elementCount )
					throw new ArgumentOutOfRangeException( "index" );

				T[] values = new T[1] { value };

				using( PinnedObject pin = new PinnedObject( values ) ) {
					Marshal.Copy( pin.Pointer, _buffer, _byteOffset + _stride * index, _elementSize );
				}
			}
		}
#endif

		public void Add( T item ) {
			throw new NotSupportedException();
		}

		public void Clear() {
			throw new NotSupportedException();
		}

		public bool Contains( T item ) {
			return IndexOf( item ) != -1;
		}

		public void CopyTo( T[] array, int arrayIndex ) {
			new ArraySegment<T>( array, arrayIndex, _elementCount );
			
			using( PinnedObject pinData = new PinnedObject( array ) ) {
				if( _stride == _elementSize ) {
					// Direct copy: fastest method
					Marshal.Copy( _buffer, _byteOffset, (IntPtr) (pinData.Pointer.ToInt64() + arrayIndex * _elementSize), _elementCount * _stride );
				}
				else {
					// Copy individual elements: slower, but not necessarily by much
					for( int i = 0; i < _elementCount; i++ ) {
						Marshal.Copy( _buffer, _byteOffset + _stride * i, (IntPtr) (pinData.Pointer.ToInt64() + (arrayIndex + i) * _elementSize), _elementSize );
					}
				}
			}
		}

		public int Count {
			get { return _elementCount; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool Remove( T item ) {
			throw new NotSupportedException();
		}

		public IEnumerator<T> GetEnumerator() {
			for( int i = 0; i < _elementCount; i++ )
				yield return this[i];
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
