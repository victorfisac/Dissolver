// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Xffect/displacement/screen" {
Properties {
	_DispMap ("Displacement Map (RG)", 2D) = "white" {}
	_MaskTex ("Mask (R)", 2D) = "white" {}
	_DispScrollSpeedX  ("Map Scroll Speed X", Float) = 0
	_DispScrollSpeedY  ("Map Scroll Speed Y", Float) = 0
	_StrengthX  ("Displacement Strength X", Float) = 1
	_StrengthY  ("Displacement Strength Y", Float) = -1
}

Category {
	Tags { "Queue"="Transparent+99" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	//AlphaTest Greater .01
	Cull Off Lighting Off ZWrite Off ZTest Always
	
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}

	SubShader {
		GrabPass {							
			Name "BASE"
			Tags { "LightMode" = "Always" }
 		}

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct appdata_t {
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float2 texcoord: TEXCOORD0;
	float2 param : TEXCOORD1;
};

struct v2f {
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float2 uvmain : TEXCOORD0;
	float2 param : TEXCOORD1;
	float4 uvgrab : TEXCOORD2;
};

uniform half _StrengthX;
uniform half _StrengthY;

uniform float4 _DispMap_ST;
uniform sampler2D _DispMap;
uniform sampler2D _MaskTex;
uniform half _DispScrollSpeedY;
uniform half _DispScrollSpeedX;

v2f vert (appdata_t v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	#if UNITY_UV_STARTS_AT_TOP
	float scale = -1.0;
	#else
	float scale = 1.0;
	#endif
	o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
	o.uvgrab.zw = o.vertex.zw;
	o.uvmain = TRANSFORM_TEX( v.texcoord, _DispMap );
	o.color = v.color;
	o.param = v.param;
	return o;
}

sampler2D _GrabTexture;

half4 frag( v2f i ) : COLOR
{
	//scroll displacement map.
	half2 mapoft = half2(_Time.y*_DispScrollSpeedX, _Time.y*_DispScrollSpeedY);

	//get displacement color
	half4 offsetColor = tex2D(_DispMap, i.uvmain + mapoft);

	//get offset
	half oftX =  offsetColor.r * _StrengthX * i.param.x;
	half oftY =  offsetColor.g * _StrengthY * i.param.x;

	i.uvgrab.x += oftX;
	i.uvgrab.y += oftY;

	half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));

	//intensity is controlled by particle color.
	col.a = i.color.a;

	//use mask's red channel to determine visibility.
	fixed4 tint = tex2D( _MaskTex, i.uvmain );

	col.a *= tint.r;

	return col;
}
ENDCG
		}
}

	// ------------------------------------------------------------------
	// Fallback for older cards and Unity non-Pro
	
	SubShader {
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			Name "BASE"
			SetTexture [_MainTex] {	combine texture * primary double, texture * primary }
		}
	}
}
}
