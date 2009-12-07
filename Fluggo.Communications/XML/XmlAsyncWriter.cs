/*
	Fluggo Communications Library
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
using System.Xml;
using System.IO;
using System.Text;
using Fluggo.Communications;

namespace Fluggo.Xml {
	public abstract class XmlAsyncWriter : XmlWriter {
		public virtual IAsyncResult BeginFlush( AsyncCallback callback, object state ) {
			EmptyAsyncResult<bool> read = new EmptyAsyncResult<bool>( callback, state );
			
			try {
				Flush();
				read.Complete( false );
			}
			catch( Exception ex ) {
				read.CompleteError( ex );
			}
			
			return read;
		}
		
		public virtual void EndFlush( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyAsyncResult<bool> read = result as EmptyAsyncResult<bool>;

			if( read == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginFlush call on this object.", "result" );

			read.End();
		}
	}
	
	class MovableTextWriter : TextWriter {
		TextWriter _root;
		bool _closed;
		object _lock = new object();

		public MovableTextWriter( TextWriter writer ) {
			if( writer == null )
				throw new ArgumentNullException( "writer" );
			
			_root = writer;
		}
		
		public void SetWriter( TextWriter writer ) {
			if( _closed )
				throw new ObjectDisposedException( null );
			
			lock( _lock ) {
				_root = writer;
			}
		}
		
		public override void Close() {
			lock( _lock ) {
				_root.Close();
				_closed = true;
			}
		}
		
		public override void Write( string value ) {
			_root.Write( value );
		}

		public override void Write( char value ) {
			_root.Write( value );
		}

		public override void Write( char[] buffer, int index, int count ) {
			_root.Write( buffer, index, count );
		}
		
		public override Encoding Encoding {
			get { return _root.Encoding; }
		}
	}
	
	public class XmlTextAsyncWriter : XmlAsyncWriter {
		XmlTextWriter _writer;
		StringWriter _stringWriter;
		MovableTextWriter _movableWriter;
		Encoding _encoding;
		Stream _root;
		object _lock = new object();
		
		public XmlTextAsyncWriter( Stream stream, Encoding encoding ) {
			_root = stream;
			_encoding = encoding;
			_stringWriter = new StringWriter();
			_movableWriter = new MovableTextWriter( _stringWriter );
			_writer = new XmlTextWriter( _movableWriter );
		}
		
		public override void Close() {
			_writer.Close();
		}

		public override IAsyncResult BeginFlush( AsyncCallback callback, object state ) {
			string result;
			
			lock( _lock ) {
				_writer.Flush();
				result = _stringWriter.ToString();
				_stringWriter = new StringWriter();
				_movableWriter.SetWriter( _stringWriter );
			}
			
			byte[] buffer = _encoding.GetBytes( result );
			return _root.BeginWrite( buffer, 0, buffer.Length, callback, state );
		}

		public override void EndFlush( IAsyncResult result ) {
			_root.EndWrite( result );
		}
		
		public override void Flush() {
			EndFlush( BeginFlush( null, null ) );
		}

		public override string LookupPrefix( string ns ) {
			return _writer.LookupPrefix( ns );
		}

		public override void WriteBase64( byte[] buffer, int index, int count ) {
			_writer.WriteBase64( buffer, index, count );
		}

		public override void WriteCData( string text ) {
			_writer.WriteCData( text );
		}

		public override void WriteCharEntity( char ch ) {
			_writer.WriteCharEntity( ch );
		}

		public override void WriteChars( char[] buffer, int index, int count ) {
			_writer.WriteChars( buffer, index, count );
		}

		public override void WriteComment( string text ) {
			_writer.WriteComment( text );
		}

		public override void WriteDocType( string name, string pubid, string sysid, string subset ) {
			_writer.WriteDocType( name, pubid, sysid, subset );
		}

		public override void WriteEndAttribute() {
			_writer.WriteEndAttribute();
		}

		public override void WriteEndDocument() {
			_writer.WriteEndDocument();
		}

		public override void WriteEndElement() {
			_writer.WriteEndElement();
		}

		public override void WriteEntityRef( string name ) {
			_writer.WriteEntityRef( name );
		}

		public override void WriteFullEndElement() {
			_writer.WriteFullEndElement();
		}

		public override void WriteProcessingInstruction( string name, string text ) {
			_writer.WriteProcessingInstruction( name, text );
		}

		public override void WriteRaw( string data ) {
			_writer.WriteRaw( data );
		}

		public override void WriteRaw( char[] buffer, int index, int count ) {
			_writer.WriteRaw( buffer, index, count );
		}

		public override void WriteStartAttribute( string prefix, string localName, string ns ) {
			_writer.WriteStartAttribute( prefix, localName, ns );
		}

		public override void WriteStartDocument( bool standalone ) {
			_writer.WriteStartDocument( standalone );
		}

		public override void WriteStartDocument() {
			_writer.WriteStartDocument();
		}

		public override void WriteStartElement( string prefix, string localName, string ns ) {
			_writer.WriteStartElement( prefix, localName, ns );
		}

		public override WriteState WriteState {
			get { return _writer.WriteState; }
		}

		public override void WriteString( string text ) {
			_writer.WriteString( text );
		}

		public override void WriteSurrogateCharEntity( char lowChar, char highChar ) {
			_writer.WriteSurrogateCharEntity( lowChar, highChar );
		}

		public override void WriteWhitespace( string ws ) {
			_writer.WriteWhitespace( ws );
		}
	}
}