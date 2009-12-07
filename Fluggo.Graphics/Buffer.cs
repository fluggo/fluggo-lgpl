using System;
using System.Runtime.InteropServices;

namespace Fluggo.Graphics.Common {
	public enum IndexElementSize {
		SixteenBits,
		ThirtyTwoBits
	}
	
	public abstract class PrimitiveBuffer : IDisposable {
		public abstract void GetData<T>( int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride ) where T : struct;

		public void GetData<T>( T[] data ) where T : struct {
			GetData<T>( data, 0, data.Length );
		}

		public void GetData<T>( T[] data, int startIndex, int elementCount ) where T : struct {
			GetData<T>( 0, data, startIndex, elementCount, Marshal.SizeOf( typeof(T) ) );
		}

		public abstract void SetData<T>( int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options ) where T : struct;

		public void SetData<T>( T[] data ) where T : struct {
			SetData<T>( data, 0, data.Length, SetDataOptions.None );
		}

		public void SetData<T>( T[] data, int startIndex, int elementCount, SetDataOptions options ) where T : struct {
			SetData<T>( 0, data, startIndex, elementCount, Marshal.SizeOf( typeof(T) ), options );
		}
		
		public void Dispose() {
			Dispose( true );
		}
		
		protected virtual void Dispose( bool disposing ) {
			if( !disposing ) {
				GC.SuppressFinalize( this );
			}
		}
		
		public abstract int SizeInBytes { get; }
		
		~PrimitiveBuffer() {
			Dispose( false );
		}
	}
	
	public abstract class VertexBuffer : PrimitiveBuffer {
	}
	
	public abstract class IndexBuffer : PrimitiveBuffer {
		public abstract IndexElementSize IndexElementSize { get; }
	}
}