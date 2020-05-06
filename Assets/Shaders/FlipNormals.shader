Shader "Flip Normals" {
    Properties {
        _MainTex ("Base (RGBA)", 2D) = "white" {}
        _ExternalTex ("External (RGBA)", 2D) = "white" {}
        _FrontAlpha("Front Alpha", Range(0,1)) = 0.1
    }
    SubShader {
		Tags { "Queue"="Transparent" "RenderType" = "Opaque" }
		Cull Off
		CGPROGRAM
			#pragma surface surf Lambert vertex:vert alpha addshadow
			sampler2D _MainTex;
			sampler2D _ExternalTex;
			half _FrontAlpha;
     
			struct Input {
				float2 uv_MainTex;
				float2 uv_ExternalTex;
				float3 viewDir;
				float4 color : COLOR;
				float3 normal : NORMAL;
			};
            
			void vert(inout appdata_full v) {
				v.normal.xyz = v.normal * -1;
			}
            
			void surf (Input IN, inout SurfaceOutput o) {
				bool internal = dot(normalize(IN.viewDir), o.Normal) >= 0;
				if (internal) {
					float4 result = tex2D(_MainTex, IN.uv_MainTex);
					o.Albedo = result.rgb * IN.color.rgb;
					o.Alpha = result.a;
				}
				if (!internal) {
					float4 result = tex2D(_ExternalTex, IN.uv_ExternalTex);
					o.Albedo = result.rgb * IN.color.rgb;
					o.Alpha = result.a * _FrontAlpha;
				}
			}
		ENDCG
    }
    Fallback "VertexLit"
}