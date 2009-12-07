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
	/// <summary>
	/// Parses entity and character references in XML, handling the "Reference" production.
	/// </summary>
	sealed class ReferenceParser : Parser {
		Mode _mode = Mode.Ampersand;
		ParserState _state;
		NameParser _nameParser;
		string _name;
		int _codePoint;
		bool _firstCodePointCharFound, _inAttribute;

		/// <summary>
		/// Creates a new instance of the <see cref='ReferenceParser'/> class.
		/// </summary>
		/// <param name="state">The current parser state.</param>
		/// <param name="inAttribute">True if the productions are attribute productions, or false if they occur in text.</param>
		public ReferenceParser( ParserState state, bool inAttribute ) {
			if( state == null )
				throw new ArgumentNullException( "state" );

			_state = state;
			_inAttribute = inAttribute;
		}

		enum Mode {
			Ampersand,
			StartName,
			StartCodePoint,
			DecimalCodePoint,
			HexCodePoint,
			Name,
			Semicolon,
			End
		}

		public override ParseAction ParseChar( char value ) {
			for( ;; ) {
				switch( _mode ) {
					case Mode.Ampersand:
						// The first character we expect in an EntityRef is an ampersand
						if( value == '&' ) {
							_mode = Mode.StartName;
							return ParseAction.Continue;
						}

						_state.ThrowUnexpectedCharacter( '&', value );
						throw new UnexpectedException();

					case Mode.StartName:
						// This might be '#' for a code point
						if( value == '#' ) {
							_mode = Mode.StartCodePoint;
							return ParseAction.Continue;
						}

						_mode = Mode.Name;
						_nameParser = new NameParser( _state );
						continue;

					case Mode.Name:
						// We've seen the ampersand, so collect the name
						if( _nameParser.ParseChar( value ) == ParseAction.End ) {
							_name = _nameParser.Result;
							_nameParser = null;
							_mode = Mode.Semicolon;
							continue;
						}

						return ParseAction.Continue;

					case Mode.StartCodePoint:
						// This might be 'x' for a hex code point
						if( value == 'x' ) {
							_mode = Mode.HexCodePoint;
							return ParseAction.Continue;
						}

						_mode = Mode.DecimalCodePoint;
						continue;

					case Mode.DecimalCodePoint:
						if( value == ';' ) {
							if( !_firstCodePointCharFound ) {
								// We were promised a code point, but the entity was empty
								_state.ThrowEmptyEntity();
								throw new UnexpectedException();
							}

							_mode = Mode.End;
							return ParseAction.LastCharacter;
						}

						_firstCodePointCharFound = true;
						_codePoint = (_codePoint * 10) + XmlHelp.ParseDecimalChar( value );
						return ParseAction.Continue;

					case Mode.HexCodePoint:
						if( value == ';' ) {
							if( !_firstCodePointCharFound ) {
								// We were promised a code point, but the entity was empty
								_state.ThrowEmptyEntity();
								throw new UnexpectedException();
							}

							_mode = Mode.End;
							return ParseAction.LastCharacter;
						}

						_firstCodePointCharFound = true;
						_codePoint = (_codePoint * 10) + XmlHelp.ParseHexChar( value );
						return ParseAction.Continue;

					case Mode.Semicolon:
						// The name has been collected, so now we just need the closing semicolon
						if( value == ';' ) {
							_mode = Mode.End;

							if( _inAttribute ) {
								if( _name == null )
									_state.Listener.PushAttributeCharRef( _codePoint );
								else
									_state.Listener.PushAttributeEntityRef( _name );
							}
							else {
								if( _name == null )
									_state.Listener.PushCharRef( _codePoint );
								else
									_state.Listener.PushEntityRef( _name );
							}
							
							return ParseAction.LastCharacter;
						}

						_state.ThrowUnexpectedCharacter( ';', value );
						throw new UnexpectedException();

					case Mode.End:
						return ParseAction.End;
				}
			}
		}
	}
}