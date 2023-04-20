Shader "Custom/Sha_Water"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Tex_Noise("Distotion", 2D) = "white" {}

		_Noise_Speed("Noise_Speed", Range(0, 1)) = 0.1
		_Noise_Power("Noise_Power", Range(0, 5)) = 1.5
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM

			#pragma surface surf Standard 

			sampler2D _MainTex;
			sampler2D _Tex_Noise;

			float _Noise_Speed;
			float _Noise_Power;

			struct Input
			{
				float2 uv_MainTex;
				float2 uv_Tex_Noise;
			};

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 d = tex2D(_Tex_Noise, IN.uv_Tex_Noise + (_Time.y * _Noise_Speed));
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex * 3.0f + (d.r * _Noise_Power));


				o.Emission = c.rgb;

				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}