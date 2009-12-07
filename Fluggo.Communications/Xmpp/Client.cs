using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Fluggo.Communications.Xmpp {
	public class XmppClient {
		Stream _stream;
		XmlWriter _writer;
		TextWriter _textWriter;
		XmlReader _reader;
		string _targetHost;
		bool _useAsync;

		public XmppClient( bool useAsync ) {
			_useAsync = useAsync;
		}

		public void Connect( Stream stream, string targetHost ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );

			if( targetHost == null )
				throw new ArgumentNullException( "targetHost" );

			if( _useAsync )
				throw new NotImplementedException( "Asynchronous operations are not implemented." );

			_targetHost = targetHost;
			_stream = stream;

			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.ConformanceLevel = ConformanceLevel.Document;
			writerSettings.Encoding = Encoding.UTF8;
			writerSettings.Indent = false;
			writerSettings.OmitXmlDeclaration = false;

			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.ConformanceLevel = ConformanceLevel.Document;
			readerSettings.CloseInput = false;
			readerSettings.IgnoreComments = false;
			readerSettings.IgnoreProcessingInstructions = false;
			readerSettings.ProhibitDtd = true;
			readerSettings.ValidationType = ValidationType.None;

			// Write the opening 
			_writer = XmlWriter.Create( _stream, writerSettings );
			WriteStreamOpen();
			_writer.Flush();

			_reader = XmlReader.Create( _stream, readerSettings );
		}

		private void WriteStreamOpen() {
			// Write the opening tag and attributes
			_writer.WriteStartElement( Xmpp.Streams.Prefix, Xmpp.Streams.Tags.Stream, Xmpp.Streams.Namespace );
			_writer.WriteAttributeString( "xmlns", null, Xmpp.Jabber.ClientNamespace );
			_writer.WriteAttributeString( "xmlns", Xmpp.Streams.Prefix, null, Xmpp.Streams.Namespace );
			_writer.WriteAttributeString( Xmpp.Streams.Attributes.To, _targetHost );
			_writer.WriteAttributeString( Xmpp.Streams.Attributes.Version, "1.0" );

			// Write the end of the start tag by appending an empty text node
			_writer.WriteString( string.Empty );
		}

		private void ReadStreamOpen() {
			_reader.Read();
		}
	}
}
