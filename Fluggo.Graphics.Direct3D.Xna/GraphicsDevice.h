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

 NS_OPEN
/*	public enum class Transform {
		View = D3DTS_VIEW,
		Projection = D3DTS_PROJECTION,
		Texture0 = D3DTS_TEXTURE0,
		Texture1 = D3DTS_TEXTURE1,
		Texture2 = D3DTS_TEXTURE2,
		Texture3 = D3DTS_TEXTURE3,
		Texture4 = D3DTS_TEXTURE4,
		Texture5 = D3DTS_TEXTURE5,
		Texture6 = D3DTS_TEXTURE6,
		Texture7 = D3DTS_TEXTURE7
	};
	
	public enum class TextureOperation {
		Disable = D3DTOP_DISABLE,
		SelectArg1 = D3DTOP_SELECTARG1,
		SelectArg2 = D3DTOP_SELECTARG2,
		Modulate = D3DTOP_MODULATE,
		Modulate2X = D3DTOP_MODULATE2X,
		Modulate4X = D3DTOP_MODULATE4X,
		Add = D3DTOP_ADD,
		AddSigned = D3DTOP_ADDSIGNED,
		AddSigned2X = D3DTOP_ADDSIGNED2X,
		Subtract = D3DTOP_SUBTRACT,
		AddSmooth = D3DTOP_ADDSMOOTH,
		BlendDiffuseAlpha = D3DTOP_BLENDDIFFUSEALPHA,
		BlendTextureAlpha = D3DTOP_BLENDTEXTUREALPHA,
		BlendFactorAlpha = D3DTOP_BLENDFACTORALPHA,
		BlendTextureAlphaPremultiplied = D3DTOP_BLENDTEXTUREALPHAPM,
		BlendCurrentAlpha = D3DTOP_BLENDCURRENTALPHA,
		Premodulate = D3DTOP_PREMODULATE,
		ModulateAlphaAddColor = D3DTOP_MODULATEALPHA_ADDCOLOR,
		ModulateColorAddAlpha = D3DTOP_MODULATECOLOR_ADDALPHA,
		ModulateInverseAlphaAddColor = D3DTOP_MODULATEINVALPHA_ADDCOLOR,
		ModulateInverseColorAddAlpha = D3DTOP_MODULATEINVCOLOR_ADDALPHA,
		BumpEnvironmentMap = D3DTOP_BUMPENVMAP,
		BumpEnvironmentMapLuma = D3DTOP_BUMPENVMAPLUMINANCE,
		DotProduct3 = D3DTOP_DOTPRODUCT3,
		MultiplyAdd = D3DTOP_MULTIPLYADD,
		Lerp = D3DTOP_LERP
	};
	
	public enum class TextureArgument : unsigned int {
		SelectMask = D3DTA_SELECTMASK,
		Diffuse = D3DTA_DIFFUSE,
		Current = D3DTA_CURRENT,
		Texture = D3DTA_TEXTURE,
		TextureFactor = D3DTA_TFACTOR,
		Specular = D3DTA_SPECULAR,
		Temporary = D3DTA_TEMP,
		Constant = D3DTA_CONSTANT,
		Complement = D3DTA_COMPLEMENT,
		AlphaReplicate = D3DTA_ALPHAREPLICATE
	};*/
	
	public enum class TextureFilter {
		None = D3DTEXF_NONE,
		Point = D3DTEXF_POINT,
		Linear = D3DTEXF_LINEAR,
		Anisotropic = D3DTEXF_ANISOTROPIC,
		PyramidalQuad = D3DTEXF_PYRAMIDALQUAD,
		GaussianQuad = D3DTEXF_GAUSSIANQUAD,
	};

	[Flags]
	public enum class ClearTargets {
		None = 0u,
		Stencil = D3DCLEAR_STENCIL,
		Target = D3DCLEAR_TARGET,
		ZBuffer = D3DCLEAR_ZBUFFER
	};
	
	public enum class FillMode {
		Point = D3DFILL_POINT,
		Wireframe = D3DFILL_WIREFRAME,
		Solid = D3DFILL_SOLID
	};
	
	public enum class CullMode {
		None = D3DCULL_NONE,
		Clockwise = D3DCULL_CW,
		CounterClockwise = D3DCULL_CCW
	};
	
	public enum class ShadeMode {
		Flat = D3DSHADE_FLAT,
		Gouraud = D3DSHADE_GOURAUD,
		Phong = D3DSHADE_PHONG
	};
	
	public enum class PrimitiveType {
		PointList = D3DPT_POINTLIST,
		LineList = D3DPT_LINELIST,
		LineStrip = D3DPT_LINESTRIP,
		TriangleList = D3DPT_TRIANGLELIST,
		TriangleStrip = D3DPT_TRIANGLESTRIP,
		TriangleFan = D3DPT_TRIANGLEFAN
	};

	public enum class VertexDeclarationType : unsigned char {
		Float1 = D3DDECLTYPE_FLOAT1,
		Float2 = D3DDECLTYPE_FLOAT2,
		Float3 = D3DDECLTYPE_FLOAT3,
		Float4 = D3DDECLTYPE_FLOAT4,
		Color = D3DDECLTYPE_D3DCOLOR,
							
		Byte4 = D3DDECLTYPE_UBYTE4,
		Short2 = D3DDECLTYPE_SHORT2,
		Short4 = D3DDECLTYPE_SHORT4,

		NormalizedByte4 = D3DDECLTYPE_UBYTE4N,
		NormalizedShort2 = D3DDECLTYPE_SHORT2N,
		NormalizedShort4 = D3DDECLTYPE_SHORT4N,
		NormalizedUShort2 = D3DDECLTYPE_USHORT2N,
		NormalizedUShort4 = D3DDECLTYPE_USHORT4N,
		Unsigned101010 = D3DDECLTYPE_UDEC3,
		NormalizedSigned101010 = D3DDECLTYPE_DEC3N,
		Float16_2 = D3DDECLTYPE_FLOAT16_2,
		Float16_4 = D3DDECLTYPE_FLOAT16_4,
		Unused = D3DDECLTYPE_UNUSED,
	};
	
	public enum class VertexDeclarationMethod : unsigned char {
		Default = D3DDECLMETHOD_DEFAULT,
		PartialU = D3DDECLMETHOD_PARTIALU,
		PartialV = D3DDECLMETHOD_PARTIALV,
		CrossUV = D3DDECLMETHOD_CROSSUV,
		UV = D3DDECLMETHOD_UV,
		Lookup = D3DDECLMETHOD_LOOKUP,
		LookupPresampled = D3DDECLMETHOD_LOOKUPPRESAMPLED
	};
	
	public enum class VertexDeclarationUsage : unsigned char {
		Position = D3DDECLUSAGE_POSITION,
		BlendWeight = D3DDECLUSAGE_BLENDWEIGHT, 
		BlendIndices = D3DDECLUSAGE_BLENDINDICES,
		Normal = D3DDECLUSAGE_NORMAL,      
		PointSize = D3DDECLUSAGE_PSIZE,       
		TextureCoord = D3DDECLUSAGE_TEXCOORD,    
		Tangent = D3DDECLUSAGE_TANGENT,     
		Binormal = D3DDECLUSAGE_BINORMAL,    
		TesselationFactor = D3DDECLUSAGE_TESSFACTOR,  
		TransformedPosition = D3DDECLUSAGE_POSITIONT,   
		Color = D3DDECLUSAGE_COLOR,       
		Fog = D3DDECLUSAGE_FOG,         
		Depth = D3DDECLUSAGE_DEPTH,       
		Sample = D3DDECLUSAGE_SAMPLE
	};

	public enum class TextureAddress {
		Wrap = D3DTADDRESS_WRAP,
		Mirror = D3DTADDRESS_MIRROR,
		Clamp = D3DTADDRESS_CLAMP,
		Border = D3DTADDRESS_BORDER,
		MirrorOnce = D3DTADDRESS_MIRRORONCE,
	};
	
	public enum class CompareFunction {
		Never = D3DCMP_NEVER,
		Less = D3DCMP_LESS,
		Equal = D3DCMP_EQUAL,
		LessEqual = D3DCMP_LESSEQUAL,
		Greater = D3DCMP_GREATER,
		NotEqual = D3DCMP_NOTEQUAL,
		GreateEqual = D3DCMP_GREATEREQUAL,
		Always = D3DCMP_ALWAYS
	};
	
	public ref class PresentationParameters  {
	private:
		int _backBufferWidth, _backBufferHeight;
		SurfaceFormat _backBufferFormat;
		int _backBufferCount;
		MultiSampleType _multiSampleType;
		int _multiSampleQuality;
		SwapEffect _swapEffect;
		IntPtr _deviceWindow;
		int _windowed, _enableAutoDepthStencil;
		DepthFormat _autoDepthStencilFormat;
		PresentOptions _flags;
		int _fullScreenRefreshRate;
		PresentInterval _interval;
		
	internal:
		property interior_ptr<D3DPRESENT_PARAMETERS> Ptr {
			interior_ptr<D3DPRESENT_PARAMETERS> get() {
				return (interior_ptr<D3DPRESENT_PARAMETERS>) &_backBufferWidth;
			}
		}
		
	public:
		PresentationParameters() {
		}
		
		static const int DefaultPresentRate = 0;
		
		SIMPLE_PROPERTY(DepthFormat,_autoDepthStencilFormat,AutoDepthStencilFormat)
		SIMPLE_PROPERTY(int,_backBufferWidth,BackBufferWidth)
		SIMPLE_PROPERTY(int,_backBufferHeight,BackBufferHeight)
		SIMPLE_PROPERTY(int,_backBufferCount,BackBufferCount)
		SIMPLE_PROPERTY(SurfaceFormat,_backBufferFormat,BackBufferFormat)
		SIMPLE_PROPERTY(IntPtr,_deviceWindow,DeviceWindowHandle)
		SIMPLE_PROPERTY(int,_fullScreenRefreshRate,FullScreenRefreshRateInHz)
		SIMPLE_PROPERTY(int,_multiSampleQuality,MultiSampleQuality)
		SIMPLE_PROPERTY(NS(MultiSampleType),_multiSampleType,MultiSampleType)
		SIMPLE_PROPERTY(NS(PresentInterval),_interval,PresentationInterval)
		SIMPLE_PROPERTY(NS(PresentOptions),_flags,PresentOptions)
		SIMPLE_PROPERTY(NS(SwapEffect),_swapEffect,SwapEffect)
		
		property bool EnableAutoDepthStencil {
			bool get() {
				return _enableAutoDepthStencil != 0;
			}
			void set( bool value ) {
				_enableAutoDepthStencil = value ? 1 : 0;
			}
		}
		
		property bool IsFullScreen {
			bool get() {
				return _windowed == 0;
			}
			void set( bool value ) {
				_windowed = value ? 0 : 1;
			}
		}
	};

	public value class DisplayMode {
	private:
		int _width, _height, _refreshRate;
		NS(SurfaceFormat) _format;
		
	public:
		SIMPLE_PROPERTY_GET(int,_width,Width)
		SIMPLE_PROPERTY_GET(int,_height,Height)
		SIMPLE_PROPERTY_GET(int,_refreshRate,RefreshRate)
		SIMPLE_PROPERTY_GET(SurfaceFormat,_format,Format)
	};
	
	public value class VertexElement {
	private:
		unsigned short _stream, _offset;
		VertexDeclarationType _type;
		VertexDeclarationMethod _method;
		VertexDeclarationUsage _usage;
		unsigned char _usageIndex;
		
	internal:
		static initonly VertexElement End = VertexElement( 0xFF, 0, VertexDeclarationUsage::Position, VertexDeclarationType::Unused, 0, VertexDeclarationMethod::Default );
		
	public:
		VertexElement( unsigned short stream, unsigned short offset,
				VertexDeclarationUsage usage, VertexDeclarationType type, unsigned char usageIndex, VertexDeclarationMethod method ) {
			_stream = stream;
			_offset = offset;
			_usage = usage;
			_type = type;
			_usageIndex = usageIndex;
			_method = method;
		}
		
		VertexElement( unsigned short stream, unsigned short offset,
				VertexDeclarationUsage usage, VertexDeclarationType type, unsigned char usageIndex ) {
			_stream = stream;
			_offset = offset;
			_usage = usage;
			_type = type;
			_usageIndex = usageIndex;
			_method = VertexDeclarationMethod::Default;
		}
	
		VertexElement( unsigned short stream, unsigned short offset,
				VertexDeclarationUsage usage, VertexDeclarationType type ) {
			_stream = stream;
			_offset = offset;
			_usage = usage;
			_type = type;
			_usageIndex = 0;
			_method = VertexDeclarationMethod::Default;
		}

		SIMPLE_PROPERTY(unsigned short,_stream,Stream)
		SIMPLE_PROPERTY(unsigned short,_offset,Offset)
		SIMPLE_PROPERTY(VertexDeclarationType,_type,Type)
		SIMPLE_PROPERTY(VertexDeclarationMethod,_method,Method)
		SIMPLE_PROPERTY(VertexDeclarationUsage,_usage,Usage)
		SIMPLE_PROPERTY(unsigned char,_usageIndex,UsageIndex)
	};
	
	public ref class VertexDeclaration {
	internal:
		GraphicsDevice^ _device;
		IDirect3DVertexDeclaration9 *_decl;
		
		VertexDeclaration( GraphicsDevice ^device, IDirect3DVertexDeclaration9 *decl ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			if( decl == NULL )
				throw gcnew ArgumentNullException( "decl" );
				
			_device = device;
			_decl = decl;
		}
		
		void EnsureDecl() {
			if( _decl == NULL )
				throw gcnew ObjectDisposedException( nullptr );
		}
		
		~VertexDeclaration() { this->!VertexDeclaration(); }
		!VertexDeclaration() {
			if( _decl != NULL ) {
				_decl->Release();
				_decl = NULL;
			}
		}
		
	public:
		VertexDeclaration( _GD ^graphicsDevice, array<VertexElement> ^elements );
	};

	ref class SamplerState;
	ref class VertexStream;
	ref class RenderState;

	public ref class TextureCollection sealed {
	internal:
		GraphicsDevice^ _device;
		Dictionary<int,Texture^>^ _dict;
		int _offset;
		
		TextureCollection( GraphicsDevice^ device, int offset ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			if( offset < 0 )
				throw gcnew ArgumentOutOfRangeException( "offset" );
				
			_device = device;
			_offset = offset;
			
			_dict = gcnew Dictionary<int,Texture^>();
		}
	
	public:
		property Texture^ default[int] {
			Texture^ get( int stage );
			void set( int stage, Texture^ value );
		}
	};
	
	// XNA-compatible
	public ref class SamplerStateCollection {
	private:
		GraphicsDevice ^_device;
	
	internal:
		SamplerStateCollection( GraphicsDevice^ device ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			_device = device;
		}
	
	public:
		property SamplerState^ default[int] {
			SamplerState^ get( int index );
		}
	};
	
	public ref class VertexStreamCollection sealed {
	private:
		GraphicsDevice ^_device;
	
	internal:
		VertexStreamCollection( GraphicsDevice ^device ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			_device = device;
		}
		
	public:
		property VertexStream ^default[int] {
			VertexStream ^get( int index );
		}
	};
	
	public ref class GraphicsDevice {
	private:
		IDirect3DDevice9 *_deviceW;
		TextureCollection ^_textures, ^_vertexTextures;
		bool _beginSceneCalled;
	
	internal:
		GraphicsDevice( IDirect3DDevice9 *device ) {
			if( device == NULL )
				throw gcnew ArgumentNullException( "device" );
			
			_deviceW = device;
			_textures = gcnew TextureCollection( this, 0 );
			_vertexTextures = gcnew TextureCollection( this, D3DVERTEXTEXTURESAMPLER0 );
		}

		~GraphicsDevice() { this->!GraphicsDevice(); }
		!GraphicsDevice() {
			if( _deviceW != NULL ) {
				_deviceW->Release();
				_deviceW = NULL;
			}
		}
		
		property IDirect3DDevice9 *Ptr {
			IDirect3DDevice9 *get() {
				if( _deviceW == NULL )
					throw gcnew ObjectDisposedException( nullptr );
					
				return _deviceW;
			}
		}
		
	private:
		int GetVertexCount( PrimitiveType primitiveType, int primitiveCount ) {
			switch( primitiveType ) {
				case PrimitiveType::PointList:
					return primitiveCount;
					
				case PrimitiveType::LineList:
					return primitiveCount * 2;
					
				case PrimitiveType::LineStrip:
					return primitiveCount + 1;
				
				case PrimitiveType::TriangleList:
					return primitiveCount * 3;
					
				case PrimitiveType::TriangleStrip:
				case PrimitiveType::TriangleFan:
					return primitiveCount + 2;
					
				default:
					throw gcnew ArgumentException( "Unknown primitive type was specified.", "primitiveType" );
			}
		}
		
		void BeginScene();
		void EndScene();
		
	public:
		GraphicsDevice( GraphicsAdapter ^adapter, DeviceType deviceType, IntPtr renderWindowHandle, CreateOptions creationOptions, PresentationParameters ^presentationParameters );
		
		// Incompatible
		void Clear( array<_Rect>^ rectangles, ClearTargets targets, unsigned int color, float z, unsigned int stencil );
		
		// XNA-beta2-compatible, but missing overrides
		void Present( Nullable<_Rect> sourceRect, Nullable<_Rect> destRect, IntPtr destWindowOverride );
		
		// Incompatible
//		void BeginScene();
		// Incompatible
//		void EndScene();

		// Incompatible
		DisplayMode GetDisplayMode( int swapChain );
		
/*		VertexBuffer^ CreateVertexBuffer( unsigned int size, BufferUsage usage, FVF fvf, Pool pool );
		IndexBuffer^ CreateIndexBuffer( unsigned int sizeInBytes, BufferUsage usage, Pool pool, bool useLongIndexes );
		VertexShader^ CreateVertexShader( array<unsigned int>^ functionData, int startIndex, int count );
		PixelShader^ CreatePixelShader( array<unsigned int>^ functionData, int startIndex, int count );*/
		
		// XNA-compatible
		void DrawPrimitives( PrimitiveType primitiveType, int startVertex, int primitiveCount );
		
		// XNA-compatible
		void DrawIndexedPrimitives( PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount );
		
		// XNA-beta2-compatible
		generic <typename T> where T : value class
		void DrawUserPrimitives( PrimitiveType primitiveType, array<T>^ vertexData, int vertexOffset, int primitiveCount );

		// Extension
		property NS(FVF) FVF {
			NS(FVF) get();
			void set( NS(FVF) value );
		}
		
		// XNA-beta2-compatible
		property RenderState ^RenderState {
			NS(RenderState) ^get();
		}
		
		// XNA-1.0-compatible
		property TextureCollection^ Textures {
			TextureCollection^ get() { return _textures; }
		}
		
		// XNA-1.0-compatible
		property TextureCollection^ VertexTextures {
			TextureCollection^ get() { return _vertexTextures; }
		}

		// XNA-beta2-compatible
		property SamplerStateCollection^ SamplerStates {
			SamplerStateCollection^ get() { return gcnew SamplerStateCollection( this ); }
		}

		// XNA-beta2-compatible
		property VertexStreamCollection ^Vertices {
			VertexStreamCollection ^get() {
				return gcnew VertexStreamCollection( this );
			}
		}
		
		// XNA-beta2-compatible
		property NS(VertexDeclaration)^ VertexDeclaration {
			NS(VertexDeclaration)^ get();
			void set( NS(VertexDeclaration)^ value );
		}
		
		// XNA-beta2-compatible
		property NS(VertexShader)^ VertexShader {
			NS(VertexShader)^ get();
			void set( NS(VertexShader)^ value );
		}
		
		// XNA-beta2-compatible
		property NS(PixelShader)^ PixelShader {
			NS(PixelShader)^ get();
			void set( NS(PixelShader)^ value );
		}
		
		// XNA-beta2-compatible
		property IndexBuffer^ Indices {
			IndexBuffer^ get();
			void set( IndexBuffer^ value );
		}
		
		// Incompatible
		//NS(VertexDeclaration)^ CreateVertexDeclaration( ... array<VertexElement>^ vertexElements );
		
//		Texture^ CreateTexture( unsigned int width, unsigned int height, unsigned int levels, ImageUsage usage, SurfaceFormat format, Pool pool );
//		Surface^ CreateOffscreenPlainSurface( unsigned int width, unsigned int height, SurfaceFormat format, Pool pool );

	};

	/*public ref class TextureStage {
	internal:
		GraphicsDevice^ _device;
		unsigned int _stage;

		TextureStage( GraphicsDevice^ device, unsigned int stage ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			_device = device;
			_stage = stage;
		}
	
	public:
		property BaseTexture^ Texture {
			BaseTexture^ get();
			void set( BaseTexture^ value );
		}
		
#define TEX_PROP(st,t,pn) \
	property t pn { \
		t get() { \
			_device->EnsureDevice(); \
			DWORD result; \
			Direct3D::CheckResult( _device->_device->GetTextureStageState( _stage, st, &result ) ); \
			return (t) result; \
		} \
		void set( t value ) { \
			_device->EnsureDevice(); \
			Direct3D::CheckResult( _device->_device->SetTextureStageState( _stage, st, (DWORD) value ) ); \
		} \
	}
#define TEX_PROP_REINTERPRET(st,t,pn) \
	property t pn { \
		t get() { \
			_device->EnsureDevice(); \
			t result; \
			Direct3D::CheckResult( _device->_device->GetTextureStageState( _stage, st, reinterpret_cast<DWORD*>(&result) ) ); \
			return result; \
		} \
		void set( t value ) { \
			_device->EnsureDevice(); \
			DWORD *result = reinterpret_cast<DWORD*>(&value); \
			Direct3D::CheckResult( _device->_device->SetTextureStageState( _stage, st, *result ) ); \
		} \
	}
	
		TEX_PROP(D3DTSS_COLOROP,TextureOperation,ColorOperation)
		TEX_PROP(D3DTSS_COLORARG1,TextureArgument,ColorArgument1)
		TEX_PROP(D3DTSS_COLORARG2,TextureArgument,ColorArgument2)
		TEX_PROP(D3DTSS_ALPHAOP,TextureOperation,AlphaOperation)
		TEX_PROP(D3DTSS_ALPHAARG1,TextureArgument,AlphaArgument1)
		TEX_PROP(D3DTSS_ALPHAARG2,TextureArgument,AlphaArgument2)
		TEX_PROP_REINTERPRET(D3DTSS_BUMPENVMAT00,float,BumpEnvironmentMatrix00)
		TEX_PROP_REINTERPRET(D3DTSS_BUMPENVMAT01,float,BumpEnvironmentMatrix01)
		TEX_PROP_REINTERPRET(D3DTSS_BUMPENVMAT10,float,BumpEnvironmentMatrix10)
		TEX_PROP_REINTERPRET(D3DTSS_BUMPENVMAT11,float,BumpEnvironmentMatrix11)
		TEX_PROP(D3DTSS_TEXCOORDINDEX,unsigned int,CoordinateIndex)
		TEX_PROP_REINTERPRET(D3DTSS_BUMPENVLSCALE,float,BumpLuminanceScale)
		TEX_PROP_REINTERPRET(D3DTSS_BUMPENVLOFFSET,float,BumpLuminanceOffset)
		TEX_PROP(D3DTSS_COLORARG0,TextureArgument,ColorArgument0)
		TEX_PROP(D3DTSS_ALPHAARG0,TextureArgument,AlphaArgument0)
		TEX_PROP(D3DTSS_RESULTARG,TextureArgument,ResultArgument)
		TEX_PROP(D3DTSS_CONSTANT,unsigned int,ConstantColor)
	};*/

	public ref class SamplerState {
	internal:
		GraphicsDevice^ _device;
		unsigned int _stage;

		SamplerState( GraphicsDevice^ device, unsigned int stage ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			_device = device;
			_stage = stage;
		}
	
	public:
#define SAMP_PROP(st,t,pn) \
	property t pn { \
		t get() { \
			DWORD result; \
			Utility::CheckResult( _device->Ptr->GetSamplerState( _stage, st, &result ) ); \
			return (t) result; \
		} \
		void set( t value ) { \
			Utility::CheckResult( _device->Ptr->SetSamplerState( _stage, st, (DWORD) value ) ); \
		} \
	}
	property bool SRGBTexture {
		bool get() {
			DWORD result;
			Utility::CheckResult( _device->Ptr->GetSamplerState( _stage, D3DSAMP_SRGBTEXTURE, &result ) );
			return (result != 0);
		}
		void set( bool value ) {
			Utility::CheckResult( _device->Ptr->SetSamplerState( _stage, D3DSAMP_SRGBTEXTURE, value ? 1 : 0 ) );
		}
	}
	
		SAMP_PROP(D3DSAMP_ADDRESSU,TextureAddress,AddressU)
		SAMP_PROP(D3DSAMP_ADDRESSV,TextureAddress,AddressV)
		SAMP_PROP(D3DSAMP_ADDRESSW,TextureAddress,AddressW)
		SAMP_PROP(D3DSAMP_MAGFILTER,TextureFilter,MagFilter)
		SAMP_PROP(D3DSAMP_MINFILTER,TextureFilter,MinFilter)
		SAMP_PROP(D3DSAMP_MIPFILTER,TextureFilter,MipFilter)
		SAMP_PROP(D3DSAMP_BORDERCOLOR,unsigned int,BorderColor)
		SAMP_PROP(D3DSAMP_MIPMAPLODBIAS,unsigned int,MipMapLevelOfDetailBias)
		SAMP_PROP(D3DSAMP_MAXMIPLEVEL,unsigned int,MaxMipLevel)
		SAMP_PROP(D3DSAMP_MAXANISOTROPY,unsigned int,MaxAnisotropy)
		SAMP_PROP(D3DSAMP_ELEMENTINDEX,unsigned int,ElementIndex)
		SAMP_PROP(D3DSAMP_DMAPOFFSET,unsigned int,DisplaceMapOffset)
	};

	// XNA-beta2-compatible
	public ref class VertexStream {
	private:
		GraphicsDevice ^_device;
		int _index;
	
	internal:
		VertexStream( GraphicsDevice ^device, int index ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			_device = device;
			_index = index;
		}
		
	public:
		void SetFrequency( int frequency );
		void SetFrequencyOfIndexData( int frequency );
		void SetFrequencyOfInstanceData( int frequency );
		void SetSource( VertexBuffer ^vb, int offsetInBytes, int vertexStride );
		
		property int OffsetInBytes {
			int get();
		}
		
		property VertexBuffer ^VertexBuffer {
			NS(VertexBuffer) ^get();
		}
		
		property int VertexStride {
			int get();
		}
	};
	
	public ref class RenderState {
	private:
		GraphicsDevice ^_device;
		
		unsigned int GetRenderState( D3DRENDERSTATETYPE state ) {
			DWORD result;
			Utility::CheckResult( _device->Ptr->GetRenderState( state, &result ) );
			return result;
		}
		
		void SetRenderState( D3DRENDERSTATETYPE state, unsigned int value ) {
			Utility::CheckResult( _device->Ptr->SetRenderState( state, value ) );
		}
		
	internal:
		RenderState( GraphicsDevice ^device ) {
			if( device == nullptr )
				throw gcnew ArgumentNullException( "device" );
				
			_device = device;
		}

	public:
#define RENDER_PROP_ENUM(s,t) \
	property NS(t) t { \
		NS(t) get() \
			{ return (NS(t)) GetRenderState( s ); } \
		void set( NS(t) value ) \
			{ SetRenderState( s, (unsigned int) value ); } \
	}
#define RENDER_PROP_ENUM_NAME(s,t,n) \
	property NS(t) n { \
		NS(t) get() \
			{ return (NS(t)) GetRenderState( s ); } \
		void set( NS(t) value ) \
			{ SetRenderState( s, (unsigned int) value ); } \
	}
#define RENDER_PROP_BOOL(s,n) \
	property bool n { \
		bool get() \
			{ return GetRenderState( s ) != 0; } \
		void set( bool value ) \
			{ SetRenderState( s, (unsigned int) value ); } \
	}

		// XNA-beta2-compatible section
		RENDER_PROP_BOOL(D3DRS_ALPHABLENDENABLE,AlphaBlendEnable)
		RENDER_PROP_BOOL(D3DRS_ALPHATESTENABLE,AlphaTestEnable)
		RENDER_PROP_ENUM(D3DRS_CULLMODE,CullMode)
		RENDER_PROP_BOOL(D3DRS_ZWRITEENABLE,DepthBufferWriteEnable)
		RENDER_PROP_ENUM(D3DRS_FILLMODE,FillMode)
		RENDER_PROP_BOOL(D3DRS_FOGENABLE,FogEnable)
		RENDER_PROP_BOOL(D3DRS_RANGEFOGENABLE,RangeFogEnable)
		RENDER_PROP_BOOL(D3DRS_STENCILENABLE,StencilEnable)
		RENDER_PROP_BOOL(D3DRS_POINTSPRITEENABLE,PointSpriteEnable)
		RENDER_PROP_ENUM_NAME(D3DRS_ZFUNC,CompareFunction,DepthBufferFunction)

		// Unchecked section
		
		// Incompatible
#if 0
		RENDER_PROP_BOOL(D3DRS_LIGHTING,Lighting)
		RENDER_PROP_ENUM(D3DRS_SHADEMODE,ShadeMode)
		RENDER_PROP_BOOL(D3DRS_COLORVERTEX,PerVertexColor)
		RENDER_PROP_BOOL(D3DRS_CLIPPING,Clipping)
		RENDER_PROP_BOOL(D3DRS_SPECULARENABLE,EnableSpecularHighlights)
		RENDER_PROP_BOOL(D3DRS_LASTPIXEL,DrawLastPixel)
		RENDER_PROP_BOOL(D3DRS_LOCALVIEWER,UseCameraSpecularHighlights)
		RENDER_PROP_BOOL(D3DRS_NORMALIZENORMALS,NormalizeNormals)
#endif
	};
NS_CLOSE