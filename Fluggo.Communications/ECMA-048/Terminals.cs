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
	#if false
	public class TerminalWriter : TextWriter {
		TerminalControlStreamWriter _writer;
		
		public TerminalWriter( Stream stream, bool is7bit ) {
			if( stream == null )
				throw new ArgumentNullException( "stream" );
				
			_writer = new TerminalControlStreamWriter( stream, is7bit );
			
		}
		
		public override void Write( string value ) {
			_writer.WriteText( value );
		}
		
		public override void Write( char value ) {
			_writer.WriteText( value );
		}

		public override void Write( char[] buffer ) {
			if( buffer != null )
				throw new ArgumentNullException( "buffer" );
			
			_writer.WriteText( buffer, 0, buffer.Length );
		}

		public override void Write( char[] buffer, int index, int count ) {
			if( buffer != null )
				throw new ArgumentNullException( "buffer" );

			_writer.WriteText( buffer, index, count );
		}
		
		private void IsBold( DisplayColor color ) {
			switch( color ) {
				case DisplayColor.Black:
				case DisplayColor.DarkRed:
				case DisplayColor.DarkGreen:
				case DisplayColor.Brown:
				case DisplayColor.DarkBlue:
				case DisplayColor.DarkMagenta:
				case DisplayColor.DarkCyan:
					return false;
					
				case DisplayColor.LightGrey:
				case DisplayColor.DarkGrey:
				case DisplayColor.Red:
				case DisplayColor.Green:
				case DisplayColor.Yellow:
				case DisplayColor.Blue:
				case DisplayColor.Magenta:
				case DisplayColor.Cyan:
				case DisplayColor.White:
					return true;
					
				default:
					throw new ArgumentOutOfRangeException( "color" );
			}
		}
		
		public void SetTextColor( DisplayColor color ) {
			bool bold = IsBold( color );
			
			if( bold )
				
			
			int param = (int)(byte) color + (int) SetGraphicRenditionParam.Black;
		}

		public override Encoding Encoding {
			get { return Encoding.ASCII; }
		}
	}
	#endif
	
	public class TerminalControlStreamWriter : ITerminalControlWriter {
		Stream _target;
		bool _7bit;
		byte[] _outBuffer = new byte[64];
		Encoding _encoding;

		/// <summary>
		/// Creates a new instance of the <see cref='TerminalControlStreamWriter'/> class.
		/// </summary>
		/// <param name="target">Stream to which terminal control data should be written.</param>
		/// <param name="is7bit">True if the terminal only accepts 7-bit encodings, or false if it accepts 8-bit encodings.</param>
		/// <exception cref='ArgumentNullException'><paramref name='target'/> is <see langword='null'/>.</exception>
		// <param name="useUnicode">True to use Unicode encoding for text, or false to use ASCII. If <paramref name="is7bit"/>
		//   is set, only UTF-7 will be used for Unicode encodings. Otherwise, UTF-8 will be used and escape sequences will be
		//   limited to their 7-bit forms.</param>
		public TerminalControlStreamWriter( Stream target, bool is7bit/*, bool useUnicode*/ ) {
			if( target == null )
				throw new ArgumentNullException( "target" );

			_target = target;
			_7bit = is7bit;
			
			_encoding = Encoding.ASCII;
/*			if( useUnicode ) {
				// Choose the right encoder
				_encoding = is7bit ? Encoding.UTF7 : Encoding.UTF8;
				
				// Make sure we stay out of the way of upper-range codes in UTF8
				is7bit = true;
			}
			else {
				_encoding = Encoding.ASCII;
			}*/
		}
		
		public void WriteText( char[] data, int offset, int count ) {
			byte[] buffer = _encoding.GetBytes( data, offset, count );
			_target.Write( buffer, 0, buffer.Length );
		}
		
		public void WriteText( string data ) {
			byte[] buffer = _encoding.GetBytes( data );
			_target.Write( buffer, 0, buffer.Length );
		}
		
		public void WriteText( char data ) {
			byte[] buffer = _encoding.GetBytes( new char[] { data } );
			_target.Write( buffer, 0, buffer.Length );
		}

		public void WriteControlCode( C0 code ) {
			_target.WriteByte( (byte) code );
		}

		public void WriteControlCode( C1 code ) {
			if( _7bit ) {
				_outBuffer[0] = 0x1B;
				_outBuffer[1] = (byte)(((byte) code) - 0x40);
				_target.Write( _outBuffer, 0, 2 );
			}
			else {
				_target.WriteByte( (byte) code );
			}
		}

		public void WriteControlSequence( int p1, int p2, ControlFinalCode final ) {
			int i = 0;
			
			if( _7bit ) {
				_outBuffer[i++] = 0x1B;
				_outBuffer[i++] = 0x5B;
			}
			else {
				_outBuffer[i++] = 0x9B;
			}
			
			if( p1 >= 0 )
				WriteParam( ref i, p1 );
			
			if( p2 >= 0 ) {
				_outBuffer[i++] = 0x3B;
				WriteParam( ref i, p2 );
			}
			
			_outBuffer[i++] = (byte) final;
			_target.Write( _outBuffer, 0, i );
		}
		
		void WriteParam( ref int i, int param ) {
			Stack<byte> stack = new Stack<byte>();
			
			do {
				stack.Push( (byte)((param % 10) + 0x30) );
				param /= 10;
			} while( param != 0 );
			
			while( stack.Count != 0 )
				_outBuffer[i++] = stack.Pop();
		}

		public void WriteControlSequence( int p1, int p2, ControlExtendedFinalCode final ) {
			int i = 0;

			if( _7bit ) {
				_outBuffer[i++] = 0x1B;
				_outBuffer[i++] = 0x5B;
			}
			else {
				_outBuffer[i++] = 0x9B;
			}

			if( p1 >= 0 )
				WriteParam( ref i, p1 );

			if( p2 >= 0 ) {
				_outBuffer[i++] = 0x3B;
				WriteParam( ref i, p2 );
			}

			_outBuffer[i++] = 0x20;
			_outBuffer[i++] = (byte) final;
			_target.Write( _outBuffer, 0, i );
		}

		public void WriteIndependentControlFunction( IndependentControlFunction function ) {
			int i = 0;

			_outBuffer[i++] = 0x1B;
			_outBuffer[i++] = (byte) function;
			
			_target.Write( _outBuffer, 0, i );
		}
	}
	
	public interface ITerminalControlWriter {
		void WriteText( char data );
		void WriteText( string data );
		void WriteText( char[] data, int offset, int count );
		void WriteControlCode( C0 code );
		void WriteControlCode( C1 code );
		void WriteControlSequence( int p1, int p2, ControlFinalCode final );
		void WriteControlSequence( int p1, int p2, ControlExtendedFinalCode final );
		void WriteIndependentControlFunction( IndependentControlFunction function );
	}
	
	[Flags]
	enum CharacterDisplayFlags : byte {
		Erased = 1,
	}
	
	/// <summary>
	/// Describes a terminal color using the 8-color system with bold attributes.
	/// </summary>
	enum DisplayColor : byte {
		Black = 0,
		DarkRed = 1,
		DarkGreen = 2,
		Brown = 3,
		DarkBlue = 4,
		DarkMagenta = 5,
		DarkCyan = 6,
		LightGrey = 7,
		DarkGrey = 8,
		Red = 9,
		Green = 10,
		Yellow = 11,
		Blue = 12,
		Magenta = 13,
		Cyan = 14,
		White = 15
	}
	
	struct TerminalDisplayCharacter {
		CharacterDisplayFlags _flags;
		byte _colors;
		byte _char;
		
		public DisplayColor ForeColor {
			get {
				return (DisplayColor)(_colors & 0x0F);
			}
			set {
				_colors =(byte)((_colors & 0xF0) | (((byte) value) & 0x0F));
			}
		}
		
		public DisplayColor BackColor {
			get {
				return (DisplayColor)(_colors >> 4);
			}
			set {
				_colors = (byte)((_colors & 0x0F) | ((((byte) value) & 0x07) << 4));
			}
		}
		
		public byte Character {
			get {
				return _char;
			}
			set {
				_char = value;
			}
		}
	}
	
	class CursorMovedEventArgs : EventArgs {
		int _posX, _posY;
		
		public CursorMovedEventArgs( int posX, int posY ) {
			_posX = posX;
			_posY = posY;
		}
		
		public int PositionX {
			get {
				return _posX;
			}
		}
		
		public int PositionY {
			get {
				return _posY;
			}
		}
	}
	
	class Vt100Terminal {
		int _presX, _presY;
		TerminalDisplayCharacter[] _display;
	
		enum CharFlags : byte {
			Erased = 1,
		}
		
		struct CharPos {
			CharFlags _flags;
			byte _value;
		}
		
		
		
		public event EventHandler<CursorMovedEventArgs> CursorMoved;
	}
	
	public class TerminalTester : ITerminalControlWriter {
		public void WriteText( char data ) {
			Console.Write( data );
		}
		
		public void WriteText( string data ) {
			Console.Write( data );
		}
		
		public void WriteText( char[] data, int offset, int count ) {
			Console.Write( data, offset, count );
		}

		public void WriteControlCode( C0 code ) {
			Console.Write( code );
		}

		public void WriteControlCode( C1 code ) {
			Console.Write( code );
		}

		public void WriteControlSequence( int p1, int p2, ControlFinalCode final ) {
			Console.Write( "{0};{1}{2}", p1, p2, final );
		}

		public void WriteControlSequence( int p1, int p2, ControlExtendedFinalCode final ) {
			Console.Write( "{0};{1}{2}", p1, p2, final );
		}

		public void WriteIndependentControlFunction( IndependentControlFunction function ) {
			Console.Write( function );
		}
	}
}