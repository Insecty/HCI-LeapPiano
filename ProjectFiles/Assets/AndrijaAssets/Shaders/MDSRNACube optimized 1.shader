// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Andrija/Remaster/Lambert Diffuse Specular Rim Ambient Textured Normal Specular Cubemap optimized 1" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse Texture", 2D) = "white" {}
		_BumpMap ("Normal Texture",2D) = "bump" {} // default value of bump tells unity to store RG in AG channels		
		_NormalZScale ("Normal Z Scale", Float) = 1
		_SpecMap ("Specular Texture", 2D) = "white" {}
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
		_Shininess ("Shininess", Float) = 10
		_EmitMap ("Emission Texture", 2D) = "black" {}
		_EmitStrength("Emission Strength", Float) = 1		
		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower ("Rim Power", Float) = 3.0	
		_Cube ("Cube Map", Cube) = "" {}
		_ReflectiveColor ("Reflective Color", Color) = (0,0,0,0)
		_AmbientColor("Ambient Color", Color) = (0,0,0,0)
		// TODO: rim color not affected by light color - pure rim light
	}
	SubShader {Cull Off
		Pass{
			Tags 
			{
				//"RenderType" = "Opaque" 
				"LightMode" = "ForwardBase"
			}
			CGPROGRAM
			
			#pragma target 3.0
			// includes
			#include "UnityCG.cginc"
			
			// pragmas
			#pragma vertex vert
			#pragma fragment frag
			
			//#pragma exclude_renderers flash
			
			// user defined variables
			
			fixed4 _Color;
			sampler2D _MainTex;
			half4 _MainTex_ST;
			sampler2D _BumpMap;
			half4 _BumpMap_ST;
			half _NormalZScale;
			
			sampler2D _SpecMap;
			half4 _SpecMap_ST;
			
			sampler2D _EmitMap;
			half4 _EmitMap_ST;
			fixed _EmitStrength;
			
			fixed4 _LightColor0;
			fixed4 _SpecColor;
			half _Shininess;
			fixed4 _RimColor;
			half _RimPower;
			
			samplerCUBE _Cube;
			fixed4 _ReflectiveColor;

			fixed4 _AmbientColor;
						
			// base input struct
			struct vertexInput {
				half4 vertex : POSITION;
				fixed3 normal : NORMAL;
				half4 texcoord : TEXCOORD0;
				fixed4 tangent : TANGENT;
			};
			struct vertexOutput {
				half4 pos : SV_POSITION;
				half4 tex : TEXCOORD0;
				fixed4 lightDirection : TEXCOORD1;				
				fixed3 viewDirection : TEXCOORD2;
				fixed3 normalWorld : TEXCOORD3;				
				fixed3 tangentWorld : TEXCOORD4;
				fixed3 binormalWorld : TEXCOORD5;
			};
			
			// vertex function			
			vertexOutput vert (vertexInput v){
				vertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.tex = v.texcoord;				
				o.normalWorld = normalize(mul( fixed4(v.normal, 0.0), unity_WorldToObject).xyz);				
				o.tangentWorld = normalize ( fixed3( mul( unity_ObjectToWorld, fixed4( fixed3(v.tangent.xyz), 0.0)).xyz));
				o.binormalWorld = normalize ( cross( o.normalWorld, o.tangentWorld).xyz * v.tangent.w);
				
				half4 posWorld = mul(unity_ObjectToWorld,  v.vertex);
				o.viewDirection = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);	
				half3 point_to_light = _WorldSpaceLightPos0.xyz - posWorld.xyz;				
				o.lightDirection = fixed4(normalize(lerp(_WorldSpaceLightPos0.xyz, point_to_light, _WorldSpaceLightPos0.w)),
									lerp ( 1.0, 1.0/length(point_to_light), _WorldSpaceLightPos0.w));
				
				return o;			
			}
			
			// fragment function
			fixed4 frag (vertexOutput i) : COLOR
			{				
					            
				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// normal map sampling
				fixed4 texN = tex2D(_BumpMap, i.tex.xy * _BumpMap_ST.xy + _BumpMap_ST.zw);
				// unpackNormal function - manually is faster
				fixed3 localCoords = fixed3(2.0 * texN.ag - fixed2(1.0, 1.0), 0.0);
				localCoords.z = _NormalZScale;
				// normal transpose matrix
				fixed3x3 local2WorldTranspose = fixed3x3(
					i.tangentWorld,
					i.binormalWorld,
					i.normalWorld
				);
				// calculate normal direction
				fixed3 normalDirection = normalize( mul( localCoords, local2WorldTranspose));
				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// lightning		
				fixed NdotL = max( 0, dot(normalDirection, i.lightDirection.xyz));																			
				fixed3 diffuseColor = NdotL * _Color.rgb;
				fixed3 ambientColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				// Anisotropy
				fixed3 h = normalize(i.lightDirection.rgb + i.viewDirection);
				fixed NdotH = dot(normalDirection, h);

				float3 specularColor = tex2D(_SpecMap, i.tex.xy * _SpecMap_ST.xy + _SpecMap_ST.zw).rgb * _LightColor0.rgb * 4 * _SpecColor.rgb *
						saturate( pow(NdotH,128.0 * _Shininess) );
				
				// old, "normal" specular
				//fixed3 specularColor = tex2D(_SpecMap, i.tex.xy * _SpecMap_ST.xy + _SpecMap_ST.zw).rgb * 
				//		_SpecColor.xyz * pow( max(0.0, dot( reflect(-i.lightDirection.xyz, normalDirection), i.viewDirection ) ), _Shininess);
				
				// NOTE: currently, rim color is not multiplied by __LightColor0.xyz
				// possible changes: multiply by light color, remove *NdotL
				fixed3 rimColor = NdotL * _RimColor.rgb *  saturate(pow((1 - saturate(dot(i.viewDirection,normalDirection))), _RimPower));	
				
				// emission
				fixed3 emitColor = _EmitStrength * tex2D(_EmitMap, i.tex.xy * _EmitMap_ST.xy + _EmitMap_ST.zw).rgb;
							
				// reflection	
				float3 reflectedDir = reflect(i.viewDirection, normalDirection);
				float3 reflectedColor = _ReflectiveColor.rgb * texCUBE (_Cube, reflectedDir).xyz;
						
				// NOTE: currently, diffuse texture color is multiplied after calculating final color
				fixed3 finalColor = _AmbientColor + emitColor + reflectedColor + _LightColor0.xyz * (
									diffuseColor + specularColor + rimColor) * i.lightDirection.w;
								
				// textures	
				fixed3 diffuseTextureColor = tex2D(_MainTex, i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw).rgb; 
				
				return fixed4(finalColor*diffuseTextureColor,1);
				
				// arguable
				// _LightColor.xyz * diffuse only, or all but ambient?
				//	/distance_to_light diffuse only, or all but ambient?
				// currently: all
				// diffuseTextureColor * all but emissive, or * all? 
				// currently: * all
			}
	
			ENDCG
		}
		
		Pass{
			Tags 
			{
				//"RenderType" = "Opaque" 
				"LightMode" = "ForwardAdd"
			}
			Blend One One
			
			CGPROGRAM
			
			#pragma target 3.0
			// includes
			#include "UnityCG.cginc"
			
			// pragmas
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma exclude_renderers flash
			
			// user defined variables
			
			fixed4 _Color;
			sampler2D _MainTex;
			half4 _MainTex_ST;
			sampler2D _BumpMap;
			half4 _BumpMap_ST;
			half _NormalZScale;
			
			sampler2D _SpecMap;
			half4 _SpecMap_ST;
			
			sampler2D _EmitMap;
			half4 _EmitMap_ST;
			fixed _EmitStrength;
			
			fixed4 _LightColor0;
			fixed4 _SpecColor;
			half _Shininess;
			fixed4 _RimColor;
			half _RimPower;			
					
			// base input struct
			struct vertexInput {
				half4 vertex : POSITION;
				fixed3 normal : NORMAL;
				half4 texcoord : TEXCOORD0;
				fixed4 tangent : TANGENT;
			};
			struct vertexOutput {
				half4 pos : SV_POSITION;
				half4 tex : TEXCOORD0;
				fixed4 lightDirection : TEXCOORD1;				
				fixed3 viewDirection : TEXCOORD2;
				fixed3 normalWorld : TEXCOORD3;				
				fixed3 tangentWorld : TEXCOORD4;
				fixed3 binormalWorld : TEXCOORD5;
			};
			
			// vertex function			
			vertexOutput vert (vertexInput v){
				vertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.tex = v.texcoord;				
				o.normalWorld = normalize(mul( fixed4(v.normal, 0.0), unity_WorldToObject).xyz);				
				o.tangentWorld = normalize ( fixed3( mul( unity_ObjectToWorld, fixed4( fixed3(v.tangent.xyz), 0.0)).xyz));
				o.binormalWorld = normalize ( cross( o.normalWorld, o.tangentWorld).xyz * v.tangent.w);
				
				half4 posWorld = mul(unity_ObjectToWorld,  v.vertex);
				o.viewDirection = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);	
				half3 point_to_light = _WorldSpaceLightPos0.xyz - posWorld.xyz;				
				o.lightDirection = fixed4(normalize(lerp(_WorldSpaceLightPos0.xyz, point_to_light, _WorldSpaceLightPos0.w)),
									lerp ( 1.0, 1.0/length(point_to_light), _WorldSpaceLightPos0.w));
				
				return o;			
			}
			
			// fragment function
			fixed4 frag (vertexOutput i) : COLOR
			{				
					            
				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// normal map sampling
				fixed4 texN = tex2D(_BumpMap, i.tex.xy * _BumpMap_ST.xy + _BumpMap_ST.zw);
				// unpackNormal function - manually is faster
				fixed3 localCoords = fixed3(2.0 * texN.ag - fixed2(1.0, 1.0), 0.0);
				localCoords.z = _NormalZScale;
				// normal transpose matrix
				fixed3x3 local2WorldTranspose = fixed3x3(
					i.tangentWorld,
					i.binormalWorld,
					i.normalWorld
				);
				// calculate normal direction
				fixed3 normalDirection = normalize( mul( localCoords, local2WorldTranspose));
				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// lightning		
				fixed NdotL = max( 0, dot(normalDirection, i.lightDirection.xyz));		
																																																																			
				fixed3 diffuseColor = NdotL * _Color.rgb;
				fixed3 ambientColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
							
				// Anisotropy
				fixed3 h = normalize(i.lightDirection.rgb + i.viewDirection);
				fixed NdotH = dot(normalDirection, h);

				float3 specularColor = tex2D(_SpecMap, i.tex.xy * _SpecMap_ST.xy + _SpecMap_ST.zw).rgb * _LightColor0.rgb * 4 * _SpecColor.rgb *
						saturate( pow(NdotH,128.0 * _Shininess) );
				

				// old, "normal" specular
				//fixed3 specularColor = tex2D(_SpecMap, i.tex.xy * _SpecMap_ST.xy + _SpecMap_ST.zw).rgb * 
				//		_SpecColor.xyz * pow( max(0.0, dot( reflect(-i.lightDirection.xyz, normalDirection), i.viewDirection ) ), _Shininess);
				
				// NOTE: currently, rim color is not multiplied by __LightColor0.xyz
				// possible changes: multiply by light color, remove *NdotL
				fixed3 rimColor = NdotL * _RimColor.rgb *  saturate(pow((1 - saturate(dot(i.viewDirection,normalDirection))), _RimPower));	
								
				// translucency
				//float3 backScatterColor = _BackScatter.rgb * max(0.0,dot(normalDirection, -i.lightDirection));
				//float3 translucenceColor = _Translucence.rgb * saturate(pow( max( 0.0,dot( -i.lightDirection, i.viewDirection ) ), _Intensity ));
				
				// NOTE: currently, diffuse texture color is multiplied after calculating final color
				fixed3 finalColor = _LightColor0.xyz * (//translucenceColor + backScatterColor + 
								diffuseColor + specularColor + rimColor) * i.lightDirection.w;
				
				// textures	
				fixed3 diffuseTextureColor = tex2D(_MainTex, i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw).rgb; 
				
				return fixed4(finalColor*diffuseTextureColor,1);
				
				// arguable
				// _LightColor.xyz * diffuse only, or all but ambient?
				//	/distance_to_light diffuse only, or all but ambient?
				// currently: all
				// diffuseTextureColor * all but emissive, or * all? 
				// currently: * all
			}
	
			ENDCG
		}
	} 
	//Fallback "Transparent/Diffuse"
}
