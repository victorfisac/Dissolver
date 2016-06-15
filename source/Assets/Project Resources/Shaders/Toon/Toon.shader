Shader "FK/FKToon/Toon"
{
	Properties
	{
		//TOONY COLORS
		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)
		_HColor ("Highlight Color", Color) = (0.6,0.6,0.6,1.0)
		_SColor ("Shadow Color", Color) = (0.4,0.4,0.4,1.0)
        _Emission ("Emission Color", Color) = (0.0, 0.0, 0.0, 0.0)
		
		//DIFFUSE
		_MainTex ("Diffuse (RGB)", 2D) = "white" {}
		
		//TOONY COLORS RAMP
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
		
		//SPECULAR
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range(0.01,2)) = 0.1
		_SpecSmooth ("Smoothness", Range(0,1)) = 0.05
		
		//RIM LIGHT
		_RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimMin ("Rim Min", Range(0,1)) = 0.5
		_RimMax ("Rim Max", Range(0,1)) = 1.0		
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		
		#include "Assets/Project Resources/Shaders/Toon/Include/ToonInclude.cginc"
		
		#pragma surface surf ToonyColorsSpec vertex:vert
		#pragma target 3.0
		#pragma glsl
		
		//================================================================
		// VARIABLES
		
		fixed4 _Color;
		sampler2D _MainTex;
        fixed4 _Emission;
		fixed _Shininess;
		fixed4 _RimColor;
		fixed _RimMin;
		fixed _RimMax;
		float4 _RimDir;
		
		struct Input
		{
			half2 uv_MainTex : TEXCOORD0;
			float3 viewDir;
		};
		
		//================================================================
		// VERTEX FUNCTION		
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
		}
		
		//================================================================
		// SURFACE FUNCTION
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color.rgb;
			o.Alpha = c.a * _Color.a;
			
			//Specular
			o.Gloss = c.a;
			o.Specular = _Shininess;

			//Rim
			float3 viewDir = normalize(IN.viewDir);
			half rim = 1.0f - saturate( dot(viewDir, o.Normal) );
			rim = smoothstep(_RimMin, _RimMax, rim);

			o.Emission += (_RimColor.rgb * rim) * _RimColor.a;
            o.Emission += _Emission;
		}
		
		ENDCG
	}
	
	Fallback "Diffuse"
}