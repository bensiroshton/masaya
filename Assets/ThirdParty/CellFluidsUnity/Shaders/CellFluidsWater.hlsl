#ifndef CELLFLUIDS_INCLUDED
#define CELLFLUIDS_INCLUDED

void ping_pong_blend(
    float time, 
    out float blend, out float power, out float time1, out float offset1, out float time2, out float offset2
){
    blend = abs(frac(time)*2-1);
    power = lerp(1, 1.5, abs(blend*2-1));
    time1 = frac(time);
    offset1 = floor(time);
    time2 = frac(time+0.5);
    offset2 = floor(time+0.5+3);
}

float4 FlowTexPhase(
    UnityTexture2D map, float2 uv, float2 veloctiy, 
    float time, float offset, float scale, float distortion
){
    uv = ((uv - time*distortion * veloctiy) + float2(6.7, 1.234) * offset) * scale;
    return SAMPLE_TEXTURE2D(map, map.samplerstate, uv);
}

float4 FlowTex(
    UnityTexture2D map, float2 uv, float2 velocity, float blend, float power, 
    float time1, float offset1, float time2, float offset2, float scale, float distortion
){
    float4 c1 = FlowTexPhase(map, uv, velocity, time1, offset1, scale, distortion);
    float4 c2 = FlowTexPhase(map, uv, velocity, time2, offset2, scale, distortion);
    return lerp(c1, c2, blend);
}

float4 FlowTexNM(
    UnityTexture2D map, float2 uv, float2 velocity, float blend, float power, 
    float time1, float offset1, float time2, float offset2, float scale, float distortion
){
    float4 c1 = FlowTexPhase(map, uv, velocity, time1, offset1, scale, distortion);
    float4 c2 = FlowTexPhase(map, uv, velocity, time2, offset2, scale, distortion);
    return lerp(c1, c2, blend);
}

float4 FlowTexPhase_lod(
    UnityTexture2D map, float2 uv, float2 veloctiy, 
    float time, float offset, float scale, float distortion, float LOD
){
    uv = ((uv - time*distortion * veloctiy) + float2(6.7, 1.234) * offset) * scale;
    return SAMPLE_TEXTURE2D_LOD(map, map.samplerstate, uv, LOD);
}

float4 FlowTex_lod(
    UnityTexture2D map, float2 uv, float2 velocity, float blend, float power, 
    float time1, float offset1, float time2, float offset2, float scale, float distortion, float LOD
){
    float4 c1 = FlowTexPhase_lod(map, uv, velocity, time1, offset1, scale, distortion, LOD);
    float4 c2 = FlowTexPhase_lod(map, uv, velocity, time2, offset2, scale, distortion, LOD);
    return lerp(c1, c2, blend);
}



void WaterVertex_float(
    UnityTexture2D waveMap, float2 uv, float2 veloctiy, float depth, float is_side,
    out float displacement
){
    float blend, power, time1, offset1, time2, offset2;
    float looped_time = frac((_Time.y*_TimeScale/_MapScale)/3.5)*3.5;
    ping_pong_blend(looped_time, blend, power, time1, offset1, time2, offset2);

    float val = FlowTex_lod(waveMap, uv, veloctiy, blend, power, time1, offset1, time2, offset2, 0.075*_MapScale, 3, 0).r;
    displacement = (val-0.7)*length(veloctiy) * smoothstep(0, 0.1, depth);
}

float3 UnpackNormalmapRGorAG(float4 packednormal)
{
    // This do the trick
   packednormal.x *= packednormal.w;

    float3 normal;
    normal.xy = packednormal.xy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}

void Water_float(
    UnityTexture2D waveMap, UnityTexture2D foamMap, float2 uv, float2 veloctiy, float depth, float is_side, float foamValue,
    out float3 color, out float alpha, out float3 normalMap
){
    float blend, power, time1, offset1, time2, offset2;
    float looped_time = frac((_Time.y*_TimeScale/_MapScale)/3.5)*3.5;
    ping_pong_blend(looped_time, blend, power, time1, offset1, time2, offset2);

    float foam = FlowTex(foamMap, uv, veloctiy, blend, power, time1, offset1, time2, offset2, 0.2*_MapScale, 3).r * _Foam * foamValue * (1-is_side);
    float ssfoam = _Subsurface_Foam*foamValue/(_Scattering*0.3+1);
    foam = max(0,(foam - 0.02)) + ssfoam;
    color = lerp(_ScatterColor.rgb, _FoamColor, foam).rgb;
    // foam += ssfoam/(_Scattering+1);

    alpha = foam;
    float4 normalTex = FlowTexNM(waveMap, uv, veloctiy, blend, power, time1, offset1, time2, offset2, 0.2*_MapScale, 3);
    normalMap = UnpackNormalmapRGorAG(normalTex);
    normalMap = lerp(float3(0,0,1), normalMap, (1-is_side) * length(veloctiy)); 
}

#endif // CELLFLUIDS_INCLUDED