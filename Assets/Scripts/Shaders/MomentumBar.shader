// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/MomentumBar"
{
    Properties
    {
        [Header(Liquid properties)]
        _LiquidColourMax ("Liquid Colour Max", Color) = (1, 1, 1, 1)
        _LiquidColourMin ("Liquid Colour Min", Color) = (0, 0, 0, 0)
        _LiquidEmmissionMax ("Liquid Emmision Max", Color) = (0, 0, 0, 0)
        _LiquidEmmissionMin ("Liquid Emmision Min", Color) = (0, 0, 0, 0)
        _LevelMax ("Level Max", Float) = 1
        _LevelMin ("Level Min", Float) = 0
        _LiquidIOR ("Liquid IOR", Float) = 1
        _LiquidThickness ("Liquid Thickness", Float) = 1
        
        [Header(Glass properties)]
        _GlassColour ("Glass Colour", Color) = (1, 1, 1, 1)
        _GlassIOR ("Glass IOR", Float) = 1
        _GlassThickness("Glass Thickness", Float) = 1
        _GlassVariation("Glass Variation Map", 2D) = "grey" {}
        _GlassVaryStr("Glass Variation Strength", Float) = 1

        [Header(Shine properties)]
        _Roughness ("Roughness", Range(0,1)) = 0.5
        _Specular ("Specular", Float) = 1

        [Header(Level properties)]
        _Level ("Level", Float) = 0
        [KeywordEnum(X, Y, Z)] _LevelAxis ("Level Axis", Float) = 0
        [Toggle(INVERT_LEVEL)] _InvertLevel ("Invert Level Axis", Float) = 0
        _LevelWaveAmp ("Wave Amplitude", Float) = 0.1
        _LevelWaveFreq ("Wave Frequency", Float) = 1
        _LevelWaveDensity ("Wave Density", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "Lightmode"="ForwardBase"}
        LOD 100

        // extra pass that renders to depth buffer only
        //Pass {
        //    ZWrite On
        //    ColorMask 0
        //}

        GrabPass{
            //"_GrabTexture1"
            // Uncomment to cause all textures to use the same grabbed texture
        }

        Pass{
            //ZWrite Off
            //Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _LEVELAXIS_X _LEVELAXIS_Y _LEVELAXIS_Z
            #pragma shader_feature INVERT_LEVEL
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // for _LightColor0
            
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f{
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                // this is a slot to put our screen coordinates into
                // it is a float4 instead of float2
                // because we need to use tex2Dproj() instead of tex2D()
                float4 screenUV : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                half3 worldNormal : TEXCOORD3;
                half3 viewNormal : TEXCOORD4;
                half3 normal : TEXCOORD5;
                float3 viewDir : TEXCOORD6;
                fixed4 diff : COLOR0; // diffuse lighting color
            };

            float4 _LiquidColourMax;
            float4 _LiquidColourMin;
            float _LevelMax;
            float _LevelMin;
            float _LiquidIOR;
            float _LiquidThickness;
            float4 _LiquidEmmissionMax;
            float4 _LiquidEmmissionMin;
            float _GlassThickness;
            float _Roughness;
            float _Level;
            float _LevelWaveDensity;
            float _LevelWaveFreq;
            float _LevelWaveAmp;
            float _LevelWaveZOffset;
            
            // builtin variable to get Grabbed Texture if GrabPass has no name
            sampler2D _GrabTexture;
            
            v2f vert (appdata v) {
                v2f o;
#ifdef _LEVELAXIS_X
                float3 levelDir = float3(0, 1, 0);
#elif _LEVELAXIS_Y
                float3 levelDir = float3(0, 0, 1);
#elif _LEVELAXIS_Z
                float3 levelDir = float3(1, 0, 0);
#endif
                float3 vertex = v.vertex - (v.normal * _GlassThickness);
                float level = _Level + (_LevelWaveAmp * sin((vertex * _LevelWaveDensity * levelDir) + (_Time.y * _LevelWaveFreq)));
#ifdef _LEVELAXIS_X
#ifdef INVERT_LEVEL
                vertex.x = max(vertex.x, level);
#else
                vertex.x = min(vertex.x, level);
#endif
#elif _LEVELAXIS_Y
#ifdef INVERT_LEVEL
                vertex.y = max(vertex.y, level);
#else
                vertex.y = min(vertex.y, level);
#endif
#elif _LEVELAXIS_Z
#ifdef INVERT_LEVEL
                vertex.z = max(vertex.z, level);
#else
                vertex.z = min(vertex.z, level);
#endif
#endif
                o.vertex = UnityObjectToClipPos(vertex);
                o.uv = v.uv;
                // compute world space position of the vertex
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewNormal = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal));
                o.normal = v.normal;

                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                half nl = max(0, dot(o.worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                o.diff = nl * _LightColor0;
                o.diff.rgb += ShadeSH9(half4(o.worldNormal,1));

                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));//ObjSpaceViewDir is similar, but localspace.

                UNITY_TRANSFER_FOG(o,o.vertex);
                // builtin function to get screen coordinates for tex2Dproj()
                o.screenUV = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed3 refract (float3 viewDir, float3 worldNormal, float thickness, float iorFrom, float iorTo) {
                float viewAngle = acos(dot(viewDir, worldNormal) / sqrt(length(viewDir) * length(worldNormal)));
                float internalAngle = asin((iorFrom * sin(viewAngle)) / iorTo);
                float rayHeightInternal = thickness * tan(internalAngle);
                float rayHeightExternal = thickness * tan(viewAngle);
                float rayOffsetLength = rayHeightExternal - rayHeightInternal;
                fixed3 rayOffset = normalize(worldNormal) * rayOffsetLength * -1;
                return rayOffset;
            }
            
            fixed4 frag(v2f i) : SV_TARGET {
                
                fixed AIR_IOR = 1;
                fixed4 liquidRefractOffset = 0;
                liquidRefractOffset.xy = refract(i.viewDir, i.worldNormal, _LiquidThickness, AIR_IOR, _LiquidIOR);

                float ndotv = dot(i.viewDir, i.worldNormal);
                float fresnel = pow(1.0 - saturate(ndotv), 2.0);

                fixed4 grab = tex2Dproj(
                    _GrabTexture,
                    i.screenUV + liquidRefractOffset
                    //i.screenUV + float4( sin((_Time.y * 10)+i.screenUV.x*32.0)*0.1, 0, 0, 0)
                );

                // compute view direction and reflection vector
                // per-pixel here
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                half3 worldRefl = reflect(-worldViewDir, i.worldNormal);

                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);
                fixed4 reflCol = 0;
                reflCol.rgb = skyColor;
                float spec = 1 - _Roughness;

                float levelBlend = abs((_Level - _LevelMin) / (_LevelMax - _LevelMin));

                float4 refractedColour = (grab * (1 - fresnel)) * lerp(_LiquidColourMin, _LiquidColourMax, levelBlend);
                refractedColour += (reflCol * spec * fresnel * i.diff);
                
                return refractedColour + lerp(_LiquidEmmissionMin, _LiquidEmmissionMax, levelBlend);// * i.diff;
                //return lerp(refractedColour * i.diff, _GlassColour * i.diff * 0.5, saturate(fresnel + _GlassThickness));
            }

            ENDCG
        }

        GrabPass{
            //"_GrabTexture2"
            // Uncomment to cause all textures to use the same grabbed texture
        }

        Pass{
            //ZWrite Off
            //Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // for _LightColor0
            
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f{
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                // this is a slot to put our screen coordinates into
                // it is a float4 instead of float2
                // because we need to use tex2Dproj() instead of tex2D()
                float4 screenUV : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                half3 worldNormal : TEXCOORD3;
                half3 viewNormal : TEXCOORD4;
                half3 normal : TEXCOORD5;
                float3 viewDir : TEXCOORD6;
                fixed4 diff : COLOR0; // diffuse lighting color
            };

            float4 _GlassColour;
            float _GlassIOR;
            float _LiquidIOR;
            float _GlassThickness;
            sampler2D _GlassVariation;
            float _GlassVaryStr;
            float _Roughness;
            float _Specular;
            
            // builtin variable to get Grabbed Texture if GrabPass has no name
            sampler2D _GrabTexture;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                // compute world space position of the vertex
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewNormal = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal));
                o.normal = v.normal;

                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                half nl = max(0, dot(o.worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                o.diff = nl * _LightColor0;
                o.diff.rgb += ShadeSH9(half4(o.worldNormal,1));

                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));//ObjSpaceViewDir is similar, but localspace.

                UNITY_TRANSFER_FOG(o,o.vertex);
                // builtin function to get screen coordinates for tex2Dproj()
                o.screenUV = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed3 refract (float3 viewDir, float3 worldNormal, float thickness, float iorFrom, float iorTo) {
                float viewAngle = acos(dot(viewDir, worldNormal) / sqrt(length(viewDir) * length(worldNormal)));
                float internalAngle = asin((iorFrom * sin(viewAngle)) / iorTo);
                float rayHeightInternal = thickness * tan(internalAngle);
                float rayHeightExternal = thickness * tan(viewAngle);
                float rayOffsetLength = rayHeightExternal - rayHeightInternal;
                fixed3 rayOffset = normalize(worldNormal) * rayOffsetLength * -1;
                return rayOffset;
            }
            
            fixed4 frag(v2f i) : SV_TARGET {
                
                fixed varyThick = (tex2D(_GlassVariation, i.uv).r - 0.5) * _GlassVaryStr;
                
                fixed AIR_IOR = 1;
                fixed4 glassRefractOffset = 0;
                glassRefractOffset.xy = refract(i.viewDir, i.worldNormal, _GlassThickness + varyThick, _LiquidIOR, _GlassIOR);

                float ndotv = dot(i.viewDir, i.worldNormal);
                float fresnel = pow(1.0 - saturate(ndotv), 2.0);

                fixed4 grab = tex2Dproj(
                    _GrabTexture,
                    i.screenUV + glassRefractOffset
                    //i.screenUV + float4( sin((_Time.y * 10)+i.screenUV.x*32.0)*0.1, 0, 0, 0)
                );

                // compute view direction and reflection vector
                // per-pixel here
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                half3 worldRefl = reflect(-worldViewDir, i.worldNormal);

                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);
                fixed4 reflCol = 0;
                reflCol.rgb = skyColor;
                float spec = 1 - _Roughness;

                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 specReflectDirection = reflect(-lightDirection, i.worldNormal);
                float3 specSeeDirection = max(0.0,dot(specReflectDirection, i.viewDir));
                float3 shininessPower = pow(specSeeDirection, _Specular);

                float4 refractedColour = (grab * (1 - fresnel)) * _GlassColour;
                refractedColour += (reflCol * spec * fresnel * i.diff);
                refractedColour.rgb += shininessPower;
                
                return refractedColour;// * i.diff;
                //return lerp(refractedColour * i.diff, _GlassColour * i.diff * 0.5, saturate(fresnel + _GlassThickness));
            }

            ENDCG
        }
    }
}
