Shader "Custom/PlayerColorSwap"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_OutlineSourceColor("Outline Source Color", Color) = (1,1,1,1)
		_OutlineTargetColor("Outline Target Color", Color) = (1,1,1,1)

		_BodySourceColor("Body Source Color", Color) = (1,1,1,1)
		_BodyTargetColor("Body Target Color", Color) = (1,1,1,1)

		_SwordSourceColor("Sword Source Color", Color) = (1,1,1,1)
		_SwordTargetColor("Sword Target Color", Color) = (1,1,1,1)

		_EyeSourceColor("Eye Source Color", Color) = (1,1,1,1)
		_EyeTargetColor("Eye Target Color", Color) = (1,1,1,1)

		_PixelAntiAliasing("Pixel Anti Aliasing", float) = 1
	}

	SubShader
	{
		Tags {"RenderType" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			float4 _OutlineSourceColor;
			float4 _OutlineTargetColor;

			float4 _BodySourceColor;
			float4 _BodyTargetColor;

			float4 _SwordSourceColor;
			float4 _SwordTargetColor;

			float4 _EyeSourceColor;
			float4 _EyeTargetColor;

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);

	#if ETC1_EXTERNAL_ALPHA
				color.a = tex2D(_AlphaTex, uv).r;
	#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			float4 GetBilinearFilteredColor(float2 texcoord)
			{
				float4 s1 = SampleSpriteTexture(texcoord + float2(0.0, _MainTex_TexelSize.y));
				float4 s2 = SampleSpriteTexture(texcoord + float2(_MainTex_TexelSize.x, 0.0));
				float4 s3 = SampleSpriteTexture(texcoord + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y));
				float4 s4 = SampleSpriteTexture(texcoord);

				float2 TexturePosition = float2(texcoord)* _MainTex_TexelSize.z;

				float fu = frac(TexturePosition.x);
				float fv = frac(TexturePosition.y);

				float4 tmp1 = lerp(s4, s2, fu);
				float4 tmp2 = lerp(s1, s3, fu);

				return lerp(tmp1, tmp2, fv);
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag(v2f data) : SV_Target
			{
				/*
				// Pixel Art Upscaling shader made by t3ssel8r, which in turn was inspired by Cole Cecil
				// Ultimately unused because I cannot for the life of me figure out how to get the palette swap to work with bilinear filtering

				// box filter size in texel units
				float2 boxSize = clamp(fwidth(data.uv) * _MainTex_TexelSize.zw, 1e-5, 1);
				// scale uv by texture size to get texel coordinate
				float2 tx = data.uv * _MainTex_TexelSize.zw - 0.5 * boxSize;
				// compute offset for pixel-sized box filter
				float2 txOffset = smoothstep(1 - boxSize, 1, frac(tx));
				// compute bilinear sample uv coordinates
				float2 uv = (floor(tx) + 0.5 + txOffset) * _MainTex_TexelSize.xy;
				// sample the texture
				return tex2Dgrad(_MainTex, uv, ddx(data.uv), ddy(data.uv));
				*/

				float4 col = tex2D(_MainTex, data.uv);

				float outlineDelta = length(col - _OutlineSourceColor);
				float bodyDelta = length(col - _BodySourceColor);
				float swordDelta = length(col - _SwordSourceColor);
				float eyeDelta = length(col - _EyeSourceColor);
				float tolerance = 0.1;

				col = step(outlineDelta, tolerance) * half4(_OutlineTargetColor) + step(tolerance, outlineDelta) * col;
				col = step(bodyDelta, tolerance) * half4(_BodyTargetColor) + step(tolerance, bodyDelta) * col; 
				col = step(swordDelta, tolerance) * half4(_SwordTargetColor) + step(tolerance, swordDelta) * col;
				col = step(eyeDelta, tolerance) * half4(_EyeTargetColor) + step(tolerance, eyeDelta) * col;

				return col;
			}

			ENDCG
		}
	}
}