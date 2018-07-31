// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Particles/Add Multiply 2pass" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	_ScaleColor1("Scale Color1", Vector) = (1,1,1,1)
	_ScaleColor("Scale Color", Vector) = (1,1,1,1)
	_ScaleAlpha("Scale Alpha", float) = 1
	_AlphaTestGreater1("Alpha test greater 1",float) = 0.01
	_AlphaTestGreater2("Alpha test greater 2",float) = 0.01
	
	//_LightDirection("Light direction",Vector) =  (1,0,0,0)
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//Blend  DstColor SrcColor// Zero //One
	AlphaTest Greater [_AlphaTestGreater1] // .01
	//ColorMask RGB
	Cull Off 
	//Lighting Off 
	ZWrite Off 
	//Fog { Color (0,0,0,0) }
	/*BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}*/
	
	// ---- Fragment program cards
	SubShader {
		Pass {
			Blend SrcAlpha One
			//Edge: 
			//Blend SrcAlpha One
			//Soft, no edge: 
			//Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma fragmentoption ARB_precision_hint_fastest
			//#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			float4 _ScaleColor1;
			
			//float4 _LightDirection;

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{				
				//float2 fromCenter = i.texcoord - float2(0.5,0.5);

				// radius of sphere is 0.5 = a half of 1 tile
				// z of the given point is 1 - len(x) / 0.5
				//		   .  |
				//      .     | this is 0.5
				//    .       |
				//  .         |
				// o----.-----.
				//      |--x--|
				// |---0.5----|

				//float3 normal = normalize(float3(fromCenter.x,fromCenter.y,1-length(fromCenter)/0.5));
				//float nDotL = 1- dot(normal,normalize(_LightDirection.xyz));

				return  _ScaleColor1* 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
			}
			ENDCG 
		}
		
		Pass {
		
			// Multiply double
			//Blend DstColor SrcColor
			// MULTIPLY 
			Blend DstColor Zero
			//AlphaTest Greater [_AlphaTestGreater2] //.01
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma fragmentoption ARB_precision_hint_fastest
			//#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			float4 _ScaleColor;
			float _ScaleAlpha;
			
			float _AlphaTestGreater2;

			//float4 _LightDirection;

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{	
				//float2 fromCenter = i.texcoord - float2(0.5,0.5);

				// radius of sphere is 0.5 = a half of 1 tile
				// z of the given point is 1 - len(x) / 0.5
				//		   .  |
				//      .     | this is 0.5
				//    .       |
				//  .         |
				// o----.-----.
				//      |--x--|
				// |---0.5----|

				//float3 normal = normalize(float3(fromCenter.x,fromCenter.y,1-length(fromCenter)/0.5));
				//float nDotL = 1-dot(normal,normalize(_LightDirection.xyz));

				fixed4 incolor = i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				// Multiply
				incolor = lerp(half4(1,1,1, _AlphaTestGreater2), _ScaleColor*incolor,_ScaleAlpha*incolor.a);			
														
				return incolor;

				// multiply double
				//return lerp(fixed4(0.5f,0.5f,0.5f,0.5f), _ScaleColor*incolor, _ScaleAlpha*incolor.a);
				//return _ScaleColor* 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
			}
			ENDCG 
		}
	} 	
}
}
