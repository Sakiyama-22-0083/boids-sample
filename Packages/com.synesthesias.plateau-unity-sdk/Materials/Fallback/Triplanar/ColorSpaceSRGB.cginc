#ifndef COLOR_SPACE_SRGB_INCLUDED
#define COLOR_SPACE_SRGB_INCLUDED

// Unityのプロジェクト設定のColorSpaceがGammaでもLinearでも同じような色合いにするための計算です。
// 参考 : https://light11.hatenadiary.com/entry/2020/02/25/204432

#ifndef FLT_EPSILON
#define FLT_EPSILON 1.192092896e-07
#endif

float3 PositivePowColorSpace(float3 base, float3 power)
{
    return pow(max(abs(base), float3(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
}
            
void LinearToSRGB_float(float3 c, out float3 sRGB)
{
    #if UNITY_COLORSPACE_GAMMA
    sRGB = c;
    #else
    float3 sRGBLo = c * 12.92;
    float3 sRGBHi = (PositivePowColorSpace(c, float3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
    sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    #endif
}

void SRGBToLinear_float(float3 c, out float3 linearRGB)
{
    #if UNITY_COLORSPACE_GAMMA
    linearRGB = c;
    #else
    float3 linearRGBLo = c / 12.92;
    float3 linearRGBHi = PositivePowColorSpace((c + 0.055) / 1.055, float3(2.4, 2.4, 2.4));
    linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    #endif
}

#endif