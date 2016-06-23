Shader "Custom/Scatter No Wave" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off
		ZWrite On
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert addshadow

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color;
		}

		void vert(inout appdata_full v)
		{
			float4 p = v.vertex;
			v.vertex = p;
		}

		ENDCG
	} 
	FallBack "Diffuse"
}
