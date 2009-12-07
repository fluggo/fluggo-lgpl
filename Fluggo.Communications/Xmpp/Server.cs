using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace Fluggo.Communications.Xmpp {
	/// <summary>
	/// Represents a single XMPP server instance.
	/// </summary>
	public class XmppServer {
		HostNameList _alternateHostNames, _expiredHostNames;
		RandomNumberGenerator _rng;
		string _hostName;
		bool _useAsync;
		Dictionary<string,XmppServerConnection> _connectionsById = new Dictionary<string,XmppServerConnection>();

		/// <summary>
		/// Creates a new instance of the <see cref='XmppServer'/> class.
		/// </summary>
		/// <param name="hostName">The default host name to give to clients.</param>
		/// <param name="useAsync">True to use asynchronous operations, false otherwise.</param>
		public XmppServer( string hostName, bool useAsync ) {
			if( useAsync )
				throw new NotImplementedException( "Asynchronous operations are not implemented." );

			if( Uri.CheckHostName( hostName ) == UriHostNameType.Unknown )
				throw new ArgumentException( "Invalid host name.", "hostName" );

			_hostName = hostName;
			_rng = RandomNumberGenerator.Create();
		}

		/// <summary>
		/// Gets the default host name of this server.
		/// </summary>
		/// <value>The default host name of this server.</value>
		public string HostName {
			get {
				return _hostName;
			}
		}

		/// <summary>
		/// Gets the list of alternative host names for this server.
		/// </summary>
		/// <value>The list of alternative host names for this server.</value>
		public HostNameList AlternateHostNames {
			get {
				return _alternateHostNames;
			}
		}

		/// <summary>
		/// Gets the list of expired host names for this server.
		/// </summary>
		/// <value>The list of expired host names for this server.</value>
		public HostNameList ExpiredHostNames {
			get {
				return _expiredHostNames;
			}
		}

		private string CreateStreamId() {
			byte[] randBytes = new byte[4];
			_rng.GetBytes( randBytes );
			
			return "stream" + BitConverter.ToUInt32( randBytes, 0 ).ToString();
		}
		
		public XmppServerConnection AcceptConnection( Stream stream ) {
			return new XmppServerConnection( this, stream, _useAsync );
		}
		
		internal string AssignStreamId( XmppServerConnection connection ) {
			lock( _connectionsById ) {
				for( int i = 0; i < 10; i++ ) {
					string streamId = CreateStreamId();
					
					if( _connectionsById.ContainsKey( streamId ) )
						continue;
						
					_connectionsById[streamId] = connection;
					return streamId;
				}
				
				throw new ResourceConstraintException( "Server is too busy. Please try again later.\nThe specific error was: Unable to create a stream ID." );
			}
		}
	}

	/// <summary>
	/// Contains a list of host names or IP addresses.
	/// </summary>
	public class HostNameList : Collection<string> {
		protected override void InsertItem( int index, string item ) {
			UriHostNameType type = Uri.CheckHostName( item );

			if( type == UriHostNameType.Unknown )
				throw new ArgumentException( "\"" + item + "\" is not a valid host name." );

			base.InsertItem( index, item );
		}

		protected override void SetItem( int index, string item ) {
			UriHostNameType type = Uri.CheckHostName( item );

			if( type == UriHostNameType.Unknown )
				throw new ArgumentException( "\"" + item + "\" is not a valid host name." );

			base.SetItem( index, item );
		}
	}

	public sealed class JabberID : IEquatable<JabberID> {
		static readonly string __jabberScheme = "jabber";
		Uri _uri;

		/// <summary>
		/// Creates a new instance of the <see cref='JabberID'/> class.
		/// </summary>
		/// <param name="domain">Domain of the jabber ID.</param>
		/// <exception cref='ArgumentNullException'><paramref name='domain'/> is <see langword='null'/>.</exception>
		public JabberID( string domain ) {
			if( domain == null )
				throw new ArgumentNullException( "domain" );
			
			_uri = new Uri( __jabberScheme + Uri.SchemeDelimiter + domain );
		}
		
		public JabberID( string node, string domain, string resource ) {
			if( domain == null )
				throw new ArgumentNullException( "domain" );

			string uri = domain;
			
			if( node != null )
				uri = node + "@" + uri;
				
			if( resource != null )
				uri += "/" + resource.TrimStart( '/' );
				
			_uri = new Uri( __jabberScheme + Uri.SchemeDelimiter + uri );
		}
		
		public string Node {
			get { return _uri.UserInfo; }
		}
		
		public string Domain {
			get { return _uri.Host; }
		}
		
		public string Resource {
			get { return _uri.AbsolutePath; }
		}

		public override string ToString() {
			string result = Domain;
			
			if( _uri.UserInfo.Length != 0 )
				result = _uri.UserInfo + "@" + result;
				
			if( _uri.AbsolutePath != "/" )
				result += _uri.AbsolutePath;
			
			return result;
		}

		public override bool Equals( object obj ) {
			JabberID other = obj as JabberID;
			
			if( other == null )
				return false;
				
			return Equals( other );
		}
		
		public bool Equals( JabberID other ) {
			if( other == null )
				return false;
				
			return Node == other.Node && Domain == other.Domain && Resource == other.Resource;
		}

		public override int GetHashCode() {
			return Node.GetHashCode() ^ Domain.GetHashCode() ^ Resource.GetHashCode();
		}
		
		public static bool operator ==( JabberID j1, JabberID j2 ) {
			if( j1 == null )
				return j2 == null;
				
			return j1.Equals( j2 );
		}
		
		public static bool operator !=( JabberID j1, JabberID j2 ) {
			return !(j1 == j2);
		}
	}
	
	public abstract class XmppStreamElement {
		public abstract XmlElement CreateElement();
	}
	
	
	
	public class XmppMessage : XmppStreamElement {
		JabberID _from, _to;
		
		public sealed override XmlElement CreateElement() {
			XmlDocument document = new XmlDocument();
			XmlElement element = document.CreateElement( Xmpp.Jabber.Tags.Message, Xmpp.Jabber.ClientNamespace );
			
			if( _to != null ) {
				XmlAttribute attr = document.CreateAttribute( Xmpp.Jabber.Attributes.To, Xmpp.Jabber.ClientNamespace );
				attr.Value = _to.ToString();
				element.Attributes.Append( attr );
			}

			if( _from != null ) {
				XmlAttribute attr = document.CreateAttribute( Xmpp.Jabber.Attributes.From, Xmpp.Jabber.ClientNamespace );
				attr.Value = _from.ToString();
				element.Attributes.Append( attr );
			}
			
			foreach( XmlNode node in CreateContentNodes( document ) )
				element.AppendChild( node );
				
			return element;
		}
		
		protected virtual List<XmlNode> CreateContentNodes( XmlDocument document ) {
			return new List<XmlNode>();
		}
	}
	
	public class XmppRequest : XmppStreamElement {
		public sealed override XmlElement CreateElement() {
			throw new Exception( "The method or operation is not implemented." );
		}
	}
	
	public class XmppResponse : XmppStreamElement {
		public sealed override XmlElement CreateElement() {
			throw new Exception( "The method or operation is not implemented." );
		}
	}
	
	/// <summary>
	/// Represents a single XMPP session from the server's viewpoint.
	/// </summary>
	public class XmppServerConnection {
		XmppServer _server;
		XmlReader _reader;
		XmlWriter _writer;
		Stream _stream;
		bool _useAsync, _isOpen;
		SessionTagInfo _sessionInfo;
		string _streamId;

		struct SessionTagInfo {
			public int ClientVersionMajor, ClientVersionMinor;
			public CultureInfo Language;
			public string TargetHost;
		}

		internal XmppServerConnection( XmppServer server, Stream stream, bool useAsync ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );

			if( useAsync )
				throw new NotImplementedException( "Asynchronous operations are not implemented." );

			if( server == null )
				throw new ArgumentNullException( "server" );

			_stream = stream;
			_server = server;

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
			readerSettings.XmlResolver = null;

			_writer = XmlWriter.Create( stream, writerSettings );
			_reader = XmlReader.Create( stream, readerSettings );

			ReadStreamOpen();
			
			_streamId = _server.AssignStreamId( this );

			SendStreamOpen();
		}

		/// <summary>
		/// Reads the opening &lt;stream&gt; tag.
		/// </summary>
		private void ReadStreamOpen() {
			try {
				while( _reader.Read() ) {
					switch( _reader.NodeType ) {
						case XmlNodeType.Comment:
						case XmlNodeType.DocumentType:
						case XmlNodeType.EndEntity:
						case XmlNodeType.Entity:
						case XmlNodeType.EntityReference:
						case XmlNodeType.Notation:
						case XmlNodeType.ProcessingInstruction:
							throw new RestrictedXmlException();
					}

					if( _reader.NodeType == XmlNodeType.XmlDeclaration )
						continue;

					// This should be the opening tag
					break;
				}

				if( _reader.NodeType != XmlNodeType.Element || _reader.LocalName != Xmpp.Streams.Tags.Stream )
					throw new InvalidXmlException();

				if( _reader.NamespaceURI != Xmpp.Streams.Namespace )
					throw new InvalidNamespaceException();

				// Check the "version" attribute, if any
				string version = _reader[Xmpp.Streams.Attributes.Version];

				if( version != null ) {
					string[] versionSplit = version.Split( new char[] { '.' }, 2 );

					if( versionSplit.Length == 2 ) {
						if( !int.TryParse( versionSplit[0], out _sessionInfo.ClientVersionMajor ) ||
								!int.TryParse( versionSplit[1], out _sessionInfo.ClientVersionMinor ) ) {
							_sessionInfo.ClientVersionMajor = 0;
							_sessionInfo.ClientVersionMinor = 0;
						}
					}
				}

				if( _sessionInfo.ClientVersionMajor < 1 )
					throw new UnsupportedVersionException();

				// Check the "to" attribute, if any
				string to = _reader[Xmpp.Streams.Attributes.To];

				if( to != null ) {
					// Validate the hostname
					if( to == _server.HostName || _server.AlternateHostNames.Contains( to ) ) {
						// Avoid memory leaks by keeping only the existing string
						_sessionInfo.TargetHost = string.Intern( to );
					}
					else {
						// Is it one we moved away from?
						if( _server.ExpiredHostNames.Contains( to ) )
							throw new HostGoneException();

						// Bad hostname
						throw new HostUnknownException();
					}
				}

				// Check the language attribute, if any
				string language = _reader[Xmpp.Xml.Attributes.Language, Xmpp.Xml.Namespace];

				if( language != null ) {
					try {
						_sessionInfo.Language = CultureInfo.GetCultureInfoByIetfLanguageTag( language );
					}
					catch( ArgumentException ) {
						throw new BadFormatException( "The client requested an unrecognized language: " + language );
					}
				}
			}
			catch( XmppStreamException ex ) {
				SendFatalStreamError( ex.TagName, ex.Message );
				throw;
			}
		}
		
		private void SendStreamOpen() {
			_writer.WriteStartElement( Xmpp.Streams.Prefix, Xmpp.Streams.Tags.Stream, Xmpp.Streams.Namespace );
			_writer.WriteAttributeString( "xmlns", null, Xmpp.Jabber.ClientNamespace );
			_writer.WriteAttributeString( "xmlns", Xmpp.Streams.Prefix, null, Xmpp.Streams.Namespace );
			_writer.WriteAttributeString( Xmpp.Streams.Attributes.From, _sessionInfo.TargetHost );
			_writer.WriteAttributeString( Xmpp.Streams.Attributes.Id, _streamId );
			_writer.WriteAttributeString( Xmpp.Streams.Attributes.Version, "1.0" );
			_writer.WriteStartElement( Xmpp.Streams.Prefix, Xmpp.Streams.Tags.Features, Xmpp.Streams.Namespace );
			_writer.WriteEndElement();
			_writer.Flush();
		}

		private void SendFatalStreamError( string errorElement, string errorMessage ) {
			if( !_isOpen ) {
				// Send the opening stream element so that we can send an error message
				_writer.WriteStartElement( Xmpp.Streams.Prefix, Xmpp.Streams.Tags.Stream, Xmpp.Streams.Namespace );
				_writer.WriteAttributeString( "xmlns", null, Xmpp.Jabber.ClientNamespace );
				_writer.WriteAttributeString( "xmlns", Xmpp.Streams.Prefix, null, Xmpp.Streams.Namespace );
				_writer.WriteAttributeString( Xmpp.Streams.Attributes.From, _server.HostName );
				_writer.WriteAttributeString( Xmpp.Streams.Attributes.Version, "1.0" );
			}

			_writer.WriteStartElement( Xmpp.Streams.Prefix, Xmpp.Streams.Tags.Error, Xmpp.Streams.Namespace );

			_writer.WriteStartElement( null, errorElement, Xmpp.StreamErrors.Namespace );
			_writer.WriteAttributeString( "xmlns", null, Xmpp.StreamErrors.Namespace );
			_writer.WriteEndElement();

			if( errorMessage != null ) {
				_writer.WriteStartElement( null, Xmpp.StreamErrors.Tags.Text, Xmpp.StreamErrors.Namespace );
				_writer.WriteAttributeString( "xmlns", null, Xmpp.StreamErrors.Namespace );
				_writer.WriteString( errorMessage );
				_writer.WriteEndElement();
			}

			_writer.WriteEndElement();
			_writer.WriteEndElement();
			
			_writer.Flush();
			_stream.Flush();
			
			_stream.Close();
		}

		/// <summary>
		/// Gets the host name requested by the client.
		/// </summary>
		/// <value>The host name requested by the client.</value>
		public string TargetHostName {
			get {
				return _sessionInfo.TargetHost;
			}
		}

		/// <summary>
		/// Gets the IETF language tag for the stream, if any.
		/// </summary>
		/// <value>The IETF language tag for the stream, if any, or <see langword='null'/> if one is not specified.</value>
		public string IetfLanguageTag {
			get {
				if( _sessionInfo.Language == null )
					return null;

				return _sessionInfo.Language.IetfLanguageTag;
			}
		}

		/// <summary>
		/// Gets the <see cref="TextInfo"/> for the stream's language, if any.
		/// </summary>
		/// <value>The <see cref="TextInfo"/> for the stream's language, if any, or <see langword='null'/> if a stream-level language has not been set.</value>
		/// <remarks>Language identifiers can be set at the stream or message level, or not at all. This property returns information
		///     about any language set at the stream level.
		///   <para>The returned <see cref="TextInfo"/> object contains information on how to deal with text in the stream's language, but the
		///     language identifier does not specify enough information to establish a <see cref="CultureInfo"/> instance for the stream.</para></remarks>
		public TextInfo LanguageInfo {
			get {
				if( _sessionInfo.Language == null )
					return null;

				return _sessionInfo.Language.TextInfo;
			}
		}
	}
}
