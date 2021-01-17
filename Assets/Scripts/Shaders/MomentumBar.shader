// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/MomentumBar"
{
    Properties
    {
        _LiquidColour ("Liquid Colour", Color) = (1, 1, 1, 1)
        _LiquidIOR ("Liquid IOR", Float) = 1
        _LiquidThickness ("Liquid Thickness", Float) = 1
        _GlassColour ("Glass Colour", Color) = (1, 1, 1, 1)
        _GlassIOR ("Glass IOR", Float) = 1
        _GlassThickness("Glass Thickness", Float) = 1
        _GlassVariation("Glass Variation Map", 2D) = "grey" {}
        _GlassVaryStr("Glass Variation Strength", Float) = 1
        _Roughness ("Roughness", Range(0,1)) = 0.5
        _MinCutoff ("Min Cutoff", Vector) = (0,0,0,0)
        _MaxCutoff ("Max Cutoff", Vector) = (1,0,0,0)
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
            // "_BGTex"
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

            float4 _LiquidColour;
            float _LiquidIOR;
            float _LiquidThickness;
            float4 _GlassColour;
            float _GlassIOR;
            float _GlassThickness;
            sampler2D _GlassVariation;
            float _GlassVaryStr;
            float4 _MinCutoff;
            float4 _MaxCutoff;
            float _Roughness;
            
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
                fixed4 liquidRefractOffset = 0;
                liquidRefractOffset.xy = refract(i.viewDir, i.worldNormal, _LiquidThickness, AIR_IOR, _LiquidIOR);
                fixed4 glassRefractOffset = 0;
                glassRefractOffset.xy = refract(i.viewDir, i.worldNormal, _GlassThickness + varyThick, _LiquidIOR, _GlassIOR);

                float ndotv = dot(i.viewDir, i.worldNormal);
                float fresnel = pow(1.0 - saturate(ndotv), 2.0);

                fixed4 grab = tex2Dproj(
                    _GrabTexture,
                    i.screenUV + liquidRefractOffset + glassRefractOffset
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

                float4 refractedColour = (grab * (1 - fresnel)) * _LiquidColour * _GlassColour;
                refractedColour += (reflCol * spec * fresnel * i.diff);
                
                return refractedColour;// * i.diff;
                //return lerp(refractedColour * i.diff, _GlassColour * i.diff * 0.5, saturate(fresnel + _GlassThickness));
            }

            ENDCG
        }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            fixed4 frag(v2f i) : SV_TARGET {
                return float4(1,1,0,1);
            }

            ENDCG
        }
    }
}
