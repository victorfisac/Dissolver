// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:Legacy Shaders/Diffuse,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:True,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:9361,x:33387,y:33054,varname:node_9361,prsc:2|emission-738-OUT,clip-6760-OUT;n:type:ShaderForge.SFN_Color,id:4969,x:32597,y:32748,ptovrint:False,ptlb:TintColor,ptin:_TintColor,varname:node_4969,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:0;n:type:ShaderForge.SFN_Add,id:738,x:33122,y:33088,varname:node_738,prsc:2|A-1431-OUT,B-9381-RGB;n:type:ShaderForge.SFN_Multiply,id:1431,x:32864,y:32966,cmnt:Rim,varname:node_1431,prsc:2|A-4969-RGB,B-205-RGB;n:type:ShaderForge.SFN_Tex2d,id:205,x:32567,y:32963,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_205,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:7588,x:30800,y:33396,ptovrint:False,ptlb:DissolveAmount,ptin:_DissolveAmount,varname:node_7588,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-0.1,cur:0,max:1.1;n:type:ShaderForge.SFN_OneMinus,id:9489,x:31139,y:33396,varname:node_9489,prsc:2|IN-7588-OUT;n:type:ShaderForge.SFN_RemapRange,id:9282,x:31310,y:33396,varname:node_9282,prsc:2,frmn:0,frmx:1,tomn:-0.6,tomx:0.6|IN-9489-OUT;n:type:ShaderForge.SFN_Add,id:3734,x:31566,y:33448,varname:node_3734,prsc:2|A-9282-OUT,B-2041-R;n:type:ShaderForge.SFN_Tex2d,id:2041,x:31310,y:33625,ptovrint:False,ptlb:Dissolve,ptin:_Dissolve,varname:node_2041,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False|UVIN-9060-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:9060,x:31114,y:33625,varname:node_9060,prsc:2,uv:1;n:type:ShaderForge.SFN_RemapRange,id:5080,x:31805,y:33182,varname:node_5080,prsc:2,frmn:0,frmx:1,tomn:-4,tomx:4|IN-3734-OUT;n:type:ShaderForge.SFN_Clamp01,id:9967,x:32014,y:33182,varname:node_9967,prsc:2|IN-5080-OUT;n:type:ShaderForge.SFN_OneMinus,id:3707,x:32220,y:33182,varname:node_3707,prsc:2|IN-9967-OUT;n:type:ShaderForge.SFN_Append,id:4251,x:32549,y:33161,varname:node_4251,prsc:2|A-3707-OUT,B-4893-OUT;n:type:ShaderForge.SFN_Vector1,id:4893,x:32220,y:33312,varname:node_4893,prsc:2,v1:0;n:type:ShaderForge.SFN_Tex2dAsset,id:3959,x:32549,y:33337,ptovrint:False,ptlb:DissolveRamp,ptin:_DissolveRamp,varname:node_3959,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9381,x:32806,y:33162,varname:node_9381,prsc:2,ntxv:0,isnm:False|UVIN-4251-OUT,TEX-3959-TEX;n:type:ShaderForge.SFN_Subtract,id:6760,x:33111,y:33308,varname:node_6760,prsc:2|A-205-A,B-3707-OUT;proporder:4969-205-7588-2041-3959;pass:END;sub:END;*/

Shader "FK/FKToon/Toon Transparent" {
    Properties {
        _TintColor ("TintColor", Color) = (0,0,0,0)
        _MainTex ("MainTex", 2D) = "white" {}
        _DissolveAmount ("DissolveAmount", Range(-0.1, 1.1)) = 0
        _Dissolve ("Dissolve", 2D) = "black" {}
        _DissolveRamp ("DissolveRamp", 2D) = "black" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TintColor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _DissolveAmount;
            uniform sampler2D _Dissolve; uniform float4 _Dissolve_ST;
            uniform sampler2D _DissolveRamp; uniform float4 _DissolveRamp_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _Dissolve_var = tex2D(_Dissolve,TRANSFORM_TEX(i.uv1, _Dissolve));
                float node_3734 = (((1.0 - _DissolveAmount)*1.2+-0.6)+_Dissolve_var.r);
                float node_3707 = (1.0 - saturate((node_3734*8.0+-4.0)));
                clip((_MainTex_var.a-node_3707) - 0.5);
////// Lighting:
////// Emissive:
                float2 node_4251 = float2(node_3707,0.0);
                float4 node_9381 = tex2D(_DissolveRamp,node_4251);
                float3 emissive = ((_TintColor.rgb*_MainTex_var.rgb)+node_9381.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _DissolveAmount;
            uniform sampler2D _Dissolve; uniform float4 _Dissolve_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float2 uv1 : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _Dissolve_var = tex2D(_Dissolve,TRANSFORM_TEX(i.uv1, _Dissolve));
                float node_3734 = (((1.0 - _DissolveAmount)*1.2+-0.6)+_Dissolve_var.r);
                float node_3707 = (1.0 - saturate((node_3734*8.0+-4.0)));
                clip((_MainTex_var.a-node_3707) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Legacy Shaders/Diffuse"
}
