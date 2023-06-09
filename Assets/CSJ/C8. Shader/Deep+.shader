﻿Shader "Mobile/Deep+" {
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "White" {}
	}
	SubShader
	{
		Tags 
		{
			"Queue" = "Transparent+2"		
		}
		Pass 
		{
			Lighting Off 
			ZWrite Off  
			Cull Back   
			Fog			 
			{
				Mode Off
			}

			Blend SrcAlpha OneMinusSrcAlpha 
			SetTexture [_MainTex]
			{
				Combine texture
			}
		}
	}

	FallBack "Unlit/Texture"	
}