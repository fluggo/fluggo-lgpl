using System;
using System.Runtime.Serialization;

namespace Fluggo.Communications.Xmpp {

	/// <summary>
	/// Base class for stream-level errors in XMPP.
	/// </summary>
	/// <remarks>All exceptions that derive from <see cref="XmppStreamException"/> cause the stream to end.</remarks>
	[Serializable]
	public abstract class XmppStreamException : Exception {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='XmppStreamException'/> class.
		/// </summary>
		public XmppStreamException() : base( "An error occured and the XMPP stream must be shut down." ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='XmppStreamException'/> class.
		/// </summary>
		public XmppStreamException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='XmppStreamException'/> class.
		/// </summary>
		public XmppStreamException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='XmppStreamException'/> class.
		/// </summary>
		protected XmppStreamException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		/// <summary>
		/// Gets the name of the stream-error tag that represents this exception.
		/// </summary>
		/// <value>The name of the stream-error tag that represents this exception.</value>
		/// <remarks>This tag must be a name from the xmpp-streams namespace.</remarks>
		public abstract string TagName { get; }
	}

	/// <summary>
	/// Represents the error that occurs when an XMPP stream contains XML that cannot be processed.
	/// </summary>
	/// <remarks>This is a general error. Where possible, you should use the more specific errors
	///	  <see cref="BadNamespacePrefixException"/>, <see cref="InvalidXmlException"/>, <see cref="RestrictedXmlException"/>,
	///	  <see cref="UnsupportedEncodingException"/>, and <see cref="XmlNotWellFormedException"/>.</remarks>
	[Serializable]
	public class BadFormatException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public BadFormatException() : base( "The XMPP stream contained XML that could not be processed." ) { }
		public BadFormatException( string message ) : base( message ) { }
		public BadFormatException( string message, System.Exception inner ) : base( message, inner ) { }
		protected BadFormatException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.BadFormat; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when an XMPP namespace prefix is incorrect or missing.
	/// </summary>
	[Serializable]
	public sealed class BadNamespacePrefixException : BadFormatException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public BadNamespacePrefixException() : base( "A namespace prefix in the XMPP stream was incorrect or missing." ) { }
		public BadNamespacePrefixException( string message ) : base( message ) { }
		public BadNamespacePrefixException( string message, Exception inner ) : base( message, inner ) { }
		private BadNamespacePrefixException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public override string TagName {
			get {
				return Xmpp.StreamErrors.Tags.BadNamespacePrefix;
			}
		}
	}

	/// <summary>
	/// Represents the error that occurs when an active stream is being closed because there is a conflict with a new stream.
	/// </summary>
	[Serializable]
	public sealed class ConflictException : XmppStreamException {
		/// <summary>
		/// Creates a new instance of the <see cref='ConflictException'/> class.
		/// </summary>
		public ConflictException() : base( "This stream is in conflict with a new stream and must be shut down." ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ConflictException'/> class.
		/// </summary>
		public ConflictException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ConflictException'/> class.
		/// </summary>
		public ConflictException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ConflictException'/> class.
		/// </summary>
		private ConflictException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.Conflict; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when a stream has been inactive for some period of time.
	/// </summary>
	[Serializable]
	public sealed class ConnectionTimeoutException : XmppStreamException {
		/// <summary>
		/// Creates a new instance of the <see cref='ConnectionTimeoutException'/> class.
		/// </summary>
		public ConnectionTimeoutException() : base( "The connection has been idle for some time and is being closed." ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ConnectionTimeoutException'/> class.
		/// </summary>
		public ConnectionTimeoutException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ConnectionTimeoutException'/> class.
		/// </summary>
		public ConnectionTimeoutException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ConnectionTimeoutException'/> class.
		/// </summary>
		private ConnectionTimeoutException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.ConnectionTimeout; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when a client requests a host name no longer hosted by the server.
	/// </summary>
	[Serializable]
	public sealed class HostGoneException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public HostGoneException() : base( "The client requested a host name no longer hosted by this server." ) { }
		public HostGoneException( string message ) : base( message ) { }
		public HostGoneException( string message, Exception inner ) : base( message, inner ) { }
		private HostGoneException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.HostGone; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when a client requests a host name not hosted by the server.
	/// </summary>
	[Serializable]
	public sealed class HostUnknownException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='HostUnknownException'/> class.
		/// </summary>
		public HostUnknownException() : base( "The client requested a host name not hosted by this server." ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='HostUnknownException'/> class.
		/// </summary>
		public HostUnknownException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='HostUnknownException'/> class.
		/// </summary>
		public HostUnknownException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='HostUnknownException'/> class.
		/// </summary>
		private HostUnknownException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.HostUnknown; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when a stanza sent between two servers lacks a "to" or a "from" attribute.
	/// </summary>
	[Serializable]
	public sealed class ImproperAddressingException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='ImproperAddressingException'/> class.
		/// </summary>
		public ImproperAddressingException() { }

		/// <summary>
		/// Creates a new instance of the <see cref='ImproperAddressingException'/> class.
		/// </summary>
		public ImproperAddressingException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ImproperAddressingException'/> class.
		/// </summary>
		public ImproperAddressingException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ImproperAddressingException'/> class.
		/// </summary>
		private ImproperAddressingException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.ImproperAddressing; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when the server has experienced a condition that prevents it from servicing the stream.
	/// </summary>
	[Serializable]
	public class InternalServerErrorException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='InternalServerErrorException'/> class.
		/// </summary>
		public InternalServerErrorException() { }

		/// <summary>
		/// Creates a new instance of the <see cref='InternalServerErrorException'/> class.
		/// </summary>
		public InternalServerErrorException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InternalServerErrorException'/> class.
		/// </summary>
		public InternalServerErrorException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InternalServerErrorException'/> class.
		/// </summary>
		private InternalServerErrorException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.InternalServerError; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when the JID or hostname provided in a 'from' address does not match an authorized JID or validated domain negotiated between servers via SASL or dialback, or between a client and a server via authentication and resource binding.
	/// </summary>
	[Serializable]
	public sealed class InvalidFromException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidFromException'/> class.
		/// </summary>
		public InvalidFromException() { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidFromException'/> class.
		/// </summary>
		public InvalidFromException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidFromException'/> class.
		/// </summary>
		public InvalidFromException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidFromException'/> class.
		/// </summary>
		private InvalidFromException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.InvalidFrom; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when the stream ID or dialback ID is invalid or does not match an ID previously provided.
	/// </summary>
	[global::System.Serializable]
	public sealed class InvalidIdException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidIdException'/> class.
		/// </summary>
		public InvalidIdException() { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidIdException'/> class.
		/// </summary>
		public InvalidIdException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidIdException'/> class.
		/// </summary>
		public InvalidIdException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidIdException'/> class.
		/// </summary>
		private InvalidIdException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.InvalidId; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when an incorrect namespace is specified.
	/// </summary>
	[Serializable]
	public sealed class InvalidNamespaceException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidNamespaceException'/> class.
		/// </summary>
		public InvalidNamespaceException() : base( "Invalid namespace in XMPP stream." ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidNamespaceException'/> class.
		/// </summary>
		public InvalidNamespaceException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidNamespaceException'/> class.
		/// </summary>
		public InvalidNamespaceException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidNamespaceException'/> class.
		/// </summary>
		private InvalidNamespaceException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.InvalidNamespace; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when invalid XML is encountered in an XMPP stream.
	/// </summary>
	[Serializable]
	public sealed class InvalidXmlException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidXmlException'/> class.
		/// </summary>
		public InvalidXmlException() : base( "Invalid XML was encountered in the XMPP stream." ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidXmlException'/> class.
		/// </summary>
		public InvalidXmlException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidXmlException'/> class.
		/// </summary>
		public InvalidXmlException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='InvalidXmlException'/> class.
		/// </summary>
		protected InvalidXmlException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.InvalidXml; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when the entity has attempted to send data before the stream has been authenticated, or otherwise is not authorized to perform an action related to stream negotiation.
	/// </summary>
	[Serializable]
	public class NotAuthorizedException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='NotAuthorizedException'/> class.
		/// </summary>
		public NotAuthorizedException() { }

		/// <summary>
		/// Creates a new instance of the <see cref='NotAuthorizedException'/> class.
		/// </summary>
		public NotAuthorizedException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='NotAuthorizedException'/> class.
		/// </summary>
		public NotAuthorizedException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='NotAuthorizedException'/> class.
		/// </summary>
		protected NotAuthorizedException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.NotAuthorized; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when an entity has violated some local service policy.
	/// </summary>
	[Serializable]
	public sealed class PolicyViolationException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='PolicyViolationException'/> class.
		/// </summary>
		public PolicyViolationException() { }

		/// <summary>
		/// Creates a new instance of the <see cref='PolicyViolationException'/> class.
		/// </summary>
		public PolicyViolationException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='PolicyViolationException'/> class.
		/// </summary>
		public PolicyViolationException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='PolicyViolationException'/> class.
		/// </summary>
		protected PolicyViolationException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.PolicyViolation; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when the server is unable to properly connect to a remote entity that is required for authentication or authorization.
	/// </summary>
	[Serializable]
	public sealed class RemoteConnectionFailedException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='RemoteConnectionFailedException'/> class.
		/// </summary>
		public RemoteConnectionFailedException() { }

		/// <summary>
		/// Creates a new instance of the <see cref='RemoteConnectionFailedException'/> class.
		/// </summary>
		public RemoteConnectionFailedException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='RemoteConnectionFailedException'/> class.
		/// </summary>
		public RemoteConnectionFailedException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='RemoteConnectionFailedException'/> class.
		/// </summary>
		protected RemoteConnectionFailedException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.RemoteConnectionFailed; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when the server lacks the system resources necessary to service the stream.
	/// </summary>
	[Serializable]
	public sealed class ResourceConstraintException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='ResourceConstraintException'/> class.
		/// </summary>
		public ResourceConstraintException() { }

		/// <summary>
		/// Creates a new instance of the <see cref='ResourceConstraintException'/> class.
		/// </summary>
		public ResourceConstraintException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ResourceConstraintException'/> class.
		/// </summary>
		public ResourceConstraintException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='ResourceConstraintException'/> class.
		/// </summary>
		protected ResourceConstraintException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.ResourceConstraint; }
		}
	}

	/// <summary>
	/// Represents the error that occurs when an entity has attempted to send restricted XML features such as a comment, processing instruction, DTD, entity reference, or unescaped character.
	/// </summary>
	[Serializable]
	public sealed class RestrictedXmlException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		/// Creates a new instance of the <see cref='RestrictedXmlException'/> class.
		/// </summary>
		public RestrictedXmlException() : base( "Restricted XML found in XMPP stream." ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='RestrictedXmlException'/> class.
		/// </summary>
		public RestrictedXmlException( string message ) : base( message ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='RestrictedXmlException'/> class.
		/// </summary>
		public RestrictedXmlException( string message, Exception inner ) : base( message, inner ) { }

		/// <summary>
		/// Creates a new instance of the <see cref='RestrictedXmlException'/> class.
		/// </summary>
		protected RestrictedXmlException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.RestrictedXml; }
		}
	}

	[global::System.Serializable]
	public sealed class SeeOtherHostException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public SeeOtherHostException() { }
		public SeeOtherHostException( string message ) : base( message ) { }
		public SeeOtherHostException( string message, Exception inner ) : base( message, inner ) { }
		protected SeeOtherHostException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.SeeOtherHost; }
		}
	}

	[global::System.Serializable]
	public sealed class SystemShutdownException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public SystemShutdownException() { }
		public SystemShutdownException( string message ) : base( message ) { }
		public SystemShutdownException( string message, Exception inner ) : base( message, inner ) { }
		protected SystemShutdownException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.SystemShutdown; }
		}
	}

	[global::System.Serializable]
	public sealed class UndefinedConditionException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public UndefinedConditionException() { }
		public UndefinedConditionException( string message ) : base( message ) { }
		public UndefinedConditionException( string message, Exception inner ) : base( message, inner ) { }
		protected UndefinedConditionException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.UndefinedCondition; }
		}
	}

	[global::System.Serializable]
	public sealed class UnsupportedEncodingException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public UnsupportedEncodingException() { }
		public UnsupportedEncodingException( string message ) : base( message ) { }
		public UnsupportedEncodingException( string message, Exception inner ) : base( message, inner ) { }
		protected UnsupportedEncodingException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.UnsupportedEncoding; }
		}
	}

	[global::System.Serializable]
	public sealed class UnsupportedStanzaTypeException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public UnsupportedStanzaTypeException() { }
		public UnsupportedStanzaTypeException( string message ) : base( message ) { }
		public UnsupportedStanzaTypeException( string message, Exception inner ) : base( message, inner ) { }
		protected UnsupportedStanzaTypeException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.UnsupportedStanzaType; }
		}
	}

	[global::System.Serializable]
	public sealed class UnsupportedVersionException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public UnsupportedVersionException() : base( "The specified XMPP version is unsupported." ) { }
		public UnsupportedVersionException( string message ) : base( message ) { }
		public UnsupportedVersionException( string message, Exception inner ) : base( message, inner ) { }
		protected UnsupportedVersionException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.UnsupportedVersion; }
		}
	}

	[global::System.Serializable]
	public sealed class XmlNotWellFormedException : XmppStreamException {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public XmlNotWellFormedException() { }
		public XmlNotWellFormedException( string message ) : base( message ) { }
		public XmlNotWellFormedException( string message, Exception inner ) : base( message, inner ) { }
		protected XmlNotWellFormedException(
		  SerializationInfo info,
		  StreamingContext context )
			: base( info, context ) { }

		public override string TagName {
			get { return Xmpp.StreamErrors.Tags.XmlNotWellFormed; }
		}
	}
}