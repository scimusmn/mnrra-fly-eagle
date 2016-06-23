Shader "Custom/Scatter Grass" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert vertex:vert alphatest:_Cutoff addshadow

sampler2D _MainTex;
fixed4 _Color;
		float _scaleadj;
		float _speedadj;
		float _distadj;


struct Input {
	float2 uv_MainTex;
	half4 color : COLOR0;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rrr * _Color * IN.color;
	o.Alpha = c.a;
}

		void vert(inout appdata_full v)
		{
			float4 p = v.vertex;

			p.x += sin((_Time.y * _speedadj) + (p.x * _distadj)) * v.color.a * _scaleadj;
			p.z += cos((_Time.y * _speedadj) + (p.z * _distadj)) * v.color.a * _scaleadj;

			v.vertex = p;
		}
ENDCG
}

Fallback "Transparent/Cutout/VertexLit"
}

