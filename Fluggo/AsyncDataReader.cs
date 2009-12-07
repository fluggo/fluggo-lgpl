using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Fluggo {
	public class AsyncDataReader : IDataReader {
		AsynchronousQueue<object[]> _rowQueue;
		object[] _current;
		string[] _columnNames;

		public AsyncDataReader( params string[] columnNames ) {
			_rowQueue = new AsynchronousQueue<object[]>( -1 );
			_columnNames = columnNames;
		}
		
		public void Close() {
			AsynchronousQueue<object[]> rowQueue = _rowQueue;
			
			if( rowQueue != null )
				rowQueue.Enqueue( null );
		}
		
		public void Push( object[] row ) {
			_rowQueue.Enqueue( row );
		}
		
		public void Kill() {
			if( _rowQueue != null )
				_rowQueue.Enqueue( new object[] { new Exception() } );
		}
		
		public int Depth {
			get { return 0; }
		}

		public DataTable GetSchemaTable() {
			throw new NotImplementedException();
		}

		public bool IsClosed {
			get { return _rowQueue == null; }
		}

		public bool NextResult() {
			Close();
			return false;
		}

		public bool Read() {
			if( _rowQueue == null )
				throw new ObjectDisposedException( null );
			
			_current = _rowQueue.Dequeue();
			
			if( _current == null ) {
				_rowQueue = null;
				return false;
			}
			
			if( _current[0] is Exception ) {
				Close();
				throw (Exception) _current[0];
			}
			
			return true;
		}

		public int RecordsAffected {
			get { throw new NotImplementedException(); }
		}

		public void Dispose() {
			throw new Exception( "The method or operation is not implemented." );
		}

		public int FieldCount {
			get { return _columnNames.Length; }
		}
		
		private void EnsureCurrent() {
			if( _rowQueue == null )
				throw new ObjectDisposedException( string.Empty );
				
			if( _current == null )
				throw new InvalidOperationException();
		}

		public bool GetBoolean( int i ) {
			EnsureCurrent();
			return Convert.ToBoolean( _current[i] );
		}

		public byte GetByte( int i ) {
			EnsureCurrent();
			throw new Exception( "The method or operation is not implemented." );
		}

		public long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length ) {
			EnsureCurrent();
			throw new Exception( "The method or operation is not implemented." );
		}

		public char GetChar( int i ) {
			EnsureCurrent();
			return Convert.ToChar( _current[i] );
		}

		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length ) {
			throw new Exception( "The method or operation is not implemented." );
		}

		public IDataReader GetData( int i ) {
			throw new Exception( "The method or operation is not implemented." );
		}

		public string GetDataTypeName( int i ) {
			throw new Exception( "The method or operation is not implemented." );
		}

		public DateTime GetDateTime( int i ) {
			EnsureCurrent();
			return Convert.ToDateTime( _current[i] );
		}

		public decimal GetDecimal( int i ) {
			EnsureCurrent();
			return Convert.ToDecimal( _current[i] );
		}

		public double GetDouble( int i ) {
			EnsureCurrent();
			return Convert.ToDouble( _current[i] );
		}

		public Type GetFieldType( int i ) {
			EnsureCurrent();

			if( _current[i] == null )
				return typeof(DBNull);
			else
				return _current[i].GetType();
		}

		public float GetFloat( int i ) {
			EnsureCurrent();
			return Convert.ToSingle( _current[i] );
		}

		public Guid GetGuid( int i ) {
			EnsureCurrent();
			return (Guid) _current[i];
		}

		public short GetInt16( int i ) {
			EnsureCurrent();
			return Convert.ToInt16( _current[i] );
		}

		public int GetInt32( int i ) {
			EnsureCurrent();
			return Convert.ToInt32( _current[i] );
		}

		public long GetInt64( int i ) {
			EnsureCurrent();
			return Convert.ToInt64( _current[i] );
		}

		public string GetName( int i ) {
			return _columnNames[i];
		}

		public int GetOrdinal( string name ) {
			for( int i = 0; i < _columnNames.Length; i++ )
				if( _columnNames[i] == name )
					return i;
					
			return -1;
		}

		public string GetString( int i ) {
			EnsureCurrent();
			return Convert.ToString( _current[i] );
		}

		public object GetValue( int i ) {
			EnsureCurrent();
			return _current[i];
		}

		public int GetValues( object[] values ) {
			EnsureCurrent();
			Array.Copy( _current, values, values.Length );
			
			return values.Length;
		}

		public bool IsDBNull( int i ) {
			EnsureCurrent();
			return _current[i] == null;
		}

		public object this[string name] {
			get { return this[GetOrdinal(name)]; }
		}

		public object this[int i] {
			get { EnsureCurrent(); return _current[i]; }
		}
	}
}
