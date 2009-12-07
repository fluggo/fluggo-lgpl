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
	sealed class PrologParser : Parser {
		ParserState _state;
		Mode _mode = Mode.Start;
		bool _xmlDeclMade, _docTypeDeclMade;
		
		public PrologParser( ParserState state ) {
			if( state == null )
				throw new ArgumentNullException( "state" );

			_state = state;
		}
		
		enum Mode {
			Start,
			LeftAngleBracket,
			XmlDeclOrPI,
			XmlDecl1,
			XmlDecl2,
			DocTypeOrComment,
			End
		}

		public override ParseAction ParseChar( char value ) {
			for( ;; ) {
				switch( _mode ) {
					case Mode.Start:
						// We expect here an XML declaration, a comment, a PI, whitespace, a doctypedecl, or an element (the document)
						if( XmlHelp.IsWhitespace( value ) ) {
							// Whitespace, skip
							return ParseAction.Continue;
						}
						
						// The only valid character now is '<'
						if( value != '<' ) {
							_state.ThrowUnexpectedCharacter( '<', value );
							throw new UnexpectedException();
						}
						
						_mode = Mode.LeftAngleBracket;
						return ParseAction.Continue;
						
					case Mode.LeftAngleBracket:
						// We've seen a '<'
						// We now expect an XML declaration, a comment, a PI, a doctypedecl, or an element
						if( value == '?' ) {
							// PI or xmldecl
							_mode = Mode.XmlDeclOrPI;
							return ParseAction.Continue;
						}
						
						if( value == '!' ) {
							// doctypedecl or comment
							_mode = Mode.DocTypeOrComment;
							return ParseAction.Continue;
						}
						
						// Only thing left now is an element, which means we should quit
						_mode = Mode.End;
						_state.PushParser( new ElementParser( _state ), '<', value );
						return ParseAction.Continue;
						
					case Mode.XmlDeclOrPI:
						// We've seen "<?", so if the next three characters are "xml", it's an xml decl
						if( value == 'x' && !_xmlDeclMade && !_docTypeDeclMade ) {
							_mode = Mode.XmlDecl1;
							return ParseAction.Continue;
						}

						// Must be a PI
						_state.PushParser( new ProcessingInstructionParser( _state ), '<', '?', value );
						_mode = Mode.Start;
						return ParseAction.Continue;
						
					case Mode.XmlDecl1:
						// We've seen "<?x", so if the next two characters are "ml", it's an xml decl
						if( value == 'm' ) {
							_mode = Mode.XmlDecl2;
							return ParseAction.Continue;
						}

						// Must be a PI
						_state.PushParser( new ProcessingInstructionParser( _state ), '<', '?', 'x', value );
						_mode = Mode.Start;
						return ParseAction.Continue;

					case Mode.XmlDecl2:
						// We've seen "<?xm", so if the next character is "l", it's an xml decl
						if( value == 'l' ) {
							_state.PushParser( new XmlDeclarationParser( _state ), '<', '?', 'x', 'm', 'l' );
							_xmlDeclMade = true;
							_mode = Mode.Start;
							return ParseAction.Continue;
						}

						// Must be a PI
						_state.PushParser( new ProcessingInstructionParser( _state ), '<', '?', 'x', 'm', value );
						_mode = Mode.Start;
						return ParseAction.Continue;

					case Mode.DocTypeOrComment:
						// "-" is not a valid name-start, so we'll test for that:
						if( value == '-' ) {
							// Must be a comment
							_state.PushParser( new CommentParser( _state ), '<', '!', '-' );
							_mode = Mode.Start;
							return ParseAction.Continue;
						}
						
						// Otherwise, it must be a doctype
						if( _docTypeDeclMade ) {
							_state.ThrowSecondDoctype();
							throw new UnexpectedException();
						}
						
						_state.PushParser( new DocTypeParser( _state ), '<', '!', value );
						_mode = Mode.Start;
						_docTypeDeclMade = true;
						return ParseAction.Continue;
						
					case Mode.End:
						return ParseAction.End;

					default:
						throw new UnexpectedException();
				}
			}
		}
	}
}