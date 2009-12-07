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
using System.Globalization;
using System.Collections.Generic;
namespace Fluggo.Xml {
	/// <summary>
	/// Represents the action to take after parsing a single character.
	/// </summary>
	enum ParseAction {
		/// <summary>
		/// Indicates that the parser needs more text to finish the current production.
		/// </summary>
		Continue,

		/// <summary>
		/// Indicates that the production is finished. The most recently submitted character is
		/// not part of the production and should be re-parsed.
		/// </summary>
		End,

		/// <summary>
		/// Indicates that the production is finished. The most recently submitted character is
		/// the last character in the production and should not be re-parsed.
		/// </summary>
		LastCharacter,
	}

	abstract class Parser {
		public abstract ParseAction ParseChar( char value );
	}

	interface IXmlParseListener {
		void PushElementName( string name );
		void PushAttributeName( string name, char quoteChar );
		void PushAttributeText( string text );
		void PushAttributeEntityRef( string name );
		void PushAttributeCharRef( int codePoint );
		void PushText( string text );
		void PushCharRef( int codePoint );
		void PushEntityRef( string name );
		void PushCloseEmptyElement();
		void PushEndElement();
		void PushComment( string text );
		void PushDoctype( string name, string publicID, string systemID );
		void PushXmlDeclaration( string encoding, bool? standalone );
		void PushParseError( int line, int column, string error );
	}

	class ParserState {
		IXmlParseListener _listener;
		Stack<Parser> _parseStack = new Stack<Parser>();
		int _row = 1, _column = 0;
		char _lastChar = '\0';
		
		public ParserState( IXmlParseListener listener ) {
			if( listener == null )
				throw new ArgumentNullException( "listener" );

			_listener = listener;
		}
		
		public void PushParser( Parser parser ) {
			_parseStack.Push( parser );
		}
		
		public void PushParser( Parser parser, params char[] values ) {
			_parseStack.Push( parser );

			if( values == null )
				throw new ArgumentNullException( "values" );

			for( int i = 0; i < values.Length; i++ ) {
				Parse( values[i] );
			}
		}
		
		public IXmlParseListener Listener
			{ get { return _listener; } }
			
		/// <summary>
		/// Parses a single character.
		/// </summary>
		/// <param name="value">Character to parse.</param>
		/// <returns>True if more data is expected, or false if the parser has read a complete stream.</returns>
		/// <remarks>This is the top of the parser chain. Everything in the parser gets triggered from here.</remarks>
		public bool Parse( char value ) {
			// Check only once for a newline
			unchecked {
				if( value == '\n' || _lastChar == '\r' ) {
					_row++;
					_column = 0;
					
					// Fix any wrapping that might occur
					if( _row < 0 )
						_row = 0;
				}
				else {
					_column++;
					
					// Fix any wrapping that might occur
					if( _column < 0 )
						_column = 0;
				}
			}
			
			_lastChar = value;
		
			for( ;; ) {
				if( _parseStack.Count == 0 )
					return false;
					
				ParseAction action = _parseStack.Peek().ParseChar( value );

				switch( action ) {
					case ParseAction.Continue:
						return true;

					case ParseAction.End:
						_parseStack.Pop();
						continue;

					case ParseAction.LastCharacter:
						_parseStack.Pop();
						return _parseStack.Count != 0;

					default:
						throw new Exception( "Unexpected return value from parser." );
				}
			}
		}
		
		public bool Parse( string value ) {
			char[] values = value.ToCharArray();
			
			for( int i = 0; i < values.Length; i++ ) {
				if( !Parse( values[i] ) )
					return false;
			}
			
			return true;
		}
			
		private void ThrowParseException( string text ) {
			_listener.PushParseError( _row, _column, text );
			throw new Exception( string.Format( "Line {0}, column {1}: {2}", _row, _column, text ) );
		}
		
		public void ThrowSecondDoctype() {
			ThrowParseException( "Encountered what looks like a second doctype." );
		}
		
		public void ThrowExpectedQuote( char value ) {
			ThrowParseException( "Expected a quote character." );
		}
		
		public void ThrowEmptyEntity() {
			ThrowParseException( "Encountered an empty entity." );
		}
		
		public void ThrowTagNameMismatch( string startName, string endName ) {
			ThrowParseException( string.Format( "The name of the end tag ({1}) did not match the name of the start tag ({0}).", startName, endName ) );
		}
		
		public void ThrowCDataCloseInText() {
			ThrowParseException( "Found \"]]>\" sequence in text." );
		}
		
		public void ThrowExpectedWhitespace( char value ) {
			ThrowParseException( string.Format( "Found the character \'{0}\' where whitespace was expected.", value ) );
		}
		
		public void ThrowUnexpectedCharacter( char value, string context ) {
			ThrowParseException( string.Format( "Unexpected character \'{0}\' in {1}.", value, context ) );
		}
		
		public void ThrowUnexpectedCharacter( char expectedValue, char actualValue ) {
			ThrowParseException( string.Format( "Unexpected character in XML stream. Expected '{0}' but found '{1}'.", expectedValue, actualValue ) );
		}
		
		/// <summary>
		/// Throws an exception if the given character is not a valid XML character.
		/// </summary>
		/// <param name="value">Character to validate</param>
		public void EnsureValidCharacter( char value ) {
			// Ensure that the given character is a valid XML character
			if( value >= 0x20 ) {
				if( value >= 0xE000 ) {
					if( value != 0xFFFE && value != 0xFFFF )
						return;
				}
				else if( value <= 0xD7FF )
					return;
			}
			else if( value == 0x9 || value == 0xA || value == 0xD )
				return;
				
			ThrowParseException( "Invalid character found in XML stream." );
		}
	}
	
	static class XmlHelp {
		public static bool IsWhitespace( char value ) {
			return (value == 0x20) || (value == 0x9) || (value == 0xD) || (value == 0xA);
		}

		public static bool IsNameStart( char value ) {
			return IsLetter( value ) || (value == '_') || (value == ':');
		}

		public static bool IsNameChar( char value ) {
			switch( char.GetUnicodeCategory( value ) ) {
				case UnicodeCategory.UppercaseLetter:		// Lu, name-start
				case UnicodeCategory.LowercaseLetter:		// Ll, name-start
				case UnicodeCategory.TitlecaseLetter:		// Lt, name-start
				case UnicodeCategory.LetterNumber:			// Nl, name-start
				case UnicodeCategory.OtherLetter:			// Lo, name-start

				case UnicodeCategory.ModifierLetter:		// Lm, name-char
				case UnicodeCategory.DecimalDigitNumber:	// Nd, name-char
				case UnicodeCategory.SpacingCombiningMark:	// Mc, name-char
				case UnicodeCategory.EnclosingMark:			// Me, name-char
				case UnicodeCategory.NonSpacingMark:		// Mn, name-char
					return true;
			}
			
			if( value == '.' || value == '-' || value == '_' || value == ':' )
				return true;
			
			return false;
		}

		public static bool IsLetter( char value ) {
			switch( char.GetUnicodeCategory( value ) ) {
				case UnicodeCategory.UppercaseLetter:		// Lu, name-start
				case UnicodeCategory.LowercaseLetter:		// Ll, name-start
				case UnicodeCategory.TitlecaseLetter:		// Lt, name-start
				case UnicodeCategory.LetterNumber:			// Nl, name-start
				case UnicodeCategory.OtherLetter:			// Lo, name-start
					return true;
					
				default:
					return false;
			}
		}

		public static int ParseHexChar( char value ) {
			switch( value ) {
				case '0': return 0;
				case '1': return 1;
				case '2': return 2;
				case '3': return 3;
				case '4': return 4;
				case '5': return 5;
				case '6': return 6;
				case '7': return 7;
				case '8': return 8;
				case '9': return 9;
				case 'A':
				case 'a': return 10;
				case 'B':
				case 'b': return 11;
				case 'C':
				case 'c': return 12;
				case 'D':
				case 'd': return 13;
				case 'E':
				case 'e': return 14;
				case 'F':
				case 'f': return 15;
			}

			throw new Exception( "Expected a hexadecimal character." );
		}

		public static int ParseDecimalChar( char value ) {
			switch( value ) {
				case '0': return 0;
				case '1': return 1;
				case '2': return 2;
				case '3': return 3;
				case '4': return 4;
				case '5': return 5;
				case '6': return 6;
				case '7': return 7;
				case '8': return 8;
				case '9': return 9;
			}

			throw new Exception( "Expected a decimal character." );
		}
	}
}