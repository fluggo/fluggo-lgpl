using System.Xml;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Fluggo.Communications.Xmpp {
	static class Xmpp {
		public static class Namespace {
			public static readonly string Session = "urn:ietf:params:xml:ns:xmpp-session";
			public static readonly string IqPrivacy = "jabber:iq:privacy";
			public static readonly string IqRoster = "jabber:iq:roster";
		}
		
		public static class Xml {
			public static readonly string Namespace = "http://www.w3.org/XML/1998/namespace";
			
			public static class Attributes {
				public static readonly string Language = "lang";
			}
		}
		
		public static class Jabber {
			public static readonly string ServerNamespace = "jabber:server";
			public static readonly string ClientNamespace = "jabber:client";
			
			public static class Tags {
				public static readonly string Message = "message";
				public static readonly string Iq = "iq";
				public static readonly string Presence = "presence";
				public static readonly string Thread = "thread";
				public static readonly string Show = "show";
				public static readonly string Status = "status";
				public static readonly string Priority = "priority";
				public static readonly string Error = "error";
				public static readonly string Body = "body";
				public static readonly string Head = "head";
			}
			
			public static class ErrorTypes {
				public static readonly string AuthenticationRequired = "auth";
				public static readonly string Unrecoverable = "cancel";
				public static readonly string Continue = "continue";
				public static readonly string Temporary = "wait";
				public static readonly string BadData = "modify";
			}
			
			public static class IqTypes {
				public static readonly string Error = "error";
				public static readonly string Get = "get";
				public static readonly string Set = "set";
				public static readonly string Result = "result";
			}

			public static class PresenceTypes {
				public static readonly string Unavailable = "unavailable";
				public static readonly string Subscribe = "subscribe";
				public static readonly string Subscribed = "subscribed";
				public static readonly string Unsubscribe = "unsubscribe";
				public static readonly string Unsubscribed = "unsubscribed";
				public static readonly string Probe = "probe";
				public static readonly string Error = "error";
			}
			
			public static class ShowValues {
				public static readonly string Away = "away";
				public static readonly string Chat = "chat";
				public static readonly string DoNotDisturb = "dnd";
				public static readonly string ExtendedAway = "xa";
			}

			public static class Attributes {
				public static readonly string To = "to";
				public static readonly string From = "from";
				public static readonly string Id = "id";
				public static readonly string Type = "type";
				public static readonly string Code = "code";
			}
		}
	
		public static class Streams {
			public static readonly string Namespace = "http://etherx.jabber.org/streams";
			public static readonly string Prefix = "stream";
			
			public static class Tags {
				public static readonly string Stream = "stream";
				public static readonly string Error = "error";
				public static readonly string Features = "features";
			}
			
			public static class Attributes {
				public static readonly string To = "to";
				public static readonly string From = "from";
				public static readonly string Id = "id";
				public static readonly string Version = "version";
			}
		}
		
		public static class StreamErrors {
			public static readonly string Namespace = "urn:ietf:params:xml:ns:xmpp-streams";
			
			public static class Tags {
				public static readonly string Text = "text";
				public static readonly string BadFormat = "bad-format";
				public static readonly string BadNamespacePrefix = "bad-namespace-prefix";
				public static readonly string Conflict = "conflict";
				public static readonly string ConnectionTimeout = "connection-timeout";
				public static readonly string HostGone = "host-gone";
				public static readonly string HostUnknown = "host-unknown";
				public static readonly string ImproperAddressing = "improper-addressing";
				public static readonly string InternalServerError = "internal-server-error";
				public static readonly string InvalidFrom = "invalid-from";
				public static readonly string InvalidId = "invalid-id";
				public static readonly string InvalidNamespace = "invalid-namespace";
				public static readonly string InvalidXml = "invalid-xml";
				public static readonly string NotAuthorized = "not-authorized";
				public static readonly string PolicyViolation = "policy-violation";
				public static readonly string RemoteConnectionFailed = "remote-connection-failed";
				public static readonly string ResourceConstraint = "resource-constraint";
				public static readonly string RestrictedXml = "restricted-xml";
				public static readonly string SeeOtherHost = "see-other-host";
				public static readonly string SystemShutdown = "system-shutdown";
				public static readonly string UndefinedCondition = "undefined-condition";
				public static readonly string UnsupportedEncoding = "unsupported-encoding";
				public static readonly string UnsupportedStanzaType = "unsupported-stanza-type";
				public static readonly string UnsupportedVersion = "unsupported-version";
				public static readonly string XmlNotWellFormed = "xml-not-well-formed";
			}
		}
		
		public static class Tls {
			public static readonly string Namespace = "urn:ietf:params:xml:ns:xmpp-tls";
			
			public static class Tags {
				public static readonly string StartTls = "starttls";
				public static readonly string Proceed = "proceed";
				public static readonly string Failure = "failure";
				public static readonly string Required = "required";
			}
		}
		
		public static class Sasl {
			public static readonly string Namespace = "urn:ietf:params:xml:ns:xmpp-sasl";
			
			public static class Tags {
				public static readonly string Mechanisms = "mechanisms";
				public static readonly string Mechanism = "mechanism";
				public static readonly string Auth = "auth";
				public static readonly string Challenge = "challenge";
				public static readonly string Failure = "failure";
				public static readonly string Response = "response";
				public static readonly string Aborted = "aborted";
				public static readonly string IncorrectEncoding = "incorrect-encoding";
				public static readonly string InvalidAuthzid = "invalid-authzid";
				public static readonly string InvalidMechanism = "invalid-mechanism";
				public static readonly string MechanismTooWeak = "mechanism-too-weak";
				public static readonly string NotAuthorized = "not-authorized";
				public static readonly string TemporaryAuthFailure = "temporary-auth-failure";
			}
			
			public static class Attributes {
				public static readonly string Mechanism = "mechanism";
			}
		}
	}
	
	
}