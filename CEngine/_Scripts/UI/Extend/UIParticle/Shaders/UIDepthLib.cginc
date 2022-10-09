#ifndef UI_DEPTH_LIB
#define UI_DEPTH_LIB

#if defined(ALPHAMODE_ALPHATEST) || defined(ALPHAMODE_DITHERING)
	uniform fixed _AlphaTestTreshold;//[0-1]
	uniform fixed _DitheringStep;//(0-1]


	inline void discardMaskByAlpha(fixed alpha, float4 screenPos)
	{
	#ifdef ALPHAMODE_ALPHATEST
		clip(alpha - _AlphaTestTreshold);
	#elif ALPHAMODE_DITHERING
		if (alpha > 0.99)
			alpha = 1;
		else
			alpha -= fmod(alpha, _DitheringStep);
		float4x4 thresholdMatrix =
		{ 
			1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
			13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
			4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
			16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
		};
		float4x4 rowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };
		float2 pos = screenPos.xy / screenPos.w;
		pos *= _ScreenParams.xy;
		clip(alpha - thresholdMatrix[fmod(pos.x, 4)] * rowAccess[fmod(pos.y, 4)]);
	#endif
	}
#else//noalpha and translucency
	inline void discardMaskByAlpha(fixed alpha, float4 screenPos)
	{
    }
#endif //UI depth mask


#endif//UI_DEPTH_LIB