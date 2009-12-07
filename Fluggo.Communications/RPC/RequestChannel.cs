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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Fluggo.Communications.Serialization;
using System.Runtime.Remoting.Messaging;
using Fluggo.Resources;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Fluggo.Communications
{
	public interface IRequestFactoryProvider {
		RequestReceiverFactory GetRequestReceiverFactory( Guid iid );
		RequestSenderFactory GetRequestSenderFactory( Guid iid );
	}
	
	public class RequestChannel {
		static TraceSource _ts = new TraceSource( "RequestChannel", SourceLevels.Error );
		ChannelMultiplexer _mux;
		Channel _shortRpcChannel;
		UnidirectionalStreamPool _pool;
		Dictionary<int, OutboundStreamRequest> _outboundsWaitingForResponses = new Dictionary<int, OutboundStreamRequest>();
		IRequestFactoryProvider _factoryProvider;
		Dictionary<int, IRequestReceiver> _localTargets = new Dictionary<int, IRequestReceiver>();
		IServiceProvider _serviceProvider;
		IResourceProvider _resourceProvider;
		AsyncCallback _receiveStreamCallback, _receiveMessageCallback;
		IRequestControlService _abortService;
		const int __callIDBitLength = 24, __contextIDBitLength = 16, __targetIDBitLength = 32;
		const int __uriLength = 256;
		int _nextCallID = 0;
//		int _defaultBufferSize = 8192;
		
		public RequestChannel( IServiceProvider serviceProvider, IRequestFactoryProvider factoryProvider, ChannelMultiplexer mux, int startIndex, int count ) {
			if( mux == null )
				throw new ArgumentNullException( "mux" );

			if( startIndex < 0 )
				throw new ArgumentOutOfRangeException( "startIndex" );

			if( count <= 0 )
				throw new ArgumentOutOfRangeException( "count" );
				
			if( factoryProvider == null )
				throw new ArgumentNullException( "factoryProvider" );
				
			if( serviceProvider == null )
				throw new ArgumentNullException( "serviceProvider" );

			_serviceProvider = serviceProvider;
			
			if( _serviceProvider != null )
				_resourceProvider = (IResourceProvider) serviceProvider.GetService( typeof(IResourceProvider) );
			
			_factoryProvider = factoryProvider;
			_mux = mux;
			
			_receiveStreamCallback = HandleReceiveStream;
			_receiveMessageCallback = HandleReceiveMessage;

			// Channel interfaces
			//_receiverFactories.Add( typeof( IRequestControlService ).GUID, new AbortRequestReceiverFactory() );
			_abortService = (IRequestControlService) new RequestControlSenderFactory().CreateRequestSender(
				new LongRequestTarget( this, string.Empty, typeof( IRequestControlService ).GUID ) );

			if( count == 1 ) {
				// Shuffle *all* calls over the one channel we have
				_pool = new UnidirectionalStreamPool( mux, startIndex, count );
			}
			else {
				// Put the short calls over a special single-message stream
				_shortRpcChannel = _mux.GetChannel( startIndex );
				_shortRpcChannel.BeginReceive( _receiveMessageCallback, null );

				_pool = new UnidirectionalStreamPool( mux, startIndex + 1, count - 1 );
			}

			_pool.BeginReceiveStream( _receiveStreamCallback, null );
		}
		
		class TargetRegistration {
			Guid _iid;
			string _uri;
			int _localTargetID;
			
			public TargetRegistration( int localTargetID, string uri, Guid iid ) {
				if( uri == null )
					throw new ArgumentNullException( "uri" );
				
				_iid = iid;
				_uri = uri;
				_localTargetID = localTargetID;
			}
			
			public Guid InterfaceID
				{ get { return _iid; } }
			
			public string ServiceUri
				{ get { return _uri; } }
				
			public int LocalTargetID
				{ get { return _localTargetID; } }
		}

		/// <summary>
		/// Describes the attributes of an RPC message.
		/// </summary>
		enum RpcMessageFlags {
			/// <summary>
			/// No flags are set.
			/// </summary>
			None = 0,

			/// <summary>
			/// The message is a response to an existing request. The flags will be followed by the call ID.
			/// <para>If this flag is not present, the message is a request, and the flags will be followed by the call ID,
			///   context ID, interface GUID, and target URL.</para>
			/// </summary>
			Response = 1,

			/// <summary>
			/// The message expects no reply, even if there is an error.
			/// </summary>
			OneWay = 2,

			/// <summary>
			/// The message is in short form. For requests, this means that the interface GUID and target URL are replaced by
			/// a target ID.
			/// </summary>
			ShortForm = 4,
		}

		// Services which should appear in the basic RPC service:
		//	- Fatal error with error message
		//	- Non-fatal error (exception?)
		//	- Enumeration of services on root target ("/")

		// On each of the IDs:
		//	- Call ID
		//		- Purpose: Identifies pairs of request-response streams
		//		- Assignment policy:
		//			- Assigned by the caller
		//			- None assigned for one-way calls
		//			- Re-assigned upon receiving end of the response message/stream
		//			- Connection aborted
		//		- Length: 3 bytes
		//		- Scope: Per endpoint (Two pools per association/connection)
		//		- Visibility: Within RPC framework to both endpoints
		//	- Context ID
		//		- Purpose: Identifies a context that has been established at the server
		//		- Assignment policy:
		//			- Assigned by a context service in the callee's service domain
		//			- Allocated/deallocated at client's request
		//			- Automatic creation of context zero
		//		- Length: 2 bytes
		//		- Scope: Per endpoint (Two pools per association/connection)
		//		- Visibility: Within RPC framework to both endpoints; the context itself is
		//			visible to all frames on the target stack through CallContext, and the
		//			context ID may be visible to all frames on caller, therefore it is up
		//			to the frameworks on each side to avoid disclosing context IDs to
		//			unauthorized parties
		//	- Target URL
		//		- Purpose: Identifies an object capable of receiving RPCs
		//		- Assignment policy:
		//			- Assigned by the callee
		//			- Created in "directory" structure similar to HTTP or Linux filesystem
		//			- Empty URL refers to the channel itself and is reserved for the channel's use
		//		- Length: 256 printable UTF-8 characters
		//		- Scope: Per service domain
		//		- Visibility: Determined by service domain
		//	- Interface GUID
		//		- Purpose: Identifies the schema used in a class of RPCs
		//		- Assignment policy:
		//			- Assigned by the schema creator
		//		- Length: 16 bytes
		//		- Scope: Global
		//		- Visibility: Global
		//	- Target ID
		//		- Purpose: Identifies a target object and interface in short form requests
		//		- Assignment policy:
		//			- Assigned by a target service in the callee's service domain
		//			- Allocated/deallocated at client's request
		//		- Length: 4 bytes
		//		- Scope: Per endpoint (Two pools per association/connection)
		//		- Visibility: Within RPC framework to both endpoints

		// How much space do we really need?
		//		Call flags: 1 byte (3 bits used so far)
		//		Call ID: 1 byte (do we need more than 256 outstanding requests? what's the scope?)

		// Things to consider when making these calls:
		//  - target
		//  - service
		//  - context
		//  - call ID
		//  - call/reply
		//  - one-wayness
		//  - ability to recover from problems

		// Several messages, each with a part of the call, and all beginning with the call's ID and call/reply status
		//  (call/reply needed to know which table is being referred to)
		//  - Context ID, target at beginning
		//  - Avoid someone exploiting resources by not working with explicit lengths
		//    and decoding as soon as the data is received
		
		// Problems that need to be dealt with:
		//  - Aborting inbound requests
		//  - Aborting outbound requests
		//  - Aborting inbound responses
		//  - Aborting outbound responses
		//  - Extra data after the end of a request/response (channel is held open until...)
		
		private void HandleReceiveMessage( IAsyncResult result ) {
			bool calledNext = false;
			
			try {
				IMessageBuffer buffer = _shortRpcChannel.EndReceive( result );
				
				_shortRpcChannel.BeginReceive( _receiveMessageCallback, null );
				calledNext = true;
				
				HandleInboundStream( buffer.GetStream() );
			}
			catch( Exception ex ) {
				#warning Report warning/error message back, and funnel the stream into /dev/null to clear the channel
				Debug.WriteLine( ex );

				try {
					if( !calledNext )
						_shortRpcChannel.BeginReceive( _receiveMessageCallback, null );
				}
				catch( Exception ex2 ) {
					Debug.WriteLine( "BeginReceive failed! " + ex2.ToString() );
				}
			}
		}
		
		private void HandleReceiveStream( IAsyncResult result ) {
			Stream stream = null;
			bool calledNext = false;
		
			try {
				stream = _pool.EndReceiveStream( result );
				
				_pool.BeginReceiveStream( _receiveStreamCallback, null );
				calledNext = true;
				
				HandleInboundStream( stream );
			}
			catch( Exception ex ) {
				Debug.WriteLine( ex );
				
				try {
					if( !calledNext )
						_pool.BeginReceiveStream( _receiveStreamCallback, null );
				}
				catch( Exception ex2 ) {
					Debug.WriteLine( "BeginReceive failed! " + ex2.ToString() );
				}
			}
		}
		
		private void HandleInboundStream( Stream stream ) {
			RpcMessageFlags flags = RpcMessageFlags.None;
			int callID = -1;
		
			try {
				BitReader reader = new BitReader( stream, 1 );
				flags = (RpcMessageFlags)(int) reader.ReadByte( 8 );
				callID = reader.ReadInt32( __callIDBitLength );
				
				if( (flags & RpcMessageFlags.Response) == RpcMessageFlags.Response ) {
					try {
						lock( _outboundsWaitingForResponses ) {
							// Response
							OutboundStreamRequest request;

							if( !_outboundsWaitingForResponses.TryGetValue( callID, out request ) ) {
								throw new Exception( "Response received for an invalid call #" + callID.ToString() );
							}
							
							request.SetResponseStream( stream );
							_outboundsWaitingForResponses.Remove( callID );
						}
					}
					catch( Exception ex ) {
						_ts.TraceEvent( TraceEventType.Information, 0, "Aborting inbound response {0}: {1}", callID, ex.Message );
						_abortService.AbortResponse( false, callID, ex.Message );
						Pipe.Redirect( stream, Stream.Null );
					}
				}
				else {
					try {
						// Request
						int contextID = reader.ReadInt32( __contextIDBitLength );
						IRequestReceiver receiver;
						
						if( (flags & RpcMessageFlags.ShortForm) == RpcMessageFlags.ShortForm ) {
							int targetID = reader.ReadInt32( __targetIDBitLength );
							receiver = GetRequestReceiver( targetID );
						}
						else {
							Guid iid = new Guid( reader.ReadBytes( 16 ) );
							string uri = reader.ReadString( __uriLength );
							receiver = GetRequestReceiver( uri, iid );
						}
						
						InboundStreamRequest request = new InboundStreamRequest( this, new NotifyEndStream( stream, null, null ), callID,
							(flags & RpcMessageFlags.OneWay) == RpcMessageFlags.OneWay );
						
						receiver.ProcessRequest( request );
					}
					catch( Exception ex ) {
						_ts.TraceEvent( TraceEventType.Information, 0, "Aborting inbound request {0}: {1}", callID, ex.Message );
						_abortService.AbortRequest( false, callID, ex.Message );
						Pipe.Redirect( stream, Stream.Null );
					}
				}
			}
			catch( Exception ex ) {
				_ts.TraceEvent( TraceEventType.Error, 0, "An inbound stream could not be understood.", ex.ToString() );
				Pipe.Redirect( stream, Stream.Null );
			}
		}
		
	#region RequestSenderResolve
/*		private object GetRequestSender( int targetID ) {
			return _localTargets[targetID];			// Throws exception if not found
		}*/

		public object GetRequestSender( string path, Guid iid ) {
			if( path == null )
				throw new ArgumentNullException( "path" );

			if( path.Length == 0 )
				throw new ArgumentException( "The path was empty.", "path" );

			return GetRequestSenderImpl( path, iid );
		}
		
		private object GetRequestSenderImpl( string path, Guid iid ) {
			// Find the service provider that maps to the given uri
			if( path == null )
				throw new ArgumentNullException( "path" );

/*			if( path != ResourceTable.RootPath && _resourceProvider == null )
				throw new Exception( "External resources are not available on this channel." );*/

			return ResolveRequestSender( iid ).CreateRequestSender( new LongRequestTarget( this, path, iid ) );
		}

		private RequestSenderFactory ResolveRequestSender( Guid guid ) {
			RequestSenderFactory sender = _factoryProvider.GetRequestSenderFactory( guid );
			
			if( sender != null )
				return sender;

/*			if( _senderFactories.TryGetValue( guid, out sender ) )
				return sender;

			// Turn to the resolver delegates
			ResolveRequestSenderEventHandler handler = RequestSenderResolve;
			Delegate[] handleList = handler.GetInvocationList();

			ResolveRequestHandlerEventArgs eventArgs = new ResolveRequestHandlerEventArgs( guid );
			object[] paramList = new object[2] { this, eventArgs };

			for( int i = 0; i < handleList.Length; i++ ) {
				sender = (RequestSenderFactory) handleList[0].DynamicInvoke( paramList );

				if( sender != null ) {
					_senderFactories[guid] = sender;
					return sender;
				}
			}*/

			throw new Exception( "Could not find a sender capable of handling interface type " + guid.ToString() + "." );
		}
	#endregion
		
	#region RequestReceiverResolve
		private IRequestReceiver GetRequestReceiver( int targetID ) {
			return _localTargets[targetID];			// Throws exception if not found
		}
		
		private IRequestReceiver GetRequestReceiver( string uri, Guid iid ) {
			// Find the service provider that maps to the given uri
			IServiceProvider provider;
			
			if( uri.Length == 0 ) {
				if( iid == typeof(IRequestControlService).GUID ) {
					return new RequestControlReceiverFactory().CreateRequestReceiver( this );
				}
			}
			
			if( uri != ResourceTable.RootPath ) {
				if( _resourceProvider == null )
					throw new Exception( "External resources are not available on this channel." );
					
				provider = _resourceProvider.GetResource( uri );
			}
			else {
				provider = _serviceProvider;
			}
				
			return ResolveRequestReceiver( iid ).CreateRequestReceiver( provider ); 
		}
		
		private RequestReceiverFactory ResolveRequestReceiver( Guid guid ) {
			if( guid == typeof(IRequestControlService).GUID )
				return new RequestControlReceiverFactory();
			
			RequestReceiverFactory receiver = _factoryProvider.GetRequestReceiverFactory( guid );
			
			if( receiver != null )
				return receiver;
			
			/*if( _receiverFactories.TryGetValue( guid, out receiver ) )
				return receiver;
			
			// Turn to the resolver delegates
			ResolveRequestReceiverEventHandler handler = RequestReceiverResolve;
			Delegate[] handleList = handler.GetInvocationList();
			
			ResolveRequestHandlerEventArgs eventArgs = new ResolveRequestHandlerEventArgs( guid );
			object[] paramList = new object[2] { this, eventArgs };
			
			for( int i = 0; i < handleList.Length; i++ ) {
				receiver = (RequestReceiverFactory) handleList[0].DynamicInvoke( paramList );
				
				if( receiver != null ) {
					_receiverFactories[guid] = receiver;
					return receiver;
				}
			}*/
			
			throw new Exception( "Could not find a receiver capable of handling interface type " + guid.ToString() + "." );
		}
	#endregion
		
		private void Abort() {
			_mux.Close();
		}
		
		/// <summary>
		/// Allocates an outbound call ID to the given request.
		/// </summary>
		/// <param name="request"><see cref="OutboundStreamRequest"/> which needs a call ID.</param>
		/// <returns>A call ID for the given request.</returns>
		private int AllocateCallID( OutboundStreamRequest request ) {
			lock( _outboundsWaitingForResponses ) {
				if( _outboundsWaitingForResponses.Count == (1 << __callIDBitLength) )
					throw new IOException( "An outbound call ID could not be allocated because the active call table is full." );
			
				// Find an empty call ID
				for( ;; ) {
					int nextCallID = _nextCallID++;

					if( _nextCallID == (1 << __callIDBitLength) )
						_nextCallID = 0;

					if( !_outboundsWaitingForResponses.ContainsKey( nextCallID ) ) {
						_outboundsWaitingForResponses[nextCallID] = request;
						return nextCallID;
					}
				}
			}
		}
		
	#region ShortRequestTarget
		/// <summary>
		/// Represents a target that can be contacted using the short form.
		/// </summary>
		sealed class ShortRequestTarget : IRequestTarget {
			RequestChannel _channel;
			TargetRegistration _registration;

			/// <summary>
			/// Creates a new instance of the <see cref='ShortRequestTarget'/> class.
			/// </summary>
			/// <param name="channel">Channel across which the messages will be sent.</param>
			/// <param name="registration">Registration with the short target ID.</param>
			/// <exception cref='ArgumentNullException'><paramref name='channel'/> is <see langword='null'/>.
			///   <para>— OR —</para>
			///   <para><paramref name='registration'/> is <see langword='null'/>.</para></exception>
			public ShortRequestTarget( RequestChannel channel, TargetRegistration registration ) {
				if( channel == null )
					throw new ArgumentNullException( "channel" );

				if( registration == null )
					throw new ArgumentNullException( "registration" );

				_channel = channel;
				_registration = registration;
			}

			/// <summary>
			/// Gets the URI of the target service.
			/// </summary>
			/// <value>The URI of the target service.</value>
			public string ServiceUri {
				get { return _registration.ServiceUri; }
			}

			/// <summary>
			/// Gets the GUID of the target interface.
			/// </summary>
			/// <value>The GUID of the target interface.</value>
			public Guid InterfaceID {
				get { return _registration.InterfaceID; }
			}

			public IStreamRequest StartRequest( bool oneWay ) {
				// Find the current context, if any
				return new OutboundStreamRequest( _channel, LocalContext.ID, _registration.LocalTargetID, oneWay );
			}
		}
	#endregion
		
	#region LongRequestTarget
		/// <summary>
		/// Represents a target that will be contacted using the long form.
		/// </summary>
		sealed class LongRequestTarget : IRequestTarget {
			RequestChannel _channel;
			string _uri;
			Guid _iid;

			/// <summary>
			/// Creates a new instance of the <see cref='LongRequestTarget'/> class.
			/// </summary>
			/// <param name="channel">Channel across which the messages will be sent.</param>
			/// <param name="uri">The URI of the target service.</param>
			/// <param name="interfaceID">The GUID of the target interface.</param>
			/// <exception cref='ArgumentNullException'><paramref name='channel'/> is <see langword='null'/>.
			///   <para>— OR —</para>
			///   <para><paramref name='uri'/> is <see langword='null'/>.</para></exception>
			public LongRequestTarget( RequestChannel channel, string uri, Guid interfaceID ) {
				if( channel == null )
					throw new ArgumentNullException( "channel" );

				if( uri == null )
					throw new ArgumentNullException( "uri" );

				_channel = channel;
				_uri = uri;
				_iid = interfaceID;
			}

			/// <summary>
			/// Gets the URI of the target service.
			/// </summary>
			/// <value>The URI of the target service.</value>
			public string ServiceUri {
				get { return _uri; }
			}

			/// <summary>
			/// Gets the GUID of the target interface.
			/// </summary>
			/// <value>The GUID of the target interface.</value>
			public Guid InterfaceID {
				get { return _iid; }
			}

			public IStreamRequest StartRequest( bool oneWay ) {
				// Find the current context, if any
				return new OutboundStreamRequest( _channel, LocalContext.ID, _uri, _iid, oneWay );
			}
		}
	#endregion
		
	#region LocalContext
		/// <summary>
		/// Stores the remote context ID in the local .NET call context.
		/// </summary>
		[Serializable]
		class LocalContext : ILogicalThreadAffinative {
			static LocalContext __default = new LocalContext( 0 );
			const string __callContextName = "__requestChannel.ContextID";
			int _contextID;
			
			private LocalContext( int contextID ) {
				_contextID = contextID;
			}
			
			public static int ID {
				get {
					LocalContext context = CallContext.GetData( __callContextName ) as LocalContext;
					
					if( context == null )
						return 0;
					else
						return context._contextID;
				}
				set {
					if( value == 0 )
						CallContext.FreeNamedDataSlot( __callContextName );
					else
						CallContext.SetData( __callContextName, new LocalContext( value ) );
				}
			}
		}
	#endregion

	#region NotifyEndStream
		// BJC: If this class seems funny, it's because I was watching Hot Shots while writing it.

		/// <summary>
		/// Represents a stream that lets you muck with an underlying stream.
		/// </summary>
		sealed class NotifyEndStream : CompositeStream {
			EventHandler _closedCallback;
			bool _closed;
			Exception _ex;
			object _tag;

			/// <summary>
			/// Creates a new instance of the <see cref='NotifyEndStream'/> class.
			/// </summary>
			/// <param name="root">Stream to aggregate.</param>
			/// <param name="closedCallback">Optional <see cref="EventHandler"/> to be called when the stream ends.</param>
			/// <param name="tag">User object.</param>
			public NotifyEndStream( Stream root, EventHandler closedCallback, object tag )
				: base( root ) {
				_closedCallback = closedCallback;
				_tag = tag;
			}

			/// <summary>
			/// Gets a value that represents whether the stream has ended.
			/// </summary>
			/// <value>True if the stream has ended, false otherwise.</value>
			public bool Closed { get { return _closed; } }

			/// <summary>
			/// Gets or sets the user object.
			/// </summary>
			/// <value>The user object.</value>
			public object Tag { get { return _tag; } set { _tag = value; } }

			public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
				if( _ex != null )
					throw _ex;
				
				return base.BeginRead( buffer, offset, count, callback, state );
			}
			
			public override int Read( byte[] buffer, int offset, int count ) {
				if( _ex != null )
					throw _ex;
				
				int result = base.Read( buffer, offset, count );

				if( count != 0 && result == 0 )
					OnClosed();

				return result;
			}

			public override int EndRead( IAsyncResult asyncResult ) {
				int result = base.EndRead( asyncResult );
				
				if( result == 0 )
					OnClosed();
					
				if( _ex != null )
					throw _ex;

				return result;
			}

			public override int ReadByte() {
				if( _ex != null )
					throw _ex;
				
				int result = base.ReadByte();

				if( result == -1 )
					OnClosed();

				return result;
			}
			
			/// <summary>
			/// Sets an exception on the stream.
			/// </summary>
			/// <param name="ex">Exception to set.</param>
			/// <remarks>The exception will be thrown on the next read method called.</remarks>
			public void SetException( Exception ex ) {
				_ex = ex;
			}

			public override void Close() {
				base.Close();
				OnClosed();
			}
			
			private void OnClosed() {
				if( !_closed ) {
					_closed = true;
					
					if( _closedCallback != null )
						_closedCallback( this, EventArgs.Empty );
				}
			}
		}
	#endregion

	#region InboundStreamRequest
		sealed class InboundStreamRequest : IStreamRequest {
			RequestChannel _channel;
			Stream _responseStream;
			NotifyEndStream _inboundStream;
			bool _getRequestStreamCalled;
			int _call;
			bool _oneWay;

			public InboundStreamRequest( RequestChannel channel, NotifyEndStream inboundStream, int call, bool oneWay ) {
				if( channel == null )
					throw new ArgumentNullException( "channel" );

				if( inboundStream == null )
					throw new ArgumentNullException( "inboundStream" );

				_channel = channel;
				_call = call;
				_oneWay = oneWay;
				_inboundStream = inboundStream;
			}

			public bool IsOutbound { get { return false; } }
			public bool IsOneWay { get { return _oneWay; } }
			
			public Stream GetRequestStream() {
				if( _getRequestStreamCalled )
					throw new InvalidOperationException( "This method has already been called." );

				_getRequestStreamCalled = true;
				return _inboundStream;
			}

			public Stream GetResponseStream() {
				if( !_getRequestStreamCalled || !_inboundStream.Closed )
					throw new InvalidOperationException( "The request stream has not ended." );

				if( _oneWay )
					throw new NotSupportedException( "This is a one-way call." );

				if( _responseStream != null )
					throw new InvalidOperationException( "This method has already been called." );

				return _responseStream = CallStream.CreateResponseStream( _channel, _call );
			}
		}
	#endregion

	#region OutboundStreamRequest
		class OutboundStreamRequest : IStreamRequest {
			ManualResetEvent _event = new ManualResetEvent( false );
			CallStream _outStream;
			Stream _responseStream;
			bool _oneWay, _requestStreamCalled;
			Exception _ex;

			public OutboundStreamRequest( RequestChannel channel, int context, int target, bool oneWay ) {
				if( channel == null )
					throw new ArgumentNullException( "channel" );
					
				int call = channel.AllocateCallID( this );

				_oneWay = oneWay;
				_outStream = CallStream.CreateRequestStream( channel, call, context, target, oneWay );
			}

			public OutboundStreamRequest( RequestChannel channel, int context, string uri, Guid interfaceID, bool oneWay ) {
				if( channel == null )
					throw new ArgumentNullException( "channel" );

				int call = channel.AllocateCallID( this );

				_oneWay = oneWay;
				_outStream = CallStream.CreateRequestStream( channel, call, context, uri, interfaceID, oneWay );
			}

			public bool IsOutbound { get { return true; } }
			public bool IsOneWay { get { return _oneWay; } }

			public Stream GetRequestStream() {
				if( _requestStreamCalled )
					throw new InvalidOperationException();

				_requestStreamCalled = true;
				return _outStream;
			}

			public void SetResponseStream( Stream stream ) {
				if( _responseStream != null )
					throw new InvalidOperationException( "The response stream has already been set." );

				_responseStream = stream;
				_event.Set();
			}

			public Stream GetResponseStream() {
				if( _oneWay )
					throw new NotSupportedException();
				
				if( !_requestStreamCalled || _outStream.CanWrite )
					throw new InvalidOperationException();

				_event.WaitOne();
				
				if( _ex != null )
					throw _ex;
				
				return _responseStream;
			}
			
			public void Abort( Exception ex ) {
				_outStream.Close();
				_ex = ex;
				_event.Set();
			}
		}
	#endregion

	#region CallStream
		sealed class CallStream : Stream {
			RequestChannel _owner;
			Stream _root;
			byte[] _buffer;
			int _bufferCount;
			object _lock = new object();

			private CallStream( RequestChannel owner ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );

				_owner = owner;

				if( _owner._shortRpcChannel != null )
					_buffer = new byte[_owner._shortRpcChannel.MaximumPayloadLength];
				else
					_buffer = new byte[2048];
			}

			public static CallStream CreateRequestStream( RequestChannel owner, int callID, int contextID, string uri, Guid interfaceID, bool oneWay ) {
				CallStream stream = new CallStream( owner );
				BitWriter writer = new BitWriter( stream );
				
				RpcMessageFlags flags = RpcMessageFlags.None;
				
				if( oneWay )
					flags |= RpcMessageFlags.OneWay;
				
				writer.Write( (int) flags, 8 );
				writer.Write( callID, __callIDBitLength );
				writer.Write( contextID, __contextIDBitLength );
				writer.WriteBytes( interfaceID.ToByteArray(), 0, 16 );
				writer.WriteString( uri, __uriLength );
				writer.Flush();
				
				return stream;
			}
			
			public static CallStream CreateRequestStream( RequestChannel owner, int callID, int contextID, int targetID, bool oneWay ) {
				CallStream stream = new CallStream( owner );
				BitWriter writer = new BitWriter( stream );

				RpcMessageFlags flags = RpcMessageFlags.ShortForm;

				if( oneWay )
					flags |= RpcMessageFlags.OneWay;

				writer.Write( (int) flags, 8 );
				writer.Write( callID, __callIDBitLength );
				writer.Write( contextID, __contextIDBitLength );
				writer.Write( targetID, __targetIDBitLength );
				writer.Flush();

				return stream;
			}

			public static CallStream CreateResponseStream( RequestChannel owner, int callID ) {
				CallStream stream = new CallStream( owner );
				BitWriter writer = new BitWriter( stream );

				RpcMessageFlags flags = RpcMessageFlags.Response;
				writer.Write( (int) flags, 8 );
				writer.Write( callID, __callIDBitLength );
				writer.Flush();

				return stream;
			}

			public override bool CanRead {
				get { return false; }
			}

			public override bool CanSeek {
				get { return false; }
			}

			public override bool CanWrite {
				get { return (_buffer != null) || (_root != null); }
			}

			public override void Flush() {
				if( _root != null )
					_root.Flush();

				// Else... what?
			}

			#region Invalid operations
			public override long Length {
				get { throw new NotSupportedException(); }
			}

			public override long Position {
				get {
					throw new NotSupportedException();
				}
				set {
					throw new NotSupportedException();
				}
			}

			public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state ) {
				throw new NotSupportedException();
			}

			public override int Read( byte[] buffer, int offset, int count ) {
				throw new NotSupportedException();
			}

			public override int EndRead( IAsyncResult asyncResult ) {
				throw new NotSupportedException();
			}

			public override long Seek( long offset, SeekOrigin origin ) {
				throw new NotSupportedException();
			}

			public override void SetLength( long value ) {
				throw new NotSupportedException();
			}
			#endregion

			public override void Write( byte[] buffer, int offset, int count ) {
				new ArraySegment<byte>( buffer, offset, count );

				// Are we still storing up data?
				if( _buffer != null ) {
					int readCount = Math.Min( count, _buffer.Length - _bufferCount );

					// Buffer up some more data
					if( readCount != 0 ) {
						Buffer.BlockCopy( buffer, offset, _buffer, _bufferCount, readCount );
						_bufferCount += readCount;
						offset += readCount;
						count -= readCount;
					}

					if( count == 0 )
						return;

					if( _bufferCount == _buffer.Length ) {
						// Negotiate an outbound stream
						_root = _owner._pool.GetStream();
						_root.Write( _buffer, 0, _buffer.Length );
						_buffer = null;
					}
				}

				if( _root == null )
					throw new ObjectDisposedException( null );

				_root.Write( buffer, offset, count );
			}

			public override void Close() {
				Dispose( true );
			}

			protected override void Dispose( bool disposing ) {
				base.Dispose( disposing );

				if( disposing ) {
					if( _buffer != null ) {
						if( _owner._shortRpcChannel != null ) {
							_owner._shortRpcChannel.Send( _buffer, 0, _bufferCount );
						}
						else {
							_root = _owner._pool.GetStream();
							_root.Write( _buffer, 0, _bufferCount );
						}

						_owner = null;
						_buffer = null;
					}

					if( _root != null ) {
						_root.Dispose();
						_root = null;
					}
				}
			}
		}
	#endregion
	
	#region AbortRequest
		const int __abortStringMaxLength = 1024;
		
		class RequestControlReceiverFactory : RequestReceiverFactory {
			public RequestControlReceiverFactory() : base( typeof(IRequestControlService).GUID ) {
			}
		
			public override IRequestReceiver CreateRequestReceiver( object target ) {
				return new RequestControlReceiver( (RequestChannel) target );
			}
			
			class RequestControlReceiver : IRequestReceiver {
				RequestChannel _channel;
				
				public RequestControlReceiver( RequestChannel channel ) {
					if( channel == null )
						throw new ArgumentNullException( "channel" );
					
					_channel = channel;
				}
			
				public void ProcessRequest( IStreamRequest request ) {
					BitReader reader = new BitReader( request.GetRequestStream() );
					
					int call = reader.ReadInt32( 4 );
					
					switch( call ) {
						case 0:
							_channel.AbortLocalRequest( !reader.ReadBoolean(), reader.ReadInt32( __callIDBitLength ), reader.ReadString( __abortStringMaxLength ) );
							break;
						
						case 1:
							_channel.AbortLocalResponse( !reader.ReadBoolean(), reader.ReadInt32( __callIDBitLength ), reader.ReadString( __abortStringMaxLength ) );
							break;

						case 2:
							_channel.AbortLocalChannel( reader.ReadString( __abortStringMaxLength ) );
							break;

						case 3:
							_channel.ReportLocalWarning( reader.ReadString( __abortStringMaxLength ) );
							break;
						
						default:
							throw new NotSupportedException();
					}
					
					reader.Close();
				}
			}
		}

		[Guid("2835079A-6259-4dfa-9826-99B6ACDBC02E")]
		interface IRequestControlService {
			/// <summary>
			/// Aborts an inbound or outbound request.
			/// </summary>
			/// <param name="outbound">True if the request originated at the caller, or false if it originated at the callee.</param>
			/// <param name="callID">Request's ID.</param>
			/// <param name="message">Optional message giving the reason for the abort.</param>
			void AbortRequest( bool outbound, int callID, string message );

			/// <summary>
			/// Aborts an inbound or outbound response.
			/// </summary>
			/// <param name="outbound">True if the response originated at the caller, or false if it originated at the callee.</param>
			/// <param name="callID">Request's ID.</param>
			/// <param name="message">Optional message giving the reason for the abort.</param>
			void AbortResponse( bool outbound, int callID, string message );
			
			void AbortChannel( string message );
			
			void ReportWarning( string message );
		}
		
		class RequestControlSenderFactory : RequestSenderFactory {
			public RequestControlSenderFactory() : base( typeof(IRequestControlService).GUID ) {
			}
		
			public override object CreateRequestSender( IRequestTarget target ) {
				return new RequestControlSender( target );
			}
			
			class RequestControlSender : IRequestControlService {
				IRequestTarget _target;
				
				public RequestControlSender( IRequestTarget target ) {
					if( target == null )
						throw new ArgumentNullException( "target" );

					_target = target;
				}
				
				public void AbortRequest( bool outbound, int callID, string message ) {
					if( message == null )
						message = string.Empty;
						
					IStreamRequest request = _target.StartRequest( true );
					BitWriter writer = new BitWriter( request.GetRequestStream() );
					
					writer.Write( 0, 4 );
					writer.Write( outbound );
					writer.Write( callID, __callIDBitLength );
					writer.WriteString( message, __abortStringMaxLength );
					writer.Close();
				}

				public void AbortResponse( bool outbound, int callID, string message ) {
					if( message == null )
						message = string.Empty;

					IStreamRequest request = _target.StartRequest( true );
					BitWriter writer = new BitWriter( request.GetRequestStream() );

					writer.Write( 1, 4 );
					writer.Write( outbound );
					writer.Write( callID, __callIDBitLength );
					writer.WriteString( message, __abortStringMaxLength );
					writer.Close();
				}

				public void AbortChannel( string message ) {
					if( message == null )
						message = string.Empty;

					IStreamRequest request = _target.StartRequest( true );
					BitWriter writer = new BitWriter( request.GetRequestStream() );

					writer.Write( 2, 4 );
					writer.WriteString( message, __abortStringMaxLength );
					writer.Close();
				}
				
				public void ReportWarning( string message ) {
					if( message == null )
						throw new ArgumentNullException( "message" );

					IStreamRequest request = _target.StartRequest( true );
					BitWriter writer = new BitWriter( request.GetRequestStream() );

					writer.Write( 3, 4 );
					writer.WriteString( message, __abortStringMaxLength );
					writer.Close();
				}
			}
		}
		
		private void AbortLocalRequest( bool outbound, int callID, string message ) {
			if( outbound ) {
				OutboundStreamRequest request;

				if( !_outboundsWaitingForResponses.TryGetValue( callID, out request ) ) {
					_ts.TraceEvent( TraceEventType.Warning, 0, "Attempt to abort nonexistent outbound request {0} with the message: {1}", callID, message );
					return;
				}

				request.Abort( new Exception( message ) );
				_ts.TraceEvent( TraceEventType.Information, 0, "Outbound request {0} aborted with the message: {1}", callID, message );
			}
			else {
				_ts.TraceEvent( TraceEventType.Warning, 0, "Inbound request {0} aborted with the message: {1}", callID, message );
				#warning Errr... implement this case
			}
		}
		
		private void AbortLocalResponse( bool outbound, int callID, string message ) {
			if( outbound ) {
				#warning Errr... implement this case
				_ts.TraceEvent( TraceEventType.Warning, 0, "Outbound response {0} aborted with the message: {1}", callID, message );
			}
			else {
				#warning Errr... implement this case
				_ts.TraceEvent( TraceEventType.Warning, 0, "Inbound response {0} aborted with the message: {1}", callID, message );
			}
		}
		
		private void AbortLocalChannel( string message ) {
			_ts.TraceEvent( TraceEventType.Error, 0, "Channel aborted with the message: {0}", message );
			Abort();
		}
		
		private void ReportLocalWarning( string message ) {
			_ts.TraceEvent( TraceEventType.Warning, 0, "Far end of channel reports: {0}", message );
		}
	#endregion
	}
}
