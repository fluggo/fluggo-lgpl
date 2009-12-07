/*
	Fluggo Direct3D Interop Library
	Copyright (C) 2005-7  Brian J. Crowell <brian@fluggo.com>

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

#pragma once
#include "Common.h"

using namespace System::Collections::Generic;

NS_OPEN namespace Shaders {
	public enum class TokenType : int {
		Comment = 0,
		DestinationParameter = 1,
		End = 2,
		Instruction = 3,
		Label = 4,
		SourceParameter = 5,
		Version = 6
	};
	
	public enum class SwizzleSource : unsigned char {
		X = 0,
		Y = 1,
		Z = 2,
		W = 3
	};
	
	public enum class RegisterFile : unsigned char {
		Temporary = 0,
		Input = 1,
		Constant1 = 2,
		Texture = 3,
		Address = 3,
		RasterOut = 4,
		AttributeOut = 5,
		TextureCoordOut = 6,
		Output = 7,
		ConstantIntegerVector = 8,
		DepthOutput = 9,
		SamplerState = 10,
		Constant2 = 11,
		Constant3 = 12,
		Constant4 = 13,
		ConstantBool = 14,
		LoopCounter = 15,
		ShortFloatTemporary = 16,
		Miscellaneous = 17,
		Label = 18,
		Predicate = 19
	};

	public enum class SourceModifier : unsigned char {
		None = 0,
		Negate = 1,
		Bias = 2,
		BiasAndNegate = 3,
		Sign = 4,
		SignAndNegate = 5,
		Complement = 6,
		Double = 7,
		DoubleAndNegate = 8,
		DivideByZ = 9,
		DivideByW = 10,
		Absolute = 11,
		AbsoluteAndNegate = 12,
		LogicalNot = 13
	};
	
	public ref class TokenWriter {
	private;
		List<unsigned int>^ _list;
	
	public:
		void EmitComment( String^ value ) {
			throw gcnew NotImplementedException();
		}
		
		void Close() {
			throw gcnew NotImplementedException();
		}
	};
} NS_CLOSE