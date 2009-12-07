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

using System.Collections.Generic;
using System;

namespace Fluggo.Xml {
	/// <summary>
	/// Parses an XML attribute, handling the "Attribute," "Eq," and "AttValue" productions.
	/// </summary>
	sealed class AttributeParser : Parser {
		ParserState _state;
		Queue<char> _charQueue = new Queue<char>();
		Mode _mode = Mode.Name;
		NameParser _nameParser;
		ReferenceParser _refParser;
		char _attQuote;
		bool _valueEmpty = true;
		
		public AttributeParser( ParserState state ) {
			if( state == null )
				throw new ArgumentNullException( "state" );

			_state = state;
		}

		enum Mode {
			Name,
			PreEq,
			PostEq,
			StartAttValue,
			AttText,
			AttEntity,
			TextCarriageReturn
		}

		public override ParseAction ParseChar( char value ) {
			for( ;; ) {
				ParseAction action;
			
				switch( _mode ) {
					case Mode.Name:
						if( _nameParser == null )
							_nameParser = new NameParser( _state );

						if( _nameParser.ParseChar( value ) == ParseAction.End ) {
							_mode = Mode.PreEq;
							continue;
						}

						return ParseAction.Continue;

					case Mode.PreEq:
						// The name has been read, but the equal sign hasn't been seen
						if( value == '=' ) {
							_mode = Mode.PostEq;
							return ParseAction.Continue;
						}

						if( XmlHelp.IsWhitespace( value ) )
							return ParseAction.Continue;

						_state.ThrowUnexpectedCharacter( '=', value );
						throw new UnexpectedException();

					case Mode.PostEq:
						if( XmlHelp.IsWhitespace( value ) )
							return ParseAction.Continue;
							
						// This should be the attribute quote
						if( value == '\'' || value == '"' ) {
							_state.Listener.PushAttributeName( _nameParser.Result, value );
							_nameParser = null;
							_attQuote = value;
							_mode = Mode.StartAttValue;
							return ParseAction.Continue;
						}

						_state.ThrowExpectedQuote( value );
						throw new UnexpectedException();
						
					case Mode.StartAttValue:
						// This could already be the end
						if( value == _attQuote ) {
/*							if( _valueEmpty )
								_state.Listener.PushAttributeText( string.Empty );*/
								
							return ParseAction.LastCharacter;
						}
						
						// Identify text
						if( value == '&' ) {
							_refParser = new ReferenceParser( _state, true );
							_mode = Mode.AttEntity;
							continue;
						}
						
						_charQueue = new Queue<char>();
						_mode = Mode.AttText;
						continue;
						
					case Mode.AttEntity:
						// Run entity parser
						action = _refParser.ParseChar( value );
						
						if( action != ParseAction.Continue ) {
							_valueEmpty = false;
							_mode = Mode.StartAttValue;
							_refParser = null;
							
							// Do we need to re-parse this character?
							if( action == ParseAction.End )
								continue;
						}
						
						return ParseAction.Continue;
						
					case Mode.AttText:
						if( value == '<' ) {
							_state.ThrowUnexpectedCharacter( value, "attribute text" );
							throw new UnexpectedException();
						}
						
						if( value == '&' || value == _attQuote ) {
							// Finish the production and hand it off to StartAttValue
							_state.Listener.PushAttributeText( new string( _charQueue.ToArray() ) );
							
							_mode = Mode.StartAttValue;
							_charQueue = null;
							
							continue;
						}
						
						if( value == 0xD ) {
							_mode = Mode.TextCarriageReturn;
							return ParseAction.Continue;
						}

						_state.EnsureValidCharacter( value );

						_charQueue.Enqueue( value );
						_valueEmpty = false;
						return ParseAction.Continue;

					case Mode.TextCarriageReturn:
						// The last thing we saw was a carriage return, which was not pushed onto the queue.
						// We'll determine what to do based on what we see after it.
						if( value == 0xA ) {
							// A CR-LF pair. Push only the LF.
							_charQueue.Enqueue( value );
							_mode = Mode.AttText;
							return ParseAction.Continue;
						}

						// We only saw the lone CR, so push a line feed and get back to text
						_charQueue.Enqueue( (char) 0xA );
						_mode = Mode.AttText;
						continue;
				}
			}
		}
	}
}