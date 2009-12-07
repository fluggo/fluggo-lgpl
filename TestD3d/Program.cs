using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Fluggo.Graphics.Direct3D.Xna;
using Fluggo.Graphics;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace TestD3d {
	struct GenericVertex {
		public Vector3f Position;
		public Vector3f Normal;
		public Vector2f TexCoord0;
	}

	struct XYZTex {
		public float X, Y, Z;
		public float U, V;
	}
	
	struct XYZColorTex {
		public XYZColorTex( Vector3f position, ColorARGB color, Vector2f tex ) {
			Position = position;
			Color = color;
			TexCoord = tex;
		}
		
		public Vector3f Position;
		public ColorARGB Color;
		public Vector2f TexCoord;
	}

	class Program {
		
		class BasicRenderPass {
			GraphicsDevice _device;
			Effect _effect;
			Texture _texture;
			EffectParameter _projectionParam, _worldParam, _lightPosParam;
			
			public BasicRenderPass( GraphicsDevice device, Texture texture ) {
				if( device == null )
					throw new ArgumentNullException( "device" );

				_device = device;
				_texture = texture;
				
				CompiledEffect compiledEffect = Effect.CompileEffectFromSource(
@"struct VS_INPUT {
	float4 Position : POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
};

struct VS_OUTPUT {
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float4 Diffuse : COLOR0;
};

float4x4 World;
float4x4 Projection;
float3 lightPos;

VS_OUTPUT myvs(const VS_INPUT v)
{
	VS_OUTPUT o;

    o.Position = mul(mul(v.Position,World),Projection);
    o.Diffuse = float4(1,1,1,1) * dot(mul(v.Normal, (float3x3) World), normalize(lightPos - (float3)mul(v.Position,World))) + float4( 0.2, 0.2, 0.2, 1.0);
    o.TexCoord = v.TexCoord;

    return o;
}

struct PS_INPUT {
	float2 TexCoord : TEXCOORD0;
	float4 Diffuse : COLOR0;
};

sampler sMine = sampler_state{ mipfilter = LINEAR; magfilter = LINEAR; minfilter = LINEAR; };

float4 ps_1tex( PS_INPUT ps ) : COLOR0 { return ps.Diffuse * tex2D(sMine,ps.TexCoord); }
float4 ps_0tex( PS_INPUT ps ) : COLOR0 { return ps.Diffuse; }

technique RenderSceneWithTexture1Light
{
    pass P0
    {          
        VertexShader = compile vs_1_1 myvs();
        PixelShader  = compile ps_1_4 ps_1tex(); 
    }
}

technique RenderSceneWithoutTexture1Light
{
    pass P0
    {          
        VertexShader = compile vs_1_1 myvs();
        PixelShader  = compile ps_1_4 ps_0tex(); 
    }
}", null, null, CompilerOptions.None, TargetPlatform.Windows );

				if( !compiledEffect.Success )
					throw new Exception( compiledEffect.ErrorsAndWarnings );
					
				_effect = new Effect( device, compiledEffect.GetEffectCode(), CompilerOptions.None, null );
				
				if( _texture != null )
					_effect.CurrentTechnique = _effect.Techniques["RenderSceneWithTexture1Light"];
				else
					_effect.CurrentTechnique = _effect.Techniques["RenderSceneWithoutTexture1Light"];
					
				_projectionParam = _effect.Parameters["Projection"];
				_worldParam = _effect.Parameters["World"];
				_lightPosParam = _effect.Parameters["lightPos"];
			}
			
			public void Begin( Matrix4f projectionTransform, Matrix4f worldTransform, Vector3f lightPos ) {
				//_device.PixelShader = _pixelShader;
				//_device.VertexShader = _vertexShader;
				//_vxConstants.Constants["Projection"].SetValue<Matrix4f>( _device, projectionTransform );
				//_vxConstants.Constants["World"].SetValue<Matrix4f>( _device, worldTransform );
				//_vxConstants.Constants["lightPos"].SetValue<Vector3f>( _device, lightPos );

				_projectionParam.SetValue( projectionTransform );
				_worldParam.SetValue( worldTransform );
				_lightPosParam.SetValue( lightPos );
				_effect.Begin();
				_effect.CurrentTechnique.Passes[0].Begin();

				_device.Textures[0] = _texture;
			}
			
			public void End() {
				_effect.CurrentTechnique.Passes[0].End();
				_effect.End();
			}
		}
		
		class Mesh {
			OgreXmlSubmesh _mesh;
			GraphicsDevice _device;
			VertexBuffer _vertices;
			IndexBuffer _indices;
			VertexDeclaration _decl;
			PerspectiveCamera _camera;
			BasicRenderPass _pass;
			BasicTransform _transform = new BasicTransform();
			uint _time = 0;
			
			public BasicTransform Transform { get { return _transform; } }
			
			public uint Time { get { return _time; } }
			
			private void ReverseTriangles() {
				for( int i = 0; i < _mesh.Triangles.Length; i++ ) {
					ushort temp = _mesh.Triangles[i].V2;
					_mesh.Triangles[i].V2 = _mesh.Triangles[i].V3;
					_mesh.Triangles[i].V3 = temp;
				}
			}
			
			public Mesh( GraphicsDevice device, PerspectiveCamera camera, OgreXmlSubmesh submesh, string textureFile ) {
				_mesh = submesh;
				
				Texture2D texture = null;
				if( textureFile != null )
					texture = Texture2D.FromFile( device, textureFile );

				_pass = new BasicRenderPass( device, texture );
					
				_device = device;
				_camera = camera;
				ReverseTriangles();

				_vertices = new VertexBuffer( device, (12 + 12 + 8) * _mesh.Vertices.Length, ResourceUsage.None );
				_indices = new IndexBuffer( device, 12 * _mesh.Triangles.Length, ResourceUsage.None, IndexElementSize.SixteenBits );

				_vertices.SetData<GenericVertex>( 0, _mesh.Vertices, 0, _mesh.Vertices.Length, SetDataOptions.None );
				_indices.SetData<Triangle>( 0, _mesh.Triangles, 0, _mesh.Triangles.Length, SetDataOptions.None );

				_decl = new VertexDeclaration( device, new VertexElement[] {
					new VertexElement( 0, 0, VertexDeclarationUsage.Position, VertexDeclarationType.Float3 ),
					new VertexElement( 0, 12, VertexDeclarationUsage.Normal, VertexDeclarationType.Float3 ),
					new VertexElement( 0, 24, VertexDeclarationUsage.TextureCoord, VertexDeclarationType.Float2 ) } );
			}

			public void Render() {
				//_vxConstants.Constants["march"].SetValue<float>( _device, 0.0f );

				//Matrix4f matrix = Matrix4f.CreateZRotation( (float) (Math.PI * 0.5) ) * Matrix4f.CreateXRotation( (float) ((double) _time / (3.0 * 256.0) * Math.PI) ) * _camera.GetTransform();
				Matrix4f matrix = Matrix4f.CreateXRotation( (float) (Math.PI * -0.5) ) * Matrix4f.CreateTranslation( -1.0f, 0.0f, -5.0f + ((float) _time / (3.0f * 256.0f)) );
				//Matrix4f matrix =  _camera.GetTransform();
				
/*				matrix.M31 *= -1.0f;
				matrix.M32 *= -1.0f;
				matrix.M33 *= -1.0f;
				matrix.M34 *= -1.0f;*/
				
//				_vxConstants.Constants["World"].SetValue<Matrix4f>( _device, matrix );
//				_psConstants.Constants["color"].SetValue<uint>( _device, 0xFFFFFFFF );

				_pass.Begin( _camera.GetTransformMatrix(), _transform.GetLocalTransformMatrix(), new Vector3f( 15.0f, 15.0f, 25.0f ) );
				
				_device.VertexDeclaration = _decl;
				_device.Vertices[0].SetSource( _vertices, 0, 32 );
				_device.Indices = _indices;

				if( (_time += 4) == (256 * 6) )
					_time = 0;

				_device.DrawIndexedPrimitives( PrimitiveType.TriangleList, 0, 0, _mesh.Vertices.Length, 0, _mesh.Triangles.Length );
				
				_pass.End();
			}
		}
		
		static void Main( string[] args ) {
			using( Form form = new Form() ) {
				ReadOnlyCollection<GraphicsAdapter> adapters = GraphicsAdapter.Adapters;
				
				Console.WriteLine( "Number of adapters: {0}", adapters.Count );
				
				//uint adapterCount = d3d.GetAdapterCount();
				//Console.WriteLine( "Number of adapters: {0}", adapterCount );
				
				GraphicsDeviceCapabilities caps = adapters[0].GetCapabilities( DeviceType.Hardware );
				
				//DeviceCaps caps = d3d.GetDeviceCaps( 0, DeviceType.Hardware );
				Console.WriteLine( caps.AlphaCompareCapabilities );
				Console.WriteLine( caps.CursorCaps );
				Console.WriteLine( caps.DriverCaps );
				Console.WriteLine( caps.DestinationBlendCapabilities );
				
				CompiledEffect simpleEffect = Effect.CompileEffectFromSource(
@"struct VS_INPUT {
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct VS_OUTPUT {
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};

float4x4 World;
float march;

VS_OUTPUT myvs(const VS_INPUT v)
{
	VS_OUTPUT o;

    o.Position = mul(v.Position,World);
    
    float2 update = { 0, march };
    o.TexCoord = v.TexCoord + update;

    return o;
}

float4 color;

struct PS_INPUT {
	float2 TexCoord : TEXCOORD0;
};

sampler sMine = sampler_state{ magfilter = POINT; minfilter = LINEAR; };

float4 myps( PS_INPUT ps ) : COLOR0 { return color * tex2D(sMine,ps.TexCoord); }

technique RenderSceneWithTexture1Light
{
    pass P0
    {          
        VertexShader = compile vs_1_1 myvs();
        PixelShader  = compile ps_1_4 myps(); 
    }
}", null, null, CompilerOptions.None, TargetPlatform.Windows );
				
				CompiledEffect advancedEffect = Effect.CompileEffectFromSource(
@"// Global variables
float4 g_MaterialAmbientColor;      // Material's ambient color
float4 g_MaterialDiffuseColor;      // Material's diffuse color
int g_nNumLights;

float3 g_LightDir[3];               // Light's direction in world space
float4 g_LightDiffuse[3];           // Light's diffuse color
float4 g_LightAmbient;              // Light's ambient color

texture g_MeshTexture;              // Color texture for mesh

float    g_fTime;                   // App's time in seconds
float4x4 g_mWorld;                  // World matrix for object
float4x4 g_mWorldViewProjection;    // World * View * Projection matrix
// Texture samplers
sampler MeshTextureSampler = 
sampler_state
{
    Texture = <g_MeshTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
struct VS_OUTPUT
{
    float4 Position   : POSITION;   // vertex position 
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords 
    float4 Diffuse    : COLOR0;     // vertex diffuse color
};
VS_OUTPUT RenderSceneVS( float4 vPos : POSITION, 
                         float3 vNormal : NORMAL,
                         float2 vTexCoord0 : TEXCOORD0,
                         uniform int nNumLights,
                         uniform bool bTexture,
                         uniform bool bAnimate )
{
    VS_OUTPUT Output;
    float3 vNormalWorldSpace;
  
    float4 vAnimatedPos = vPos;
    
    // Animation the vertex based on time and the vertex's object space position
    if( bAnimate )
		vAnimatedPos += float4(vNormal, 0) * (sin(g_fTime+5.5)+0.5)*5;
    
    // Transform the position from object space to homogeneous projection space
    Output.Position = mul(vAnimatedPos, g_mWorldViewProjection);
    
    // Transform the normal from object space to world space    
    vNormalWorldSpace = normalize(mul(vNormal, (float3x3)g_mWorld));
    
    // Compute simple directional lighting equation
    float3 vTotalLightDiffuse = float3(0,0,0);
    for(int i=0; i < nNumLights; i++ )
        vTotalLightDiffuse += g_LightDiffuse[i] * max(0,dot(vNormalWorldSpace, g_LightDir[i]));
        
    Output.Diffuse.rgb = g_MaterialDiffuseColor * vTotalLightDiffuse + 
                         g_MaterialAmbientColor * g_LightAmbient;   
    Output.Diffuse.a = 1.0f; 
    
    // Just copy the texture coordinate through
    if( bTexture ) 
        Output.TextureUV = vTexCoord0; 
    else
        Output.TextureUV = 0; 
    
    return Output;    
}
struct PS_OUTPUT
{
    float4 RGBColor : COLOR0;  // Pixel color    
};
PS_OUTPUT RenderScenePS( VS_OUTPUT In,
                         uniform bool bTexture ) 
{ 
    PS_OUTPUT Output;

    // Lookup mesh texture and modulate it with diffuse
    if( bTexture )
        Output.RGBColor = tex2D(MeshTextureSampler, In.TextureUV) * In.Diffuse;
    else
        Output.RGBColor = In.Diffuse;

    return Output;
}
technique RenderSceneWithTexture1Light
{
    pass P0
    {          
        VertexShader = compile vs_1_1 RenderSceneVS( 1, true, true );
        PixelShader  = compile ps_1_1 RenderScenePS( true ); 
    }
}", null, null, CompilerOptions.None, TargetPlatform.Windows );
				Console.WriteLine( simpleEffect.ErrorsAndWarnings );
				
//				Console.ReadKey();
				
//				Console.WriteLine( "Caps: {0}", caps.Caps );
//				Console.WriteLine( "Caps2: {0}", caps.Caps2 );
//				Console.WriteLine( "Caps3: {0}", caps.Caps3 );
//				Console.WriteLine( "Cursor caps: {0}", caps.CursorCaps );
				
				form.CreateControl();
				form.ClientSize = new System.Drawing.Size( 640, 480 );
				
				PresentationParameters @params = new PresentationParameters();
				@params.SwapEffect = SwapEffect.Discard;
				@params.EnableAutoDepthStencil = true;
				@params.AutoDepthStencilFormat = DepthFormat.Depth24;
				@params.BackBufferFormat = SurfaceFormat.Bgr32;
				@params.BackBufferWidth = 640;
				@params.BackBufferHeight = 480;
				@params.DeviceWindowHandle = form.Handle;
				@params.IsFullScreen = false;
				@params.PresentationInterval = PresentInterval.One;

				using( GraphicsDevice device = new GraphicsDevice( adapters[0], DeviceType.Hardware, form.Handle, CreateOptions.HardwareVertexProcessing, @params ) ) {
/*					Effect myEffect = new Effect( device, simpleEffect.GetEffectCode(), CompilerOptions.None, null );
					foreach( EffectTechnique technique in myEffect.Techniques.GetValidTechniques() ) {
						Console.WriteLine( technique.Name );
						myEffect.CurrentTechnique = technique;
						myEffect.Begin();
						
						foreach( EffectPass pass in technique.Passes ) {
							Console.WriteLine( "\t" + pass.Name );
							pass.Begin();
							pass.End();
						}
						
						myEffect.End();
					}*/
					
					Texture2D tex = new Texture2D( device, 2, 2, 1, ResourceUsage.Dynamic, SurfaceFormat.Bgr32 );
					tex.SetData<uint>( 0, null, new uint[] { 0x00FFFFFF, 0x00000000, 0x00000000, 0x00FFFFFF }, 0, 4, SetDataOptions.None );
					
					device.Textures[0] = tex;

					//device.ShadeMode = ShadeMode.Gouraud;
					device.RenderState.FillMode = FillMode.Solid;
					//device.Lighting = false;
					device.RenderState.CullMode = CullMode.CounterClockwise;
					device.RenderState.DepthBufferWriteEnable = true;
					//device.RenderState.DepthBufferFunction = CompareFunction.Greater;

					form.Show();
					
					PerspectiveCamera camera = new PerspectiveCamera();
					camera.AspectRatio = 4.0f / 3.0f;
					camera.FieldOfView = (float)(Math.PI / 3.0);
					//camera.AspectRatio = 1.0f;
					camera.FarZ = -100.0f;
					camera.NearZ = -1.0f;
					
					CircleOfDeath cod = new CircleOfDeath( device, camera );
					Grass grass = new Grass( device, camera );
					Waterfall waterfall = new Waterfall( device, camera );

					OgreXmlSubmesh[] meshes = OgreXmlMeshReader.ReadMeshFile( @"..\..\Mesh.001.mesh.xml" );
					Mesh shipMesh = new Mesh( device, camera, meshes[0], @"..\..\SketchedTexture_Scaled.jpg" );
					Mesh towerMesh = new Mesh( device, camera, meshes[1], null );

					shipMesh.Transform.Pitch = (float) (Math.PI * -0.5);
					towerMesh.Transform.Pitch = (float) (Math.PI * -0.5);
					int time = 0;
					
					Debug.WriteLine( caps.MaxAnisotropy );
					
					device.SamplerStates[0].MagFilter = TextureFilter.Point;
					device.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
					device.SamplerStates[0].MipFilter = TextureFilter.Linear;
					device.SamplerStates[0].MaxAnisotropy = 4;

					while( form.Created ) {
						device.Clear( null, ClearTargets.Target | ClearTargets.ZBuffer, 32, 1.0f, 0 );

						//camera.Position = new Vector3f( (float) Math.Cos( (double) shipMesh.Time / (3.0 * 256.0) * Math.PI ) * 20.0f,
						//	(float) Math.Sin( (double) shipMesh.Time / (3.0 * 256.0) * Math.PI ) * 20.0f, 26.0f );
						//camera.LookAt( Vector3f.Zero );

						//camera.Position = new Vector3f( 0.0f, 0.0f, 3.0f );
//						camera.Yaw = (float) Math.Sin( (double) cod.Time / (3.0 * 256.0) * Math.PI );
						//cod.Render();

						camera.Position = new Vector3f( 0.0f, 2.0f, 10.0f );
						Vector3f position = new Vector3f( -2.0f, 0.0f, -3.0f + ((float) time / (1.0f * 256.0f)) );
						
						shipMesh.Transform.Position = position;
						towerMesh.Transform.Position = position;
						camera.LookAt( position );
						//Matrix4f matrix = Matrix4f.CreateXRotation( (float) (Math.PI * -0.5) ) * Matrix4f.CreateTranslation( -1.0f, 0.0f, -5.0f + ((float) _time / (3.0f * 256.0f)) );
						//grass.Render();
						//waterfall.Render();
						
						shipMesh.Render();
						towerMesh.Render();
						
						device.Present( null, null, IntPtr.Zero );
						Application.DoEvents();
						time += 4;
						
//							Console.ReadKey();
					}
					
					Application.DoEvents();
				}
				
				//Console.ReadKey();
			}
		}
	}
}
