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

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Represents a comment in the code text.
	/// </summary>
	public sealed class CommentExpression : Expression {
		string[] _comments;

		/// <summary>
		/// Creates a new instance of the <see cref='CommentExpression'/> class.
		/// </summary>
		/// <param name="comment">Comment to place in the code text.</param>
		public CommentExpression( string comment ) {
			_comments = comment.Split( new string[] { "\r\n", "\n" }, StringSplitOptions.None );
		}
		
		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return "// " + string.Join( "\n// ", _comments );
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
			if( cxt == null )
				throw new ArgumentNullException( "cxt" );
		}
	}
	
	/// <summary>
	/// Represents a blank line in the code text.
	/// </summary>
	public sealed class BlankLineExpression : Expression {
		/// <summary>
		/// Creates a new instance of the <see cref='BlankLineExpression'/> class.
		/// </summary>
		public BlankLineExpression() {
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="ToString"]/*'/>
		public override string ToString() {
			return string.Empty;
		}

		/// <include file='../Common.xml' path='/root/Expression/method[@name="Emit"]/*'/>
		public override void Emit( ILGeneratorContext cxt ) {
		}
		
	}
}