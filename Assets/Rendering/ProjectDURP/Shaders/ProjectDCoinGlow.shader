Shader "ProjectD/Coin Glow"
{
 Properties
 {
  [Header(Color Layers)]
  [HDR] _CenterColor("Center Gold", Color) = (1,0.61,0.11,1)
  [HDR] _EdgeColor("Edge Orange", Color) = (0.34,0.105,0.012,1)
  _LayerPower("Color Falloff", Range(0.2,5)) = 1.15
  [Header(Soft Rim)]
  [HDR] _RimColor("Rim Gold", Color) = (1,0.45,0.055,1)
  _RimIntensity("Rim Intensity", Range(0,5)) = 1.65
  _RimPower("Rim Falloff", Range(0.5,12)) = 3.5
  [Header(Glow And Pulse)]
  _EmissionIntensity("Glow Intensity", Range(0,8)) = 1.65
  _PulseAmount("Pulse Amount", Range(0,0.5)) = 0.08
  _PulseSpeed("Pulse Speed (Hz)", Range(0,4)) = 0.45
  [ToggleUI] _ApplyFog("Apply Scene Fog", Float) = 1
 }
 SubShader
 {
  Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
  HLSLINCLUDE
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
    half4 _CenterColor, _EdgeColor, _RimColor;
    float _LayerPower;
    float _RimIntensity, _RimPower, _EmissionIntensity, _PulseAmount, _PulseSpeed, _ApplyFog;
   CBUFFER_END
  ENDHLSL
  Pass
  {
   Name "CoinGlow"
   Tags { "LightMode"="SRPDefaultUnlit" }
   Cull Back ZWrite On
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_fog
   #pragma multi_compile_instancing
   struct Attributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Varyings {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float3 positionWS:TEXCOORD1;float fog:TEXCOORD2;float phase:TEXCOORD3;UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO};
   Varyings Vert(Attributes input)
   {
    Varyings o=(Varyings)0;UNITY_SETUP_INSTANCE_ID(input);UNITY_TRANSFER_INSTANCE_ID(input,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);o.positionCS=p.positionCS;o.positionWS=p.positionWS;
    o.normalWS=TransformObjectToWorldNormal(input.normalOS);o.fog=ComputeFogFactor(p.positionCS.z);
    // Use the object origin so every fragment of a coin shares one stable pulse phase.
    float3 origin=TransformObjectToWorld(float3(0,0,0));o.phase=frac(dot(origin,float3(0.173,0.317,0.137)))*6.2831853;
    return o;
   }
   half4 Frag(Varyings input):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(input);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    float3 normal=normalize(input.normalWS);float3 view=GetWorldSpaceNormalizeViewDir(input.positionWS);
    float facing=saturate(dot(normal,view));float fresnel=pow(1-facing,_RimPower);
    half3 color=lerp(_EdgeColor.rgb,_CenterColor.rgb,pow(facing,_LayerPower));
    color+=_RimColor.rgb*fresnel*_RimIntensity;
    float pulse=1+sin(_Time.y*_PulseSpeed*6.2831853+input.phase)*_PulseAmount;
    color*=max(0,_EmissionIntensity*pulse);
    color=lerp(color,MixFog(color,input.fog),saturate(_ApplyFog));
    return half4(color,1);
   }
   ENDHLSL
  }
  Pass
  {
   Name "DepthOnly"
   Tags { "LightMode"="DepthOnly" }
   ZWrite On ColorMask R Cull Back
   HLSLPROGRAM
   #pragma vertex DepthVert
   #pragma fragment DepthFrag
   #pragma multi_compile_instancing
   struct Input {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Output {float4 positionCS:SV_POSITION;UNITY_VERTEX_OUTPUT_STEREO};
   Output DepthVert(Input input){Output o=(Output)0;UNITY_SETUP_INSTANCE_ID(input);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(input.positionOS.xyz);return o;}
   half4 DepthFrag(Output input):SV_Target{return 0;}
   ENDHLSL
  }
  Pass
  {
   Name "DepthNormalsOnly"
   Tags { "LightMode"="DepthNormalsOnly" }
   ZWrite On Cull Back
   HLSLPROGRAM
   #pragma vertex NormalsVert
   #pragma fragment NormalsFrag
   #pragma multi_compile_instancing
   struct Input {float4 positionOS:POSITION;float3 normalOS:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Output {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
   Output NormalsVert(Input input){Output o=(Output)0;UNITY_SETUP_INSTANCE_ID(input);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(input.positionOS.xyz);o.normalWS=TransformObjectToWorldNormal(input.normalOS);return o;}
   half4 NormalsFrag(Output input):SV_Target{return half4(normalize(input.normalWS),0);}
   ENDHLSL
  }
 }
 Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
