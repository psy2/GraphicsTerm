Shader "Transparency_Refractive" {
	Properties {
		_EnvTex ("Environment Texture", CUBE) = "black" {}
		_FresnelPower ("Fresnel Power", Range(0, 8)) = 1.55
		_FresnelAlpha ("Fresnel Alpha Intensity", Range(0, 2)) = 1
		_BumpAmt("Distortion", range(0,4096)) = 10
		_MainTex("Tint Color (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		GrabPass{
			Name "BASE"
			Tags{ "LightMode" = "Always" }
		}

		Pass {		//refractive
			Name "BASE"
			Tags{ "LightMode" = "Always" }
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord: TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 view : TEXCOORD0;
				float2 norm : TEXCOORD1;
				float2 uvmain : TEXCOORD2;
				UNITY_FOG_COORDS(3)
			};

			float _BumpAmt;
			float4 _MainTex_ST;
			float3 worldRefl;

			v2f vert(appdata_t v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				o.view.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
				o.view.zw = o.vertex.zw;
				o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			sampler2D None = 0;
			sampler2D _MainTex;

			half4 frag(v2f i) : SV_Target {
				half2 bump = UnpackNormal(tex2D(None, i.norm)).rg;
				float2 offset = bump * _BumpAmt * _GrabTexture_TexelSize.xy;
				i.view.xy = offset * i.view.z + i.view.xy;
				half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.view));
				half4 tint = tex2D(_MainTex, i.uvmain);
				col *= tint;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG

		}

		Pass {			//reflection
			Blend SrcAlpha OneMinusSrcAlpha
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform samplerCUBE _EnvTex;
			uniform float _FresnelPower;
			uniform float _FresnelAlpha;
	
			struct v2f{
				float4 pos : POSITION;
				float3 view : TEXCOORD0;
				float3 norm : TEXCOORD1;
			};
			v2f vert(appdata_tan v)	{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.norm = mul((float3x3)_Object2World, v.normal);
				o.view = WorldSpaceViewDir(v.vertex);
				return o;
			}

			sampler2D None = 0;

			float4 frag(v2f i) : COLOR {
				float3 N = normalize(i.norm);
				float3 V = normalize(i.view);
				float3 R = reflect(-V, N);
				float3 refl = texCUBE(_EnvTex, R).rgb;

				float3 tr = refract(V, N, -0.8f);
				float3 tg = refract(V, N, -0.8f);
				float3 tb = refract(V, N, -0.8f);
				float3 refr;
				refr.r = texCUBE(_EnvTex, tr).r;
				refr.g = texCUBE(_EnvTex, tg).g;
				refr.b = texCUBE(_EnvTex, tb).b;

				float fresnel = saturate(-dot(N, -V));
				fresnel = pow(fresnel, _FresnelPower);
				float3 c = refl * fresnel + refr * (1 - fresnel);
				return float4(c, 1 - fresnel * _FresnelAlpha);
			}
			ENDCG
		}
	}
	FallBack Off
}
