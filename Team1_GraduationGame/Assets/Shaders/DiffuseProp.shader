Shader "Mobile/DiffuseProp"
{
	Properties
	{
		_Color("Color",COLOR) = (0.5,0.5,0.5,1.0)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_EmissionMap ("Emission Map", 2D) = "black" {}
		_EmissionColor ("Emission Color", Color) = (1,1,1)
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 150
		CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

		}
		ENDCG
		half4 emission = tex2D(_EmissionMap, i.uv) * _EmissionColor;
		output.rgb += emission.rgb;
		return output;
	}
		Fallback "Mobile/VertexLit"
}