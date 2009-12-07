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

namespace Fluggo.Xml {
	sealed class ElementParser : Parser {
		ParserState _state;
		Mode _mode = Mode.Start;
		NameParser _nameParser, _endNameParser;
		bool _sawWhitespace;
		
		public ElementParser( ParserState state ) {
			if( state == null )
				throw new ArgumentNullException( "state" );

			_state = state;
		}
		
		enum Mode {
			Start,
			Name,
			MidElem,
			Attribute,
			CloseTag,
			CloseTagWhitespace,
			StartEndTag,
			EndTag,
			End
		}

		public override ParseAction ParseChar( char value ) {
			for( ;; ) {
				switch( _mode ) {
					case Mode.Start:
						// Read '<'
						if( value != '<' ) {
							_state.ThrowUnexpectedCharacter( '<', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.Name;
						_nameParser = new NameParser( _state );
						return ParseAction.Continue;
						
					case Mode.Name:
						if( _nameParser.ParseChar( value ) == ParseAction.End ) {
							_state.Listener.PushElementName( _nameParser.Result );
							_mode = Mode.MidElem;
							continue;
						}
						
						return ParseAction.Continue;
						
					case Mode.MidElem:
						// We can expect whitespace, an attribute, or the end of the tag
						if( value == '/' ) {
							// Empty tag!
							_state.Listener.PushCloseEmptyElement();
							_mode = Mode.CloseTag;
							return ParseAction.Continue;
						}
						
						if( value == '>' ) {
							// Non-empty tag!
							_state.PushParser( new ElementContentParser( _state ) );
							
							// Set up name parser for when we return
							_endNameParser = new NameParser( _state );
							_mode = Mode.StartEndTag;
							
							return ParseAction.Continue;
						}
						
						if( XmlHelp.IsWhitespace( value ) ) {
							// Whitespace, we ignore
							_sawWhitespace = true;
							return ParseAction.Continue;
						}
						
						// We expect an attribute now, but first, did we see whitespace?
						if( !_sawWhitespace ) {
							_state.ThrowExpectedWhitespace( value );
							throw new UnexpectedException();
						}
						
						_sawWhitespace = false;
						_state.PushParser( new AttributeParser( _state ), value );
						return ParseAction.Continue;
						
					case Mode.CloseTag:
						if( value != '>' ) {
							_state.ThrowUnexpectedCharacter( '>', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.End;
						return ParseAction.LastCharacter;
						
					case Mode.StartEndTag:
						// In this special case, we've been called to process the "/" character
						// in the end tag
						if( value != '/' ) {
							_state.ThrowUnexpectedCharacter( '/', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.EndTag;
						return ParseAction.Continue;
						
					case Mode.EndTag:
						if( _endNameParser.ParseChar( value ) == ParseAction.End ) {
							if( _nameParser.Result != _endNameParser.Result ) {
								_state.ThrowTagNameMismatch( _nameParser.Result, _endNameParser.Result );
								throw new UnexpectedException();
							}
							
							_state.Listener.PushEndElement();

							_nameParser = null;
							_endNameParser = null;
							
							_mode = Mode.CloseTagWhitespace;
							continue;
						}
						
						return ParseAction.Continue;
						
					case Mode.CloseTagWhitespace:
						// Just eat whitespace between here and the end of the tag
						if( XmlHelp.IsWhitespace( value ) ) {
							return ParseAction.Continue;
						}
						
						_mode = Mode.CloseTag;
						continue;
						
					case Mode.End:
						return ParseAction.End;

					default:
						throw new UnexpectedException();
				}
			}
		}
	}
}