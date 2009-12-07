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

#include "Effect.h"
#include "GraphicsDevice.h"
#include <vcclr.h>

NS_OPEN
	EffectPool::EffectPool() {
		pin_ptr<ID3DXEffectPool*> ppool = &_pool;
		Utility::CheckResult( ::D3DXCreateEffectPool( ppool ) );
	}
	
	Effect::Effect( GraphicsDevice ^graphicsDevice, array<unsigned char> ^effectCode, CompilerOptions options, EffectPool ^pool ) {
		// This is where we bind an effect to a device
		if( graphicsDevice == nullptr )
			throw gcnew ArgumentNullException( "graphicsDevice" );
			
		if( effectCode == nullptr )
			throw gcnew ArgumentNullException( "effectCode" );

		// Pin all of our code pointers
		pin_ptr<unsigned char> pcode = &effectCode[0];
		pin_ptr<ID3DXEffect*> peffect = &_effect;
		ID3DXEffectPool *ppool = (pool == nullptr) ? NULL : pool->Ptr;
		ID3DXBuffer *errorBuffer = NULL;

		try {
			// Create the effect directly
			HRESULT result = ::D3DXCreateEffect( graphicsDevice->Ptr, pcode, effectCode->Length, NULL, NULL, (DWORD) options, ppool, peffect, &errorBuffer );
			
			// Marshal error messages back if we need to
			if( FAILED( result ) && errorBuffer != NULL ) {
				throw gcnew Exception( Marshal::PtrToStringAnsi( (IntPtr) errorBuffer->GetBufferPointer(), errorBuffer->GetBufferSize() ) );
			}
			
			// Check for any other problems
			Utility::CheckResult( result );
		}
		catch( Exception ^ex ) {
			SAFE_RELEASE( _effect );
			throw ex;
		}
		finally {
			SAFE_RELEASE( errorBuffer );
		}
		
		// Populate the description
		PopulateDesc();
	}
	
	Effect::Effect( GraphicsDevice ^graphicsDevice, Effect ^cloneSource ) {
		if( graphicsDevice == nullptr )
			throw gcnew ArgumentNullException( "graphicsDevice" );
		
		if( cloneSource == nullptr )
			throw gcnew ArgumentNullException( "cloneSource" );
			
		// Clone the original effect
		pin_ptr<ID3DXEffect*> peffect = &_effect;
		Utility::CheckResult( cloneSource->Ptr->CloneEffect( graphicsDevice->Ptr, peffect ) );

		PopulateDesc();
	}

	Effect::Effect( GraphicsDevice ^graphicsDevice, System::IO::Stream ^effectCodeFileStream, CompilerOptions options, EffectPool ^pool ) {
		throw gcnew NotImplementedException();
	}

	Effect::Effect( GraphicsDevice ^graphicsDevice, System::IO::Stream ^effectCodeFileStream, int numberBytes, CompilerOptions options, EffectPool ^pool ) {
		throw gcnew NotImplementedException();
	}

	Effect::Effect( GraphicsDevice ^graphicsDevice, String ^effectCodeFile, CompilerOptions options, EffectPool ^pool ) {
		throw gcnew NotImplementedException();
	}
	
	void Effect::PopulateDesc() {
		D3DXEFFECT_DESC desc;
		Utility::CheckResult( _effect->GetDesc( &desc ) );
		
		_creator = Marshal::PtrToStringAnsi( (IntPtr)(void*) desc.Creator );
		_parameterCount = desc.Parameters;
		_functionCount = desc.Functions;
		_techniqueCount = desc.Techniques;
	}
	
	void Effect::Begin() {
		Begin( SaveStateMode::SaveState );
	}
	
	void Effect::Begin( SaveStateMode saveStateMode ) {
		if( _active )
			throw gcnew InvalidOperationException( "This effect is already active." );
		
		if( Ptr->GetCurrentTechnique() == NULL )
			throw gcnew InvalidOperationException( "No current technique has been set." );
		
		unsigned int passCount;
		Utility::CheckResult( Ptr->Begin( &passCount, (saveStateMode == SaveStateMode::SaveState) ? 0 : D3DXFX_DONOTSAVESTATE ) );
		_active = true;
	}
	
	void Effect::End() {
		if( !_active )
			throw gcnew InvalidOperationException( "This effect is not active." );
		
		Utility::CheckResult( Ptr->End() );
		_active = false;
	}
	
	CompiledEffect Effect::CompileEffectFromSource( String ^effectFileSource, array<CompilerMacro> ^preprocessorDefines,
			CompilerIncludeHandler ^includeHandler, CompilerOptions options, TargetPlatform platform ) {
		// This method is for offline compilation of an effect to a byte array
		
		if( effectFileSource == nullptr )
			throw gcnew ArgumentNullException( "effectFileSource" );
			
		if( includeHandler != nullptr )
			throw gcnew NotImplementedException( "Include handlers are not implemented." );
			
		// Set up variables and initial string conversions
		//IntPtr sourcePtr = Marshal::StringToHGlobalAnsi( effectFileSource );
		ID3DXBuffer *errorMessagesBuffer = NULL, *effectBuffer = NULL;
		ID3DXEffectCompiler *effectCompiler = NULL;
		String ^preprocessorErrorMessages = nullptr, ^compilerErrorMessages = nullptr;
		
		array<IntPtr>^ macros;
		pin_ptr<IntPtr> macroPtr = nullptr;
		
		// Marshal the preprocessor define strings
		if( preprocessorDefines != nullptr ) {
			macros = gcnew array<IntPtr>( (preprocessorDefines->Length + 1) * 2 );
			
			for( int i = 0; i < preprocessorDefines->Length; i++ ) {
				macros[i * 2] = Marshal::StringToHGlobalUni( preprocessorDefines[i].Name );
				macros[i * 2 + 1] = Marshal::StringToHGlobalUni( preprocessorDefines[i].Definition );
			}
			
			macroPtr = &macros[0];
		}
		
		try {
			pinLPSTR sourcePtr = PIN_STRING_ANSI( effectFileSource );
			
			// Create a compiler for this effect
			HRESULT result = ::D3DXCreateEffectCompiler( sourcePtr, effectFileSource->Length,
				(D3DXMACRO*) macroPtr, NULL, (DWORD) options, &effectCompiler, &errorMessagesBuffer );
				
			// If there are error messages, marshal them back
			if( errorMessagesBuffer != NULL ) {
				preprocessorErrorMessages = Marshal::PtrToStringAnsi( (IntPtr) errorMessagesBuffer->GetBufferPointer(), errorMessagesBuffer->GetBufferSize() );
				SAFE_RELEASE( errorMessagesBuffer );
			}
				
			// Return a failed compilation if we need to
			if( FAILED( result ) && preprocessorErrorMessages != nullptr ) {
				return CompiledEffect( nullptr, preprocessorErrorMessages );
			}
			
			// If there's any other kind of failure, throw it here
			Utility::CheckResult( result );
			
			// Compile the effect with the new compiler
			result = effectCompiler->CompileEffect( (DWORD) options, &effectBuffer, &errorMessagesBuffer );
			
			// Did the compilation fail?
			bool complFailed = FAILED( result ) && errorMessagesBuffer != NULL;
			
			// Marshal all error message strings back
			if( errorMessagesBuffer != NULL ) {
				compilerErrorMessages = Marshal::PtrToStringAnsi( (IntPtr) errorMessagesBuffer->GetBufferPointer(), errorMessagesBuffer->GetBufferSize() );
				SAFE_RELEASE( errorMessagesBuffer );
				
				if( preprocessorErrorMessages != nullptr ) {
					compilerErrorMessages = String::Concat( preprocessorErrorMessages, compilerErrorMessages );
				}
			}
			else if( preprocessorErrorMessages != nullptr ) {
				compilerErrorMessages = preprocessorErrorMessages;
			}

			// Return a failed compilation if we need to
			if( complFailed ) {
				return CompiledEffect( nullptr, compilerErrorMessages );
			}
			
			// If there's any other kind of failure, throw it here
			Utility::CheckResult( result );
		}
		finally {
			SAFE_RELEASE( errorMessagesBuffer );
			SAFE_RELEASE( effectCompiler );
			//Marshal::FreeHGlobal( sourcePtr );
			
			if( macros != nullptr ) {
				for( int i = 0; i < macros->Length; i++ ) {
					if( macros[i] != IntPtr::Zero )
						Marshal::FreeHGlobal( macros[i] );
				}
			}
		}
		
		// Pull all the results back into managed memory
		unsigned char *effectPtr = (unsigned char *) effectBuffer->GetBufferPointer();
		array<unsigned char>^ effect = gcnew array<unsigned char>( effectBuffer->GetBufferSize() );
		pin_ptr<unsigned char> effectArrayPtr = &effect[0];
		
		memcpy( effectArrayPtr, effectPtr, effect->Length );
		SAFE_RELEASE( effectBuffer );

		return CompiledEffect( effect, compilerErrorMessages );
	}
	
	void Effect::CurrentTechnique::set( EffectTechnique ^value ) {
		// Actually, I don't know if XNA does this
		if( value == nullptr )
			throw gcnew ArgumentNullException( "value" );
			
		Utility::CheckResult( Ptr->SetTechnique( (D3DXHANDLE)(void*) value->Ptr ) );
	}
	
	EffectTechnique ^Effect::CurrentTechnique::get() {
		IntPtr result = (IntPtr)(void*) Ptr->GetCurrentTechnique();
		
		if( result == IntPtr::Zero )
			return nullptr;
			
		return gcnew EffectTechnique( this, result );
	}
	
	// EffectPass

	void EffectParameter::EnsureDesc() {
		if( _hasDesc )
			return;
		
		D3DXPARAMETER_DESC desc;
		Utility::CheckResult( _effect->Ptr->GetParameterDesc( (D3DXHANDLE)(void*) _parameter, &desc ) );
		
		_name = Marshal::PtrToStringAnsi( (IntPtr)(void*) desc.Name );
		_annotationCount = desc.Annotations;
	}
	
	void EffectParameter::SetValue(int value) {
		Utility::CheckResult( _effect->Ptr->SetInt( (D3DXHANDLE)(void*) _parameter, value ) );
	}
	
	void EffectParameter::SetValue(float value) {
		Utility::CheckResult( _effect->Ptr->SetFloat( (D3DXHANDLE)(void*) _parameter, value ) );
	}
	
	void EffectParameter::SetValue(Matrix4f value) {
		pin_ptr<D3DXMATRIX> matrix = (interior_ptr<D3DXMATRIX>) &value.M11;
		Utility::CheckResult( _effect->Ptr->SetMatrix( (D3DXHANDLE)(void*) _parameter, matrix ) );
	}
	
	void EffectParameter::SetValue(Vector2f value) {
		D3DXVECTOR4 vector( value.X, value.Y, 0.0f, 1.0f );
		Utility::CheckResult( _effect->Ptr->SetVector( (D3DXHANDLE)(void*) _parameter, &vector ) );
	}

	void EffectParameter::SetValue(array<Vector2f> ^value) {
		if( value == nullptr )
			throw gcnew ArgumentNullException( "value" );
			
		// We have to construct a new array for this one; the types are different sizes
		array<Vector4f> ^vectors = gcnew array<Vector4f>( value->Length );
		
		for( int i = 0; i < value->Length; i++ )
			vectors[i] = value[i].ToPointVector4f();
	
		pin_ptr<D3DXVECTOR4> ptr = (interior_ptr<D3DXVECTOR4>) &vectors[0];
		Utility::CheckResult( _effect->Ptr->SetVectorArray( (D3DXHANDLE)(void*) _parameter, ptr, value->Length ) );
	}

	void EffectParameter::SetValue(Vector3f value) {
		D3DXVECTOR4 vector( value.X, value.Y, value.Z, 1.0f );
		Utility::CheckResult( _effect->Ptr->SetVector( (D3DXHANDLE)(void*) _parameter, &vector ) );
	}

	void EffectParameter::SetValue(array<Vector3f> ^value) {
		if( value == nullptr )
			throw gcnew ArgumentNullException( "value" );
			
		// We have to construct a new array for this one; the types are different sizes
		array<Vector4f> ^vectors = gcnew array<Vector4f>( value->Length );
		
		for( int i = 0; i < value->Length; i++ )
			vectors[i] = value[i].ToPointVector4f();
	
		pin_ptr<D3DXVECTOR4> ptr = (interior_ptr<D3DXVECTOR4>) &vectors[0];
		Utility::CheckResult( _effect->Ptr->SetVectorArray( (D3DXHANDLE)(void*) _parameter, ptr, value->Length ) );
	}

	void EffectParameter::SetValue(Vector4f value) {
		pin_ptr<D3DXVECTOR4> pValue = (interior_ptr<D3DXVECTOR4>) &value.X;
		Utility::CheckResult( _effect->Ptr->SetVector( (D3DXHANDLE)(void*) _parameter, pValue ) );
	}

	void EffectParameter::SetValue(array<Vector4f> ^value) {
		if( value == nullptr )
			throw gcnew ArgumentNullException( "value" );
			
		// We have to construct a new array for this one; the types are different sizes
		pin_ptr<D3DXVECTOR4> ptr = (interior_ptr<D3DXVECTOR4>) &value[0];
		Utility::CheckResult( _effect->Ptr->SetVectorArray( (D3DXHANDLE)(void*) _parameter, ptr, value->Length ) );
	}

	// EffectPass

	void EffectPass::EnsureDesc() {
		if( _hasDesc )
			return;
		
		D3DXPASS_DESC desc;
		Utility::CheckResult( _effect->Ptr->GetPassDesc( (D3DXHANDLE)(void*) _pass, &desc ) );
		
		_name = Marshal::PtrToStringAnsi( (IntPtr)(void*) desc.Name );
		_annotationCount = desc.Annotations;
	}
	
	void EffectPass::Begin() {
//		if( _active )
//			throw gcnew InvalidOperationException( "This pass is already active." );

		// Ensure that our technique was started
		if( !_effect->IsActive || ((IntPtr)(void*)_effect->Ptr->GetCurrentTechnique()) != _technique )
			throw gcnew InvalidOperationException( "This pass is not part of an active technique for this effect. Set this technique as the current technique and call the Effect.Begin() method before you call this method." );
		
		Utility::CheckResult( _effect->Ptr->BeginPass( _index ) );
//		_active = true;
	}
	
	void EffectPass::End() {
//		if( !_active )
//			throw gcnew InvalidOperationException( "This pass is not active." );

		Utility::CheckResult( _effect->Ptr->EndPass() );
//		_active = false;
	}
	
	// EffectPassCollection

	EffectPassCollection::EffectPassCollection( Effect ^effect, IntPtr technique, int count ) {
		if( effect == nullptr )
			throw gcnew ArgumentNullException( "effect" );
			
		if( technique == IntPtr::Zero )
			throw gcnew ArgumentNullException( "technique" );
			
		_effect = effect;
		_technique = technique;
		_count = count;
	}
	
	EffectPass ^EffectPassCollection::default::get( int index ) {
		IntPtr handle = (IntPtr)(void*) _effect->Ptr->GetPass( (D3DXHANDLE)(void*) _technique, index );
		
		if( handle == IntPtr::Zero )
			throw gcnew ArgumentOutOfRangeException( "index" );
			
		return gcnew EffectPass( _effect, _technique, handle, index );
	}

	// EffectParameterCollection
	
	EffectParameter ^EffectParameterCollection::default::get(int index) {
		IntPtr handle = (IntPtr)(void*) _effect->Ptr->GetParameter( NULL, (unsigned int) index );
		
		if( handle == IntPtr::Zero )
			throw gcnew ArgumentOutOfRangeException( "index" );
			
		return gcnew EffectParameter( _effect, handle );
	}

	EffectParameter ^EffectParameterCollection::default::get(String ^name) {
		pinLPSTR pName = PIN_STRING_ANSI( name );
		IntPtr handle = (IntPtr)(void*) _effect->Ptr->GetParameterByName( NULL, pName );
		
		if( handle == IntPtr::Zero )
			throw gcnew KeyNotFoundException();
			
		return gcnew EffectParameter( _effect, handle );
	}
	
	// EffectTechnique
	
	void EffectTechnique::EnsureDesc() {
		if( _hasDesc )
			return;
		
		D3DXTECHNIQUE_DESC desc;
		Utility::CheckResult( _effect->Ptr->GetTechniqueDesc( (D3DXHANDLE)(void*) _technique, &desc ) );
		
		_name = Marshal::PtrToStringAnsi( (IntPtr)(void*) desc.Name );
		_passCount = desc.Passes;
		_annotationCount = desc.Annotations;
	}
	
	// EffectTechniqueCollection

	EffectTechniqueCollection::EffectTechniqueCollection( Effect ^effect, int count ) {
		if( effect == nullptr )
			throw gcnew ArgumentNullException( "effect" );
			
		_effect = effect;
		_count = count;
	}
	
	EffectTechnique ^EffectTechniqueCollection::default::get( int index ) {
		IntPtr handle = (IntPtr)(void*) _effect->Ptr->GetTechnique( index );
		
		if( handle == IntPtr::Zero )
			throw gcnew ArgumentOutOfRangeException( "index" );
			
		return gcnew EffectTechnique( _effect, handle );
	}
	
	EffectTechnique ^EffectTechniqueCollection::default::get( String ^name ) {
		pin_ptr<char> pName = PIN_STRING_ANSI( name );
		IntPtr handle = (IntPtr)(void*) _effect->Ptr->GetTechniqueByName( pName );
		
		if( handle == IntPtr::Zero )
			throw gcnew KeyNotFoundException();
			
		return gcnew EffectTechnique( _effect, handle );
	}

	// EffectTechniqueCollection::ValidTechniqueEnumerator
	
	EffectTechniqueCollection::ValidTechniqueEnumerator::ValidTechniqueEnumerator( Effect ^effect ) {
		if( effect == nullptr )
			throw gcnew ArgumentNullException( "effect" );
			
		_effect = effect;
		Reset();
	}
	
	EffectTechnique ^EffectTechniqueCollection::ValidTechniqueEnumerator::Current::get() {
		if( _currentTechnique == IntPtr::Zero )
			throw gcnew InvalidOperationException( "The enumerator is not in a valid position in the list." );
			
		return gcnew EffectTechnique( _effect, _currentTechnique );
	}
	
	bool EffectTechniqueCollection::ValidTechniqueEnumerator::MoveNext() {
		if( _currentTechnique == IntPtr::Zero )
			return false;
		
		if( _currentTechnique == (IntPtr) -1 )
			_currentTechnique = IntPtr::Zero;
		
		pin_ptr<D3DXHANDLE> ptech = (interior_ptr<D3DXHANDLE>) &_currentTechnique;
		Utility::CheckResult( _effect->Ptr->FindNextValidTechnique( *ptech, ptech ) );

		return _currentTechnique != IntPtr::Zero;
	}
	
	void EffectTechniqueCollection::ValidTechniqueEnumerator::Reset() {
//		pin_ptr<D3DXHANDLE> ptech = (interior_ptr<D3DXHANDLE>) &_currentTechnique;
//		Utility::CheckResult( _effect->Ptr->FindNextValidTechnique( NULL, ptech ) );
		_currentTechnique = (IntPtr) -1;
	}
NS_CLOSE