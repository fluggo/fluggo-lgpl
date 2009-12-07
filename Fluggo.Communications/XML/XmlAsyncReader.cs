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
using System.Threading;
using Fluggo.Communications;
using System.Collections.Generic;

namespace Fluggo.Xml {
	public abstract class XmlAsyncReader : XmlReader {
		ProcessingQueue<XmlOperation> _processQueue;
		
		protected XmlAsyncReader() {
			_processQueue = new ProcessingQueue<XmlOperation>( Process );
		}
		
	#region Operations
		private WaitHandle Process( XmlOperation operation, bool synchronous ) {
			if( operation == null )
				throw new ArgumentNullException( "operation" );

			return operation.Process( synchronous );
		}
		
		/// <summary>
		/// Queues the given operation for processing.
		/// </summary>
		/// <param name="operation"><see cref="XmlOperation"/> to process.</param>
		/// <remarks>This method allows several operations to be queued and processed sequentially. If no operation is currently
		///   waiting, the given operation is processed right away.</remarks>
		protected void QueueOperation( XmlOperation operation ) {
			if( operation == null )
				throw new ArgumentNullException( "operation" );

			_processQueue.Enqueue( operation );
		}
		
		protected abstract class XmlOperation : BaseAsyncResult {
			protected XmlOperation( AsyncCallback callback, object state ) : base( callback, state ) {
			}
			
			/// <summary>
			/// Performs as much of the operation as possible.
			/// </summary>
			/// <param name="synchronous">True if the call has been invoked on the same thread as the original caller, or false if it might be on another thread.</param>
			/// <returns>Returns <see langword='null'/> if the operation is complete, or a <see cref="WaitHandle"/> for an operation that must complete
			///   first. The method will be invoked again when the <see cref="WaitHandle"/> is signaled.</returns>
			public abstract WaitHandle Process( bool synchronous );
		}

		// BJC: This is pretty much the template for a new operation.
		/*class ReadSubtreeAsNodeOperation : XmlOperation {
			XmlAsyncReader _reader;
			IAsyncResult _lastRead;
			XmlNode _result;

			public ReadSubtreeAsNodeOperation( XmlAsyncReader reader, AsyncCallback callback, object state ) : base( callback, state ) {
				if( reader == null )
					throw new ArgumentNullException( "reader" );

				_reader = reader;
			}
		
			public override WaitHandle Process( bool synchronous ) {
				if( IsCompleted )
					return null;
				
				for( ;; ) {
					if( _lastRead != null ) {
						try {
							if( !_reader.EndRead( _lastRead ) ) {
								_result = XmlNodeType.None;
								Complete( synchronous );
								return null;
							}
						}
						catch( Exception ex ) {
							CompleteError( ex );
						}
						
						_lastRead = null;
					}

		 			// TODO: Fill in per-node
		 
					if( !_lastRead.IsCompleted )
						return _lastRead.AsyncWaitHandle;
				}
			}
			
			public new XmlNodeType End() {
				base.End();
				return _result;
			}
		}*/
	#endregion
		
	#region MoveToContent
		class MoveToContentOperation : XmlOperation {
			XmlAsyncReader _reader;
			IAsyncResult _lastRead;
			XmlNodeType _result;

			public MoveToContentOperation( XmlAsyncReader reader, AsyncCallback callback, object state ) : base( callback, state ) {
				if( reader == null )
					throw new ArgumentNullException( "reader" );

				_reader = reader;
			}
		
			public override WaitHandle Process( bool synchronous ) {
				if( IsCompleted )
					return null;
				
				for( ;; ) {
					if( _lastRead != null ) {
						try {
							if( !_reader.EndRead( _lastRead ) ) {
								_result = XmlNodeType.None;
								Complete( synchronous );
								return null;
							}
						}
						catch( Exception ex ) {
							CompleteError( ex );
						}
						
						_lastRead = null;
					}
					
					switch( _reader.NodeType ) {
						// End conditions
						case XmlNodeType.None:
							if( _reader.ReadState == ReadState.Initial ) {
								_lastRead = _reader.BeginRead( null, null );
							}
							else {
								_result = XmlNodeType.None;
								Complete( synchronous );
								return null;
							}
							break;
						
						case XmlNodeType.CDATA:
						case XmlNodeType.Element:
						case XmlNodeType.EndElement:
						case XmlNodeType.EntityReference:
						case XmlNodeType.EndEntity:
						case XmlNodeType.Text:
						case XmlNodeType.Entity:
							_result = _reader.NodeType;
							Complete( synchronous );
							return null;

						// Move conditions
						case XmlNodeType.ProcessingInstruction:
						case XmlNodeType.DocumentType:
						case XmlNodeType.Comment:
						case XmlNodeType.Whitespace:
						case XmlNodeType.SignificantWhitespace:
							_lastRead = _reader.BeginRead( null, null );
							break;

						// Not mentioned
						case XmlNodeType.Notation:
						case XmlNodeType.Attribute:
						case XmlNodeType.Document:
						case XmlNodeType.DocumentFragment:
						case XmlNodeType.XmlDeclaration:
							goto case XmlNodeType.ProcessingInstruction;

						default:
							throw new Exception( "Invalid node type encountered." );
					}

					if( !_lastRead.IsCompleted )
						return _lastRead.AsyncWaitHandle;
				}
			}
			
			public new XmlNodeType End() {
				base.End();
				return _result;
			}
		}

		public virtual IAsyncResult BeginMoveToContent( AsyncCallback callback, object state ) {
			MoveToContentOperation op = new MoveToContentOperation( this, callback, state );
			QueueOperation( op );

			return op;
		}

		public virtual XmlNodeType EndMoveToContent( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			MoveToContentOperation op = result as MoveToContentOperation;

			if( op == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginMoveToContent call on this object.", "result" );

			return op.End();
		}

		public override XmlNodeType MoveToContent() {
			return EndMoveToContent( BeginMoveToContent( null, null ) );
		}
	#endregion
		
		/// <summary>
		/// Begins an asynchronous read operation.
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="state"></param>
		/// <returns>Because this call modifies the state of the reader, only one read operation can be scheduled at a time.</returns>
		/// <exception cref="InvalidOperationException">A read operation is already pending on this reader.</exception>
		public virtual IAsyncResult BeginRead( AsyncCallback callback, object state ) {
			EmptyAsyncResult<bool> read = new EmptyAsyncResult<bool>( callback, state );
			
			try {
				read.Complete( Read() );
			}
			catch( Exception ex ) {
				read.CompleteError( ex );
			}
			
			return read;
		}
		
		public virtual bool EndRead( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			EmptyAsyncResult<bool> read = result as EmptyAsyncResult<bool>;

			if( read == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginRead call on this object.", "result" );

			return read.End();
		}
		
	#region ReadSubtreeAsNode
		class ReadSubtreeAsNodeOperation : XmlOperation {
			XmlAsyncReader _reader;
			IAsyncResult _lastRead;
			XmlDocument _document = new XmlDocument();
			XmlNode _result;
			Stack<XmlNode> _parentStack = new Stack<XmlNode>();

			public ReadSubtreeAsNodeOperation( XmlAsyncReader reader, AsyncCallback callback, object state ) : base( callback, state ) {
				if( reader == null )
					throw new ArgumentNullException( "reader" );

				_reader = reader;
			}
			
			private XmlNode CreateNodeFromReader( XmlAsyncReader reader ) {
				// BJC: This creates one node, and only one node, from the reader in question, and does not advance the reader.
				XmlNode result;
				
				switch( reader.NodeType ) {
					case XmlNodeType.Attribute:
						result = _document.CreateAttribute( reader.Prefix, reader.LocalName, reader.NamespaceURI );
						result.Value = reader.Value;
						break;
					case XmlNodeType.CDATA:
						result = _document.CreateCDataSection( reader.Value );
						break;
					case XmlNodeType.Comment:
						result = _document.CreateComment( reader.Value );
						break;
					case XmlNodeType.Document:
					case XmlNodeType.DocumentFragment:
					case XmlNodeType.EndElement:
					case XmlNodeType.Entity:
					case XmlNodeType.EndEntity:
					case XmlNodeType.None:
					case XmlNodeType.Notation:
					case XmlNodeType.ProcessingInstruction:
					default:
						throw new NotImplementedException( "An encountered node type is not yet supported by this class." );

					case XmlNodeType.DocumentType:
						result = _document.CreateDocumentType( reader.Name, reader["PUBLIC"], reader["SYSTEM"], null );
						break;
					case XmlNodeType.Element: {
						XmlElement element = _document.CreateElement( reader.Prefix, reader.LocalName, reader.NamespaceURI );

						for( int i = 0; i < reader.AttributeCount; i++ ) {
							reader.MoveToAttribute( i );

							XmlAttribute attr = _document.CreateAttribute( reader.Prefix, reader.LocalName, reader.NamespaceURI );
							attr.Value = reader.Value;
							
							element.Attributes.Append( attr );
						}
						
						reader.MoveToElement();
						
						result = element;
					}
						break;

					case XmlNodeType.EntityReference:
						result = _document.CreateEntityReference( reader.Name );
						break;
					case XmlNodeType.SignificantWhitespace:
						result = _document.CreateSignificantWhitespace( reader.Value );
						break;
					case XmlNodeType.Text:
						result = _document.CreateTextNode( reader.Value );
						break;
					case XmlNodeType.Whitespace:
						result = _document.CreateWhitespace( reader.Value );
						break;
					case XmlNodeType.XmlDeclaration:
						result = _document.CreateXmlDeclaration( "1.0", null, null );
						break;
				}
				
				return result;
			}
			
			public override WaitHandle Process( bool synchronous ) {
				if( IsCompleted )
					return null;
				
				for( ;; ) {
					if( _lastRead != null ) {
						try {
							if( !_reader.EndRead( _lastRead ) ) {
								// _parentStack.Count is not zero; therefore this can only successfully be one thing
								if( _parentStack.Count == 1 && _reader.NodeType == XmlNodeType.EndElement )
									Complete( synchronous );
								else
									CompleteError( new XmlException( "The end of the stream was reached." ) );
								
								return null;
							}
						}
						catch( Exception ex ) {
							CompleteError( ex );
						}
						
						_lastRead = null;
					}
					
					// Special processing for various node types
					if( _reader.NodeType == XmlNodeType.EndElement ) {
						// Just pop the current location
						if( _parentStack.Count == 0 ) {
							CompleteError( new InvalidOperationException( "This method cannot be called on the end tag of an element." ) );
							return null;
						}
							
						_parentStack.Pop();
					}
					else if( _reader.NodeType == XmlNodeType.Attribute ) {
						_reader.MoveToElement();
						continue;
					}
					else {
						XmlNode currentNode;
						
						try {
							currentNode = CreateNodeFromReader( _reader );
						}
						catch( Exception ex ) {
							CompleteError( ex );
							return null;
						}
						
						if( _parentStack.Count == 0 )
							_result = currentNode;
						else
							_parentStack.Peek().AppendChild( currentNode );
						
						if( _reader.NodeType == XmlNodeType.Element && !_reader.IsEmptyElement )
							_parentStack.Push( currentNode );
					}

					if( _parentStack.Count == 0 ) {
						Complete( synchronous );
						return null;
					}
					
					_lastRead = _reader.BeginRead( null, null );

					if( !_lastRead.IsCompleted )
						return _lastRead.AsyncWaitHandle;
				}
			}
			
			public new XmlNode End() {
				base.End();
				return _result;
			}
		}

		public virtual IAsyncResult BeginReadSubtreeAsNode( AsyncCallback callback, object state ) {
			ReadSubtreeAsNodeOperation op = new ReadSubtreeAsNodeOperation( this, callback, state );
			QueueOperation( op );
			
			return op;
		}
		
		public virtual XmlNode EndReadSubtreeAsNode( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			ReadSubtreeAsNodeOperation op = result as ReadSubtreeAsNodeOperation;

			if( op == null )
				throw new ArgumentException( "The given asynchronous result did not originate from a BeginReadSubtreeAsNode call on this object.", "result" );

			return op.End();
		}
		
		public virtual XmlNode ReadSubtreeAsNode() {
			return EndReadSubtreeAsNode( BeginReadSubtreeAsNode( null, null ) );
		}
	#endregion
	}
}