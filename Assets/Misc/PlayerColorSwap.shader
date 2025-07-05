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

			float4 _OutlineSourceColor;
			float4 _OutlineTargetColor;

			float4 _BodySourceColor;
			float4 _BodyTargetColor;

			float4 _SwordSourceColor;
			float4 _SwordTargetColor;

			float4 _EyeSourceColor;
			float4 _EyeTargetColor;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);
				half4 newCol = col;

				float outlineDelta = length(col - _OutlineSourceColor);
				float bodyDelta = length(col - _BodySourceColor);
				float swordDelta = length(col - _SwordSourceColor);
				float eyeDelta = length(col - _EyeSourceColor);
				float tolerance = 0.1;

				newCol = step(outlineDelta, tolerance) * half4(_OutlineTargetColor) + step(tolerance, outlineDelta) * newCol;
				newCol = step(bodyDelta, tolerance) * half4(_BodyTargetColor) + step(tolerance, bodyDelta) * newCol; 
				newCol = step(swordDelta, tolerance) * half4(_SwordTargetColor) + step(tolerance, swordDelta) * newCol;
				newCol = step(eyeDelta, tolerance) * half4(_EyeTargetColor) + step(tolerance, eyeDelta) * newCol;

				return newCol;
			}

			ENDCG
		}
	}
}