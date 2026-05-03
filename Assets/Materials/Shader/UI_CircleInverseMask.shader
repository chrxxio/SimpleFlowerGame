Shader "UI/Circle Inverse Mask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}   // REQUIRED for UI
        _Color ("Overlay Color", Color) = (0,0,0,1)

        _Center ("Circle Center", Vector) = (0.5,0.5,0,0)
        _Radius ("Circle Radius", Float) = 0.5
        _Softness ("Edge Softness", Float) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;   // REQUIRED
            float4 _MainTex_ST;

            fixed4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.uv, _Center.xy);

                // 0 inside circle (visible hole), 1 outside (black)
                float mask = smoothstep(_Radius, _Radius + _Softness, dist);

                fixed4 col = _Color * i.color;
                col.a *= mask;

                return col;
            }

            ENDCG
        }
    }
}