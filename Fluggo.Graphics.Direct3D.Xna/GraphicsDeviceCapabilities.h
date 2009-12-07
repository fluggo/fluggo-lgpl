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

NS_OPEN
/*	[Flags]
	enum class Caps : unsigned int {
		None = 0u,
		ReadScanline = D3DCAPS_READ_SCANLINE
	};

	[Flags]
	enum class Caps2 : unsigned int {
		None = 0u,
		FullScreenGamma = D3DCAPS2_FULLSCREENGAMMA,
		CanCalibrateGamma = D3DCAPS2_CANCALIBRATEGAMMA,
		CanManageResource = D3DCAPS2_CANMANAGERESOURCE,
		DynamicTextures = D3DCAPS2_DYNAMICTEXTURES,
		CanAutoGenMipMap = D3DCAPS2_CANAUTOGENMIPMAP 
	};

	[Flags]
	enum class Caps3 : unsigned int {
		None = 0u,
		AlphaFullscreenFlipOrDiscard = D3DCAPS3_ALPHA_FULLSCREEN_FLIP_OR_DISCARD,
		LinearToSrgbPresentation = D3DCAPS3_LINEAR_TO_SRGB_PRESENTATION,
		CopyToVideoMemory = D3DCAPS3_COPY_TO_VIDMEM,
		CopyToSystemMemory = D3DCAPS3_COPY_TO_SYSTEMMEM
	};
	
	[Flags]
	enum class DevCaps : unsigned int {
		ExecuteSystemMemory = D3DDEVCAPS_EXECUTESYSTEMMEMORY,
		ExecuteVideoMemory = D3DDEVCAPS_EXECUTEVIDEOMEMORY,
		TLVertexSystemMemory = D3DDEVCAPS_TLVERTEXSYSTEMMEMORY,
		TLVertexVideoMemory = D3DDEVCAPS_TLVERTEXVIDEOMEMORY,
		TextureSystemMemory = D3DDEVCAPS_TEXTURESYSTEMMEMORY,
		TextureVideoMemory = D3DDEVCAPS_TEXTUREVIDEOMEMORY,
		DrawPrimTLVertex = D3DDEVCAPS_DRAWPRIMTLVERTEX,
		CanRenderAfterFlip = D3DDEVCAPS_CANRENDERAFTERFLIP,
		TextureNonLocalVidMem = D3DDEVCAPS_TEXTURENONLOCALVIDMEM,
		DrawPrimitives2 = D3DDEVCAPS_DRAWPRIMITIVES2,
		SeparateTextureMemories = D3DDEVCAPS_SEPARATETEXTUREMEMORIES,
		DrawPrimitives2Ex = D3DDEVCAPS_DRAWPRIMITIVES2EX,
		HWTransformAndLight = D3DDEVCAPS_HWTRANSFORMANDLIGHT,
		CanBltSysToNonLocal = D3DDEVCAPS_CANBLTSYSTONONLOCAL,
		HWRasterization = D3DDEVCAPS_HWRASTERIZATION,
		PureDevice = D3DDEVCAPS_PUREDEVICE,
		QuinticRTPatches = D3DDEVCAPS_QUINTICRTPATCHES,
		RTPatches = D3DDEVCAPS_RTPATCHES,
		RTPatchHandleZero = D3DDEVCAPS_RTPATCHHANDLEZERO,
		NPatches = D3DDEVCAPS_NPATCHES           
	};
	
	[Flags]
	enum class DevCaps2 : unsigned int {
		StreamOffset = D3DDEVCAPS2_STREAMOFFSET,
		DMapNPatch = D3DDEVCAPS2_DMAPNPATCH,
		AdaptiveTessRTPatch = D3DDEVCAPS2_ADAPTIVETESSRTPATCH,
		AdaptiveTessNPatch = D3DDEVCAPS2_ADAPTIVETESSNPATCH,
		CanStretchRectFromTextures = D3DDEVCAPS2_CAN_STRETCHRECT_FROM_TEXTURES,
		PresamplesDMapNPatch = D3DDEVCAPS2_PRESAMPLEDDMAPNPATCH,
		VertexElementScanShareStreamOffset = D3DDEVCAPS2_VERTEXELEMENTSCANSHARESTREAMOFFSET
	};
	
	[Flags]
	enum class CursorCaps : unsigned int {
		None = 0u,
		ColorHighResolution = D3DCURSORCAPS_COLOR, 
		ColorLowResolution = D3DCURSORCAPS_LOWRES
	};
	
	[Flags]
	enum class PrimitiveMiscCaps : unsigned int {
		None = 0u,
		MaskZ = D3DPMISCCAPS_MASKZ,
		CullNone = D3DPMISCCAPS_CULLNONE,
		CullCW = D3DPMISCCAPS_CULLCW,
		CullCCW = D3DPMISCCAPS_CULLCCW,
		ColorWriteEnable = D3DPMISCCAPS_COLORWRITEENABLE,
		ClipPlaneScaledPoints = D3DPMISCCAPS_CLIPPLANESCALEDPOINTS,
		ClipTLVerts = D3DPMISCCAPS_CLIPTLVERTS,
		TSSArgTemp = D3DPMISCCAPS_TSSARGTEMP,
		BlendOp = D3DPMISCCAPS_BLENDOP,
		NullReference = D3DPMISCCAPS_NULLREFERENCE,
		IndependentWriteMasks = D3DPMISCCAPS_INDEPENDENTWRITEMASKS,
		PerStageConstant = D3DPMISCCAPS_PERSTAGECONSTANT,
		FogAndSpecularAlpha = D3DPMISCCAPS_FOGANDSPECULARALPHA,

		SeparateAlphaBlend = D3DPMISCCAPS_SEPARATEALPHABLEND,
		MRTIndependentBitDepths = D3DPMISCCAPS_MRTINDEPENDENTBITDEPTHS,
		MRTPostPixelShaderBlending = D3DPMISCCAPS_MRTPOSTPIXELSHADERBLENDING,
		FogVertexClamped = D3DPMISCCAPS_FOGVERTEXCLAMPED
	};
	
	[Flags]
	enum class RasterCaps : unsigned int {
		None = 0u,
		Dither = D3DPRASTERCAPS_DITHER,
		ZTest = D3DPRASTERCAPS_ZTEST,
		FogVertex = D3DPRASTERCAPS_FOGVERTEX,
		FogTable = D3DPRASTERCAPS_FOGTABLE,
		MipMapLodBias = D3DPRASTERCAPS_MIPMAPLODBIAS,
		ZBufferLessHSR = D3DPRASTERCAPS_ZBUFFERLESSHSR,
		FogRange = D3DPRASTERCAPS_FOGRANGE,
		Anisotropy = D3DPRASTERCAPS_ANISOTROPY,
		WBuffer = D3DPRASTERCAPS_WBUFFER,
		WFog = D3DPRASTERCAPS_WFOG,
		ZFog = D3DPRASTERCAPS_ZFOG,
		ColorPerspective = D3DPRASTERCAPS_COLORPERSPECTIVE,
		ScissorTest = D3DPRASTERCAPS_SCISSORTEST,
		SlopeScaleDepthBias = D3DPRASTERCAPS_SLOPESCALEDEPTHBIAS,
		DepthBias = D3DPRASTERCAPS_DEPTHBIAS,
		MultisampleToggle = D3DPRASTERCAPS_MULTISAMPLE_TOGGLE 
	};
	
	[Flags]
	enum class CmpCaps : unsigned int {
		None = 0u,
		Never = D3DPCMPCAPS_NEVER,
		Less = D3DPCMPCAPS_LESS,
		Equal = D3DPCMPCAPS_EQUAL,
		LessEqual = D3DPCMPCAPS_LESSEQUAL,
		Greater = D3DPCMPCAPS_GREATER,
		NotEqual = D3DPCMPCAPS_NOTEQUAL,
		GreaterEqual = D3DPCMPCAPS_GREATEREQUAL,
		Always = D3DPCMPCAPS_ALWAYS
	};
	
	[Flags]
	enum class BlendCaps : unsigned int {
		None = 0u,
		Zero = D3DPBLENDCAPS_ZERO,
		One = D3DPBLENDCAPS_ONE,
		SrcColor = D3DPBLENDCAPS_SRCCOLOR,
		InvSrcColor = D3DPBLENDCAPS_INVSRCCOLOR,
		SrcAlpha = D3DPBLENDCAPS_SRCALPHA,
		InvSrcAlpha = D3DPBLENDCAPS_INVSRCALPHA,
		DestAlpha = D3DPBLENDCAPS_DESTALPHA,
		InvDestAlpha = D3DPBLENDCAPS_INVDESTALPHA,
		DestColor = D3DPBLENDCAPS_DESTCOLOR,
		InvDestColor = D3DPBLENDCAPS_INVDESTCOLOR,
		SrcAlphaSat  =D3DPBLENDCAPS_SRCALPHASAT,
		BothSrcAlpha = D3DPBLENDCAPS_BOTHSRCALPHA,
		BothInvSrcAlpha = D3DPBLENDCAPS_BOTHINVSRCALPHA,
		BlendFactor = D3DPBLENDCAPS_BLENDFACTOR
	};
	
	[Flags]
	enum class ShadeCaps : unsigned int {
		None = 0u,
		ColorGouraudRGB = D3DPSHADECAPS_COLORGOURAUDRGB,
		SpecularGouraudRGB = D3DPSHADECAPS_SPECULARGOURAUDRGB,
		AlphaGouraudBlend = D3DPSHADECAPS_ALPHAGOURAUDBLEND,
		FogGouraud = D3DPSHADECAPS_FOGGOURAUD        
	};
	
	[Flags]
	enum class TextureCaps : unsigned int {
		None = 0u,
		Perspective = D3DPTEXTURECAPS_PERSPECTIVE,
		Pow2 = D3DPTEXTURECAPS_POW2,
		Alpha = D3DPTEXTURECAPS_ALPHA,
		SquareOnly = D3DPTEXTURECAPS_SQUAREONLY,
		TexRepeatNotScaledBySize = D3DPTEXTURECAPS_TEXREPEATNOTSCALEDBYSIZE,
		AlphaPalette = D3DPTEXTURECAPS_ALPHAPALETTE,
		NonPow2Conditional = D3DPTEXTURECAPS_NONPOW2CONDITIONAL,
		Projected = D3DPTEXTURECAPS_PROJECTED,
		CubeMap = D3DPTEXTURECAPS_CUBEMAP,
		VolumeMap = D3DPTEXTURECAPS_VOLUMEMAP,
		MipMap = D3DPTEXTURECAPS_MIPMAP,
		MipVolumeMap = D3DPTEXTURECAPS_MIPVOLUMEMAP,
		MipCubeMap = D3DPTEXTURECAPS_MIPCUBEMAP,
		CubeMapPow2 = D3DPTEXTURECAPS_CUBEMAP_POW2,
		VolumeMapPow2 = D3DPTEXTURECAPS_VOLUMEMAP_POW2,
		NoProjectedBumpEnv = D3DPTEXTURECAPS_NOPROJECTEDBUMPENV
	};
	
	[Flags]
	enum class TextureFilterCaps : unsigned int {
		MinFPoint = D3DPTFILTERCAPS_MINFPOINT,
		MinFLinear = D3DPTFILTERCAPS_MINFLINEAR,
		MinFAnisotropic = D3DPTFILTERCAPS_MINFANISOTROPIC,
		MinFPyramidalQuad = D3DPTFILTERCAPS_MINFPYRAMIDALQUAD,
		MinFGaussianQuad = D3DPTFILTERCAPS_MINFGAUSSIANQUAD,
		MipFPoint = D3DPTFILTERCAPS_MIPFPOINT,
		MipFLinear = D3DPTFILTERCAPS_MIPFLINEAR,
		MagFPoint = D3DPTFILTERCAPS_MAGFPOINT,
		MagFLinear = D3DPTFILTERCAPS_MAGFLINEAR,
		MagFAnisotropic = D3DPTFILTERCAPS_MAGFANISOTROPIC,
		MagFPyramidalQuad = D3DPTFILTERCAPS_MAGFPYRAMIDALQUAD,
		MagFGaussianQuad = D3DPTFILTERCAPS_MAGFGAUSSIANQUAD 
	};
	
	[Flags]
	enum class TextureAddressCaps : unsigned int {
		Wrap = D3DPTADDRESSCAPS_WRAP,
		Mirror = D3DPTADDRESSCAPS_MIRROR,
		Clamp = D3DPTADDRESSCAPS_CLAMP,
		Border = D3DPTADDRESSCAPS_BORDER,
		IndependentUV = D3DPTADDRESSCAPS_INDEPENDENTUV,
		MirrorOnce = D3DPTADDRESSCAPS_MIRRORONCE   
	};
	
	[Flags]
	enum class StencilCaps : unsigned int {
		Keep = D3DSTENCILCAPS_KEEP,
		Zero = D3DSTENCILCAPS_ZERO,
		Replace = D3DSTENCILCAPS_REPLACE,
		IncrSat = D3DSTENCILCAPS_INCRSAT,
		DecrSat = D3DSTENCILCAPS_DECRSAT,
		Invert = D3DSTENCILCAPS_INVERT,
		Incr = D3DSTENCILCAPS_INCR,
		Decr = D3DSTENCILCAPS_DECR,
		TwoSided = D3DSTENCILCAPS_TWOSIDED
	};
	
	[Flags]
	enum class TextureOpCaps : unsigned int {
		Disable = D3DTEXOPCAPS_DISABLE,
		SelectArg1 = D3DTEXOPCAPS_SELECTARG1,
		SelectArg2 = D3DTEXOPCAPS_SELECTARG2,
		Modulate = D3DTEXOPCAPS_MODULATE,
		Modulate2X = D3DTEXOPCAPS_MODULATE2X,
		Modulate4X = D3DTEXOPCAPS_MODULATE4X,
		Add = D3DTEXOPCAPS_ADD,
		AddSigned = D3DTEXOPCAPS_ADDSIGNED,
		AddSigned2X = D3DTEXOPCAPS_ADDSIGNED2X,
		Subtract = D3DTEXOPCAPS_SUBTRACT,
		AddSmooth = D3DTEXOPCAPS_ADDSMOOTH,
		BlendDiffuseAlpha = D3DTEXOPCAPS_BLENDDIFFUSEALPHA,
		BlendTextureAlpha = D3DTEXOPCAPS_BLENDTEXTUREALPHA,
		BlendFactorAlpha = D3DTEXOPCAPS_BLENDFACTORALPHA,
		BlendTextureAlphaPM = D3DTEXOPCAPS_BLENDTEXTUREALPHAPM,
		BlendCurrentAlpha = D3DTEXOPCAPS_BLENDCURRENTALPHA,
		Premodulate = D3DTEXOPCAPS_PREMODULATE,
		ModulateAlphaAddColor = D3DTEXOPCAPS_MODULATEALPHA_ADDCOLOR,
		ModulateColorAddAlpha = D3DTEXOPCAPS_MODULATECOLOR_ADDALPHA,
		AddColor = D3DTEXOPCAPS_MODULATEINVALPHA_ADDCOLOR,
		AddAlpha = D3DTEXOPCAPS_MODULATEINVCOLOR_ADDALPHA,
		BumpEnvMap = D3DTEXOPCAPS_BUMPENVMAP,
		BumpEnvMapLuminance = D3DTEXOPCAPS_BUMPENVMAPLUMINANCE,
		DotProduct3 = D3DTEXOPCAPS_DOTPRODUCT3,
		MultiplyAdd = D3DTEXOPCAPS_MULTIPLYADD,
		Lerp = D3DTEXOPCAPS_LERP                     
	};
	
	[Flags]
	enum class FvfCaps : unsigned int {
		TexCoordCountMask = D3DFVFCAPS_TEXCOORDCOUNTMASK,
		DoNotStripElements = D3DFVFCAPS_DONOTSTRIPELEMENTS,
		PSize = D3DFVFCAPS_PSIZE             
	};
	
	[Flags]
	enum class VertexProcessingCaps : unsigned int {
		TexGen = D3DVTXPCAPS_TEXGEN,
		MaterialSource7 = D3DVTXPCAPS_MATERIALSOURCE7,
		DirectionalLights = D3DVTXPCAPS_DIRECTIONALLIGHTS,
		PositionalLights = D3DVTXPCAPS_POSITIONALLIGHTS,
		LocalViewer = D3DVTXPCAPS_LOCALVIEWER,
		Tweening = D3DVTXPCAPS_TWEENING,
		SphereMap = D3DVTXPCAPS_TEXGEN_SPHEREMAP,
		TexGenNonLocalViewer = D3DVTXPCAPS_NO_TEXGEN_NONLOCALVIEWER
	};
	
	[Flags]
	enum class DeclTypeCaps : unsigned int {
		UByte4 = D3DDTCAPS_UBYTE4,
		UByte4N = D3DDTCAPS_UBYTE4N,
		Short2N = D3DDTCAPS_SHORT2N,
		Short4N = D3DDTCAPS_SHORT4N,
		UShort2N = D3DDTCAPS_USHORT2N,
		UShort4N = D3DDTCAPS_USHORT4N,
		UDec3 = D3DDTCAPS_UDEC3,
		Dec3N = D3DDTCAPS_DEC3N,
		Float16_2 = D3DDTCAPS_FLOAT16_2,
		Float16_4 = D3DDTCAPS_FLOAT16_4
	};

	[Flags]
	enum class LineCaps : unsigned int {
		Texture = D3DLINECAPS_TEXTURE,
		ZTest = D3DLINECAPS_ZTEST,
		Blend = D3DLINECAPS_BLEND,
		AlphaCmp = D3DLINECAPS_ALPHACMP,
		Fog = D3DLINECAPS_FOG,
		Antialias = D3DLINECAPS_ANTIALIAS
	};

	BEGIN_DECLARE_THUNK_STRUCT(D3DCAPS9,DeviceCaps)
	public:
		THUNK_PROPERTY_GET_CAST(D3DCAPS9,D3DDEVTYPE,DeviceType,Fluggo::Graphics::Direct3D::DeviceType)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,AdapterOrdinal)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,Caps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,Caps2)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,Caps3)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,PresentationIntervals)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,CursorCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,DevCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,PrimitiveMiscCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,RasterCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,ZCmpCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,SrcBlendCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,DestBlendCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,AlphaCmpCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,ShadeCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,TextureCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,TextureFilterCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,CubeTextureFilterCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,VolumeTextureFilterCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,TextureAddressCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,VolumeTextureAddressCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,LineCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxTextureWidth)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxTextureHeight)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxVolumeExtent)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxTextureRepeat)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxTextureAspectRatio)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxAnisotropy)
		THUNK_PROPERTY_GET(D3DCAPS9,float,MaxVertexW)
		THUNK_PROPERTY_GET(D3DCAPS9,float,GuardBandLeft)
		THUNK_PROPERTY_GET(D3DCAPS9,float,GuardBandTop)
		THUNK_PROPERTY_GET(D3DCAPS9,float,GuardBandRight)
		THUNK_PROPERTY_GET(D3DCAPS9,float,GuardBandBottom)
		THUNK_PROPERTY_GET(D3DCAPS9,float,ExtentsAdjust)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,StencilCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,FVFCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,TextureOpCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxTextureBlendStages)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxSimultaneousTextures)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,VertexProcessingCaps)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxActiveLights)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxUserClipPlanes)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxVertexBlendMatrices)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxVertexBlendMatrixIndex)
		THUNK_PROPERTY_GET(D3DCAPS9,float,MaxPointSize)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxPrimitiveCount)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxVertexIndex)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxStreams)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxStreamStride)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,VertexShaderVersion)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MaxVertexShaderConst)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,PixelShaderVersion)
		THUNK_PROPERTY_GET(D3DCAPS9,float,PixelShader1xMaxValue)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,DevCaps2)
		THUNK_PROPERTY_GET(D3DCAPS9,float,MaxNpatchTessellationLevel)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,MasterAdapterOrdinal)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,AdapterOrdinalInGroup)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,NumberOfAdaptersInGroup)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,DeclTypes)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,NumSimultaneousRTs)
		THUNK_PROPERTY_GET(D3DCAPS9,unsigned int,StretchRectFilterCaps)
	END_DECLARE_THUNK_STRUCT()
*/

#define CAP_VFLAG(v,n,f) \
	property bool n { \
		bool get() { \
			return (v & f) == f; \
		} \
	}
#define CAP_FLAG(n,f) CAP_VFLAG(_value,n,f)
#define SUPPORT_STRING(n) \
	if( Supports##n ) { \
		list->Add( #n ); \
	}
#define CAN_STRING(n) \
	if( n ) { \
		list->Add( #n ); \
	}
#define PROP_FLAGS(nn,n,t) \
	property t n { \
		t get() { \
			return t( _caps.nn ); \
		} \
	}

	public ref class GraphicsDeviceCapabilities {
	public:
		value class CompareCapabilities {
		private:
			unsigned int _value;
			
		internal:
			CompareCapabilities( unsigned int value ) {
				_value = value;
			}
			
		public:
			CAP_FLAG(SupportsNever,D3DPCMPCAPS_NEVER)
			CAP_FLAG(SupportsLess,D3DPCMPCAPS_LESS)
			CAP_FLAG(SupportsEqual,D3DPCMPCAPS_EQUAL)
			CAP_FLAG(SupportsLessEqual,D3DPCMPCAPS_LESSEQUAL)
			CAP_FLAG(SupportsGreater,D3DPCMPCAPS_GREATER)
			CAP_FLAG(SupportsNotEqual,D3DPCMPCAPS_NOTEQUAL)
			CAP_FLAG(SupportsGreaterEqual,D3DPCMPCAPS_GREATEREQUAL)
			CAP_FLAG(SupportsAlways,D3DPCMPCAPS_ALWAYS)
			
			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( Never )
				SUPPORT_STRING( Less )
				SUPPORT_STRING( Equal )
				SUPPORT_STRING( LessEqual )
				SUPPORT_STRING( Greater )
				SUPPORT_STRING( NotEqual )
				SUPPORT_STRING( GreaterEqual )
				SUPPORT_STRING( Always )
				
				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class DriverCapabilities {
		private:
			unsigned int _caps, _caps2, _caps3;
				
		internal:
			DriverCapabilities( unsigned int caps, unsigned int caps2, unsigned int caps3 ) {
				_caps = caps;
				_caps2 = caps2;
				_caps3 = caps3;
			}
			
		public:
			CAP_VFLAG(_caps2,CanAutoGenerateMipMap,D3DCAPS2_CANAUTOGENMIPMAP)
			CAP_VFLAG(_caps2,CanCalibrateGamma,D3DCAPS2_CANCALIBRATEGAMMA)
			CAP_VFLAG(_caps2,CanManageResource,D3DCAPS2_CANMANAGERESOURCE)
			CAP_VFLAG(_caps, ReadScanLine,D3DCAPS_READ_SCANLINE)
			CAP_VFLAG(_caps3,SupportsAlphaFullScreenFlipOrDiscard,D3DCAPS3_ALPHA_FULLSCREEN_FLIP_OR_DISCARD)
			CAP_VFLAG(_caps3,SupportsCopyToSystemMemory,D3DCAPS3_COPY_TO_SYSTEMMEM)
			CAP_VFLAG(_caps3,SupportsCopyToVideoMemory,D3DCAPS3_COPY_TO_VIDMEM)
			CAP_VFLAG(_caps2,SupportsDynamicTextures,D3DCAPS2_DYNAMICTEXTURES)
			CAP_VFLAG(_caps2,SupportsFullScreenGamma,D3DCAPS2_FULLSCREENGAMMA)
			CAP_VFLAG(_caps3,SupportsLinearToSrgbPresentation,D3DCAPS3_LINEAR_TO_SRGB_PRESENTATION)
			
			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				CAN_STRING(CanAutoGenerateMipMap)
				CAN_STRING(CanCalibrateGamma)
				CAN_STRING(CanManageResource)
				CAN_STRING(ReadScanLine)
				SUPPORT_STRING( AlphaFullScreenFlipOrDiscard )
				SUPPORT_STRING( CopyToSystemMemory )
				SUPPORT_STRING( CopyToVideoMemory )
				SUPPORT_STRING( DynamicTextures )
				SUPPORT_STRING( FullScreenGamma )
				SUPPORT_STRING( LinearToSrgbPresentation )
				
				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class FilterCapabilities {
		private:
			unsigned int _value;
			
		internal:
			FilterCapabilities( unsigned int value ) {
				_value = value;
			}
			
		public:
			CAP_FLAG(SupportsMinifyPoint,D3DPTFILTERCAPS_MINFPOINT)
			CAP_FLAG(SupportsMinifyLinear,D3DPTFILTERCAPS_MINFLINEAR)
			CAP_FLAG(SupportsMinifyAnisotropic,D3DPTFILTERCAPS_MINFANISOTROPIC)
			CAP_FLAG(SupportsMinifyPyramidalQuad,D3DPTFILTERCAPS_MINFPYRAMIDALQUAD)
			CAP_FLAG(SupportsMinifyGaussianQuad,D3DPTFILTERCAPS_MINFGAUSSIANQUAD)
			CAP_FLAG(SupportsMipMapPoint,D3DPTFILTERCAPS_MIPFPOINT)
			CAP_FLAG(SupportsMipMapLinear,D3DPTFILTERCAPS_MIPFLINEAR)
			CAP_FLAG(SupportsMagnifyPoint,D3DPTFILTERCAPS_MAGFPOINT)
			CAP_FLAG(SupportsMagnifyLinear,D3DPTFILTERCAPS_MAGFLINEAR)
			CAP_FLAG(SupportsMagnifyAnisotropic,D3DPTFILTERCAPS_MAGFANISOTROPIC)
			CAP_FLAG(SupportsMagnifyPyramidalQuad,D3DPTFILTERCAPS_MAGFPYRAMIDALQUAD)
			CAP_FLAG(SupportsMagnifyGaussianQuad,D3DPTFILTERCAPS_MAGFGAUSSIANQUAD)
			
			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( MinifyPoint )
				SUPPORT_STRING( MinifyLinear )
				SUPPORT_STRING( MinifyAnisotropic )
				SUPPORT_STRING( MinifyPyramidalQuad )
				SUPPORT_STRING( MinifyGaussianQuad )
				SUPPORT_STRING( MipMapPoint )
				SUPPORT_STRING( MipMapLinear )
				SUPPORT_STRING( MagnifyPoint )
				SUPPORT_STRING( MagnifyLinear )
				SUPPORT_STRING( MagnifyAnisotropic )
				SUPPORT_STRING( MagnifyPyramidalQuad )
				SUPPORT_STRING( MagnifyGaussianQuad )
				
				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class CursorCapabilities {
		private:
			unsigned int _value;
			
		internal:
			CursorCapabilities( unsigned int value ) {
				_value = value;
			}
			
		public:
			CAP_FLAG(SupportsColor,D3DCURSORCAPS_COLOR)
			CAP_FLAG(SupportsLowResolution,D3DCURSORCAPS_LOWRES)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( Color )
				SUPPORT_STRING( LowResolution )
				
				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class DeclarationTypeCapabilities {
		private:
			unsigned int _value;
			
		internal:
			DeclarationTypeCapabilities( unsigned int value ) {
				_value = value;
			}
			
		public:
			CAP_FLAG(SupportsByte4,D3DDTCAPS_UBYTE4)
			CAP_FLAG(SupportsRgba32,D3DDTCAPS_UBYTE4N)
			CAP_FLAG(SupportsNormalizedShort2,D3DDTCAPS_SHORT2N)
			CAP_FLAG(SupportsNormalizedShort4,D3DDTCAPS_SHORT4N)
			CAP_FLAG(SupportsRg32,D3DDTCAPS_USHORT2N)
			CAP_FLAG(SupportsRgba64,D3DDTCAPS_USHORT4N)
			CAP_FLAG(SupportsUInt101010,D3DDTCAPS_UDEC3)
			CAP_FLAG(SupportsNormalized101010,D3DDTCAPS_DEC3N)
			CAP_FLAG(SupportsHalfVector2,D3DDTCAPS_FLOAT16_2)
			CAP_FLAG(SupportsHalfVector4,D3DDTCAPS_FLOAT16_4)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( Byte4 )
				SUPPORT_STRING( Rgba32 )
				SUPPORT_STRING( NormalizedShort2 )
				SUPPORT_STRING( NormalizedShort4 )
				SUPPORT_STRING( Rg32 )
				SUPPORT_STRING( Rgba64 )
				SUPPORT_STRING( UInt101010 )
				SUPPORT_STRING( Normalized101010 )
				SUPPORT_STRING( HalfVector2 )
				SUPPORT_STRING( HalfVector4 )
				
				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class DeviceCapabilitiesW {
		private:
			unsigned int _caps, _caps2;
			
		internal:
			DeviceCapabilitiesW( unsigned int caps, unsigned int caps2 ) {
				_caps = caps;
				_caps2 = caps2;
			}
			
		public:
			CAP_VFLAG(_caps,SupportsExecuteSystemMemory,D3DDEVCAPS_EXECUTESYSTEMMEMORY)
			CAP_VFLAG(_caps,SupportsExecuteVideoMemory,D3DDEVCAPS_EXECUTEVIDEOMEMORY)
			CAP_VFLAG(_caps,SupportsTransformedVertexSystemMemory,D3DDEVCAPS_TLVERTEXSYSTEMMEMORY)
			CAP_VFLAG(_caps,SupportsTransformedVertexVideoMemory,D3DDEVCAPS_TLVERTEXVIDEOMEMORY)
			CAP_VFLAG(_caps,SupportsTextureSystemMemory,D3DDEVCAPS_TEXTURESYSTEMMEMORY)
			CAP_VFLAG(_caps,SupportsTextureVideoMemory,D3DDEVCAPS_TEXTUREVIDEOMEMORY)
			CAP_VFLAG(_caps,SupportsDrawPrimitivesTransformedVertex,D3DDEVCAPS_DRAWPRIMTLVERTEX)
			CAP_VFLAG(_caps,CanRenderAfterFlip,D3DDEVCAPS_CANRENDERAFTERFLIP)
			CAP_VFLAG(_caps,SupportsTextureNonLocalVideoMemory,D3DDEVCAPS_TEXTURENONLOCALVIDMEM)
			CAP_VFLAG(_caps,SupportsDrawPrimitives2,D3DDEVCAPS_DRAWPRIMITIVES2)
			CAP_VFLAG(_caps,SupportsSeparateTextureMemories,D3DDEVCAPS_SEPARATETEXTUREMEMORIES)
			CAP_VFLAG(_caps,SupportsDrawPrimitives2Ex,D3DDEVCAPS_DRAWPRIMITIVES2EX)
			CAP_VFLAG(_caps,SupportsHardwareTransformAndLight,D3DDEVCAPS_HWTRANSFORMANDLIGHT)
			CAP_VFLAG(_caps,CanDrawSystemToNonLocal,D3DDEVCAPS_CANBLTSYSTONONLOCAL)
			CAP_VFLAG(_caps,SupportsHardwareRasterization,D3DDEVCAPS_HWRASTERIZATION)
			//CAP_VFLAG(_caps,SupportsPureDevice = D3DDEVCAPS_PUREDEVICE,
			//CAP_VFLAG(_caps,SupportsQuinticRTPatches = D3DDEVCAPS_QUINTICRTPATCHES,
			//CAP_VFLAG(_caps,SupportsRTPatches = D3DDEVCAPS_RTPATCHES,
			//CAP_VFLAG(_caps,SupportsRTPatchHandleZero = D3DDEVCAPS_RTPATCHHANDLEZERO,
			//CAP_VFLAG(_caps,SupportsNPatches = D3DDEVCAPS_NPATCHES           
			CAP_VFLAG(_caps2,SupportsStreamOffset,D3DDEVCAPS2_STREAMOFFSET)
			//CAP_VFLAG(_caps2,SupportsDMapNPatch = D3DDEVCAPS2_DMAPNPATCH,
			//CAP_VFLAG(_caps2,SupportsAdaptiveTessRTPatch = D3DDEVCAPS2_ADAPTIVETESSRTPATCH,
			//CAP_VFLAG(_caps2,SupportsAdaptiveTessNPatch = D3DDEVCAPS2_ADAPTIVETESSNPATCH,
			CAP_VFLAG(_caps2,CanStretchRectangleFromTextures,D3DDEVCAPS2_CAN_STRETCHRECT_FROM_TEXTURES)
			//CAP_VFLAG(_caps2,SupportsPresamplesDMapNPatch = D3DDEVCAPS2_PRESAMPLEDDMAPNPATCH,
			CAP_VFLAG(_caps2,SupportsVertexElementsCanShareStreamOffset,D3DDEVCAPS2_VERTEXELEMENTSCANSHARESTREAMOFFSET)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( ExecuteSystemMemory )
				SUPPORT_STRING( ExecuteVideoMemory )
				SUPPORT_STRING( TransformedVertexSystemMemory )
				SUPPORT_STRING( TransformedVertexVideoMemory )
				SUPPORT_STRING( TextureSystemMemory )
				SUPPORT_STRING( TextureVideoMemory )
				SUPPORT_STRING( DrawPrimitivesTransformedVertex )
				CAN_STRING( CanRenderAfterFlip )
				SUPPORT_STRING( TextureNonLocalVideoMemory )
				SUPPORT_STRING( DrawPrimitives2 )
				SUPPORT_STRING( SeparateTextureMemories )
				SUPPORT_STRING( DrawPrimitives2Ex )
				SUPPORT_STRING( HardwareTransformAndLight )
				CAN_STRING( CanDrawSystemToNonLocal )
				SUPPORT_STRING( HardwareRasterization )
				SUPPORT_STRING( StreamOffset )
				CAN_STRING( CanStretchRectangleFromTextures )
				SUPPORT_STRING( VertexElementsCanShareStreamOffset )
				
				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class BlendCapabilities {
		private:
			unsigned int _value;
			
		internal:
			BlendCapabilities( unsigned int value ) {
				_value = value;
			}
			
		public:
			CAP_FLAG(SupportsZero,D3DPBLENDCAPS_ZERO)
			CAP_FLAG(SupportsOne,D3DPBLENDCAPS_ONE)
			CAP_FLAG(SupportsSourceColor,D3DPBLENDCAPS_SRCCOLOR)
			CAP_FLAG(SupportsInverseSourceColor,D3DPBLENDCAPS_INVSRCCOLOR)
			CAP_FLAG(SupportsSourceAlpha,D3DPBLENDCAPS_SRCALPHA)
			CAP_FLAG(SupportsInverseSourceAlpha,D3DPBLENDCAPS_INVSRCALPHA)
			CAP_FLAG(SupportsDestinationAlpha,D3DPBLENDCAPS_DESTALPHA)
			CAP_FLAG(SupportsInverseDestinationAlpha,D3DPBLENDCAPS_INVDESTALPHA)
			CAP_FLAG(SupportsDestinationColor,D3DPBLENDCAPS_DESTCOLOR)
			CAP_FLAG(SupportsInverseDestinationColor,D3DPBLENDCAPS_INVDESTCOLOR)
			CAP_FLAG(SupportsSourceAlphaSat,D3DPBLENDCAPS_SRCALPHASAT)
			CAP_FLAG(SupportsBothSourceAlpha,D3DPBLENDCAPS_BOTHSRCALPHA)
			CAP_FLAG(SupportsBothInverseSourceAlpha,D3DPBLENDCAPS_BOTHINVSRCALPHA)
			CAP_FLAG(SupportsBlendFactor,D3DPBLENDCAPS_BLENDFACTOR)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( Zero )
				SUPPORT_STRING( One )
				SUPPORT_STRING( SourceColor )
				SUPPORT_STRING( InverseSourceColor )
				SUPPORT_STRING( SourceAlpha )
				SUPPORT_STRING( InverseSourceAlpha )
				SUPPORT_STRING( DestinationAlpha )
				SUPPORT_STRING( InverseDestinationAlpha )
				SUPPORT_STRING( DestinationColor )
				SUPPORT_STRING( InverseDestinationColor )
				SUPPORT_STRING( SourceAlphaSat )
				SUPPORT_STRING( BothSourceAlpha )
				SUPPORT_STRING( BothInverseSourceAlpha )
				SUPPORT_STRING( BlendFactor )
				
				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class LineCapabilities {
		private:
			unsigned int _value;
			
		internal:
			LineCapabilities( unsigned int value ) : _value( value ) {
			}
			
		public:
			CAP_FLAG(SupportsTextureMapping,D3DLINECAPS_TEXTURE)
			CAP_FLAG(SupportsDepthBufferTest,D3DLINECAPS_ZTEST)
			CAP_FLAG(SupportsBlend,D3DLINECAPS_BLEND)
			CAP_FLAG(SupportsAlphaCompare,D3DLINECAPS_ALPHACMP)
			CAP_FLAG(SupportsFog,D3DLINECAPS_FOG)
			CAP_FLAG(SupportsAntiAlias,D3DLINECAPS_ANTIALIAS)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( TextureMapping )
				SUPPORT_STRING( DepthBufferTest )
				SUPPORT_STRING( Blend )
				SUPPORT_STRING( AlphaCompare )
				SUPPORT_STRING( Fog )
				SUPPORT_STRING( AntiAlias )
				
				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class StencilCapabilities {
		private:
			unsigned int _value;
			
		internal:
			StencilCapabilities( unsigned int value ) : _value( value ) {
			}
			
		public:
			CAP_FLAG(SupportsKeep,D3DSTENCILCAPS_KEEP)
			CAP_FLAG(SupportsZero,D3DSTENCILCAPS_ZERO)
			CAP_FLAG(SupportsReplace,D3DSTENCILCAPS_REPLACE)
			CAP_FLAG(SupportsIncrementSaturation,D3DSTENCILCAPS_INCRSAT)
			CAP_FLAG(SupportsDecrementSaturation,D3DSTENCILCAPS_DECRSAT)
			CAP_FLAG(SupportsInvert,D3DSTENCILCAPS_INVERT)
			CAP_FLAG(SupportsIncrement,D3DSTENCILCAPS_INCR)
			CAP_FLAG(SupportsDecrement,D3DSTENCILCAPS_DECR)
			CAP_FLAG(SupportsTwoSided,D3DSTENCILCAPS_TWOSIDED)
			
			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( Keep )
				SUPPORT_STRING( Zero )
				SUPPORT_STRING( Replace )
				SUPPORT_STRING( IncrementSaturation )
				SUPPORT_STRING( DecrementSaturation )
				SUPPORT_STRING( Invert )
				SUPPORT_STRING( Increment )
				SUPPORT_STRING( Decrement )
				SUPPORT_STRING( TwoSided )

				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class VertexProcessingCapabilities {
		private:
			unsigned int _value;
			
		internal:
			VertexProcessingCapabilities( unsigned int value ) : _value( value ) {
			}
			
		public:
			CAP_FLAG(SupportsTextureGeneration,D3DVTXPCAPS_TEXGEN)
			//CAP_FLAG(SupportsMaterialSource7,D3DVTXPCAPS_MATERIALSOURCE7)
			//CAP_FLAG(SupportsDirectionalLights,D3DVTXPCAPS_DIRECTIONALLIGHTS)
			//CAP_FLAG(SupportsPositionalLights,D3DVTXPCAPS_POSITIONALLIGHTS)
			CAP_FLAG(SupportsLocalViewer,D3DVTXPCAPS_LOCALVIEWER)
			//CAP_FLAG(SupportsTweening,D3DVTXPCAPS_TWEENING)
			CAP_FLAG(SupportsTextureGenerationSphereMap,D3DVTXPCAPS_TEXGEN_SPHEREMAP)
			CAP_FLAG(SupportsNoTextureGenerationNonLocalViewer,D3DVTXPCAPS_NO_TEXGEN_NONLOCALVIEWER)
			
			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( TextureGeneration )
				SUPPORT_STRING( LocalViewer )
				SUPPORT_STRING( TextureGenerationSphereMap )
				SUPPORT_STRING( NoTextureGenerationNonLocalViewer )

				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class PrimitiveCapabilities {
		private:
			unsigned int _value;
			
		internal:
			PrimitiveCapabilities( unsigned int value ) : _value( value ) {
			}
			
		public:
			CAP_FLAG(HasFogVertexClamped,D3DPMISCCAPS_FOGVERTEXCLAMPED)
			CAP_FLAG(IsNullReference,D3DPMISCCAPS_NULLREFERENCE)
			CAP_FLAG(SupportsBlendOperation,D3DPMISCCAPS_BLENDOP)
			CAP_FLAG(SupportsClipPlaneScaledPoints,D3DPMISCCAPS_CLIPPLANESCALEDPOINTS)
			CAP_FLAG(SupportsClipTransformedVertices,D3DPMISCCAPS_CLIPTLVERTS)
			CAP_FLAG(SupportsColorWrite,D3DPMISCCAPS_COLORWRITEENABLE)
			CAP_FLAG(SupportsCullClockwiseFace,D3DPMISCCAPS_CULLCW)
			CAP_FLAG(SupportsCullCounterClockwiseFace,D3DPMISCCAPS_CULLCCW)
			CAP_FLAG(SupportsCullNone,D3DPMISCCAPS_CULLNONE)
			CAP_FLAG(SupportsFogAndSpecularAlpha,D3DPMISCCAPS_FOGANDSPECULARALPHA)
			CAP_FLAG(SupportsIndependentWriteMasks,D3DPMISCCAPS_INDEPENDENTWRITEMASKS)
			CAP_FLAG(SupportsMaskZ,D3DPMISCCAPS_MASKZ)
			CAP_FLAG(SupportsMultipleRenderTargetsIndependentBitDepths,D3DPMISCCAPS_MRTINDEPENDENTBITDEPTHS)
			CAP_FLAG(SupportsMultipleRenderTargetsPostPixelShaderBlending,D3DPMISCCAPS_MRTPOSTPIXELSHADERBLENDING)
			CAP_FLAG(SupportsPerStageConstant,D3DPMISCCAPS_PERSTAGECONSTANT)
			CAP_FLAG(SupportsSeparateAlphaBlend,D3DPMISCCAPS_SEPARATEALPHABLEND)
			CAP_FLAG(SupportsTextureStageStateArgumentTemp,D3DPMISCCAPS_TSSARGTEMP)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				CAN_STRING( HasFogVertexClamped )
				CAN_STRING( IsNullReference )
				SUPPORT_STRING( BlendOperation )
				SUPPORT_STRING( ClipPlaneScaledPoints )
				SUPPORT_STRING( ClipTransformedVertices )
				SUPPORT_STRING( ColorWrite )
				SUPPORT_STRING( CullClockwiseFace )
				SUPPORT_STRING( CullCounterClockwiseFace )
				SUPPORT_STRING( CullNone )
				SUPPORT_STRING( FogAndSpecularAlpha )
				SUPPORT_STRING( IndependentWriteMasks )
				SUPPORT_STRING( MaskZ )
				SUPPORT_STRING( MultipleRenderTargetsIndependentBitDepths )
				SUPPORT_STRING( MultipleRenderTargetsPostPixelShaderBlending )
				SUPPORT_STRING( PerStageConstant )
				SUPPORT_STRING( SeparateAlphaBlend )
				SUPPORT_STRING( TextureStageStateArgumentTemp )

				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class RasterCapabilities {
		private:
			unsigned int _value;
			
		internal:
			RasterCapabilities( unsigned int value ) : _value( value ) {
			}
			
		public:
			CAP_FLAG(SupportsAnisotropy,D3DPRASTERCAPS_ANISOTROPY)
			CAP_FLAG(SupportsColorPerspective,D3DPRASTERCAPS_COLORPERSPECTIVE)
			CAP_FLAG(SupportsDepthBias,D3DPRASTERCAPS_DEPTHBIAS)
			CAP_FLAG(SupportsDepthBufferLessHsr,D3DPRASTERCAPS_ZBUFFERLESSHSR)
			CAP_FLAG(SupportsDepthBufferTest,D3DPRASTERCAPS_ZTEST)
			CAP_FLAG(SupportsDepthFog,D3DPRASTERCAPS_ZFOG)
			CAP_FLAG(SupportsFogRange,D3DPRASTERCAPS_FOGRANGE)
			CAP_FLAG(SupportsFogTable,D3DPRASTERCAPS_FOGTABLE)
			CAP_FLAG(SupportsFogVertex,D3DPRASTERCAPS_FOGVERTEX)
			CAP_FLAG(SupportsMipMapLevelOfDetailBias,D3DPRASTERCAPS_MIPMAPLODBIAS)
			CAP_FLAG(SupportsMultisampleToggle,D3DPRASTERCAPS_MULTISAMPLE_TOGGLE)
			CAP_FLAG(SupportsScissorTest,D3DPRASTERCAPS_SCISSORTEST)
			CAP_FLAG(SupportsSlopeScaleDepthBias,D3DPRASTERCAPS_SLOPESCALEDEPTHBIAS)
			CAP_FLAG(SupportsWFog,D3DPRASTERCAPS_WFOG)

//			CAP_FLAG(SupportsDither,D3DPRASTERCAPS_DITHER)
//			CAP_FLAG(SupportsWBuffer,D3DPRASTERCAPS_WBUFFER)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( Anisotropy )
				SUPPORT_STRING( ColorPerspective )
				SUPPORT_STRING( DepthBias )
				SUPPORT_STRING( DepthBufferLessHsr )
				SUPPORT_STRING( DepthBufferTest )
				SUPPORT_STRING( DepthFog )
				SUPPORT_STRING( FogRange )
				SUPPORT_STRING( FogTable )
				SUPPORT_STRING( FogVertex )
				SUPPORT_STRING( MipMapLevelOfDetailBias )
				SUPPORT_STRING( MultisampleToggle )
				SUPPORT_STRING( ScissorTest )
				SUPPORT_STRING( SlopeScaleDepthBias )
				SUPPORT_STRING( WFog )

				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class ShadingCapabilities {
		private:
			unsigned int _value;
			
		internal:
			ShadingCapabilities( unsigned int value ) : _value( value ) {
			}
			
		public:
			CAP_FLAG(SupportsColorGouraudRgb,D3DPSHADECAPS_COLORGOURAUDRGB)
			CAP_FLAG(SupportsSpecularGouraudRgb,D3DPSHADECAPS_SPECULARGOURAUDRGB)
			CAP_FLAG(SupportsAlphaGouraudBlend,D3DPSHADECAPS_ALPHAGOURAUDBLEND)
			CAP_FLAG(SupportsFogGouraud,D3DPSHADECAPS_FOGGOURAUD)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( ColorGouraudRgb )
				SUPPORT_STRING( SpecularGouraudRgb )
				SUPPORT_STRING( AlphaGouraudBlend )
				SUPPORT_STRING( FogGouraud )

				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class TextureCapabilities {
		private:
			unsigned int _value;
			
		internal:
			TextureCapabilities( unsigned int value ) : _value( value ) {
			}
			
		public:
			CAP_FLAG(SupportsAlpha,D3DPTEXTURECAPS_ALPHA)
			CAP_FLAG(SupportsAlphaPalette,D3DPTEXTURECAPS_ALPHAPALETTE)
			CAP_FLAG(SupportsCubeMap,D3DPTEXTURECAPS_CUBEMAP)
			CAP_FLAG(SupportsCubeMapPower2,D3DPTEXTURECAPS_CUBEMAP_POW2)
			CAP_FLAG(SupportsMipCubeMap,D3DPTEXTURECAPS_MIPCUBEMAP)
			CAP_FLAG(SupportsMipMap,D3DPTEXTURECAPS_MIPMAP)
			CAP_FLAG(SupportsMipVolumeMap,D3DPTEXTURECAPS_MIPVOLUMEMAP)
			CAP_FLAG(SupportsNonPower2Conditional,D3DPTEXTURECAPS_NONPOW2CONDITIONAL)
			CAP_FLAG(SupportsNoProjectedBumpEnvironment,D3DPTEXTURECAPS_NOPROJECTEDBUMPENV)
			CAP_FLAG(SupportsPerspective,D3DPTEXTURECAPS_PERSPECTIVE)
			CAP_FLAG(SupportsPower2,D3DPTEXTURECAPS_POW2)
			CAP_FLAG(SupportsProjected,D3DPTEXTURECAPS_PROJECTED)
			CAP_FLAG(SupportsSquareOnly,D3DPTEXTURECAPS_SQUAREONLY)
			CAP_FLAG(SupportsTextureRepeatNotScaledBySize,D3DPTEXTURECAPS_TEXREPEATNOTSCALEDBYSIZE)
			CAP_FLAG(SupportsVolumeMap,D3DPTEXTURECAPS_VOLUMEMAP)
			CAP_FLAG(SupportsVolumeMapPower2,D3DPTEXTURECAPS_VOLUMEMAP_POW2)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( Alpha )
				SUPPORT_STRING( AlphaPalette )
				SUPPORT_STRING( CubeMap )
				SUPPORT_STRING( CubeMapPower2 )
				SUPPORT_STRING( MipCubeMap )
				SUPPORT_STRING( MipMap )
				SUPPORT_STRING( MipVolumeMap )
				SUPPORT_STRING( NonPower2Conditional )
				SUPPORT_STRING( NoProjectedBumpEnvironment )
				SUPPORT_STRING( Perspective )
				SUPPORT_STRING( Power2 )
				SUPPORT_STRING( Projected )
				SUPPORT_STRING( SquareOnly )
				SUPPORT_STRING( TextureRepeatNotScaledBySize )
				SUPPORT_STRING( VolumeMap )
				SUPPORT_STRING( VolumeMapPower2 )

				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class AddressCapabilities {
		private:
			unsigned int _value;
			
		internal:
			AddressCapabilities( unsigned int value ) : _value( value ) {
			}
			
		public:
			CAP_FLAG(SupportsWrap,D3DPTADDRESSCAPS_WRAP)
			CAP_FLAG(SupportsMirror,D3DPTADDRESSCAPS_MIRROR)
			CAP_FLAG(SupportsClamp,D3DPTADDRESSCAPS_CLAMP)
			CAP_FLAG(SupportsBorder,D3DPTADDRESSCAPS_BORDER)
			CAP_FLAG(SupportsIndependentUV,D3DPTADDRESSCAPS_INDEPENDENTUV)
			CAP_FLAG(SupportsMirrorOnce,D3DPTADDRESSCAPS_MIRRORONCE)

			virtual String ^ToString() override {
				List<String^>^ list = gcnew List<String^>();
				
				SUPPORT_STRING( Wrap )
				SUPPORT_STRING( Mirror )
				SUPPORT_STRING( Clamp )
				SUPPORT_STRING( Border )
				SUPPORT_STRING( IndependentUV )
				SUPPORT_STRING( MirrorOnce )

				return String::Join( ", ", list->ToArray() );
			}
		};
		
		value class VertexShader20Capabilities {
		private:
			bool _predication;
			int _dynamicFlowDepth, _staticFlowDepth, _numTemps;
			
		internal:
			VertexShader20Capabilities( D3DVSHADERCAPS2_0 value ) : _predication( value.Caps != 0 ),
				_staticFlowDepth( value.StaticFlowControlDepth ), _dynamicFlowDepth( value.DynamicFlowControlDepth ), _numTemps( value.NumTemps ) {
			}
			
		public:
			SIMPLE_PROPERTY_GET(bool,_predication,SupportsPredication)
			SIMPLE_PROPERTY_GET(int,_staticFlowDepth,StaticFlowControlDepth)
			SIMPLE_PROPERTY_GET(int,_dynamicFlowDepth,DynamicFlowControlDepth)
			SIMPLE_PROPERTY_GET(int,_numTemps,NumberTemps)
		};
	
	private:
		CompareCapabilities _alphaCmpCaps, _zCmpCaps;
		FilterCapabilities _textureFilterCaps, _cubeFilterCaps, _volumeTextureFilterCaps, _stretchRectFilterCaps;
		DriverCapabilities _driverCaps;
		CursorCapabilities _cursorCaps;
		DeclarationTypeCapabilities _declTypeCaps;
		DeviceCapabilitiesW _devCaps;
		BlendCapabilities _destBlendCaps, _srcBlendCaps;
		LineCapabilities _lineCaps;
		StencilCapabilities _stencilCaps;
		VertexProcessingCapabilities _vertexProcessingCaps;
		RasterCapabilities _rasterCaps;
		PrimitiveCapabilities _primitiveMiscCaps;
		ShadingCapabilities _shadeCaps;
		TextureCapabilities _textureCaps;
		AddressCapabilities _textureAddressCaps, _volumeTextureAddressCaps;
		VertexShader20Capabilities _vsCaps;
		int _adapterOrdinalInGroup, _maxTextureWidth, _maxTextureHeight, _maxVolumeExtent, _maxTextureRepeat,
			_maxAnisotropy, _maxTextureAspectRatio, _maxActiveLights, _maxStreams, _maxStreamStride, _maxPrimitiveCount,
			_maxVertexIndex, _maxUserClipPlanes, _maxVertexBlendMatrices, _maxVertexBlendMatrixIndex,
			_maxTextureBlendStages, _maxSimultaneousTextures, _maxVertexShaderConst,
			_masterAdapterOrdinal, _numberOfAdaptersInGroup, _numSimultaneousRTs;
		DeviceType _deviceType;
		PresentInterval _presentInterval;
		float _extentsAdjust, _guardBandBottom, _guardBandLeft, _guardBandRight, _guardBandTop, _maxVertexW,
			_maxPointSize, _pixelShader1xMaxValue;
		Version ^_vertexShaderVersion, ^_pixelShaderVersion;
		
	internal:
		GraphicsDeviceCapabilities( D3DCAPS9 *caps ) {
			if( caps == NULL )
				throw gcnew ArgumentNullException( "caps" );
		
			_deviceType = (NS(DeviceType)) caps->DeviceType;
    //UINT        AdapterOrdinal;
			_driverCaps = DriverCapabilities( caps->Caps, caps->Caps2, caps->Caps3 );
			_presentInterval = (NS(PresentInterval))(int) caps->PresentationIntervals;
			_cursorCaps = CursorCapabilities( caps->CursorCaps );
			_devCaps = DeviceCapabilitiesW( caps->DevCaps, caps->DevCaps2 );
			_primitiveMiscCaps = PrimitiveCapabilities( caps->PrimitiveMiscCaps );
			_rasterCaps = RasterCapabilities( caps->RasterCaps );
			_zCmpCaps = CompareCapabilities( caps->ZCmpCaps );
			_srcBlendCaps = BlendCapabilities( caps->SrcBlendCaps );
			_destBlendCaps = BlendCapabilities( caps->DestBlendCaps );
			_alphaCmpCaps = CompareCapabilities( caps->AlphaCmpCaps );
			_shadeCaps = ShadingCapabilities( caps->ShadeCaps );
			_textureCaps = TextureCapabilities( caps->TextureCaps );
			_textureFilterCaps = FilterCapabilities( caps->TextureFilterCaps );
			_cubeFilterCaps = FilterCapabilities( caps->CubeTextureFilterCaps );
			_volumeTextureFilterCaps = FilterCapabilities( caps->VolumeTextureFilterCaps );
			_textureAddressCaps = AddressCapabilities( caps->TextureAddressCaps );
			_volumeTextureAddressCaps = AddressCapabilities( caps->VolumeTextureAddressCaps );

			_lineCaps = LineCapabilities( caps->LineCaps );

			_maxTextureWidth = (int) caps->MaxTextureWidth;
			_maxTextureHeight = (int) caps->MaxTextureHeight;
			_maxVolumeExtent = (int) caps->MaxVolumeExtent;

			_maxTextureRepeat = (int) caps->MaxTextureRepeat;
			_maxTextureAspectRatio = (int) caps->MaxTextureAspectRatio;
			_maxAnisotropy = (int) caps->MaxAnisotropy;
			_maxVertexW = caps->MaxVertexW;

			_guardBandLeft = caps->GuardBandLeft;
			_guardBandTop = caps->GuardBandTop;
			_guardBandRight = caps->GuardBandRight;
			_guardBandBottom = caps->GuardBandBottom;

			_extentsAdjust = caps->ExtentsAdjust;
			_stencilCaps = StencilCapabilities( caps->StencilCaps );

    //DWORD   FVFCaps;
    //DWORD   TextureOpCaps;
			_maxTextureBlendStages = (int) caps->MaxTextureBlendStages;
			_maxSimultaneousTextures = (int) caps->MaxSimultaneousTextures;

			_vertexProcessingCaps = VertexProcessingCapabilities( caps->VertexProcessingCaps );
			_maxActiveLights = (int) caps->MaxActiveLights;
			_maxUserClipPlanes = (int) caps->MaxUserClipPlanes;
			_maxVertexBlendMatrices = (int) caps->MaxVertexBlendMatrices;
			_maxVertexBlendMatrixIndex = (int) caps->MaxVertexBlendMatrixIndex;

			_maxPointSize = caps->MaxPointSize;

			_maxPrimitiveCount = (int) caps->MaxPrimitiveCount;
			_maxVertexIndex = (int) caps->MaxVertexIndex;
			_maxStreams = (int) caps->MaxStreams;
			_maxStreamStride = (int) caps->MaxStreamStride;

			_vertexShaderVersion = gcnew Version( (int)(caps->VertexShaderVersion & 0xFF00) >> 8, (int)(caps->VertexShaderVersion & 0xFF) );
			_maxVertexShaderConst = (int) caps->MaxVertexShaderConst;

			_pixelShaderVersion = gcnew Version( (int)(caps->PixelShaderVersion & 0xFF00) >> 8, (int)(caps->PixelShaderVersion & 0xFF) );
			_pixelShader1xMaxValue = caps->PixelShader1xMaxValue;

    //float   MaxNpatchTessellationLevel;
    //DWORD   Reserved5;

			_masterAdapterOrdinal = (int) caps->MasterAdapterOrdinal;
			_adapterOrdinalInGroup = (int) caps->AdapterOrdinalInGroup;
			_numberOfAdaptersInGroup = (int) caps->NumberOfAdaptersInGroup;
			_declTypeCaps = DeclarationTypeCapabilities( caps->DeclTypes );
			_numSimultaneousRTs = (int) caps->NumSimultaneousRTs;
			_stretchRectFilterCaps = FilterCapabilities( caps->StretchRectFilterCaps );
    //D3DVSHADERCAPS2_0 VS20Caps;
			_vsCaps = VertexShader20Capabilities( caps->VS20Caps );
    //D3DPSHADERCAPS2_0 PS20Caps;
    //DWORD   VertexTextureFilterCaps;    // D3DPTFILTERCAPS for IDirect3DTexture9's for texture, used in vertex shaders
    //DWORD   MaxVShaderInstructionsExecuted; // maximum number of vertex shader instructions that can be executed
    //DWORD   MaxPShaderInstructionsExecuted; // maximum number of pixel shader instructions that can be executed
    //DWORD   MaxVertexShader30InstructionSlots; 
    //DWORD   MaxPixelShader30InstructionSlots;
			
		}
		
	public:
		SIMPLE_PROPERTY_GET(NS(DeviceType),_deviceType,DeviceType)
    //UINT        AdapterOrdinal;
		SIMPLE_PROPERTY_GET(DriverCapabilities,_driverCaps,DriverCaps)
		SIMPLE_PROPERTY_GET(NS(PresentInterval),_presentInterval,PresentInterval)
		SIMPLE_PROPERTY_GET(CursorCapabilities,_cursorCaps,CursorCaps)
		SIMPLE_PROPERTY_GET(DeviceCapabilitiesW,_devCaps,DeviceCaps)
		SIMPLE_PROPERTY_GET(PrimitiveCapabilities,_primitiveMiscCaps,PrimitiveMiscCapabilities)
		SIMPLE_PROPERTY_GET(RasterCapabilities,_rasterCaps,RasterCaps)
		SIMPLE_PROPERTY_GET(CompareCapabilities,_zCmpCaps,DepthBufferCompareCapabilities)
		SIMPLE_PROPERTY_GET(BlendCapabilities,_srcBlendCaps,SourceBlendCapabilities)
		SIMPLE_PROPERTY_GET(BlendCapabilities,_destBlendCaps,DestinationBlendCapabilities)
		SIMPLE_PROPERTY_GET(CompareCapabilities,_alphaCmpCaps,AlphaCompareCapabilities)
		SIMPLE_PROPERTY_GET(ShadingCapabilities,_shadeCaps,ShadeCapabilities)
		SIMPLE_PROPERTY_GET(TextureCapabilities,_textureCaps,TextureCaps)
		SIMPLE_PROPERTY_GET(FilterCapabilities,_textureFilterCaps,TextureFilterCapabilities)
		SIMPLE_PROPERTY_GET(FilterCapabilities,_cubeFilterCaps,CubeTextureFilterCapabilities)
		SIMPLE_PROPERTY_GET(FilterCapabilities,_volumeTextureFilterCaps,VolumeTextureFilterCapabilities)
		SIMPLE_PROPERTY_GET(AddressCapabilities,_textureAddressCaps,TextureAddressCapabilities)
		SIMPLE_PROPERTY_GET(AddressCapabilities,_volumeTextureAddressCaps,VolumeTextureAddressCapabilities)

		SIMPLE_PROPERTY_GET(LineCapabilities,_lineCaps,LineCaps)

		SIMPLE_PROPERTY_GET(int,_maxTextureWidth,MaxTextureWidth)
		SIMPLE_PROPERTY_GET(int,_maxTextureHeight,MaxTextureHeight)
		SIMPLE_PROPERTY_GET(int,_maxVolumeExtent,MaxVolumeExtent)

		SIMPLE_PROPERTY_GET(int,_maxTextureRepeat,MaxTextureRepeat)
		SIMPLE_PROPERTY_GET(int,_maxTextureAspectRatio,MaxTextureAspectRatio)
		SIMPLE_PROPERTY_GET(int,_maxAnisotropy,MaxAnisotropy)
		SIMPLE_PROPERTY_GET(float,_maxVertexW,MaxVertexW)

		SIMPLE_PROPERTY_GET(float,_guardBandLeft,GuardBandLeft)
		SIMPLE_PROPERTY_GET(float,_guardBandTop,GuardBandTop)
		SIMPLE_PROPERTY_GET(float,_guardBandRight,GuardBandRight)
		SIMPLE_PROPERTY_GET(float,_guardBandBottom,GuardBandBottom)

		SIMPLE_PROPERTY_GET(float,_extentsAdjust,ExtentsAdjust)
		SIMPLE_PROPERTY_GET(StencilCapabilities,_stencilCaps,StencilCaps)

    //DWORD   FVFCaps;
    //DWORD   TextureOpCaps;
		SIMPLE_PROPERTY_GET(int,_maxTextureBlendStages,MaxTextureBlendStages)
		SIMPLE_PROPERTY_GET(int,_maxSimultaneousTextures,MaxSimultaneousTextures)

		SIMPLE_PROPERTY_GET(VertexProcessingCapabilities,_vertexProcessingCaps,VertexProcessingCaps)
		SIMPLE_PROPERTY_GET(int,_maxActiveLights,MaxActiveLights)
		SIMPLE_PROPERTY_GET(int,_maxUserClipPlanes,MaxUserClipPlanes)
		SIMPLE_PROPERTY_GET(int,_maxVertexBlendMatrices,MaxVertexBlendMatrices)
		SIMPLE_PROPERTY_GET(int,_maxVertexBlendMatrixIndex,MaxVertexBlendMatrixIndex)

		SIMPLE_PROPERTY_GET(float,_maxPointSize,MaxPointSize)

		SIMPLE_PROPERTY_GET(int,_maxPrimitiveCount,MaxPrimitiveCount)
		SIMPLE_PROPERTY_GET(int,_maxVertexIndex,MaxVertexIndex)
		SIMPLE_PROPERTY_GET(int,_maxStreams,MaxStreams)
		SIMPLE_PROPERTY_GET(int,_maxStreamStride,MaxStreamStride)

		SIMPLE_PROPERTY_GET(Version^,_vertexShaderVersion,VertexShaderVersion)
		SIMPLE_PROPERTY_GET(int,_maxVertexShaderConst,MaxVertexShaderConst)

		SIMPLE_PROPERTY_GET(Version^,_pixelShaderVersion,PixelShaderVersion)
		SIMPLE_PROPERTY_GET(float,_pixelShader1xMaxValue,PixelShader1xMaxValue)

    //float   MaxNpatchTessellationLevel;
    //DWORD   Reserved5;

		SIMPLE_PROPERTY_GET(int,_masterAdapterOrdinal,MasterAdapterOrdinal)
		SIMPLE_PROPERTY_GET(int,_adapterOrdinalInGroup,AdapterOrdinalInGroup)
		SIMPLE_PROPERTY_GET(int,_numberOfAdaptersInGroup,NumberOfAdaptersInGroup)
		SIMPLE_PROPERTY_GET(DeclarationTypeCapabilities,_declTypeCaps,DeclarationTypes)
		SIMPLE_PROPERTY_GET(int,_numSimultaneousRTs,NumberSimultaneousRenderTargets)
		SIMPLE_PROPERTY_GET(FilterCapabilities,_stretchRectFilterCaps,StretchRectangleFilterCapabilities)
    //D3DVSHADERCAPS2_0 VS20Caps;
		SIMPLE_PROPERTY_GET(VertexShader20Capabilities,_vsCaps,VertexShaderCapabilities)
    //D3DPSHADERCAPS2_0 PS20Caps;
    //DWORD   VertexTextureFilterCaps;    // D3DPTFILTERCAPS for IDirect3DTexture9's for texture, used in vertex shaders
    //DWORD   MaxVShaderInstructionsExecuted; // maximum number of vertex shader instructions that can be executed
    //DWORD   MaxPShaderInstructionsExecuted; // maximum number of pixel shader instructions that can be executed
    //DWORD   MaxVertexShader30InstructionSlots; 
    //DWORD   MaxPixelShader30InstructionSlots;

	};
NS_CLOSE