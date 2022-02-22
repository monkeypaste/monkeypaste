//float Brightness : register(C0);
//float Contrast : register(C1);
//
//sampler2D Texture1Sampler : register(S0);
//
//float4 main(float2 uv : TEXCOORD) : COLOR
//{
//
//   float4 pixelColor = tex2D(Texture1Sampler, uv);
//   pixelColor.rgb /= pixelColor.a;
//
//   // Apply contrast.
//   pixelColor.rgb = ((pixelColor.rgb - 0.5f) * max(Contrast, 0)) + 0.5f;
//
//   // Apply brightness.
//   pixelColor.rgb += Brightness;
//
//   // Return final pixel color.
//   pixelColor.rgb *= pixelColor.a;
//
//
//  return pixelColor;
//}

//from: https://stackoverflow.com/questions/45093399/how-to-invert-color-of-xaml-png-images-using-c/45096471#45096471 
// how to compile:
// fxc /T ps_3_0 /E main /Fo <my shader file>.ps <my shader file>.hlsl

sampler2D input : register(s0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(input, uv);
    float alpha = color.a;

    color = 1 - color;
    color.a = alpha;
    color.rgb *= alpha;

    return color;
}