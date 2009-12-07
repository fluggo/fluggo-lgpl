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

namespace Fluggo.Xml {
	sealed class XmlDeclarationParser : Parser {
		ParserState _state;
		Mode _mode = Mode.Start;
		Stack<Mode> _endAction = new Stack<Mode>();
		Queue<char> _charQueue;
		readonly static string __encodingStart = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		readonly static string __encoding = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789._-";
		char _quoteChar;
		bool _whitespaceSeen;
		string _encoding;
		bool? _standalone;

		public XmlDeclarationParser( ParserState state ) {
			if( state == null )
				throw new ArgumentNullException( "state" );

			_state = state;
		}

		enum Mode {
			Start,
			ReadString,
			StartVersion,
			StartVersionString,
			StartEq,
			StartVersionQuote,
			StartEncodingQuote,
			StartEncoding,
			Encoding,
			EndEncodingQuote,
			StartStandaloneQuote,
			Standalone,
			StartEncodingDecl,
			
			ReadEq,
			EatSpace,
			EatRequiredSpace,
			QuestionMark,
			End
		}

		public override ParseAction ParseChar( char value ) {
			for( ;; ) {
				switch( _mode ) {
					case Mode.Start:
						_charQueue = new Queue<char>( "<?xml".ToCharArray() );
						_mode = Mode.ReadString;
						_endAction.Push( Mode.StartVersion );
						continue;
						
					case Mode.ReadString:
						// Read the string, then move to the end action
						if( value != _charQueue.Peek() ) {
							_state.ThrowUnexpectedCharacter( _charQueue.Peek(), value );
							throw new UnexpectedException();
						}
						
						_charQueue.Dequeue();
						
						if( _charQueue.Count == 0 ) {
							_mode = _endAction.Pop();
							_charQueue = null;
						}
						
						return ParseAction.Continue;
						
					case Mode.EatRequiredSpace:
						// Eat at least one whitespace character
						if( !XmlHelp.IsWhitespace( value ) ) {
							_state.ThrowExpectedWhitespace( value );
							throw new UnexpectedException();
						}
						
						_whitespaceSeen = true;
						_mode = Mode.EatSpace;
						return ParseAction.Continue;
					
					case Mode.EatSpace:
						// Eat any whitespace, then move to the end action
						if( !XmlHelp.IsWhitespace( value ) ) {
							_mode = _endAction.Pop();
							continue;
						}
						
						_whitespaceSeen = true;
						return ParseAction.Continue;
						
					case Mode.StartVersion:
						_mode = Mode.EatRequiredSpace;
						_endAction.Push( Mode.StartVersionString );
						continue;
						
					case Mode.StartVersionString:
						_mode = Mode.ReadString;
						_charQueue = new Queue<char>( "version".ToCharArray() );
						_endAction.Push( Mode.StartVersionQuote );
						_endAction.Push( Mode.StartEq );
						continue;
						
					case Mode.StartEq:
						_mode = Mode.EatSpace;
						_endAction.Push( Mode.ReadEq );
						continue;
						
					case Mode.ReadEq:
						if( value != '=' ) {
							_state.ThrowUnexpectedCharacter( '=', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.EatSpace;
						return ParseAction.Continue;
						
					case Mode.StartVersionQuote:
						if( value != '\'' && value != '"' ) {
							_state.ThrowExpectedQuote( value );
							throw new UnexpectedException();
						}
						
						_charQueue = new Queue<char>( ("1.0" + value).ToCharArray() );
						_mode = Mode.ReadString;
						_endAction.Push( Mode.StartEncodingDecl );
						_endAction.Push( Mode.EatSpace );
						
						return ParseAction.Continue;
						
					case Mode.StartEncodingDecl:
						// Figure out what's next; it may not be the encoding decl
						if( value == '?' ) {
							// It's the end... queue the question mark
							_mode = Mode.QuestionMark;
							return ParseAction.Continue;
						}
						
						if( value == 'e' && _encoding == null && _standalone == null && _whitespaceSeen ) {
							// It's the encoding decl start
							_charQueue = new Queue<char>( "encoding".ToCharArray() );
							_mode = Mode.ReadString;
							_endAction.Push( Mode.StartEncodingQuote );
							_endAction.Push( Mode.StartEq );
							continue;
						}
						
						if( value == 's' && _standalone == null && _whitespaceSeen ) {
							// It's the standalone decl start
							_charQueue = new Queue<char>( "standalone".ToCharArray() );
							_mode = Mode.ReadString;
							_endAction.Push( Mode.StartStandaloneQuote );
							_endAction.Push( Mode.StartEq );
							continue;
						}
						
						_state.ThrowUnexpectedCharacter( value, "XML declaration" );
						throw new UnexpectedException();
						
					case Mode.StartEncodingQuote:
						if( value != '\'' && value != '"' ) {
							_state.ThrowExpectedQuote( value );
							throw new UnexpectedException();
						}

						_quoteChar = value;
						_charQueue = new Queue<char>();
						_mode = Mode.StartEncoding;

						return ParseAction.Continue;
					
					case Mode.StartEncoding:
						if( __encodingStart.IndexOf( value ) == -1 ) {
							_state.ThrowUnexpectedCharacter( value, "encoding name" );
							throw new UnexpectedException();
						}
						
						_charQueue.Enqueue( value );
						_mode = Mode.Encoding;
						return ParseAction.Continue;
					
					case Mode.Encoding:
						if( value == _quoteChar ) {
							_encoding = new string( _charQueue.ToArray() );
							_whitespaceSeen = false;
							_endAction.Push( Mode.StartEncodingDecl );
							_mode = Mode.EatSpace;

							return ParseAction.Continue;
						}
						
						if( __encoding.IndexOf( value ) == -1 ) {
							_state.ThrowUnexpectedCharacter( value, "encoding name" );
							throw new UnexpectedException();
						}
						
						_charQueue.Enqueue( value );
						return ParseAction.Continue;
					
					case Mode.StartStandaloneQuote:
						if( value != '\'' && value != '"' ) {
							_state.ThrowExpectedQuote( value );
							throw new UnexpectedException();
						}

						_quoteChar = value;
						_mode = Mode.Standalone;

						return ParseAction.Continue;
						
					case Mode.Standalone:
						// Determine "yes" or "no"
						if( value == 'y' ) {
							// Read the remainder of the string
							_standalone = true;
							_charQueue = new Queue<char>( ("es" + _quoteChar).ToCharArray() );
						}
						else if( value == 'n' ) {
							// Read the remainder of the string
							_standalone = false;
							_charQueue = new Queue<char>( ("o" + _quoteChar).ToCharArray() );
						}
						else {
							_state.ThrowUnexpectedCharacter( value, "standalone attribute" );
							throw new UnexpectedException();
						}

						_mode = Mode.ReadString;
						_whitespaceSeen = false;
						_endAction.Push( Mode.StartEncodingDecl );
						_endAction.Push( Mode.EatSpace );
						return ParseAction.Continue;
						
					case Mode.QuestionMark:
						// We've seen the question mark, now what?
						if( value != '>' ) {
							_state.ThrowUnexpectedCharacter( '>', value );
							throw new UnexpectedException();
						}

						_mode = Mode.End;
						_state.Listener.PushXmlDeclaration( _encoding, _standalone );
						
						return ParseAction.LastCharacter;
						
					case Mode.End:
						return ParseAction.End;
						
					default:
						throw new UnexpectedException();
				}
			}
		}
	}
}