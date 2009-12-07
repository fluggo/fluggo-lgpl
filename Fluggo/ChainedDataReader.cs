using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Fluggo {
	public class ChainedDataReader : IDataReader {
		IDataReader[] _list;
		IDataReader _current;
		int _position = 0;
		
		public ChainedDataReader( IDataReader[] readers ) {
			_list = readers;
			
			if( _list.Length != 0 )
				_current = _list[0];
		}
		
		public void Close() {
			if( _current != null ) {
				_current.Close();
				_current.Dispose();
				_current = null;
			}
		}

		public int Depth {
			get { return 0; }
		}

		public DataTable GetSchemaTable() {
			return _current.GetSchemaTable();
		}

		public bool IsClosed {
			get { return _current.IsClosed; }
		}

		public bool NextResult() {
			throw new NotSupportedException();
		}

		public bool Read() {
			for( ;; ) {
				if( _current != null ) {
					// Return the next item from the current enumerator, if available
					if( _current.Read() )
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
				_current = _list[_position];
			}
		}

		public int RecordsAffected {
			get { throw new NotSupportedException(); }
		}

		public void Dispose() {
			Close();
		}

		public int FieldCount {
			get { if( _list.Length != 0 ) return _list[0].FieldCount; else return 0; }
		}

		public bool GetBoolean( int i ) {
			return _current.GetBoolean( i );
		}

		public byte GetByte( int i ) {
			return _current.GetByte( i );
		}

		public long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length ) {
			return _current.GetBytes( i, fieldOffset, buffer, bufferoffset, length );
		}

		public char GetChar( int i ) {
			return _current.GetChar( i );
		}

		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length ) {
			return _current.GetChars( i, fieldoffset, buffer, bufferoffset, length );
		}

		public IDataReader GetData( int i ) {
			return _current.GetData( i );
		}

		public string GetDataTypeName( int i ) {
			return _current.GetDataTypeName( i );
		}

		public DateTime GetDateTime( int i ) {
			return _current.GetDateTime( i );
		}

		public decimal GetDecimal( int i ) {
			return _current.GetDecimal( i );
		}

		public double GetDouble( int i ) {
			return _current.GetDouble( i );
		}

		public Type GetFieldType( int i ) {
			return _current.GetFieldType( i );
		}

		public float GetFloat( int i ) {
			return _current.GetFloat( i );
		}

		public Guid GetGuid( int i ) {
			return _current.GetGuid( i );
		}

		public short GetInt16( int i ) {
			return _current.GetInt16( i );
		}

		public int GetInt32( int i ) {
			return _current.GetInt32( i );
		}

		public long GetInt64( int i ) {
			return _current.GetInt64( i );
		}

		public string GetName( int i ) {
			return _current.GetName( i );
		}

		public int GetOrdinal( string name ) {
			return _current.GetOrdinal( name );
		}

		public string GetString( int i ) {
			return _current.GetString( i );
		}

		public object GetValue( int i ) {
			return _current.GetValue( i );
		}

		public int GetValues( object[] values ) {
			return _current.GetValues( values );
		}

		public bool IsDBNull( int i ) {
			return _current.IsDBNull( i );
		}

		public object this[string name] {
			get { return _current[name]; }
		}

		public object this[int i] {
			get { return _current[i]; }
		}
	}
}
