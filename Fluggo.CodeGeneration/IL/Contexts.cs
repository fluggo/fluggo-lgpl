/*
	Fluggo Code Generation Library
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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a position in a text file.
	/// </summary>
	public class FilePosition {
		int _line, _column;

		/// <summary>
		/// Creates a new instance of the <see cref='FilePosition'/> class.
		/// </summary>
		/// <param name="line">Line number of the position, starting at line one.</param>
		/// <param name="column">Column number of the position, starting at column zero.</param>
		public FilePosition( int line, int column ) {
			_line = line;
			_column = column;
		}

		/// <summary>
		/// Gets the line number of the position.
		/// </summary>
		/// <value>The line number of the position, starting at line one.</value>
		public int Line {
			get {
				return _line;
			}
		}

		/// <summary>
		/// Gets the column number of the position.
		/// </summary>
		/// <value>The column number of the position, starting at column zero.</value>
		public int Column {
			get {
				return _column;
			}
		}
	}
	
	/// <summary>
	/// Provides methods for writing formatted code text to a file.
	/// </summary>
	public class CodeTextWriter {
		TextWriter _writer;
		int _indent, _currentLine, _currentColumn;
		string _filename;
		const string __newline = "\r\n", __tab = "\t";
		bool _indentationWritten = false;

		/// <summary>
		/// Creates a new instance of the <see cref='CodeTextWriter'/> class.
		/// </summary>
		/// <param name="filename">Name of the file to open.</param>
		public CodeTextWriter( string filename ) {
			_writer = new StreamWriter( filename );
			_filename = filename;
			_currentColumn = 0;
			_currentLine = 1;
			_indent = 0;
		}
		
		private void Write( string[] lines ) {

			for( int i = 0; i < lines.Length - 1; i++ ) {
				WriteIndent();

				_writer.WriteLine( lines[i] );

				_currentLine++;
				_currentColumn = 1;
				_indentationWritten = false;
			}

			string lastValue = lines[lines.Length - 1];
			
			if( lastValue.Length != 0 ) {
				WriteIndent();

				_writer.Write( lastValue );
				_currentColumn += lastValue.Length;
			}
		}
		
		/// <summary>
		/// Writes the beginning indentation for the line if it hasn't already been written.
		/// </summary>
		public void WriteIndent() {
			if( !_indentationWritten ) {
				for( int j = 0; j < _indent; j++ )
					_writer.Write( __tab );

				_currentColumn = __tab.Length * _indent + 1;
				_indentationWritten = true;
			}
		}

		/// <summary>
		/// Gets the name of the file.
		/// </summary>
		/// <value>The name of the file being written to.</value>
		public string Filename {
			get {
				return _filename;
			}
		}
		
		/// <summary>
		/// Writes the given string to the file.
		/// </summary>
		/// <param name="value">String to write to the file. New lines are brought to the correct indentation level.</param>
		public void Write( string value ) {
			if( value == null )
				throw new ArgumentNullException( "value" );
			
			Write( value.Split( new string[] { __newline }, StringSplitOptions.None ) );
		}

		/// <summary>
		/// Writes the given formatted string to the file.
		/// </summary>
		/// <param name="format">Format for the string to write to the file. New lines are brought to the correct indentation level.</param>
		/// <param name="args">Arguments for the format string.</param>
		public void Write( string format, params object[] args ) {
			Write( string.Format( format, args ) );
		}

		/// <summary>
		/// Writes a blank line to the file.
		/// </summary>
		public void WriteLine() {
			Write( new string[] { string.Empty, string.Empty } );
		}

		/// <summary>
		/// Writes the given string to the file, followed by a newline.
		/// </summary>
		/// <param name="value">String to write to the file.</param>
		public void WriteLine( string value ) {
			Write( new string[] { value, string.Empty } );
		}
		
		/// <summary>
		/// Writes the given formatted string to the file, followed by a newline.
		/// </summary>
		/// <param name="format">Format for the string to write to the file.</param>
		/// <param name="args">Arguments for the format string.</param>
		public void WriteLine( string format, params object[] args ) {
			Write( new string[] { string.Format( format, args ), string.Empty } );
		}

		/// <summary>
		/// Gets the current position in the target file.
		/// </summary>
		/// <value>The current position in the target file.</value>
		public FilePosition Position {
			get {
				return new FilePosition( _currentLine, _currentColumn );
			}
		}
		
		/// <summary>
		/// Gets or sets the number of tabs to indent for a new line.
		/// </summary>
		/// <value>The number of tabs to indent for a new line.</value>
		public int Indent {
			get {
				return _indent;
			}
			set {
				if( value < 0 )
					throw new ArgumentOutOfRangeException( "value" );
				
				_indent = value;
			}
		}
		
		/// <summary>
		/// Closes the writer.
		/// </summary>
		public void Close() {
			_writer.Close();
		}
	}
	
	
	/// <summary>
	/// Represents a code stream object that can be marked at a different point than its creation.
	/// </summary>
	public interface IMarkable {
		/// <summary>
		/// Marks the object at the current point in code stream.
		/// </summary>
		void Mark();
	}

	
	
}