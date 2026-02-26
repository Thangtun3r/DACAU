Shader "Custom/SeamlessIKOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        // --- PASS 1: THE FILL (The "Union" Mask) ---
        // This draws the grey limb and marks the area as 'occupied'
        Pass
        {
            Stencil
            {
                Ref 1
                Pass Replace
                Comp Always
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            fixed4 _Color;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                clip(col.a - 0.1); // Don't write stencil for transparent pixels
                return col;
            }
            ENDCG
        }

        // --- PASS 2: THE OUTLINE ---
        // This draws the black border but IGNORES pixels already filled in Pass 1
        Pass
        {
            Stencil
            {
                Ref 1
                Comp NotEqual // THE MAGIC: Only draw where there is NO limb pixel
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v) {
                v2f o;
                // Move vertices outward along their normals to create the border
                float3 norm = normalize(v.vertex.xyz);
                v.vertex.xyz += norm * _OutlineWidth;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}