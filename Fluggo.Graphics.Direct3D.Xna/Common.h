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
#include <d3d9.h>
#include <d3dx9.h>
#pragma intrinsic(memcpy,strcmp,strcpy)

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace Fluggo::Graphics;

#pragma warning( disable : 4127 )

// {7D351EFE-7A4F-42f6-A995-E0B1F2007FDA}
DEFINE_GUID(UDATA_TAG, 
0x7d351efe, 0x7a4f, 0x42f6, 0xa9, 0x95, 0xe0, 0xb1, 0xf2, 0x0, 0x7f, 0xda);

// {89B43F08-1B9C-4a87-955F-841288406FB9}
DEFINE_GUID(UDATA_NAME, 
0x89b43f08, 0x1b9c, 0x4a87, 0x95, 0x5f, 0x84, 0x12, 0x88, 0x40, 0x6f, 0xb9);

#define SAFE_RELEASE( x ) do { if( x != NULL ) { x->Release(); x = NULL; } } while( 0 )
#define internal_ptr(t,x) ((interior_ptr<t>)(&(x)._v[0]))
#define pC( x ) internal_ptr(D3DCAPS9,x)
#define pPP( x ) internal_ptr(D3DPRESENT_PARAMETERS,x)
#define BEGIN_DECLARE_THUNK_STRUCT(t,n) \
	value class n { \
	internal: \
		array<unsigned char>^ _v; \
		\
		n( int ) { \
			_v = gcnew array<unsigned char>( sizeof(t) ); \
		}
#define END_DECLARE_THUNK_STRUCT() \
	};
#define BEGIN_DECLARE_THUNK_CLASS(t,n) \
	ref class n { \
	internal: \
		array<unsigned char>^ _v; \
		\
		n() { \
			_v = gcnew array<unsigned char>( sizeof(t) ); \
		}
#define END_DECLARE_THUNK_CLASS() \
	};
#define CHECK_V(t) do { if( _v == nullptr ) { _v = gcnew array<unsigned char>( sizeof(t) ); } } while( 0 )
#define THUNK_PROPERTY(t,pt,n) \
	property pt n { \
		void set( pt value ) { \
			CHECK_V(t); \
			internal_ptr(t,*this)->n = value; \
		} \
		pt get() { \
			CHECK_V(t); \
			return internal_ptr(t,*this)->n; \
		} \
	}
#define THUNK_PROPERTY_RENAME(t,pt,nn,pn) \
	property pt pn { \
		void set( pt value ) { \
			CHECK_V(t); \
			internal_ptr(t,*this)->nn = value; \
		} \
		pt get() { \
			CHECK_V(t); \
			return internal_ptr(t,*this)->nn; \
		} \
	}
#define THUNK_PROPERTY_RENAME_GET(t,pt,nn,pn) \
	property pt pn { \
		pt get() { \
			CHECK_V(t); \
			return internal_ptr(t,*this)->nn; \
		} \
	}
#define THUNK_PROPERTY_GET(t,pt,n) \
	property pt n { \
		pt get() { \
			CHECK_V(t); \
			return internal_ptr(t,*this)->n; \
		} \
	}
#define THUNK_PROPERTY_BOOL(t,n) \
	property bool n { \
		void set( bool value ) { \
			CHECK_V(t); \
			internal_ptr(t,*this)->n = value; \
		} \
		bool get() { \
			CHECK_V(t); \
			return internal_ptr(t,*this)->n != 0; \
		} \
	}
#define THUNK_PROPERTY_CAST(t,nt,n,pt) \
	property pt n { \
		void set( pt value ) { \
			CHECK_V(t); \
			internal_ptr(t,*this)->n = (nt) value; \
		} \
		pt get() { \
			CHECK_V(t); \
			return (pt) internal_ptr(t,*this)->n; \
		} \
	}
#define THUNK_PROPERTY_GET_CAST(t,nt,n,pt) \
	property pt n { \
		pt get() { \
			CHECK_V(t); \
			return (pt) internal_ptr(t,*this)->n; \
		} \
	}
#define THUNK_PROPERTY_CAST_INTPTR(t,nt,n) \
	property IntPtr n { \
		void set( IntPtr value ) { \
			CHECK_V(t); \
			internal_ptr(t,*this)->n = (nt)(void*) value; \
		} \
		IntPtr get() { \
			CHECK_V(t); \
			return (IntPtr)(void*) internal_ptr(t,*this)->n; \
		} \
	}
	
#define SIMPLE_PROPERTY(t,n,pn) \
	property t pn { \
		void set( t value ) { n = value; } \
		t get() { return n; } \
	}
#define SIMPLE_PROPERTY_GET(t,n,pn) \
	property t pn { \
		t get() { return n; } \
	}
	
// Use these constructs to pin a string, which should be faster than an HGLOBAL for short-lived strings
typedef pin_ptr<char> pinLPSTR;
#define PIN_STRING_ANSI(x) ((interior_ptr<char>) &System::Text::Encoding::ASCII->GetBytes(x)[0])
#define PIN_STRING_UTF16(x) ((interior_ptr<char>) &System::Text::Encoding::Unicode->GetBytes(x)[0])

typedef System::Runtime::InteropServices::Marshal _Marshal;
typedef Fluggo::Graphics::Rectangle _Rect;
#define NS(x) Fluggo::Graphics::Direct3D::Xna::##x
#define NS_OPEN namespace Fluggo { namespace Graphics { namespace Direct3D { namespace Xna {
#define NS_CLOSE }}}}

NS_OPEN
	ref class Direct3D;
	ref class GraphicsAdapter;
	ref class GraphicsDeviceCapabilities;
	ref class GraphicsDevice;
	typedef GraphicsDevice _GD;
	ref class Texture;
	ref class VertexBuffer;
	ref class IndexBuffer;
	ref class VertexDeclaration;
	ref class Surface;
	ref class VertexShader;
	ref class PixelShader;
	value class VertexElement;
	value class DisplayMode;
	ref class PresentationParameters;

	public enum class SurfaceFormat : int {
		Unknown = D3DFMT_UNKNOWN,

		Alpha8 = D3DFMT_A8,
		Bgr233 = D3DFMT_R3G3B2,
		Bgr24 = D3DFMT_R8G8B8,
		Bgr32 = D3DFMT_X8R8G8B8,
		Bgr444 = D3DFMT_X4R4G4B4,
		Bgr555 = D3DFMT_X1R5G5B5,
		Bgr565 = D3DFMT_R5G6B5,
		Bgra1010102 = D3DFMT_A2R10G10B10,
		Bgra2338 = D3DFMT_A8R3G3B2,
		Bgra5551 = D3DFMT_A1R5G5B5,
		Bgra4444 = D3DFMT_A4R4G4B4,
		Color = D3DFMT_A8R8G8B8,
		Depth15Stencil1 = D3DFMT_D15S1,
		Depth16 = D3DFMT_D16,
		//Depth16Lockable = D3DFMT_D16_LOCKABLE,
		Depth24 = D3DFMT_D24X8,
		Depth24Stencil4 = D3DFMT_D24X4S4,
		Depth24Stencil8 = D3DFMT_D24S8,
		Depth24Stencil8Single = D3DFMT_D24FS8,
		Depth32 = D3DFMT_D32,
		//Depth32SingleLockable = D3DFMT_D32F_LOCKABLE,

		Rgba32 = D3DFMT_A8B8G8R8,
		Rgb32 = D3DFMT_X8B8G8R8,
		Rg32 = D3DFMT_G16R16,
		Rgba64 = D3DFMT_A16B16G16R16,
		Rgba1010102 = D3DFMT_A2B10G10R10,

		PaletteAlpha16 = D3DFMT_A8P8,
		Palette8 = D3DFMT_P8,

		Luminance8 = D3DFMT_L8,
		LuminanceAlpha16 = D3DFMT_A8L8,
		LuminanceAlpha8 = D3DFMT_A4L4,
		Luminance16 = D3DFMT_L16,

		NormalizedByte2 = D3DFMT_V8U8,
		NormalizedByte2Computed = D3DFMT_CxV8U8,
		NormalizedLuminance16 = D3DFMT_L6V5U5,
		NormalizedLuminance32 = D3DFMT_X8L8V8U8,
		NormalizedByte4 = D3DFMT_Q8W8V8U8,
		NormalizedShort2 = D3DFMT_V16U16,
		NormalizedShort4 = D3DFMT_Q16W16V16U16,
		NormalizedAlpha1010102 = D3DFMT_A2W10V10U10,

		VideoYuYv = D3DFMT_UYVY,
		VideoRgBg = D3DFMT_R8G8_B8G8,
		VideoUyVy = D3DFMT_YUY2,
		VideoGrGb = D3DFMT_G8R8_G8B8,
		Dxt1 = D3DFMT_DXT1,
		Dxt2 = D3DFMT_DXT2,
		Dxt3 = D3DFMT_DXT3,
		Dxt4 = D3DFMT_DXT4,
		Dxt5 = D3DFMT_DXT5,

/*		VertexData = D3DFMT_VERTEXDATA,
		Index16 = D3DFMT_INDEX16,
		Index32 = D3DFMT_INDEX32,*/

		Multi2Brga32 = D3DFMT_MULTI2_ARGB8,

		HalfSingle = D3DFMT_R16F,
		HalfVector2 = D3DFMT_G16R16F,
		HalfVector4 = D3DFMT_A16B16G16R16F,

		Single = D3DFMT_R32F,
		Vector2 = D3DFMT_G32R32F,
		Vector4 = D3DFMT_A32B32G32R32F,
	};
	
	public enum class DepthFormat : int {
		Unknown = D3DFMT_UNKNOWN,
		Depth15Stencil1 = D3DFMT_D15S1,
		Depth16 = D3DFMT_D16,
		Depth24 = D3DFMT_D24X8,
		Depth24Stencil4 = D3DFMT_D24X4S4,
		Depth24Stencil8 = D3DFMT_D24S8,
		Depth24Stencil8Single = D3DFMT_D24FS8,
		Depth32 = D3DFMT_D32,
	};

/*
	public enum class SurfaceFormat : unsigned int {
		Unknown = D3DFMT_UNKNOWN,

		R8G8B8 = D3DFMT_R8G8B8,
		A8R8G8B8 = D3DFMT_A8R8G8B8,
		X8R8G8B8 = D3DFMT_X8R8G8B8,
		R5G6B5 = D3DFMT_R5G6B5,
		X1R5G5B5 = D3DFMT_X1R5G5B5,
		A1R5G5B5 = D3DFMT_A1R5G5B5,
		A4R4G4B4 = D3DFMT_A4R4G4B4,
		R3G3B2 = D3DFMT_R3G3B2,
		Alpha8 = D3DFMT_A8,
		A8R3G3B2 = D3DFMT_A8R3G3B2,
		X4R4G4B4 = D3DFMT_X4R4G4B4,
		A2B10G10R10 = D3DFMT_A2B10G10R10,
		A8B8G8R8 = D3DFMT_A8B8G8R8,
		X8B8G8R8 = D3DFMT_X8B8G8R8,
		Green16Red16 = D3DFMT_G16R16,
		A2R10G10B10 = D3DFMT_A2R10G10B10,
		A16B16G16R16 = D3DFMT_A16B16G16R16,

		Alpha8Palette8 = D3DFMT_A8P8,
		Palette8 = D3DFMT_P8,

		Luma8 = D3DFMT_L8,
		Alpha8Luma8 = D3DFMT_A8L8,
		Alpha4Luma4 = D3DFMT_A4L4,

		BumpMap2D = D3DFMT_V8U8,
		LitBumpMap2D655 = D3DFMT_L6V5U5,
		LitBumpMap2D888 = D3DFMT_X8L8V8U8,
		BumpMap4D = D3DFMT_Q8W8V8U8,
		LitBumpMap2D16 = D3DFMT_V16U16,
		AlphaBumpMap3D = D3DFMT_A2W10V10U10,

		UYVY = D3DFMT_UYVY,
		R8G8_B8G8 = D3DFMT_R8G8_B8G8,
		VUY2 = D3DFMT_YUY2,
		G8R8_G8B8 = D3DFMT_G8R8_G8B8,
		DXT1 = D3DFMT_DXT1,
		DXT2 = D3DFMT_DXT2,
		DXT3 = D3DFMT_DXT3,
		DXY4 = D3DFMT_DXT4,
		DXT5 = D3DFMT_DXT5,

		Depth16Lockable = D3DFMT_D16_LOCKABLE,
		Depth32 = D3DFMT_D32,
		Depth15Stencil1 = D3DFMT_D15S1,
		Depth24Stencil8 = D3DFMT_D24S8,
		Depth24X8 = D3DFMT_D24X8,
		Depth24X4Stencil4 = D3DFMT_D24X4S4,
		Depth16 = D3DFMT_D16,

		DepthFloat32Lockable = D3DFMT_D32F_LOCKABLE,
		DepthFloat24Stencil8 = D3DFMT_D24FS8,

		Luminance16 = D3DFMT_L16,

		VertexData = D3DFMT_VERTEXDATA,
		Index16 = D3DFMT_INDEX16,
		Index32 = D3DFMT_INDEX32,

		Q16W16V16U16 = D3DFMT_Q16W16V16U16,

		D3DFMT_MULTI2_ARGB8,

		D3DFMT_R16F        ,
		D3DFMT_G16R16F     ,
		D3DFMT_A16B16G16R16F,

		D3DFMT_R32F,
		D3DFMT_G32R32F,
		D3DFMT_A32B32G32R32F,

		D3DFMT_CxV8U8,
	};
*/

	public enum class DeviceType : unsigned int {
		Hardware = 1,
		Reference = 2,
		Software = 3,
		Null = 4
	};
	
	public enum class MultiSampleType : unsigned int {
		None = D3DMULTISAMPLE_NONE,
		Nonmaskable = D3DMULTISAMPLE_NONMASKABLE,
		Sample2 = D3DMULTISAMPLE_2_SAMPLES,
		Sample3 = D3DMULTISAMPLE_3_SAMPLES,
		Sample4 = D3DMULTISAMPLE_4_SAMPLES,
		Sample5 = D3DMULTISAMPLE_5_SAMPLES,
		Sample6 = D3DMULTISAMPLE_6_SAMPLES,
		Sample7 = D3DMULTISAMPLE_7_SAMPLES,
		Sample8 = D3DMULTISAMPLE_8_SAMPLES,
		Sample9 = D3DMULTISAMPLE_9_SAMPLES,
		Sample10 = D3DMULTISAMPLE_10_SAMPLES,
		Sample11 = D3DMULTISAMPLE_11_SAMPLES,
		Sample12 = D3DMULTISAMPLE_12_SAMPLES,
		Sample13 = D3DMULTISAMPLE_13_SAMPLES,
		Sample14 = D3DMULTISAMPLE_14_SAMPLES,
		Sample15 = D3DMULTISAMPLE_15_SAMPLES,
		Sample16 = D3DMULTISAMPLE_16_SAMPLES,
	};
	
	public enum class TextureFilterType : unsigned int {
		None = D3DTEXF_NONE,
		Point = D3DTEXF_POINT,
		Linear = D3DTEXF_LINEAR,
		Anisotropic = D3DTEXF_ANISOTROPIC,
		PyramidalQuad = D3DTEXF_PYRAMIDALQUAD,
		GaussianQuad = D3DTEXF_GAUSSIANQUAD
	};
	
	[Flags]
	public enum class PresentInterval : int {
		Default = D3DPRESENT_INTERVAL_DEFAULT,
		One = D3DPRESENT_INTERVAL_ONE,
		Two = D3DPRESENT_INTERVAL_TWO,
		Three = D3DPRESENT_INTERVAL_THREE,
		Four = D3DPRESENT_INTERVAL_FOUR,
		Immediate = (int) D3DPRESENT_INTERVAL_IMMEDIATE
	};
	
	[Flags]
	public enum class PresentOptions : int {
		None = 0,
		//LockableBackbuffer = D3DPRESENTFLAG_LOCKABLE_BACKBUFFER,
		DiscardDepthStencil = D3DPRESENTFLAG_DISCARD_DEPTHSTENCIL,
		DeviceClip = D3DPRESENTFLAG_DEVICECLIP,
		Video = D3DPRESENTFLAG_VIDEO
	};
	
	public enum class TargetPlatform : int {
		Unknown = 0,
		Windows = 1,
		Xbox360 = 2
	};
	
	// XNA-1.0-compatible
	[Flags]
	public enum class CompilerOptions : int {
		/// <summary>Hints to the compiler to avoid using flow-control instructions.</summary>
		AvoidFlowControl = D3DXSHADER_AVOID_FLOW_CONTROL,
		Debug = D3DXSHADER_DEBUG,
		ForcePixelShaderSoftwareNoOptimizations = D3DXSHADER_FORCE_PS_SOFTWARE_NOOPT,
		ForceVertexShaderSoftwareNoOptimizations = D3DXSHADER_FORCE_VS_SOFTWARE_NOOPT,
		None = 0,
		NoPreShader = D3DXSHADER_NO_PRESHADER,
		NotCloneable = 2048,
		PackMatrixColumnMajor = D3DXSHADER_PACKMATRIX_COLUMNMAJOR,
		PackMatrixRowMajor = D3DXSHADER_PACKMATRIX_ROWMAJOR,
		PartialPrecision = D3DXSHADER_PARTIALPRECISION,
		PreferFlowControl = D3DXSHADER_PREFER_FLOW_CONTROL,
		SkipOptimization = D3DXSHADER_SKIPOPTIMIZATION,
		SkipValidation = D3DXSHADER_SKIPVALIDATION,
	};
	
	// XNA-1.0-compliant
	public value class CompilerMacro {
	public:
		String ^Name;
		String ^Definition;
	
		// Not XNA-1.0-compliant
		CompilerMacro( String ^name, String ^definition ) {
			if( name == nullptr )
				throw gcnew ArgumentNullException( "name" );
				
			if( definition == nullptr )
				throw gcnew ArgumentNullException( "definition" );
			
			Name = name;
			Definition = definition;
		}
	};
	
	public ref class CompilerIncludeHandler abstract : public IDisposable {
	private:
		!CompilerIncludeHandler() {
		}
		~CompilerIncludeHandler() {
		}
	};
	
/*	public enum class BufferUsage : unsigned int {
		Static = 0,
		Dynamic = D3DUSAGE_DYNAMIC,
		WriteOnly = D3DUSAGE_WRITEONLY,
		SoftwareProcessing = D3DUSAGE_SOFTWAREPROCESSING,
		DoNotClip = D3DUSAGE_DONOTCLIP,
		Points = D3DUSAGE_POINTS,
		Patches = D3DUSAGE_RTPATCHES,
		NPatches = D3DUSAGE_NPATCHES
	};
	
	public enum class ImageUsage : unsigned int {
		Static = 0,
		AutoGenMipMap = D3DUSAGE_AUTOGENMIPMAP,
		DepthStencil = D3DUSAGE_DEPTHSTENCIL,
		DisplacementMap = D3DUSAGE_DMAP,
		Dynamic = D3DUSAGE_DYNAMIC,
		RenderTarget = D3DUSAGE_RENDERTARGET,
		SoftwareProcessing = D3DUSAGE_SOFTWAREPROCESSING
	};*/
	
	// XNA-beta2-compatible
	public enum class ResourceType : int {
		DepthStencilBuffer = D3DRTYPE_SURFACE,
		Texture2D = D3DRTYPE_TEXTURE,
		Texture3D = D3DRTYPE_VOLUMETEXTURE,
		Texture3DVolume = D3DRTYPE_VOLUME,
		TextureCube = D3DRTYPE_CUBETEXTURE,
		VertexBuffer = D3DRTYPE_VERTEXBUFFER,
		IndexBuffer = D3DRTYPE_INDEXBUFFER,
	};
	
	typedef ResourceType _ResType;
	
	[Flags]
	public enum class FVF : unsigned int {
//D3DFVF_RESERVED0,
		None = 0,
		PositionMask = D3DFVF_POSITION_MASK,
		XYZ = D3DFVF_XYZ,
		XYZRhw = D3DFVF_XYZRHW,
		XYZB1 = D3DFVF_XYZB1,
		XYZB2 = D3DFVF_XYZB2,
		XYZB3 = D3DFVF_XYZB3,
		XYZB4 = D3DFVF_XYZB4,
		XYZB5 = D3DFVF_XYZB5,
		XYZW = D3DFVF_XYZW,

		Normal = D3DFVF_NORMAL,
		PointSize = D3DFVF_PSIZE,
		DiffuseColor = D3DFVF_DIFFUSE,
		SpecularColor = D3DFVF_SPECULAR,

		TextureCountMask = D3DFVF_TEXCOUNT_MASK,
		TextureCountShift = D3DFVF_TEXCOUNT_SHIFT,
		Texture0 = D3DFVF_TEX0,
		Texture1 = D3DFVF_TEX1,
		Texture2 = D3DFVF_TEX2,
		Texture3 = D3DFVF_TEX3,
		Texture4 = D3DFVF_TEX4,
		Texture5 = D3DFVF_TEX5,
		Texture6 = D3DFVF_TEX6,
		Texture7 = D3DFVF_TEX7,
		Texture8 = D3DFVF_TEX8,

//D3DFVF_LASTBETA_UBYTE4,
//D3DFVF_LASTBETA_D3DCOLOR,
	};
	
	[Flags]
	public enum class SetDataOptions : int {
		None = 0,
		NoOverwrite = 1,
		Discard = 2
	};

	[Flags]
	public enum class CreateOptions : int {
		None = 0,
		
//		FpuPreserve = D3DCREATE_FPU_PRESERVE,
//		Multithreaded = D3DCREATE_MULTITHREADED,
		SingleThreaded = 0x10000000,

//		PureDevice = D3DCREATE_PUREDEVICE,
		SoftwareVertexProcessing = D3DCREATE_SOFTWARE_VERTEXPROCESSING,
		HardwareVertexProcessing = D3DCREATE_HARDWARE_VERTEXPROCESSING,
		MixedVertexProcessing = D3DCREATE_MIXED_VERTEXPROCESSING,

//		DisableDriverManagement = D3DCREATE_DISABLE_DRIVER_MANAGEMENT,
//		AdapterGroupDevice = D3DCREATE_ADAPTERGROUP_DEVICE,
//		DisableDriverManagementEx = D3DCREATE_DISABLE_DRIVER_MANAGEMENT_EX,
		NoWindowChanges = D3DCREATE_NOWINDOWCHANGES
	};

	public enum class SwapEffect : int {
		None = 0,
		Discard = 1,
		Flip = 2,
		Copy = 3
	};
	
	ref class Utility {
	public:
		static void CheckResult( HRESULT result );
		static bool IsSimpleValueType( Type ^type );
		static int GetBytesPerElement( SurfaceFormat format );
	};
NS_CLOSE
