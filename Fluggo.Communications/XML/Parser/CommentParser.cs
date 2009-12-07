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
	sealed class CommentParser : Parser {
		ParserState _state;
		Mode _mode = Mode.Start;
		Queue<char> _charDataQueue;
		
		public CommentParser( ParserState state ) {
			if( state == null )
				throw new ArgumentNullException( "state" );

			_state = state;
		}
		
		enum Mode {
			Start,
			LeftAngleBracket,
			Exclamation,
			OpeningDash,
			ClosingDash1,
			ClosingDash2,
			Text,
			End,
		}

		public override ParseAction ParseChar( char value ) {
			for( ;; ) {
				switch( _mode ) {
					case Mode.Start:
						if( value != '<' ) {
							_state.ThrowUnexpectedCharacter( '<', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.LeftAngleBracket;
						return ParseAction.Continue;
						
					case Mode.LeftAngleBracket:
						if( value != '!' ) {
							_state.ThrowUnexpectedCharacter( '!', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.Exclamation;
						return ParseAction.Continue;
						
					case Mode.Exclamation:
						if( value != '-' ) {
							_state.ThrowUnexpectedCharacter( '-', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.OpeningDash;
						return ParseAction.Continue;
						
					case Mode.OpeningDash:
						if( value != '-' ) {
							_state.ThrowUnexpectedCharacter( '-', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.Text;
						_charDataQueue = new Queue<char>();
						return ParseAction.Continue;
						
					case Mode.Text:
						if( value == '-' ) {
							_mode = Mode.ClosingDash1;
							return ParseAction.Continue;
						}
						
						_state.EnsureValidCharacter( value );
						_charDataQueue.Enqueue( value );
						return ParseAction.Continue;
						
					case Mode.ClosingDash1:
						// We've seen a dash in the stream; this means nothing if not followed by another dash
						if( value == '-' ) {
							// Go ahead and clear the text, as this *should* be it
							_state.Listener.PushComment( new string( _charDataQueue.ToArray() ) );
							_charDataQueue = null;
							
							_mode = Mode.ClosingDash2;
							return ParseAction.Continue;
						}
						
						// Re-parse as text
						_charDataQueue.Enqueue( '-' );
						_mode = Mode.Text;
						continue;
						
					case Mode.ClosingDash2:
						// We've seen two dashes; this must be the end, as this is illegal in the middle of a comment
						if( value != '>' ) {
							_state.ThrowUnexpectedCharacter( '>', value );
							throw new UnexpectedException();
						}
						
						return ParseAction.LastCharacter;
				}
			}
		}
	}
}