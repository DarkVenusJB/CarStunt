Shader "Custom/MetalPlatformColor" {
	Properties {
		[Enum(UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0
		_ColorA ("Color A", Vector) = (1,1,1,1)
		_ColorB ("Color B", Vector) = (1,1,1,1)
		_MaskTex ("Mask Tex", 2D) = "white" {}
		_MainTex ("Albedo", 2D) = "white" {}
		[NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
		_NormalStrength ("Normal Strength", Range(-10, 10)) = 0
		_AlbedoStrength ("Albedo Strength", Range(0, 1)) = 0
		_SpecColor ("Specular Color", Vector) = (1,1,1,1)
		[PowerSlider(5)] _Shininess ("Shininess", Range(0.03, 2)) = 0.078125
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Mobile/VertexLit"
}