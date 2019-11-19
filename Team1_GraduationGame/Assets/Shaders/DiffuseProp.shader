Shader "Mobile/DiffuseProp"
{
	Properties
	{
		_Color("Color",COLOR) = (0.5,0.5,0.5,1.0)
		_LightLevel("Light Level", Float) = 0.2
		_MainTex("Base (RGB)", 2D) = "white" {}
		_EmissionMap("Emission Map", 2D) = "black" {}
		_EmissionColor("Emission Color", Color) = (1,1,1,0)
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 150
		CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		sampler2D _EmissionMap;
		fixed4 _Color;
		fixed4 _EmissionColor;
		sampler1D _LightLevel;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_EmissionMap;
			float1 _EmissionColor;
		};
		struct Input2
		{
			float2 _EmissionMap;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			o.Albedo = c.rgb;
			o.Emission = _EmissionColor * tex2D(_EmissionMap, IN.uv_EmissionMap);
		}




		//half4 emission = tex2D(_EmissionMap, IN2.uv_EmissionMap) * _EmissionColor;
		//surf.rgb += emission.rgb;
		//return surf;


		ENDCG

	}
		Fallback "Mobile/VertexLit"
}