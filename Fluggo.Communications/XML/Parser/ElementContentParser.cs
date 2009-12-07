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
	sealed class ElementContentParser : Parser {
		ParserState _state;
		Mode _mode = Mode.Start;
		Queue<char> _charDataQueue;
		
		public ElementContentParser( ParserState state ) {
			if( state == null )
				throw new ArgumentNullException( "state" );

			_state = state;
		}

		enum Mode {
			Start,
			Text,
			LeftAngleBracket,
			Exclamation,
			PI,
			TextCDClose1,
			TextCDClose2,
			TextCarriageReturn,
		}

		public override ParseAction ParseChar( char value ) {
			for( ;; ) {
				switch( _mode ) {
					case Mode.Start:
						// Inspect the character and find a mode for it
						if( value == '<' ) {
							_mode = Mode.LeftAngleBracket;
							return ParseAction.Continue;
						}
						
						if( value == '&' ) {
							_state.PushParser( new ReferenceParser( _state, false ), '&' );
							return ParseAction.Continue;
						}
						
						// Re-parse as text
						_charDataQueue = new Queue<char>();
						_mode = Mode.Text;
						continue;
						
					case Mode.Text:
						// Parse and add text as long as it's in the valid character categories
						if( value == ']' ) {
							// This is the first character of a CData close, which is an invalid sequence
							// Track it and throw an exception if it's a CData close
							_mode = Mode.TextCDClose1;
							return ParseAction.Continue;
						}
						
						if( value == 0xD ) {
							// This is a carriage return, which is not allowed in the output
							_mode = Mode.TextCarriageReturn;
							return ParseAction.Continue;
						}
						
						if( value == '&' || value == '<' ) {
							// The text section ends
							string text = new string( _charDataQueue.ToArray() );
							_state.Listener.PushText( text );
							
							_charDataQueue = null;
							_mode = Mode.Start;
							continue;
						}
						
						_state.EnsureValidCharacter( value );
						_charDataQueue.Enqueue( value );
						return ParseAction.Continue;
						
					case Mode.TextCarriageReturn:
						// The last thing we saw was a carriage return, which was not pushed onto the queue.
						// We'll determine what to do based on what we see after it.
						if( value == 0xA ) {
							// A CR-LF pair. Push only the LF.
							_charDataQueue.Enqueue( value );
							_mode = Mode.Text;
							return ParseAction.Continue;
						}
						
						// We only saw the lone CR, so push a line feed and get back to text
						_charDataQueue.Enqueue( (char) 0xA );
						_mode = Mode.Text;
						continue;
						
					case Mode.TextCDClose1:
						// We've seen one ']' already; if the next is not a ']', we're okay
						if( value == ']' ) {
							// This is the second character of a CData close, which is an invalid sequence in the middle of text
							// Track it and throw an exception if it's a CData close
							_mode = Mode.TextCDClose2;
							return ParseAction.Continue;
						}
						
						// We're okay; push the last character and move back to text
						_charDataQueue.Enqueue( ']' );
						_mode = Mode.Text;
						continue;
						
					case Mode.TextCDClose2:
						// We've seen "]]" already; if the next is a '>', throw!
						if( value == '>' ) {
							// This is the final character of a CData close, which is an invalid sequence in the middle of text
							_state.ThrowCDataCloseInText();
							throw new UnexpectedException();
						}
						
						// We're okay; push the last two characters and move back to text
						_charDataQueue.Enqueue( ']' );
						_charDataQueue.Enqueue( ']' );
						_mode = Mode.Text;
						continue;
						
					case Mode.LeftAngleBracket:
						// This could be a start element, end element, CData, PI, or comment
						if( value == '!' ) {
							// Narrowed to CData or comment
							_mode = Mode.Exclamation;
							return ParseAction.Continue;
						}
						
						if( value == '?' ) {
							// PI
							throw new NotImplementedException();
						}
						
						if( value == '/' ) {
							// Close element! In this special case, we just hand back to the caller
							// and hope he understands
							return ParseAction.End;
						}
						
						// The only thing left is an element
						_state.PushParser( new ElementParser( _state ), '<', value );
						_mode = Mode.Start;
						return ParseAction.Continue;
						
					case Mode.Exclamation:
						// Three things start "<!": a CDATA, a comment, and a PI
						if( value == '[' ) {
							// CDATA!
							throw new NotImplementedException();
						}
						
						if( value == '-' ) {
							// Comment!
							_state.PushParser( new CommentParser( _state ), '<', '!', '-' );
							_mode = Mode.Start;
							return ParseAction.Continue;
						}
						
						// PI!
						throw new NotImplementedException();
				}
			}
		}
	}
}