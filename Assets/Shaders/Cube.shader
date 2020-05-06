Shader "Cube"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _InternalTex ("Internal Albedo (RGB)", 2D) = "white" {}
        _ExternalTex ("External Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _FrontAlpha("Front Alpha", Range(0,1)) = 0.1
    }
    SubShader
    {
		Tags { "Queue"="Transparent" "RenderType" = "Opaque" }
        Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _InternalTex;
        sampler2D _ExternalTex;

        struct Input {
            float2 uv_InternalTex;
            float2 uv_ExternalTex;
            float3 viewDir;
        };

        half _Glossiness;
        half _Metallic;
        half _FrontAlpha;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
            
		void vert(inout appdata_full v) {
			v.normal.xyz = v.normal * -1;
		}

        void surf (Input IN, inout SurfaceOutputStandard o) {
            bool internal = dot(normalize(IN.viewDir), o.Normal * -1) >= 0;
            if (internal) {
                // Albedo comes from a texture tinted by color
                fixed4 c = tex2D (_InternalTex, IN.uv_InternalTex) * _Color;
                o.Albedo = c.rgb;
                o.Alpha = 1;
            }
            if (!internal) {
                fixed4 c = tex2D (_ExternalTex, IN.uv_ExternalTex) * _Color;
                o.Albedo = c.rgb;
                o.Alpha = _FrontAlpha;
			}

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
