Shader "Custom/ForceField" {
	Properties {
		_Color ( "Color", Color ) = ( 1, 0, 0, 1 )
		_Opacity ( "Opacity", Range ( 0, 1 ) ) = 0.5
		_OpacityExponent ( "Opacity Exponent", Range ( 0.5, 1.5 ) ) = 1
		_Additiveness ( "Additiveness", Range ( 0, 1 ) ) = 0
		_Convexiness ( "Convexiness", Float ) = 2

		_WaveTex ( "Wave (RGB)", 2D ) = "white" {}
		_WaveColor ( "Wave Color", Color ) = ( 0, 1, 0, 1 )
		_WaveThickness ( "Wave Thickness", Float ) = 1
		_NumWaves ( "Num Waves", Float ) = 1
		_Twirl ( "Twirl", Float ) = 0
		_WaveSpeed ( "Wave Speed", Float ) = 1

		_RadialShiftTex ( "Radial Shift (RGB)", 2D ) = "black" {}
		_RadialShiftAmount ( "Radial Shift Amount", Range ( 0, 1 ) ) = 0
	}
	SubShader {
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		LOD 200
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off } 
		Blend One SrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _WaveTex;
			uniform float4 _WaveTex_ST;
			uniform float4 _Color;
			uniform float _Opacity;
			uniform float _OpacityExponent;
			uniform float _Additiveness;
			uniform float _Convexiness;
			uniform float4 _WaveColor;
			uniform float _WaveThickness;
			uniform float _NumWaves;
			uniform float _Twirl;
			uniform float _WaveSpeed;

			sampler2D _RadialShiftTex;
			uniform float4 _RadialShiftTex_ST;
			uniform float _RadialShiftAmount;

			struct VertexInput {
				float2 tc0 : TEXCOORD0;
				float4 vertex : POSITION;
			};

			struct FragmentInput {
				float2 tc0 : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			FragmentInput vert ( VertexInput i ) {
				FragmentInput o;
				o.tc0 = i.tc0;
				o.pos = mul ( UNITY_MATRIX_MVP, i.vertex );

				return	o;
			}

			#define Radius		0.5
			#define Pi			3.141592653589793
			#define Pi2			( Pi * 2 )

			float4 frag ( FragmentInput i ) : COLOR {
				float2 rv = i.tc0 - float2 ( 0.5, 0.5 );
				float t = saturate ( pow ( length ( rv ) / Radius, _Convexiness ) );
				float tAlpha = lerp ( _Opacity, 0, t );
				tAlpha = pow ( tAlpha, _OpacityExponent );

				float rvAngle = atan2 ( rv.y, rv.x );
				float tPolar = saturate ( ( rvAngle + Pi ) / Pi2 );
				float tPolarRefl = tPolar * 2;
				tPolarRefl = tPolarRefl > 1 ? 2 - tPolarRefl : tPolarRefl;
				float2 polarTc = float2 ( tPolarRefl, t );
				float4 ps = tex2D ( _RadialShiftTex, TRANSFORM_TEX ( polarTc, _RadialShiftTex ) );
				float tRadialShift = ps.a * ( ps.r + ps.g + ps.b ) / 3;

				float waveAngle = t * Pi2 * _NumWaves - _Time.y * _WaveSpeed * Pi2;
				waveAngle += tPolar * Pi2 * round ( _Twirl );
				waveAngle += tRadialShift * Pi2 * _RadialShiftAmount;

				float tWave = saturate ( sin ( waveAngle % Pi2 ) );
				tWave = pow ( tWave, _WaveThickness );

				float4 waveColor = _WaveColor * tex2D ( _WaveTex, TRANSFORM_TEX ( polarTc, _WaveTex ) );
				float bgAlpha = _Color.a * tAlpha;
				float waveAlpha = waveColor.a * tWave * tAlpha;
				float4 c = lerp ( _Color * bgAlpha, waveColor, waveAlpha );
				c.a = ( 1 - bgAlpha ) * ( 1 - waveAlpha );
				c.a = lerp ( c.a, 1, _Additiveness );

				return	float4 ( c.rgb, c.a );
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
