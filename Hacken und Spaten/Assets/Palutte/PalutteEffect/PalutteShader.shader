Shader "Palutte/Palutte"
{
	Properties
	{
		//the image we're gonna make into a pretty dithered paletteified thingy
		_MainTex ("Texture", 2D) = "grey" {}

		//lookup texture, this shader takes linear LUTs only
		_LUTTex("LUT", 2D) = "white" {}

		//how many tiles wide is the lookup texture
		_LUTBlueTilesX("LUT grid width", Float) = 16.0
		_LUTBlueTilesY("LUT grid height", Float) = 16.0
		_GridFractionX("LUT grid fraction X", Float) = 0.0625
		_GridFractionY("LUT grid fraction Y", Float) = 0.0625

		//4x4 texture to determine the dither pattern
		_DitherTex("Dither Matrix", 2D) = "grey" {}

		//just how much dither do we ditherrrrrrr
		_DitherRange("Dither Range", Range(0,0.5)) = 0.1
		
		//how many pixels wide/tall we divide the render into
		_PWidth("PixelsWide", Float) = 200
		_PHeight("PixelsTall", Float) = 150
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		//LOD 100
			

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"


			sampler2D _MainTex;
			//float4 _MainTex_ST;

			sampler2D _LUTTex;
			sampler2D _DitherTex;

			float _DitherRange;

			float _LUTBlueTilesX;
			float _LUTBlueTilesY;
			float _GridFractionX;
			float _GridFractionY;

			float _PWidth;
			float _PHeight;


			fixed4 DoLUT(fixed4 c) {
				//float gridFractionX = 1.0 / _LUTBlueTilesX;
				//float gridFractionY = 1.0 / _LUTBlueTilesY;
				//float gridFraction = 0.0625;

				c.r = clamp(0.001, 0.99, c.r);
				c.g = clamp(0.001, 0.99, c.g);

				float2 lutuv;


				lutuv.x = c.r * _GridFractionX + floor(c.b*_LUTBlueTilesX)*_GridFractionX;
				//lutuv.x = c.r * 0.0625 + floor(c.b*16)*0.0625;
				
				//lutuv.y = 1.0 - c.g;
				lutuv.y = c.g * _GridFractionY + floor(c.b*_LUTBlueTilesY)*_GridFractionY;
				lutuv.y = 1.0 - lutuv.y;

				return  tex2D(_LUTTex, lutuv);
			}
			
			fixed4 frag (v2f_img i) : SV_Target {

				// sample the texture based on pixel width/height
				float2 pixelUV = i.uv;
				pixelUV.x = floor(pixelUV.x*_PWidth)*(1.0/ _PWidth);
				pixelUV.y = floor(pixelUV.y*_PHeight)*(1.0 / _PHeight);
				fixed4 col = tex2D(_MainTex, pixelUV);


				// add dither values to the colours
				float2 ditherUV = i.uv;
				ditherUV.x *= _PWidth / 4;//these are assuming 4x4 matrix, change if using a different resolution
				ditherUV.y *= _PHeight / 4;
				col += _DitherRange * (tex2D(_DitherTex, ditherUV) - 0.5);

				// get final colour by sampling the Lookup Texture
				col = DoLUT(col);

				return col;
			}
			ENDCG
		}
	}
}
