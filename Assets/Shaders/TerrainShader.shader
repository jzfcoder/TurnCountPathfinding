Shader "PushyPixels/TerrainShader" {
Properties
{
	_Color ("Top Color", Color) = (1,1,1,1)
	_Color2 ("Bottom Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_TerrainHeight("Terrain Height", Range(1, 50)) = 10
}
SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

	CGPROGRAM
	#pragma surface surf Lambert
	float _TerrainHeight;

sampler2D _MainTex;
fixed4 _Color;
fixed4 _Color2;

struct Input
{
	float2 uv_MainTex;
	float3 worldPos;
};

void surf (Input IN, inout SurfaceOutput o) {
	float2 screenUV = IN.worldPos.y / _TerrainHeight;
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * lerp(_Color2,_Color,screenUV.y);
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "VertexLit"
}

