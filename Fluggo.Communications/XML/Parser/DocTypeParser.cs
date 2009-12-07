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
	sealed class DocTypeParser : Parser {
		ParserState _state;
		Mode _mode = Mode.Start;
		Queue<char> _openQueue = new Queue<char>( "<!DOCTYPE".ToCharArray() ),
			_charDataQueue;
		const string __legalPubid = " \r\n0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-'()+,./:=?;!*#@$_%";
		NameParser _nameParser;
		string _name, _pub, _sysid;
		char _quoteChar;

		public DocTypeParser( ParserState state ) {
			if( state == null )
				throw new ArgumentNullException( "state" );

			_state = state;
		}

		enum Mode {
			Start,
			AfterStart,
			Name,
			AfterName,
			Public,
			StartPubidLiteral,
			PubidLiteral,
			AfterPubidLiteral,
			StartSystemLiteral,
			SystemLiteral,
			System,
			IntSubset,
			End
		}

		public override ParseAction ParseChar( char value ) {
			for( ;; ) {
				switch( _mode ) {
					case Mode.Start:
						// Validate all the characters in the opening queue
						if( _openQueue.Count == 0 ) {
							if( !XmlHelp.IsWhitespace( value ) ) {
								_state.ThrowUnexpectedCharacter( ' ', value );
								throw new UnexpectedException();
							}
							
							_mode = Mode.AfterStart;
							return ParseAction.Continue;
						}
						
						if( value != _openQueue.Peek() ) {
							_state.ThrowUnexpectedCharacter( _openQueue.Peek(), value );
							throw new UnexpectedException();
						}
						
						_openQueue.Dequeue();
						return ParseAction.Continue;
						
					case Mode.AfterStart:
						// Eat whitespace
						if( XmlHelp.IsWhitespace( value ) ) {
							return ParseAction.Continue;
						}
						
						_mode = Mode.Name;
						_state.PushParser( _nameParser = new NameParser( _state ), value );
						return ParseAction.Continue;
						
					case Mode.Name:
						_name = _nameParser.Result;
						_nameParser = null;
						
						if( !XmlHelp.IsWhitespace( value ) && value != '>' ) {
							_state.ThrowUnexpectedCharacter( '>', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.AfterName;
						return ParseAction.Continue;
						
					case Mode.AfterName:
						// We can expect whitespace, an external ID, the end of the declaration, or an intSubset, which begins with '['
						if( XmlHelp.IsWhitespace( value ) ) {
							return ParseAction.Continue;
						}
						
						if( value == '[' ) {
							_mode = Mode.IntSubset;
							return ParseAction.Continue;
						}
						
						if( value == '>' ) {
							_state.Listener.PushDoctype( _name, _pub, _sysid );
							_mode = Mode.End;
							return ParseAction.LastCharacter;
						}
						
						if( value == 'P' && _pub == null && _sysid == null ) {
							// Beginning of a PUBLIC external ID
							_openQueue = new Queue<char>( "PUBLIC".ToCharArray() );
							_mode = Mode.Public;
							continue;
						}
						
						if( value == 'S' && _pub == null && _sysid == null ) {
							// Beginning of a SYSTEM external ID
							_openQueue = new Queue<char>( "SYSTEM".ToCharArray() );
							_mode = Mode.System;
							continue;
						}

						_state.ThrowUnexpectedCharacter( value, "DOCTYPE declaration" );
						throw new UnexpectedException();
						
					case Mode.Public:
						// Validate all the characters in the opening queue
						if( _openQueue.Count == 0 ) {
							if( !XmlHelp.IsWhitespace( value ) ) {
								_state.ThrowUnexpectedCharacter( ' ', value );
								throw new UnexpectedException();
							}

							_mode = Mode.StartPubidLiteral;
							continue;
						}

						if( value != _openQueue.Peek() ) {
							_state.ThrowUnexpectedCharacter( _openQueue.Peek(), value );
							throw new UnexpectedException();
						}

						_openQueue.Dequeue();
						return ParseAction.Continue;
						
					case Mode.StartPubidLiteral:
						if( XmlHelp.IsWhitespace( value ) )
							return ParseAction.Continue;
						
						if( value != '\'' && value != '"' ) {
							_state.ThrowExpectedQuote( value );
							throw new UnexpectedException();
						}
						
						_quoteChar = value;
						_charDataQueue = new Queue<char>();
						_mode = Mode.PubidLiteral;
						return ParseAction.Continue;
						
					case Mode.PubidLiteral:
						if( value == _quoteChar ) {
							// Time for this to end
							_pub = new string( _charDataQueue.ToArray() );
							_charDataQueue = null;
							_mode = Mode.AfterPubidLiteral;
							return ParseAction.Continue;
						}
						
						if( __legalPubid.IndexOf( value ) == -1 ) {
							_state.ThrowUnexpectedCharacter( value, "public identifier" );
							throw new UnexpectedException();
						}
						
						_charDataQueue.Enqueue( value );
						return ParseAction.Continue;
						
					case Mode.AfterPubidLiteral:
						// We're just waiting on some whitespace
						if( !XmlHelp.IsWhitespace( value ) ) {
							_state.ThrowExpectedWhitespace( value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.StartSystemLiteral;
						return ParseAction.Continue;
						
					case Mode.StartSystemLiteral:
						if( XmlHelp.IsWhitespace( value ) )
							return ParseAction.Continue;

						if( value != '\'' && value != '"' ) {
							_state.ThrowExpectedQuote( value );
							throw new UnexpectedException();
						}

						_quoteChar = value;
						_charDataQueue = new Queue<char>();
						_mode = Mode.SystemLiteral;
						return ParseAction.Continue;
						
					case Mode.SystemLiteral:
						if( value == _quoteChar ) {
							// Time for this to end
							_sysid = new string( _charDataQueue.ToArray() );
							_charDataQueue = null;
							_mode = Mode.AfterName;
							return ParseAction.Continue;
						}
						
						_state.EnsureValidCharacter( value );
						_charDataQueue.Enqueue( value );
						return ParseAction.Continue;
						
					case Mode.IntSubset:
						throw new NotImplementedException();
						
					case Mode.End:
						return ParseAction.End;
				}
			}
		}
	}
}