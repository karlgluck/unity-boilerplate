Shader "Custom/RenderGameToScreen" {
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Smoothing("Smoothing", Float) = 0.0625
		_Color("Color", Color) = (1,1,1,1)
		_Thickness("Thickness", Float) = 0.5
		_OutlineColor("OutlineColor", Color) = (1,1,1,1)
		_OutlineThickness("OutlineThickness", Float) = 0.5
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Background"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas"="True"
		}
		Lighting Off
		Cull Off
		ZTest Always
		ZWrite Off
		Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
		fixed4 _Color;
			float _Smoothing;
			float _Thickness;
			fixed4 _OutlineColor;
			float _OutlineThickness;

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				//float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord.xy;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 color = tex2D(_MainTex, i.texcoord);
				float distance = color.a;
				float smoothing = fwidth(distance) * _Smoothing;
				float alpha = smoothstep(_Thickness - smoothing, _Thickness + smoothing, distance);

				float4 finalColor = float4(_Color.rgb, _Color.a * alpha);


				half outlineAlpha = smoothstep(_OutlineThickness - smoothing, _OutlineThickness + smoothing, distance);
				half4 outline = half4(_OutlineColor.rgb, _OutlineColor.a * outlineAlpha);
				finalColor = lerp(outline, finalColor, alpha);

				return finalColor;
			}
			ENDCG
		}
	}

	Fallback "Transparent/VertexLit"
}
