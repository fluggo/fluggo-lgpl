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
#include <d3dx9.h>

using namespace System::Collections::Generic;

NS_OPEN
	ref class Effect;
	
	public enum class SaveStateMode {
		None,
		SaveState
	};
	
	public value class CompiledEffect {
	private:
		array<unsigned char> ^_code;
		String ^_errors;
		
	public:
		CompiledEffect( array<unsigned char> ^compiledEffectCode, String ^errors ) {
			_code = compiledEffectCode;
			_errors = errors;
		}
	
		array<unsigned char> ^GetEffectCode()
			{ return _code; }
			
		property String ^ErrorsAndWarnings {
			String ^get() { return _errors; }
		}
		
		property bool Success {
			bool get() { return _code != nullptr; }
		}
		
		virtual String ^ToString() override {
			return (_errors == nullptr) ? String::Empty : _errors;
		}
	};
	
	public ref class EffectParameter sealed {
	private:
		Effect ^_effect;
		IntPtr _parameter;
		bool _hasDesc;
		String ^_name;
		int _annotationCount;
		
		void EnsureDesc();
		
	internal:
		property IntPtr Ptr {
			IntPtr get() { return _parameter; }
		}
		
		EffectParameter( Effect ^effect, IntPtr parameter ) {
			if( effect == nullptr )
				throw gcnew ArgumentNullException( "effect" );
		
			if( parameter == IntPtr::Zero )
				throw gcnew ArgumentNullException( "parameter" );
				
			_effect = effect;
			_parameter = parameter;
		}
		
	public:
		property String ^Name {
			String ^get() {
				EnsureDesc();
				return _name;
			}
		}
		
		void SetValue( Matrix4f value );
		void SetValue( int value );
		void SetValue( float value );
		void SetValue( Vector2f value );
		void SetValue( array<Vector2f> ^value );
		void SetValue( Vector3f value );
		void SetValue( array<Vector3f> ^ value );
		void SetValue( Vector4f value );
		void SetValue( array<Vector4f> ^ value );
	};
	
	public ref class EffectParameterCollection {
	private:
		Effect ^_effect;
		int _count;
		
	internal:
		EffectParameterCollection( Effect ^effect, int count ) {
			if( effect == nullptr )
				throw gcnew ArgumentNullException( "effect" );
			
			_effect = effect;
			_count = count;
		}
		
	public:
		property int Count {
			int get() {
				return _count;
			}
		}
		
		// XNA-1.0-compatible
		property EffectParameter ^default[int] {
			EffectParameter ^get(int index);
		}
		
		// XNA-1.0-compatible
		property EffectParameter ^default[String^] {
			EffectParameter ^get(String ^name);
		}
	};

	public ref class EffectPass sealed {
	private:
		Effect ^_effect;
		IntPtr _technique, _pass;
		bool _hasDesc, _active;
		String ^_name;
		int _annotationCount;
		int _index;
		
		void EnsureDesc();
		
	internal:
		property IntPtr Ptr {
			IntPtr get() { return _pass; }
		}
		
		EffectPass( Effect ^effect, IntPtr technique, IntPtr pass, int index ) {
			if( effect == nullptr )
				throw gcnew ArgumentNullException( "effect" );
		
			if( technique == IntPtr::Zero )
				throw gcnew ArgumentNullException( "technique" );
			
			if( pass == IntPtr::Zero )
				throw gcnew ArgumentNullException( "pass" );
				
			_effect = effect;
			_technique = technique;
			_pass = pass;
			_index = index;
		}
		
	public:
		property String ^Name {
			String ^get() {
				EnsureDesc();
				return _name;
			}
		}
		
		void Begin();
		void End();
	};

	public ref class EffectPassCollection sealed : public IEnumerable<EffectPass^> {
	private:
		Effect ^_effect;
		IntPtr _technique;
		int _count;
		
		ref class PassEnumerator : public IEnumerator<EffectPass^> {
		private:
			EffectPassCollection ^_coll;
			int _currentIndex;
			
		internal:
			PassEnumerator( EffectPassCollection ^coll ) {
				if( coll == nullptr )
					throw gcnew ArgumentNullException( "coll" );
					
				_coll = coll;
				_currentIndex = -1;
			}
		
		public:
			property EffectPass ^Current {
				virtual EffectPass ^get() {
					if( _currentIndex < 0 || _currentIndex >= _coll->Count )
						throw gcnew InvalidOperationException();
						
					return _coll[_currentIndex];
				}
			}
			
			virtual Object ^current_get() = System::Collections::IEnumerator::Current::get {
				return Current;
			}
			
			virtual bool MoveNext() {
				return (++_currentIndex < _coll->Count);
			}
			
			virtual void Reset() {
				_currentIndex = -1;
			}
			
			virtual ~PassEnumerator() {}
		};
		
	internal:
		EffectPassCollection( Effect ^effect, IntPtr technique, int count );
		
	public:
		property int Count {
			int get() {
				return _count;
			}
		}
		
		property EffectPass ^default[int] {
			EffectPass ^get( int index );
		}
		
		virtual IEnumerator<EffectPass^> ^GetEnumerator() {
			return gcnew PassEnumerator( this );
		}
		
		virtual System::Collections::IEnumerator ^old_GetEnumerator() = System::Collections::IEnumerable::GetEnumerator {
			return GetEnumerator();
		}
	};

	public ref class EffectTechnique sealed {
	private:
		Effect ^_effect;
		IntPtr _technique;
		bool _hasDesc;
		String ^_name;
		int _passCount, _annotationCount;
		
		void EnsureDesc();
		
	internal:
		property IntPtr Ptr {
			IntPtr get() { return _technique; }
		}
		
		EffectTechnique( Effect ^effect, IntPtr technique ) {
			if( effect == nullptr )
				throw gcnew ArgumentNullException( "effect" );
		
			if( technique == IntPtr::Zero )
				throw gcnew ArgumentNullException( "technique" );
				
			_effect = effect;
			_technique = technique;
		}
		
	public:
		property String ^Name {
			String ^get() {
				EnsureDesc();
				return _name;
			}
		}
		
		property EffectPassCollection ^Passes {
			EffectPassCollection ^get() {
				EnsureDesc();
				return gcnew EffectPassCollection( _effect, _technique, _passCount );
			}
		}
	};
	
	public ref class EffectTechniqueCollection sealed : public IEnumerable<EffectTechnique^> {
	private:
		Effect ^_effect;
		int _count;
		
		ref class ValidTechniqueEnumerator sealed : public IEnumerator<EffectTechnique^>, public IEnumerable<EffectTechnique^> {
		private:
			Effect ^_effect;
			IntPtr _currentTechnique;
			
		internal:
			ValidTechniqueEnumerator( Effect ^effect );
			
		public:
			property EffectTechnique ^Current {
				virtual EffectTechnique ^get();
			}
			
			virtual Object ^current_get() = System::Collections::IEnumerator::Current::get {
				return Current;
			}
			
			virtual bool MoveNext();
			virtual void Reset();

			virtual IEnumerator<EffectTechnique^> ^GetEnumerator() {
				return this;
			}
			
			virtual System::Collections::IEnumerator ^old_GetEnumerator() = System::Collections::IEnumerable::GetEnumerator {
				return this;
			}
			
			virtual ~ValidTechniqueEnumerator() {}
		};
		
		ref class TechniqueEnumerator : public IEnumerator<EffectTechnique^> {
		private:
			EffectTechniqueCollection ^_coll;
			int _currentIndex;
			
		internal:
			TechniqueEnumerator( EffectTechniqueCollection ^coll ) {
				if( coll == nullptr )
					throw gcnew ArgumentNullException( "coll" );
					
				_coll = coll;
				_currentIndex = -1;
			}
		
		public:
			property EffectTechnique ^Current {
				virtual EffectTechnique ^get() {
					if( _currentIndex < 0 || _currentIndex >= _coll->Count )
						throw gcnew InvalidOperationException();
						
					return _coll[_currentIndex];
				}
			}
			
			virtual Object ^current_get() = System::Collections::IEnumerator::Current::get {
				return Current;
			}
			
			virtual bool MoveNext() {
				return (++_currentIndex < _coll->Count);
			}
			
			virtual void Reset() {
				_currentIndex = -1;
			}
			
			virtual ~TechniqueEnumerator() {}
		};
		
	internal:
		EffectTechniqueCollection( Effect ^effect, int count );
		
	public:
		IEnumerable<EffectTechnique^> ^GetValidTechniques() {
			return gcnew ValidTechniqueEnumerator( _effect );
		}
		
		property int Count {
			int get() {
				return _count;
			}
		}
		
		property EffectTechnique ^default[int] {
			EffectTechnique ^get( int index );
		}
		
		property EffectTechnique ^default[String ^] {
			EffectTechnique ^get( String ^name );
		}
		
		virtual IEnumerator<EffectTechnique^> ^GetEnumerator() {
			return gcnew TechniqueEnumerator( this );
		}
		
		virtual System::Collections::IEnumerator ^old_GetEnumerator() = System::Collections::IEnumerable::GetEnumerator {
			return GetEnumerator();
		}
	};

	public ref class EffectPool {
	private:
		ID3DXEffectPool *_pool;
		
	public:
		EffectPool();
		
	internal:
		property ID3DXEffectPool *Ptr {
			ID3DXEffectPool *get() {
				if( _pool == NULL )
					throw gcnew ObjectDisposedException( nullptr );
					
				return _pool;
			}
		}
		
		~EffectPool() {
			this->!EffectPool();
		}
		!EffectPool() {
			if( _pool != NULL ) {
				_pool->Release();
				_pool = NULL;
			}
		}
	};
	
	public ref class Effect {
	private:
		ID3DXEffect *_effect;
		String ^_creator;
		int _parameterCount, _techniqueCount, _functionCount;
		bool _active;
		
		void PopulateDesc();
		
	internal:
		property ID3DXEffect *Ptr {
			ID3DXEffect *get() {
				if( _effect == NULL )
					throw gcnew ObjectDisposedException( nullptr );
					
				return _effect;
			}
		}
		
		property bool IsActive {
			bool get() { return _active; }
		}
		
		~Effect() {
			this->!Effect();
		}
		!Effect() {
			if( _effect != NULL ) {
				_effect->Release();
				_effect = NULL;
			}
		}
	
	public:
		Effect( GraphicsDevice ^graphicsDevice, array<unsigned char> ^effectCode, CompilerOptions options, EffectPool ^pool );
		Effect( GraphicsDevice ^graphicsDevice, Effect ^cloneSource );
		Effect( GraphicsDevice ^graphicsDevice, System::IO::Stream ^effectCodeFileStream, CompilerOptions options, EffectPool ^pool );
		Effect( GraphicsDevice ^graphicsDevice, System::IO::Stream ^effectCodeFileStream, int numberBytes, CompilerOptions options, EffectPool ^pool );
		Effect( GraphicsDevice ^graphicsDevice, String ^effectCodeFile, CompilerOptions options, EffectPool ^pool );
		
		void Begin();
		void Begin( SaveStateMode saveStateMode );
		
		void End();
		
		static CompiledEffect CompileEffectFromSource( String ^effectFileSource, array<CompilerMacro> ^preprocessorDefines,
			CompilerIncludeHandler ^includeHandler, CompilerOptions options, TargetPlatform platform );
			
		// XNA-1.0-compatible
		property EffectTechniqueCollection ^Techniques {
			EffectTechniqueCollection ^get() {
				return gcnew EffectTechniqueCollection( this, _techniqueCount );
			}
		}
		
		property EffectParameterCollection ^Parameters {
			EffectParameterCollection ^get() {
				return gcnew EffectParameterCollection( this, _parameterCount );
			}
		}
		
		property EffectTechnique ^CurrentTechnique {
			EffectTechnique ^get();
			void set( EffectTechnique ^value );
		}
	};
NS_CLOSE