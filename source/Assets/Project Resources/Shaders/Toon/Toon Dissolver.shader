// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:Legacy Shaders/Diffuse,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:True,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:9361,x:33387,y:33054,varname:node_9361,prsc:2|emission-738-OUT,custl-2214-OUT,clip-3734-OUT,voffset-1249-OUT;n:type:ShaderForge.SFN_NormalVector,id:5119,x:30142,y:33895,prsc:2,pt:False;n:type:ShaderForge.SFN_LightVector,id:9111,x:30142,y:34077,varname:node_9111,prsc:2;n:type:ShaderForge.SFN_Dot,id:2325,x:30338,y:33952,varname:node_2325,prsc:2,dt:0|A-5119-OUT,B-9111-OUT;n:type:ShaderForge.SFN_Multiply,id:5082,x:30539,y:34043,varname:node_5082,prsc:2|A-2325-OUT,B-6426-OUT;n:type:ShaderForge.SFN_Vector1,id:6426,x:30338,y:34161,varname:node_6426,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Add,id:6921,x:30721,y:34164,varname:node_6921,prsc:2|A-5082-OUT,B-6426-OUT;n:type:ShaderForge.SFN_Max,id:9945,x:30903,y:34224,cmnt:ndl,varname:node_9945,prsc:2|A-6921-OUT,B-5103-OUT;n:type:ShaderForge.SFN_Vector1,id:5103,x:30721,y:34322,varname:node_5103,prsc:2,v1:0;n:type:ShaderForge.SFN_Tex2d,id:9920,x:31289,y:34181,ptovrint:False,ptlb:Ramp,ptin:_Ramp,cmnt:Ramp,varname:node_9920,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:a2daa9b606711a643808bebed4c59a0c,ntxv:2,isnm:False|UVIN-6893-OUT;n:type:ShaderForge.SFN_Append,id:6893,x:31101,y:34181,varname:node_6893,prsc:2|A-9945-OUT,B-9945-OUT;n:type:ShaderForge.SFN_LightAttenuation,id:4059,x:31289,y:34381,varname:node_4059,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7295,x:31486,y:34294,varname:node_7295,prsc:2|A-9920-R,B-4059-OUT;n:type:ShaderForge.SFN_Color,id:2582,x:31486,y:33929,ptovrint:False,ptlb:ShadowColor,ptin:_ShadowColor,varname:node_2582,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Color,id:4087,x:31486,y:34108,ptovrint:False,ptlb:HighColor,ptin:_HighColor,varname:_SColor_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Lerp,id:3323,x:31799,y:34132,varname:node_3323,prsc:2|A-2582-RGB,B-4087-RGB,T-7295-OUT;n:type:ShaderForge.SFN_LightVector,id:2427,x:29672,y:35015,varname:node_2427,prsc:2;n:type:ShaderForge.SFN_ViewVector,id:9594,x:29672,y:35170,varname:node_9594,prsc:2;n:type:ShaderForge.SFN_Add,id:8176,x:29851,y:35077,varname:node_8176,prsc:2|A-2427-OUT,B-9594-OUT;n:type:ShaderForge.SFN_Normalize,id:5298,x:30048,y:35077,cmnt:H,varname:node_5298,prsc:2|IN-8176-OUT;n:type:ShaderForge.SFN_Dot,id:8270,x:30237,y:34974,varname:node_8270,prsc:2,dt:0|A-290-OUT,B-5298-OUT;n:type:ShaderForge.SFN_NormalVector,id:290,x:30048,y:34893,prsc:2,pt:False;n:type:ShaderForge.SFN_Max,id:3739,x:30406,y:35040,cmnt:NDH,varname:node_3739,prsc:2|A-8270-OUT,B-2076-OUT;n:type:ShaderForge.SFN_Vector1,id:2076,x:30213,y:35143,varname:node_2076,prsc:2,v1:0;n:type:ShaderForge.SFN_Color,id:6776,x:30166,y:35447,ptovrint:False,ptlb:Specular,ptin:_Specular,varname:node_6776,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:0;n:type:ShaderForge.SFN_Vector1,id:5151,x:30190,y:35226,varname:node_5151,prsc:2,v1:128;n:type:ShaderForge.SFN_Multiply,id:2252,x:30406,y:35207,varname:node_2252,prsc:2|A-5151-OUT,B-6776-R;n:type:ShaderForge.SFN_Power,id:1046,x:30607,y:35040,varname:node_1046,prsc:2|VAL-3739-OUT,EXP-2252-OUT;n:type:ShaderForge.SFN_Multiply,id:492,x:30894,y:35088,varname:node_492,prsc:2|A-1046-OUT,B-6776-A;n:type:ShaderForge.SFN_Multiply,id:656,x:31177,y:34942,varname:node_656,prsc:2|A-7081-OUT,B-492-OUT;n:type:ShaderForge.SFN_Vector1,id:7081,x:30973,y:34976,varname:node_7081,prsc:2,v1:2;n:type:ShaderForge.SFN_Smoothstep,id:9029,x:31405,y:34795,cmnt:Spec,varname:node_9029,prsc:2|A-2866-OUT,B-3127-OUT,V-656-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3403,x:30547,y:34795,ptovrint:False,ptlb:SpecSmooth,ptin:_SpecSmooth,varname:node_3403,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:2866,x:31048,y:34779,varname:node_2866,prsc:2|A-4174-OUT,B-337-OUT;n:type:ShaderForge.SFN_Vector1,id:2255,x:30559,y:34660,varname:node_2255,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Multiply,id:8680,x:30832,y:34662,varname:node_8680,prsc:2|A-3403-OUT,B-2255-OUT;n:type:ShaderForge.SFN_Subtract,id:3127,x:31013,y:34561,varname:node_3127,prsc:2|A-2255-OUT,B-8680-OUT;n:type:ShaderForge.SFN_Vector1,id:4174,x:30562,y:34908,varname:node_4174,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Multiply,id:337,x:30835,y:34910,varname:node_337,prsc:2|A-3403-OUT,B-4174-OUT;n:type:ShaderForge.SFN_LightAttenuation,id:6823,x:31428,y:35086,varname:node_6823,prsc:2;n:type:ShaderForge.SFN_Multiply,id:3929,x:31664,y:35014,varname:node_3929,prsc:2|A-9029-OUT,B-6823-OUT;n:type:ShaderForge.SFN_Tex2d,id:5819,x:31707,y:34344,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_5819,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2148,x:31974,y:34231,cmnt:Ramp  Diffuse,varname:node_2148,prsc:2|A-3323-OUT,B-5819-RGB;n:type:ShaderForge.SFN_LightColor,id:4898,x:31707,y:34535,varname:node_4898,prsc:2;n:type:ShaderForge.SFN_Multiply,id:3940,x:32149,y:34386,cmnt:Ramp  Diffuse  Light,varname:node_3940,prsc:2|A-2148-OUT,B-4898-RGB;n:type:ShaderForge.SFN_LightColor,id:2099,x:31894,y:34732,varname:node_2099,prsc:2;n:type:ShaderForge.SFN_Multiply,id:8558,x:31894,y:34920,cmnt:Specular  SpecColor,varname:node_8558,prsc:2|A-6776-RGB,B-3929-OUT;n:type:ShaderForge.SFN_Multiply,id:2472,x:32210,y:34821,cmnt:Specular  SpecColor LightColor,varname:node_2472,prsc:2|A-2099-RGB,B-8558-OUT;n:type:ShaderForge.SFN_Fresnel,id:877,x:32597,y:33270,varname:node_877,prsc:2|EXP-7438-OUT;n:type:ShaderForge.SFN_Multiply,id:5530,x:32878,y:33178,cmnt:Rim,varname:node_5530,prsc:2|A-9013-RGB,B-877-OUT,C-9013-A;n:type:ShaderForge.SFN_Color,id:9013,x:32597,y:33117,ptovrint:False,ptlb:RimColor,ptin:_RimColor,varname:node_9013,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:0;n:type:ShaderForge.SFN_ValueProperty,id:7438,x:32368,y:33304,ptovrint:False,ptlb:RimPower,ptin:_RimPower,varname:node_7438,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:3;n:type:ShaderForge.SFN_Color,id:4969,x:32597,y:32748,ptovrint:False,ptlb:EmissionColor,ptin:_EmissionColor,varname:node_4969,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:0;n:type:ShaderForge.SFN_Add,id:738,x:33138,y:33160,varname:node_738,prsc:2|A-1431-OUT,B-5530-OUT,C-9381-RGB;n:type:ShaderForge.SFN_Add,id:2122,x:32457,y:34493,varname:node_2122,prsc:2|A-3940-OUT,B-2472-OUT;n:type:ShaderForge.SFN_Multiply,id:1431,x:32867,y:32955,cmnt:Rim,varname:node_1431,prsc:2|A-4969-RGB,B-205-RGB;n:type:ShaderForge.SFN_Tex2d,id:205,x:32597,y:32923,ptovrint:False,ptlb:EmissionTex,ptin:_EmissionTex,varname:node_205,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:7588,x:30800,y:33396,ptovrint:False,ptlb:DissolveAmount,ptin:_DissolveAmount,varname:node_7588,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-0.1,cur:0,max:1.1;n:type:ShaderForge.SFN_OneMinus,id:9489,x:31139,y:33396,varname:node_9489,prsc:2|IN-7588-OUT;n:type:ShaderForge.SFN_RemapRange,id:9282,x:31310,y:33396,varname:node_9282,prsc:2,frmn:0,frmx:1,tomn:-0.6,tomx:0.6|IN-9489-OUT;n:type:ShaderForge.SFN_Add,id:3734,x:31545,y:33480,varname:node_3734,prsc:2|A-9282-OUT,B-2041-R;n:type:ShaderForge.SFN_Tex2d,id:2041,x:31310,y:33625,ptovrint:False,ptlb:Dissolve,ptin:_Dissolve,varname:node_2041,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False|UVIN-9060-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:9060,x:31114,y:33625,varname:node_9060,prsc:2,uv:1;n:type:ShaderForge.SFN_RemapRange,id:5080,x:31739,y:33480,varname:node_5080,prsc:2,frmn:0,frmx:1,tomn:-4,tomx:4|IN-3734-OUT;n:type:ShaderForge.SFN_Clamp01,id:9967,x:31903,y:33480,varname:node_9967,prsc:2|IN-5080-OUT;n:type:ShaderForge.SFN_OneMinus,id:3707,x:32092,y:33478,varname:node_3707,prsc:2|IN-9967-OUT;n:type:ShaderForge.SFN_Append,id:4251,x:32597,y:33452,varname:node_4251,prsc:2|A-3707-OUT,B-4893-OUT;n:type:ShaderForge.SFN_Vector1,id:4893,x:32081,y:33645,varname:node_4893,prsc:2,v1:0;n:type:ShaderForge.SFN_Tex2dAsset,id:3959,x:32597,y:33640,ptovrint:False,ptlb:DissolveRamp,ptin:_DissolveRamp,varname:node_3959,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9381,x:32853,y:33452,varname:node_9381,prsc:2,ntxv:0,isnm:False|UVIN-4251-OUT,TEX-3959-TEX;n:type:ShaderForge.SFN_NormalVector,id:5112,x:32667,y:34021,prsc:2,pt:False;n:type:ShaderForge.SFN_Multiply,id:1249,x:32852,y:33888,cmnt:Rim,varname:node_1249,prsc:2|A-4632-OUT,B-5112-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9171,x:32406,y:34036,ptovrint:False,ptlb:Displace,ptin:_Displace,varname:node_9171,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:4632,x:32667,y:33856,cmnt:Rim,varname:node_4632,prsc:2|A-6547-OUT,B-9171-OUT;n:type:ShaderForge.SFN_Clamp,id:6547,x:32440,y:33856,varname:node_6547,prsc:2|IN-3707-OUT,MIN-4255-OUT,MAX-2491-OUT;n:type:ShaderForge.SFN_Vector1,id:4255,x:32169,y:33871,varname:node_4255,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:2491,x:32193,y:33964,varname:node_2491,prsc:2,v1:0.05;n:type:ShaderForge.SFN_LightAttenuation,id:2151,x:32387,y:34170,varname:node_2151,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2214,x:32681,y:34254,varname:node_2214,prsc:2|A-2151-OUT,B-2122-OUT;proporder:5819-4087-2582-4969-205-9920-6776-3403-9013-7438-7588-2041-3959-9171;pass:END;sub:END;*/

Shader "FK/FKToon/Toon Dissolve" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _HighColor ("HighColor", Color) = (1,1,1,1)
        _ShadowColor ("ShadowColor", Color) = (0,0,0,1)
        _EmissionColor ("EmissionColor", Color) = (0,0,0,0)
        _EmissionTex ("EmissionTex", 2D) = "white" {}
        _Ramp ("Ramp", 2D) = "black" {}
        _Specular ("Specular", Color) = (0.5,0.5,0.5,0)
        _SpecSmooth ("SpecSmooth", Float ) = 0
        _RimColor ("RimColor", Color) = (0,0,0,0)
        _RimPower ("RimPower", Float ) = 3
        _DissolveAmount ("DissolveAmount", Range(-0.1, 1.1)) = 0
        _Dissolve ("Dissolve", 2D) = "black" {}
        _DissolveRamp ("DissolveRamp", 2D) = "black" {}
        _Displace ("Displace", Float ) = 0
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
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            #pragma glsl
            uniform sampler2D _Ramp; uniform float4 _Ramp_ST;
            uniform float4 _ShadowColor;
            uniform float4 _HighColor;
            uniform float4 _Specular;
            uniform float _SpecSmooth;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _RimColor;
            uniform float _RimPower;
            uniform float4 _EmissionColor;
            uniform sampler2D _EmissionTex; uniform float4 _EmissionTex_ST;
            uniform float _DissolveAmount;
            uniform sampler2D _Dissolve; uniform float4 _Dissolve_ST;
            uniform sampler2D _DissolveRamp; uniform float4 _DissolveRamp_ST;
            uniform float _Displace;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                LIGHTING_COORDS(4,5)
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 _Dissolve_var = tex2Dlod(_Dissolve,float4(TRANSFORM_TEX(o.uv1, _Dissolve),0.0,0));
                float node_3734 = (((1.0 - _DissolveAmount)*1.2+-0.6)+_Dissolve_var.r);
                float node_3707 = (1.0 - saturate((node_3734*8.0+-4.0)));
                v.vertex.xyz += ((clamp(node_3707,0.0,0.05)*_Displace)*v.normal);
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _Dissolve_var = tex2D(_Dissolve,TRANSFORM_TEX(i.uv1, _Dissolve));
                float node_3734 = (((1.0 - _DissolveAmount)*1.2+-0.6)+_Dissolve_var.r);
                clip(node_3734 - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
////// Emissive:
                float4 _EmissionTex_var = tex2D(_EmissionTex,TRANSFORM_TEX(i.uv0, _EmissionTex));
                float node_3707 = (1.0 - saturate((node_3734*8.0+-4.0)));
                float2 node_4251 = float2(node_3707,0.0);
                float4 node_9381 = tex2D(_DissolveRamp,node_4251);
                float3 emissive = ((_EmissionColor.rgb*_EmissionTex_var.rgb)+(_RimColor.rgb*pow(1.0-max(0,dot(normalDirection, viewDirection)),_RimPower)*_RimColor.a)+node_9381.rgb);
                float node_6426 = 0.5;
                float node_9945 = max(((dot(i.normalDir,lightDirection)*node_6426)+node_6426),0.0); // ndl
                float2 node_6893 = float2(node_9945,node_9945);
                float4 _Ramp_var = tex2D(_Ramp,TRANSFORM_TEX(node_6893, _Ramp)); // Ramp
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_4174 = 0.5;
                float node_2255 = 0.5;
                float3 finalColor = emissive + (attenuation*(((lerp(_ShadowColor.rgb,_HighColor.rgb,(_Ramp_var.r*attenuation))*_MainTex_var.rgb)*_LightColor0.rgb)+(_LightColor0.rgb*(_Specular.rgb*(smoothstep( (node_4174+(_SpecSmooth*node_4174)), (node_2255-(_SpecSmooth*node_2255)), (2.0*(pow(max(dot(i.normalDir,normalize((lightDirection+viewDirection))),0.0),(128.0*_Specular.r))*_Specular.a)) )*attenuation)))));
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            #pragma glsl
            uniform sampler2D _Ramp; uniform float4 _Ramp_ST;
            uniform float4 _ShadowColor;
            uniform float4 _HighColor;
            uniform float4 _Specular;
            uniform float _SpecSmooth;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _RimColor;
            uniform float _RimPower;
            uniform float4 _EmissionColor;
            uniform sampler2D _EmissionTex; uniform float4 _EmissionTex_ST;
            uniform float _DissolveAmount;
            uniform sampler2D _Dissolve; uniform float4 _Dissolve_ST;
            uniform sampler2D _DissolveRamp; uniform float4 _DissolveRamp_ST;
            uniform float _Displace;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                LIGHTING_COORDS(4,5)
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 _Dissolve_var = tex2Dlod(_Dissolve,float4(TRANSFORM_TEX(o.uv1, _Dissolve),0.0,0));
                float node_3734 = (((1.0 - _DissolveAmount)*1.2+-0.6)+_Dissolve_var.r);
                float node_3707 = (1.0 - saturate((node_3734*8.0+-4.0)));
                v.vertex.xyz += ((clamp(node_3707,0.0,0.05)*_Displace)*v.normal);
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _Dissolve_var = tex2D(_Dissolve,TRANSFORM_TEX(i.uv1, _Dissolve));
                float node_3734 = (((1.0 - _DissolveAmount)*1.2+-0.6)+_Dissolve_var.r);
                clip(node_3734 - 0.5);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float node_6426 = 0.5;
                float node_9945 = max(((dot(i.normalDir,lightDirection)*node_6426)+node_6426),0.0); // ndl
                float2 node_6893 = float2(node_9945,node_9945);
                float4 _Ramp_var = tex2D(_Ramp,TRANSFORM_TEX(node_6893, _Ramp)); // Ramp
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_4174 = 0.5;
                float node_2255 = 0.5;
                float3 finalColor = (attenuation*(((lerp(_ShadowColor.rgb,_HighColor.rgb,(_Ramp_var.r*attenuation))*_MainTex_var.rgb)*_LightColor0.rgb)+(_LightColor0.rgb*(_Specular.rgb*(smoothstep( (node_4174+(_SpecSmooth*node_4174)), (node_2255-(_SpecSmooth*node_2255)), (2.0*(pow(max(dot(i.normalDir,normalize((lightDirection+viewDirection))),0.0),(128.0*_Specular.r))*_Specular.a)) )*attenuation)))));
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
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
            #pragma glsl
            uniform float _DissolveAmount;
            uniform sampler2D _Dissolve; uniform float4 _Dissolve_ST;
            uniform float _Displace;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 _Dissolve_var = tex2Dlod(_Dissolve,float4(TRANSFORM_TEX(o.uv1, _Dissolve),0.0,0));
                float node_3734 = (((1.0 - _DissolveAmount)*1.2+-0.6)+_Dissolve_var.r);
                float node_3707 = (1.0 - saturate((node_3734*8.0+-4.0)));
                v.vertex.xyz += ((clamp(node_3707,0.0,0.05)*_Displace)*v.normal);
                o.posWorld = mul(_Object2World, v.vertex);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _Dissolve_var = tex2D(_Dissolve,TRANSFORM_TEX(i.uv1, _Dissolve));
                float node_3734 = (((1.0 - _DissolveAmount)*1.2+-0.6)+_Dissolve_var.r);
                clip(node_3734 - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Legacy Shaders/Diffuse"
}
