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
using System.Threading;
using System.Collections.Generic;
using Fluggo.Communications;

namespace Fluggo.Xml {
	class ScopedDictionary<TKey,TValue> {
		Stack<KeyValuePair<Dictionary<TKey,TValue>,int>> _stack = new Stack<KeyValuePair<Dictionary<TKey,TValue>,int>>();
		Dictionary<TKey,TValue> _currentDictionary = null;
		int _levelCount;

		public void Add( TKey key, TValue value ) {
			if( _currentDictionary == null )
				_currentDictionary = new Dictionary<TKey,TValue>();

			_currentDictionary.Add( key, value );
		}

		public bool ContainsKey( TKey key ) {
			if( _currentDictionary != null && _currentDictionary.ContainsKey( key ) )
				return true;

			foreach( KeyValuePair<Dictionary<TKey, TValue>, int> pair in _stack ) {
				if( pair.Key.ContainsKey( key ) )
					return true;
			}
			
			return false;
		}

/*		public bool Remove( TKey key ) {
			if( _currentDictionary == null )
				return false;
				
			return _currentDictionary.Remove( key );
		}*/

		public bool TryGetValue( TKey key, out TValue value ) {
			if( _currentDictionary != null && _currentDictionary.TryGetValue( key, out value ) )
				return true;

			foreach( KeyValuePair<Dictionary<TKey, TValue>, int> pair in _stack ) {
				if( pair.Key.TryGetValue( key, out value ) )
					return true;
			}
			
			value = default(TValue);
			
			return false;
		}

/*		public TValue this[TKey key] {
			get {
				throw new Exception( "The method or operation is not implemented." );
			}
			set {
				throw new Exception( "The method or operation is not implemented." );
			}
		}*/

/*		public void Clear() {
			if( _currentDictionary != null )
				_currentDictionary.Clear();
		}*/

		public void PushScope() {
			if( _currentDictionary == null || _currentDictionary.Count == 0 ) {
				// Don't push an empty dictionary on the stack; just remember that there was an empty dictionary here
				_levelCount++;
			}
			else {
				_stack.Push( new KeyValuePair<Dictionary<TKey,TValue>,int>( _currentDictionary, _levelCount ) );
				_currentDictionary = null;
				_levelCount = 0;
			}
		}
		
		public void PopScope() {
			if( _levelCount == 0 ) {
				KeyValuePair<Dictionary<TKey,TValue>,int> pair = _stack.Pop();
				_currentDictionary = pair.Key;
				_levelCount = pair.Value;
			}
			else {
				// There was an empty dictionary at the previous level, so make sure our current dictionary goes away
				_levelCount--;
				_currentDictionary = null;
			}
			
		}
	}
	
	public abstract class XmlAsyncTextReader : XmlAsyncReader {
		ParserState _state;
		Node _currentNode = null;
		AsynchronousQueue<Node> _nodeQueue = new AsynchronousQueue<Node>();
		ReadState _readState = ReadState.Initial;
		XmlException _error;
		IAsyncResult _lastAsyncResult;
		object _readLock = new object();
		bool _lastReadEmpty;

	#region Nodes
		static string NodeToValue( Node node ) {
			return node.ToString();
		}
		
		sealed class Attribute : Node {
			List<Node> _nodes = new List<Node>();
			char _quoteChar;
			int _currentNode = -1;
			
			public Attribute( string name, int depth, char quoteChar ) : base(XmlNodeType.Attribute, name, depth, null) {
				_quoteChar = quoteChar;
			}
			
			public void AddNode( Node node ) {
				if( node == null )
					throw new ArgumentNullException( "node" );

				_nodes.Add( node );
			}
			
			public override string Value {
				get {
					if( _currentNode == -1 ) {
						// Concatenate the values of all nodes
						if( _nodes == null )
							return string.Empty;
							
						return string.Concat( _nodes.ConvertAll<string>( NodeToValue ).ToArray() );
					}
					
					return _nodes[_currentNode].Value;
				}
			}

			public override string Name {
				get {
					if( _currentNode == -1 )
						return base.Name;
						
					return _nodes[_currentNode].Name;
				}
			}

			public override string LocalName {
				get {
					if( _currentNode == -1 )
						return base.LocalName;
						
					return _nodes[_currentNode].LocalName;
				}
			}

			public override string Prefix {
				get {
					if( _currentNode == -1 )
						return base.Prefix;
						
					return _nodes[_currentNode].Prefix;
				}
			}

			public override string NamespaceURI {
				get {
					if( _currentNode == -1 )
						return base.NamespaceURI;
						
					return _nodes[_currentNode].NamespaceURI;
				}
			}
			
			public override int Depth {
				get {
					if( _currentNode == -1 )
						return base.Depth;
						
					return base.Depth + 1;
				}
			}

			public override XmlNodeType NodeType {
				get {
					if( _currentNode == -1 )
						return XmlNodeType.Attribute;
						
					return _nodes[_currentNode].NodeType;
				}
			}

			public override char QuoteChar {
				get {
					return _quoteChar;
				}
			}
			
			public void StartRead() {
				_currentNode = 0;
			}
			
			public void ResetRead() {
				_currentNode = -1;
			}
			
			public bool Read() {
//				if( _currentNode == -1 )
//					return false;
				
				if( _currentNode >= (_nodes.Count - 1) )
					return false;
					
				_currentNode++;
				return true;
			}
		}
		
		sealed class ElementNode : Node {
			List<Attribute> _attributes = null;
			int _currentAttr = -1;
			bool _isEmpty;

			public ElementNode( string name, int depth ) : base( XmlNodeType.Element, name, depth, null ) {
			}

			public void AddAttribute( Attribute attribute ) {
				if( attribute == null )
					throw new ArgumentNullException( "attribute" );

				if( _attributes == null )
					_attributes = new List<Attribute>();
				
				_attributes.Add( attribute );
			}
			
			public override void ResolveNamespaces( ScopedDictionary<string, string> namespaces ) {
				base.ResolveNamespaces( namespaces );
				
				foreach( Attribute attr in _attributes )
					attr.ResolveNamespaces( namespaces );
			}

			public override int AttributeCount {
				get {
					if( _attributes == null )
						return 0;
						
					return _attributes.Count;
				}
			}

			public override string GetAttribute( int i ) {
				if( _attributes == null )
					throw new ArgumentOutOfRangeException( "i" );
				
				return _attributes[i].Value;
			}
			
			public void SetEmpty() {
				_isEmpty = true;
			}

			public override int Depth {
				get {
					if( _currentAttr != -1 )
						return _attributes[_currentAttr].Depth;
						
					return base.Depth;
				}
			}

			public override string Name {
				get {
					if( _currentAttr != -1 )
						return _attributes[_currentAttr].Name;
						
					return base.Name;
				}
			}

			public override string LocalName {
				get {
					if( _currentAttr != -1 )
						return _attributes[_currentAttr].LocalName;
						
					return base.LocalName;
				}
			}

			public override string NamespaceURI {
				get {
					if( _currentAttr != -1 )
						return _attributes[_currentAttr].NamespaceURI;
						
					return base.NamespaceURI;
				}
			}

			public override string Prefix {
				get {
					if( _currentAttr != -1 )
						return _attributes[_currentAttr].Prefix;
						
					return base.Prefix;
				}
			}

			public override XmlNodeType NodeType {
				get {
					if( _currentAttr != -1 )
						return _attributes[_currentAttr].NodeType;
						
					return XmlNodeType.Element;
				}
			}

			public override char QuoteChar {
				get {
					if( _currentAttr != -1 )
						return _attributes[_currentAttr].QuoteChar;
						
					return base.QuoteChar;
				}
			}

			public override string Value {
				get {
					if( _currentAttr != -1 )
						return _attributes[_currentAttr].Value;
						
					return string.Empty;
				}
			}

			public override string GetAttribute( string name ) {
				if( _attributes == null )
					return null;
				
				foreach( Attribute attribute in _attributes ) {
					if( attribute.Name == name )
						return attribute.Value;
				}
				
				return null;
			}

			public override string GetAttribute( string name, string namespaceURI ) {
				if( _attributes == null )
					return null;

				foreach( Attribute attribute in _attributes ) {
					if( attribute.LocalName == name && attribute.NamespaceURI == namespaceURI )
						return attribute.Value;
				}
				
				return null;
			}

			public override void MoveToAttribute( int i ) {
				if( _attributes == null || i < 0 || i >= _attributes.Count )
					throw new ArgumentOutOfRangeException( "i" );
					
				_currentAttr = i;
			}

			public override bool MoveToAttribute( string name ) {
				if( _attributes == null )
					return false;
				
				for( int i = 0; i < _attributes.Count; i++ ) {
					if( _attributes[i].Name == name ) {
						_currentAttr = i;
						_attributes[i].ResetRead();
						return true;
					}
				}
				
				return false;
			}

			public override bool MoveToAttribute( string name, string namespaceURI ) {
				if( _attributes == null )
					return false;
				
				for( int i = 0; i < _attributes.Count; i++ ) {
					if( _attributes[i].LocalName == name && _attributes[i].NamespaceURI == namespaceURI ) {
						_currentAttr = i;
						_attributes[i].ResetRead();
						return true;
					}
				}

				return false;
			}

			public override bool MoveToFirstAttribute() {
				if( _attributes == null || _attributes.Count == 0 )
					return false;
					
				_currentAttr = 0;
				_attributes[0].ResetRead();
				return true;
			}

			public override bool MoveToNextAttribute() {
				if( _attributes == null || _currentAttr >= (_attributes.Count - 1) )
					return false;
					
				_currentAttr++;
				_attributes[_currentAttr].ResetRead();
				return true;
			}

			public override bool MoveToElement() {
				try {
					return _currentAttr != -1;
				}
				finally {
					_currentAttr = -1;
				}
			}

			public override bool ReadAttributeValue() {
				if( _currentAttr != -1 )
					return _attributes[_currentAttr].Read();
					
				return false;
			}

			public override bool IsEmptyElement {
				get {
					return _isEmpty;
				}
			}
		}
		
		class Node {
			int _depth;
			string _baseURI = string.Empty;
			string _value;
			XmlNodeType _type;
			string _localName;
			string _prefix;
			string _namespaceURI;
			
			public Node( XmlNodeType type, string name, int depth, string value ) {
				_type = type;
				_value = value;
				
				if( _value == null )
					_value = string.Empty;
				
				if( name != null ) {
					string[] nameParts = name.Split( new char[] { ':' }, 2 );

					if( nameParts.Length == 1 ) {
						_prefix = null;
						_namespaceURI = string.Empty;
						_localName = nameParts[0];
					}
					else if( nameParts.Length == 2 ) {
						_prefix = nameParts[0];
						_namespaceURI = null;
						
						_localName = nameParts[1];
					}
					else
						throw new UnexpectedException();
				}
				else {
					_prefix = null;
					_namespaceURI = string.Empty;
					_localName = string.Empty;
				}
				
				_depth = depth;
			}
			
			public void AppendValue( string value ) {
				_value += value;
			}

			public virtual void ResolveNamespaces( ScopedDictionary<string, string> namespaces ) {
				string uri;
				
				if( !namespaces.TryGetValue( (_prefix == null) ? string.Empty : _prefix, out uri ) ) {
					if( _prefix == null )
						_namespaceURI = null;
					else
						throw new XmlException( "Could not resolve the namespace \"" + _prefix + "\".", null );
				}
					
				_namespaceURI = uri;
			}
			
			public virtual int AttributeCount {
				get {
					return 0;
				}
			}
			
			public virtual string GetAttribute( int i ) {
				throw new ArgumentOutOfRangeException( "i" );
			}
			
			public virtual string GetAttribute( string name ) {
				return null;
			}
			
			public virtual string GetAttribute( string name, string namespaceURI ) {
				return null;
			}
			
			public virtual bool MoveToFirstAttribute() {
				return false;
			}
			
			public virtual bool MoveToNextAttribute() {
				return false;
			}
			
			public virtual void MoveToAttribute( int i ) {
				throw new ArgumentOutOfRangeException( "i" );
			}
			
			public virtual bool MoveToAttribute( string name ) {
				return false;
			}
			
			public virtual bool MoveToAttribute( string name, string namespaceURI ) {
				return false;
			}
			
			public virtual bool ReadAttributeValue() {
				return false;
			}
			
			public virtual bool IsEmptyElement {
				get { return false; }
			}
			
			public virtual bool MoveToElement() {
				return false;
			}
			
			public virtual char QuoteChar
				{ get { return '"'; } }
			
			public string BaseURI { get { return _baseURI; } }
			public virtual int Depth { get { return _depth; } }

			public virtual string Value {
				get {
					return _value;
				}
			}
			
			public virtual XmlNodeType NodeType {
				get {
					return _type;
				}
			}
			
			public virtual string LocalName {
				get {
					return _localName;
				}
			}
			
			public virtual string Prefix {
				get {
					if( _prefix == null )
						return string.Empty;
					
					return _prefix;
				}
			}
			
			public virtual string Name {
				get {
					if( _prefix != null )
						return _prefix + ":" + _localName;
					
					return _localName;
				}
			}
			
			public virtual string NamespaceURI {
				get {
					return _namespaceURI;
				}
			}
			
			public bool HasValue {
				get {
					switch( _type ) {
						case XmlNodeType.Attribute:
						case XmlNodeType.CDATA:
						case XmlNodeType.Comment:
						case XmlNodeType.DocumentType:
						case XmlNodeType.ProcessingInstruction:
						case XmlNodeType.SignificantWhitespace:
						case XmlNodeType.Text:
						case XmlNodeType.Whitespace:
						case XmlNodeType.XmlDeclaration:
							return true;
							
						default:
							return false;
					}
				}
			}

			public override string ToString() {
				if( NodeType == XmlNodeType.EntityReference )
					return "&" + Name + ";";
					
				return Value;
			}
		}
	#endregion

		protected XmlAsyncTextReader() {
			_state = new ParserState( new ParserListener( this ) );
			_state.PushParser( new PrologParser( _state ) );
		}

		protected bool Parse( string text ) {
			return _state.Parse( text );
		}

	#region Listener
		class ParserListener : IXmlParseListener {
			XmlAsyncTextReader _owner;
			ScopedDictionary<string, string> _namespaces;
			Stack<string> _elementNames = new Stack<string>();
			ElementNode _element;
			Attribute _attribute;
			StringBuilder _text;
			
			public ParserListener( XmlAsyncTextReader owner ) {
				if( owner == null )
					throw new ArgumentNullException( "owner" );

				_owner = owner;

				_namespaces = new ScopedDictionary<string, string>();
				_namespaces.Add( "xml", "http://www.w3.org/XML/1998/namespace" );
				_namespaces.Add( "xmlns", "http://www.w3.org/2000/xmlns/" );
			}
		
			public void PushElementName( string name ) {
				ClearText();

				_element = new ElementNode( name, _elementNames.Count );
				_elementNames.Push( name );
				_namespaces.PushScope();
			}

			public void PushAttributeName( string name, char quoteChar ) {
				if( _element == null )
					throw new InvalidOperationException( "Attribute name pushed with no active element." );
					
				ClearAttribute();
				_attribute = new Attribute( name, _elementNames.Count, quoteChar );
			}

			public void PushAttributeText( string text ) {
				if( _attribute == null )
					throw new InvalidOperationException( "Attribute text pushed with no active attribute." );
					
				if( _text == null )
					_text = new StringBuilder();
				
				_text.Append( text );
			}

			public void PushAttributeEntityRef( string name ) {
				if( _attribute == null )
					throw new InvalidOperationException( "Attribute text pushed with no active attribute." );

				// Recognize the base set of entities
				switch( name ) {
					case "lt":
					case "gt":
					case "amp":
					case "apos":
					case "quot":
						if( _text == null )
							_text = new StringBuilder();
							
						switch( name ) {
							case "lt":
								_text.Append( '<' );
								break;
								
							case "gt":
								_text.Append( '>' );
								break;
								
							case "amp":
								_text.Append( '&' );
								break;
								
							case "apos":
								_text.Append( '\'' );
								break;
								
							case "quot":
								_text.Append( '"' );
								break;
						}
						
						return;
				}
				
				if( _text != null ) {
					_attribute.AddNode( new Node( XmlNodeType.Text, string.Empty, 0, _text.ToString() ) );
					_text = null;
				}
				
				_attribute.AddNode( new Node( XmlNodeType.EntityReference, name, 0, null ) );
			}

			public void PushAttributeCharRef( int codePoint ) {
				if( _attribute == null )
					throw new InvalidOperationException( "Attribute text pushed with no active attribute." );
					
				if( _text == null )
					_text = new StringBuilder();
					
				_text.Append( (char) codePoint );
			}
			
			void ClearAttribute() {
				if( _attribute != null ) {
					// Close out the current text on the attribute
					if( _text != null ) {
						_attribute.AddNode( new Node( XmlNodeType.Text, string.Empty, 0, _text.ToString() ) );
						_text = null;
					}
//					else {
//						_attribute.AddNode( new Node( XmlNodeType.Text, string.Empty, 0, string.Empty ) );
//					}

					_element.AddAttribute( _attribute );
					
					// Figure out if it's a new namespace
					if( _attribute.Prefix == "xmlns" ) {
						_namespaces.Add( _attribute.LocalName, _attribute.Value );
					}
					
					_attribute = null;
				}
			}
			
			void ClearElement() {
				if( _element != null ) {
					ClearAttribute();
					
					// Resolve any and all namespace references
					_element.ResolveNamespaces( _namespaces );
					
					_owner._nodeQueue.Enqueue( _element );
					_element = null;
				}
			}
			
			void ClearText() {
				if( _element != null ) {
					ClearElement();
					return;
				}
				
				if( _text != null ) {
					// Submit the text!
					_owner._nodeQueue.Enqueue( new Node( XmlNodeType.Text, string.Empty, _elementNames.Count, _text.ToString() ) );
					_text = null;
				}
			}

			public void PushText( string text ) {
				ClearElement();
				
				if( _text == null )
					_text = new StringBuilder();
					
				_text.Append( text );
			}

			public void PushCharRef( int codePoint ) {
				ClearText();

				if( _text == null )
					_text = new StringBuilder();

				_text.Append( (char) codePoint );
			}

			public void PushEntityRef( string name ) {
				ClearText();
				
				// Recognize the base set of entities
				switch( name ) {
					case "lt":
					case "gt":
					case "amp":
					case "apos":
					case "quot":
						if( _text == null )
							_text = new StringBuilder();

						switch( name ) {
							case "lt":
								_text.Append( '<' );
								break;

							case "gt":
								_text.Append( '>' );
								break;

							case "amp":
								_text.Append( '&' );
								break;

							case "apos":
								_text.Append( '\'' );
								break;

							case "quot":
								_text.Append( '"' );
								break;
						}

						return;
				}

				if( _text != null ) {
					_owner._nodeQueue.Enqueue( new Node( XmlNodeType.Text, string.Empty, _elementNames.Count, _text.ToString() ) );
					_text = null;
				}
				
				_owner._nodeQueue.Enqueue( new Node( XmlNodeType.EntityReference, name, _elementNames.Count, null ) );
			}

			public void PushCloseEmptyElement() {
				if( _element == null )
					throw new InvalidOperationException( "Empty element close pushed without an active element." );
				
				_element.SetEmpty();
				ClearElement();
				_namespaces.PopScope();
				_elementNames.Pop();
			}

			public void PushEndElement() {
				ClearText();
				
				_namespaces.PopScope();
				_owner._nodeQueue.Enqueue( new Node( XmlNodeType.EndElement, _elementNames.Pop(), _elementNames.Count, null ) );
			}

			public void PushComment( string text ) {
				ClearText();
				_owner._nodeQueue.Enqueue( new Node( XmlNodeType.Comment, string.Empty, _elementNames.Count, text ) );
			}

			public void PushDoctype( string name, string publicID, string systemID ) {
				// Do nothing for the moment
			}

			public void PushXmlDeclaration( string encoding, bool? standalone ) {
				StringBuilder builder = new StringBuilder();
				builder.Append( "version='1.0'" );
				
				if( encoding != null ) {
					builder.AppendFormat( " encoding={0}'", encoding );
				}
				
				if( standalone != null ) {
					builder.Append( standalone.Value ? " standalone='yes'" : " standalone='no'" );
				}
				
				_owner._nodeQueue.Enqueue( new Node( XmlNodeType.XmlDeclaration, "xml", 0, builder.ToString() ) );
			}

			public void PushParseError( int line, int column, string error ) {
				lock( _owner._readLock ) {
					_owner._error = new XmlException( error, null, line, column );
					_owner._readState = ReadState.Error;
				}
				
				#warning TODO: Cease parsing
			}
		}
	#endregion

	#region Node properties and methods
		public override int AttributeCount {
			get {
				if( _currentNode == null )
					return 0;

				return _currentNode.AttributeCount;
			}
		}

		public override string BaseURI {
			get {
				if( _currentNode == null )
					return string.Empty;
				
				return _currentNode.BaseURI;
			}
		}

		public override int Depth {
			get {
				if( _currentNode == null )
					return 0;
			
				return _currentNode.Depth;
			}
		}

		public override bool EOF {
			get { return ReadState == ReadState.EndOfFile; }
		}

		public override string GetAttribute( int i ) {
			if( _currentNode == null )
				throw new InvalidOperationException();
				
			return _currentNode.GetAttribute( i );
		}

		public override string GetAttribute( string name, string namespaceURI ) {
			if( _currentNode == null )
				throw new InvalidOperationException();

			return _currentNode.GetAttribute( name, namespaceURI );
		}

		public override string GetAttribute( string name ) {
			if( _currentNode == null )
				throw new InvalidOperationException();

			return _currentNode.GetAttribute( name );
		}

		public override bool HasValue {
			get {
				if( _currentNode == null )
					return false;
				
				return _currentNode.HasValue;
			}
		}

		public override bool IsEmptyElement {
			get {
				if( _currentNode == null )
					return false;

				return _currentNode.IsEmptyElement;
			}
		}

		public override string LocalName {
			get {
				if( _currentNode == null )
					return string.Empty;

				return _currentNode.LocalName;
			}
		}

		public override string Name {
			get {
				if( _currentNode == null )
					return string.Empty;
					
				return _currentNode.Name;
			}
		}

		public override bool MoveToAttribute( string name, string ns ) {
			if( _currentNode == null )
				throw new InvalidOperationException();

			return _currentNode.MoveToAttribute( name, ns );
		}

		public override bool MoveToAttribute( string name ) {
			if( _currentNode == null )
				throw new InvalidOperationException();

			return _currentNode.MoveToAttribute( name );
		}

		public override bool MoveToFirstAttribute() {
			if( _currentNode == null )
				throw new InvalidOperationException();

			return _currentNode.MoveToFirstAttribute();
		}

		public override bool MoveToNextAttribute() {
			if( _currentNode == null )
				throw new InvalidOperationException();

			return _currentNode.MoveToNextAttribute();
		}

		public override string NamespaceURI {
			get {
				if( _currentNode == null )
					return string.Empty;

				return _currentNode.NamespaceURI;
			}
		}

		public override XmlNodeType NodeType {
			get {
				if( _currentNode == null )
					return XmlNodeType.None;
				
				return _currentNode.NodeType;
			}
		}

		public override string Prefix {
			get {
				if( _currentNode == null )
					return string.Empty;
				
				return _currentNode.Prefix;
			}
		}

		public override string Value {
			get {
				if( _currentNode == null )
					return string.Empty;
					
				return _currentNode.Value;
			}
		}

		public override char QuoteChar {
			get {
				if( _currentNode == null )
					return '"';
				
				return _currentNode.QuoteChar;
			}
		}
	#endregion

		public override void Close() {
			throw new Exception( "The method or operation is not implemented." );
		}

		public override string LookupNamespace( string prefix ) {
			throw new Exception( "The method or operation is not implemented." );
		}

		public override bool MoveToElement() {
			if( _currentNode == null )
				throw new InvalidOperationException();

			return _currentNode.MoveToElement();
		}

		public override XmlNameTable NameTable {
			get { throw new NotSupportedException(); }
		}

		class EmptyRead : BaseAsyncResult {
			public EmptyRead( AsyncCallback callback, object state ) : base( callback, state ) {
				Complete( true );
			}
		}
		
		public override IAsyncResult BeginRead( AsyncCallback callback, object state ) {
			if( _error != null )
				throw _error;
				
			lock( _readLock ) {
				if( _lastAsyncResult != null ) {
					// Only allow one read at a time
					throw new InvalidOperationException( "A read operation is already pending on this reader." );
				}
			
				if( _readState == ReadState.EndOfFile || _readState == ReadState.Closed ) {
					_lastReadEmpty = true;
					return (_lastAsyncResult = new EmptyRead( callback, state ));
				}
				
				_lastReadEmpty = false;
				return (_lastAsyncResult = _nodeQueue.BeginDequeue( callback, state ));
			}
		}

		public override bool EndRead( IAsyncResult result ) {
			if( result == null )
				throw new ArgumentNullException( "result" );

			// If these are equal, we won't really need to take a lock until the end; no
			// new read can be started
			if( result != _lastAsyncResult )
				throw new ArgumentException( "The given asynchronous result did not originate with this XML reader." );

			if( _error != null ) {
				_lastAsyncResult = null;
				throw _error;
			}
				
			if( _lastReadEmpty )
				return false;

			Node node = _nodeQueue.EndDequeue( result );

			lock( _readLock ) {
				if( _error != null ) {
					_lastAsyncResult = null;
					throw _error;
				}

				if( node.NodeType == XmlNodeType.None ) {
					_readState = ReadState.EndOfFile;
					_lastAsyncResult = null;
					return false;
				}
				else {
					_readState = ReadState.Interactive;
					_currentNode = node;
					_lastAsyncResult = null;
					return true;
				}
			}
		}

		public override bool Read() {
			return EndRead( BeginRead( null, null ) );
		}

		public override bool ReadAttributeValue() {
			if( _currentNode == null )
				throw new InvalidOperationException();

			return _currentNode.ReadAttributeValue();
		}

		public override ReadState ReadState {
			get {
				return _readState;
			}
		}

		public override void ResolveEntity() {
			throw new Exception( "The method or operation is not implemented." );
		}

	}
}