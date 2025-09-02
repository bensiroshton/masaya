Shader "Unlit/SpriteSheetShader"
{
    Properties
    {
        [NoScaleOffset]
        _MainTex ("Texture", 2D) = "white" {}
        _TilesX ("Tiles X", Int) = 4
        _TilesY ("Tiles Y", Int) = 4
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100" 
        }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TilesX;
            float _TilesY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
    
                float2 tileSize = float2(1.0f / _TilesX, 1.0f / _TilesY);
                float row = floor(v.uv.z / _TilesX);
                float col = floor(v.uv.z - row * _TilesX);
                //float2 tileOffSet = float2((float) col * tileSize.x, (float) row * tileSize.y);
                float2 tileOffSet = float2(col * tileSize.x, (1.0f - tileSize.y) - row * tileSize.y);
                o.uv = float2(tileOffSet.x + tileSize.x * v.uv.x, tileOffSet.y + tileSize.y * v.uv.y);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
