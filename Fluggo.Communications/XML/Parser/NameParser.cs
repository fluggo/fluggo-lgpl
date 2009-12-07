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
	/// Parses XML names, handling the "Name" production.
	/// </summary>
	sealed class NameParser : Parser {
		ParserState _state;
		Mode _mode = Mode.Start;
		Queue<char> _queue = new Queue<char>();
		string _result;
		
		public NameParser( ParserState state ) {
			if( state == null )
				throw new ArgumentNullException( "state" );
				
			_state = state;
		}

		enum Mode {
			Start,
			Middle,
			End
		}

		public override ParseAction ParseChar( char value ) {
			switch( _mode ) {
				case Mode.Start:
					if( !XmlHelp.IsNameStart( value ) )
						_state.ThrowUnexpectedCharacter( value, "name" );

					_queue.Enqueue( value );
					_mode = Mode.Middle;
					return ParseAction.Continue;

				case Mode.Middle:
					if( !XmlHelp.IsNameChar( value ) ) {
						_result = new string( _queue.ToArray() );
						_mode = Mode.End;
						return ParseAction.End;
					}

					_queue.Enqueue( value );
					return ParseAction.Continue;

				case Mode.End:
					return ParseAction.End;

				default:
					throw new UnexpectedException();
			}
		}

		public string Result {
			get {
				if( _result == null )
					throw new InvalidOperationException();

				return _result;
			}
		}
	}
}