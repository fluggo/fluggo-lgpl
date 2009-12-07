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
using System.Runtime.InteropServices;
using Fluggo.Communications.Serialization;
using System.IO;
using System.Threading;

namespace Fluggo.Communications {
	public delegate RequestSenderFactory ResolveRequestSenderEventHandler( object sender, ResolveRequestHandlerEventArgs e );
	public delegate RequestReceiverFactory ResolveRequestReceiverEventHandler( object sender, ResolveRequestHandlerEventArgs e );
	
	public class ResolveRequestHandlerEventArgs : EventArgs {
		Guid _interfaceID;
		
		public ResolveRequestHandlerEventArgs( Guid interfaceID ) {
			_interfaceID = interfaceID;
		}
		
		public Guid InterfaceID {
			get { return _interfaceID; }
		}
	}
	
	/// <summary>
	/// Creates objects that send messages for a specific interface to remote services.
	/// </summary>
	public abstract class RequestSenderFactory {
		Guid _interfaceID;
		
		protected RequestSenderFactory( Guid interfaceID ) {
			_interfaceID = interfaceID;
		}
		
		public Guid InterfaceID
			{ get { return _interfaceID; } }
		
		/// <summary>
		/// Creates an object that will send messages to the given <see cref="IRequestTarget"/>.
		/// </summary>
		/// <param name="target"><see cref="IRequestTarget"/> that will receive the messages.</param>
		/// <returns>An object that can send messages to <paramref name="target"/>.</returns>
		public abstract object CreateRequestSender( IRequestTarget target );
	}
	
	/// <summary>
	/// Creates objects that receive messages for a specific interface on a local object.
	/// </summary>
	public abstract class RequestReceiverFactory {
		Guid _interfaceID;
		
		protected RequestReceiverFactory( Guid interfaceID ) {
			_interfaceID = interfaceID;
		}
		
		public Guid InterfaceID
			{ get { return _interfaceID; } }
		
		/// <summary>
		/// Creates an <see cref="RpcReceiver"/> to process messages from a channel.
		/// </summary>
		/// <param name="target">Target of the messages.</param>
		/// <returns>An <see cref="IRequestReceiver"/> that will process inbound messages.</returns>
		public abstract IRequestReceiver CreateRequestReceiver( object target );
	}

	public interface IRequestReceiver {
		void ProcessRequest( IStreamRequest request );
	}
	
	/// <summary>
	/// Stores interface types and allows lookup by GUID.
	/// </summary>
	public sealed class InterfaceRegistry {
		Dictionary<Guid,Type> _reg = new Dictionary<Guid,Type>();
		
		/// <summary>
		/// Registers the given interface type in the registry.
		/// </summary>
		/// <param name="type">Type of the interface to register.</param>
		/// <exception cref="ArgumentException">A different type with the same <see cref="Type.GUID"/> has already been registered.
		///   <para>-or-</para>
		///   <para><paramref name="type"/> is not an interface.</para></exception>
		/// <exception cref='ArgumentNullException'><paramref name='type'/> is <see langword='null'/>.</exception>
		public void RegisterInterface( Type type ) {
			if( type == null )
				throw new ArgumentNullException( "type" );
				
			if( !type.IsInterface )
				throw new ArgumentException( "The given type is not an interface.", "type" );
				
			Guid guid = type.GUID;
			
			if( _reg.ContainsKey( guid ) ) {
				if( _reg[guid] != type )
					throw new ArgumentException( "A different type with the same GUID has already been registered.", "type" );
			}
			
			_reg[guid] = type;
		}
		
		/// <summary>
		/// Gets the interface type, if any, with the given GUID.
		/// </summary>
		/// <param name="guid">GUID of the interface to retrieve.</param>
		/// <returns>The interface type if it has been registered, or <see langword='null'/> if no interface with that GUID has been registered.</returns>
		public Type GetType( Guid guid ) {
			return _reg[guid];
		}
	}
	
	[Guid("7862E169-11F2-4da1-9A47-4AA76D70BBD0")]
	interface IDummyRpc {
		void SomeMethod();
		
		void SomeMethod( int extraThing );
		
		int SomeOtherMethod();
		
		int SomeOtherMethod( int extraThing );
	}
	
	interface IDummyRpcCalls {
		void SomeMethod_0( [Range( 0, 65535 )] int contextID );
		void SomeMethod_1( [Range( 0, 65535 )] int contextID, int extraThing );
		void SomeOtherMethod_2( [Range( 0, 65535 )] int contextID );
		void SomeOtherMethod_3( [Range( 0, 65535 )] int contextID, int extraThing );
	}
	
	interface IDummyRpcReplies {
		void SomeOtherMethod_2( int returnValue );
		void SomeOtherMethod_3( int returnValue );
	}
	
	class DummyRpc : IDummyRpc {
		IRequestTarget _target;
		BitSerializer bs = new BitSerializer();
	
		public void SomeMethod() {
			// Create request
			IStreamRequest request = _target.StartRequest( false );
			
			// Do call
			Stream requestStream = request.GetRequestStream();
			requestStream.Write( new byte[] { 0 }, 0, 1 );
			requestStream.Close();

			// Deserialize reply
			Stream responseStream = request.GetResponseStream();

			if( responseStream.ReadByte() != -1 )
				throw new IOException();
			
			// In the case of a void return, there should be nothing in the reply; if an exception
			// occurs, it will be thrown inside the Invoke -> EndInvoke call. 
		}

		public void SomeMethod( int extraThing ) {
			// Create request
/*			MemoryStream stream = new MemoryStream( _target.SmallMessageLength );
			BitWriter writer = new BitWriter( stream );
			bs.SerializeValue( writer, extraThing, typeof(int), null );

			IMessageBuffer requestMessage = new MessageBufferWrapper( stream.GetBuffer(), 0, stream.Length );

			stream = null;
			writer = null;

			// Do call
			IMessageBuffer replyMessage = _target.Invoke( requestMessage, false );

			// Deserialize reply
			if( replyMessage.Length != 0 )
				throw new IOException();

			// In the case of a void return, there should be nothing in the reply; if an exception
			// occurs, it will be thrown inside the Invoke -> EndInvoke call. 
 */
		}

		public int SomeOtherMethod() {
			throw new NotImplementedException();
		}

		public int SomeOtherMethod( int extraThing ) {
			throw new NotImplementedException();
		
			// Create request
/*			MemoryStream stream = new MemoryStream( _target.SmallMessageLength );
			BitWriter writer = new BitWriter( stream );
			bs.SerializeValue( writer, extraThing, typeof( int ), null );

			IMessageBuffer requestMessage = new MessageBufferWrapper( stream.GetBuffer(), 0, stream.Length );
			
			stream = null;
			writer = null;

			// Do call
			IMessageBuffer replyMessage = _target.Invoke( requestMessage, false );

			// Deserialize reply
			BitReader reader = new BitReader( replyMessage.GetStream() );
			
			return reader.ReadInt32( 32 );

			// In the case of a void return, there should be nothing in the reply; if an exception
			// occurs, it will be thrown inside the Invoke -> EndInvoke call. 
 */
		}
	}
	
	interface ISimpleExceptionReporter {
		void ReportException( int callID, string exceptionTypeName, string message, string stackTrace );
	}
	
	interface IManagedExceptionReporter {
		void ReportException( int callID, byte[] serializedException );
	}
}