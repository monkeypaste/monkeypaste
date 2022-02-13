float Brightness : register(C0);
float Contrast : register(C1);

sampler2D Texture1Sampler : register(S0);

float4 main(float2 uv : TEXCOORD) : COLOR
{

   float4 pixelColor = tex2D(Texture1Sampler, uv);
   pixelColor.rgb /= pixelColor.a;

   // Apply contrast.
   pixelColor.rgb = ((pixelColor.rgb - 0.5f) * max(Contrast, 0)) + 0.5f;

   // Apply brightness.
   pixelColor.rgb += Brightness;

   // Return final pixel color.
   pixelColor.rgb *= pixelColor.a;


  return pixelColor;
}