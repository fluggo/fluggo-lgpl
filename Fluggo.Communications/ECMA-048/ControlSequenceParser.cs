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
using System.IO;
using System.Text;

namespace Fluggo.Communications.Terminals {
	/// <summary>
	/// Interprets control codes in a stream of bytes.
	/// </summary>
	public class ControlSequenceParseStream : Stream
	{
		bool _7bit = true;
		State _currentState = State.Text;
		int _param1, _param2;
		byte _intermediate;
		ITerminalControlWriter _control;
		Queue<char> _escapeSequenceLog;
		bool _debug = true;
		byte[] _dumbByteBuffer = new byte[1];
		char[] _dumbCharBuffer = new char[1];
//		Decoder _decoder;

		/// <summary>
		/// Creates a new instance of the <see cref='ControlSequenceParseStream'/> class.
		/// </summary>
		/// <param name="stream"><see cref="ITerminalControlWriter"/> to which the interpreted command sequence should be sent.</param>
		/// <param name="is7bit">True if the source data should be treated as having seven significant bits, or false to treat it as 8-bit data.</param>
		/// <param name="debug">True if unrecognized escape sequences should be sent to the terminal as text, or false if they should be ignored.</param>
		/// <exception cref='ArgumentNullException'><paramref name='stream'/> is <see langword='null'/>.</exception>
		public ControlSequenceParseStream( ITerminalControlWriter stream, bool is7bit, bool debug ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );

			_control = stream;
			_7bit = is7bit;
			_debug = debug;
//			_decoder = _7bit ? Encoding.UTF8.GetDecoder() : Encoding.ASCII.GetDecoder();
		}

		enum State
		{
			Text,
			Escape,
			EscapeStart,
			Parameter1,
			Parameter2,
			Intermediate,
		}

		bool IsIntermediateCode( byte data ) {
			return (data & 0xF0) == 0x20;
		}

		bool IsC0Code( byte data ) {
			return (data & 0xE0) == 0x00;
		}

		bool IsC1Code( byte data ) {
			if( _7bit )
				return (data & 0xE0) == 0x40;
			else
				return (data & 0xE0) == 0x80;
		}

		bool IsIndependentControlFunction( byte data ) {
			return (data & 0xE0) == 0x60;
		}

		bool IsFinalCode( byte data ) {
			return (data & 0xC0) == 0x40;
		}

		bool IsParameterCode( byte data ) {
			return ((data & 0xF8) == 0x30) || ((data & 0xFC) == 0x38);
		}

		bool IsParameterStringSeparator( byte data ) {
			return data == 0x3B;
		}

		bool IsParameterSeparator( byte data ) {
			return data == 0x3A;
		}

		bool IsParameterData( byte data ) {
			return IsParameterCode( data ) && (data < 0x3A);
		}

		public override void Write( byte[] data, int offset, int length ) {
			new ArraySegment<byte>( data, offset, length );

			for( int i = offset; i < (offset + length); i++ )
				WriteByte( data[i] );
		}

		void ProcessFinalCode( byte data ) {
			if( _intermediate == 0x00 ) {
				// Process without an intermediate
				_control.WriteControlSequence( _param1, _param2, (ControlFinalCode) data );
			}
			else if( _intermediate == 0x20 ) {
				// Single 0x20 intermediate
				_control.WriteControlSequence( _param1, _param2, (ControlExtendedFinalCode) data );
			}
			else if( _escapeSequenceLog != null ) {
				// Send the control sequence as text for debug purposes
				while( _escapeSequenceLog.Count != 0 )
					_control.WriteText( _escapeSequenceLog.Dequeue() );
			}

			_currentState = State.Text;
			_escapeSequenceLog = null;
		}

		public override void WriteByte( byte data ) {
			if( !_7bit && (data >= 0xA0) ) {
				// Translate down so that we can understand
				data -= 0x80;
			}
			
			for( ; ; ) {
				if( _escapeSequenceLog != null ) {
					_dumbByteBuffer[0] = data;
					_escapeSequenceLog.Enqueue( Encoding.ASCII.GetChars( _dumbByteBuffer, 0, 1 )[0] );
				}

				switch( _currentState ) {
					case State.Text:
						if( !_7bit && IsC1Code( data ) ) {
							if( data == (byte) C1.ControlSequenceIntroducer ) {
								_currentState = State.Escape;
								return;
							}
							else {
								_control.WriteControlCode( (C1) data );
								return;
							}
						}
						if( IsC0Code( data ) ) {
							if( data == (byte) C0.Escape ) {
								_currentState = State.EscapeStart;
								return;
							}
							else {
								_control.WriteControlCode( (C0) data );
								return;
							}
						}
						else {
							_dumbByteBuffer[0] = data;
							//_decoder.GetChars( _dumbByteBuffer, 0, 1, _dumbCharBuffer, 0, false );
							
							_control.WriteText( Encoding.ASCII.GetChars( _dumbByteBuffer, 0, 1 ), 0, 1 );
							return;
						}

					case State.Escape:
						_intermediate = 0x00;
						_param1 = -1;
						_param2 = -1;

						if( _debug ) {
							_escapeSequenceLog = new Queue<char>();
							_escapeSequenceLog.Enqueue( '^' );
						}

						if( IsParameterCode( data ) ) {
							_currentState = State.Parameter1;
							// Repeat processing
						}
						else if( IsIntermediateCode( data ) ) {
							_intermediate = data;
							_currentState = State.Intermediate;
							return;
						}
						else if( IsFinalCode( data ) ) {
							_currentState = State.Text;
							ProcessFinalCode( data );
							return;
						}

						break;

					case State.EscapeStart:
						if( data == 0x5B ) {
							_currentState = State.Escape;
							return;
						}
						if( _7bit && IsC1Code( data ) ) {
							// Translate up to 8-bit depth
							_currentState = State.Text;
							_control.WriteControlCode( (C1) ((data & 0x1F) | 0x80) );
							return;
						}
						if( IsIndependentControlFunction( data ) ) {
							_currentState = State.Text;
							_control.WriteIndependentControlFunction( (IndependentControlFunction) data );
							return;
						}
						else {
							_currentState = State.Text;
							_control.WriteText( '^' );
							// Repeat processing
						}

						break;

					case State.Parameter1:
						if( !IsParameterCode( data ) ) {
							if( IsIntermediateCode( data ) ) {
								_intermediate = data;
								_currentState = State.Intermediate;
								return;
							}
							else if( IsFinalCode( data ) ) {
								_currentState = State.Text;
								ProcessFinalCode( data );
								return;
							}
						}
						else if( IsParameterStringSeparator( data ) ) {
							_currentState = State.Parameter2;
							return;
						}
						else if( !IsParameterData( data ) ) {
							// Don't exactly know what to do with this yet...
							while( _escapeSequenceLog.Count != 0 )
								_control.WriteText( _escapeSequenceLog.Dequeue() );

							_escapeSequenceLog = null;
							_currentState = State.Text;
							return;
						}

						if( _param1 == -1 )
							_param1 = 0;

						_param1 *= 10;
						_param1 |= (0x0F & data);
						return;

					case State.Parameter2:
						if( !IsParameterCode( data ) ) {
							if( IsIntermediateCode( data ) ) {
								_intermediate = data;
								_currentState = State.Intermediate;
								return;
							}
							else if( IsFinalCode( data ) ) {
								_currentState = State.Text;
								ProcessFinalCode( data );
								return;
							}
						}
						else if( IsParameterStringSeparator( data ) || IsParameterSeparator( data ) ) {
							// Don't exactly know what to do with this yet...
							while( _escapeSequenceLog.Count != 0 )
								_control.WriteText( _escapeSequenceLog.Dequeue() );

							_escapeSequenceLog = null;
							_currentState = State.Text;
							return;
						}

						if( _param2 == -1 )
							_param2 = 0;

						_param2 *= 10;
						_param2 |= (0x0F & data);
						return;

					case State.Intermediate:
						// At this point, we have support for only one intermediate, so the next one had better be a final
						if( !IsFinalCode( data ) ) {
							// Don't exactly know what to do with this yet...
							while( _escapeSequenceLog.Count != 0 )
								_control.WriteText( _escapeSequenceLog.Dequeue() );

							_escapeSequenceLog = null;
							_currentState = State.Text;
							return;
						}

						ProcessFinalCode( data );
						return;

					default:
						throw new UnexpectedException();
				}
			}
		}

		#region Stream support
		public override bool CanRead {
			get { return false; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override void Flush() {
		}

		public override long Length {
			get { throw new NotSupportedException(); }
		}

		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			throw new NotSupportedException();
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			throw new NotSupportedException();
		}

		public override void SetLength( long value ) {
			throw new NotSupportedException();
		}
		#endregion
	}
}