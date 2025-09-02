#define cameraPos _WorldSpaceCameraPos
#define cameraForward -UNITY_MATRIX_V[2].xyz
#define screenWidth _ScreenParams.x
#define screenHeight _ScreenParams.y
#define screenSize _ScreenParams.xy

// v1 and v2 should be normalized first.
float GetAngle(float3 v1, float3 v2)
{
	return acos(dot(v1, v2));
}

float4x4 inverse(float4x4 m) {
	float
		a00 = m[0][0], a01 = m[0][1], a02 = m[0][2], a03 = m[0][3],
		a10 = m[1][0], a11 = m[1][1], a12 = m[1][2], a13 = m[1][3],
		a20 = m[2][0], a21 = m[2][1], a22 = m[2][2], a23 = m[2][3],
		a30 = m[3][0], a31 = m[3][1], a32 = m[3][2], a33 = m[3][3],

		b00 = a00 * a11 - a01 * a10,
		b01 = a00 * a12 - a02 * a10,
		b02 = a00 * a13 - a03 * a10,
		b03 = a01 * a12 - a02 * a11,
		b04 = a01 * a13 - a03 * a11,
		b05 = a02 * a13 - a03 * a12,
		b06 = a20 * a31 - a21 * a30,
		b07 = a20 * a32 - a22 * a30,
		b08 = a20 * a33 - a23 * a30,
		b09 = a21 * a32 - a22 * a31,
		b10 = a21 * a33 - a23 * a31,
		b11 = a22 * a33 - a23 * a32,

		det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;

	return float4x4(
		a11 * b11 - a12 * b10 + a13 * b09,
		a02 * b10 - a01 * b11 - a03 * b09,
		a31 * b05 - a32 * b04 + a33 * b03,
		a22 * b04 - a21 * b05 - a23 * b03,
		a12 * b08 - a10 * b11 - a13 * b07,
		a00 * b11 - a02 * b08 + a03 * b07,
		a32 * b02 - a30 * b05 - a33 * b01,
		a20 * b05 - a22 * b02 + a23 * b01,
		a10 * b10 - a11 * b08 + a13 * b06,
		a01 * b08 - a00 * b10 - a03 * b06,
		a30 * b04 - a31 * b02 + a33 * b00,
		a21 * b02 - a20 * b04 - a23 * b00,
		a11 * b07 - a10 * b09 - a12 * b06,
		a00 * b09 - a01 * b07 + a02 * b06,
		a31 * b01 - a30 * b03 - a32 * b00,
		a20 * b03 - a21 * b01 + a22 * b00) / det;
}

float3 ClipToNDC(float4 hcs)
{
	return hcs.xyz / hcs.w;
}

float2 NDCToScreenUV(float3 ndc)
{
	ndc.x = Remap(-1.0, 1.0, 0.0, 1.0, ndc.x);
	ndc.y = Remap(-1.0, 1.0, 0.0, 1.0, ndc.y);
	return ndc;
}

float2 ClipToScreenUV(float4 hcs)
{
	return NDCToScreenUV(ClipToNDC(hcs));
}

float2 ClipToScreen(float4 hcs)
{
	return ClipToScreenUV(hcs) * screenSize;
}

float2 UVToScreen(float2 uv)
{
	return uv * screenSize;
}

float2 ObjectToUV(float3 pos)
{
	return ClipToScreenUV(TransformObjectToHClip(pos));
}

float2 ObjectToScreen(float3 pos)
{
	return ObjectToUV(pos) * screenSize;
}

float3 UVToView(float2 uv, float linearDepth)
{
	// Screen is y-inverted.
	uv.y = 1.0 - uv.y;

	//float zScale = linearDepth / _ProjectionParams.y; // divide by near plane
	float zScale = linearDepth * _ProjectionParams.z;

	float4 topLeftCorner = mul(unity_CameraInvProjection, float4(-1, 1, -1, 1));
	float4 topRightCorner = mul(unity_CameraInvProjection, float4(1, 1, -1, 1));
	float4 bottomLeftCorner = mul(unity_CameraInvProjection, float4(-1, -1, -1, 1));

	float3 viewPos = topLeftCorner.xyz
		+ (topRightCorner - topLeftCorner).xyz * uv.x
		+ (bottomLeftCorner - topLeftCorner).xyz * uv.y;
	viewPos *= zScale;

	return float3(viewPos);
}

float3 ViewToWorld(float3 pos)
{
	return mul((real3x3)GetViewToWorldMatrix(), pos).xyz;
}

float3 ClipToWorld(float4 pos)
{
	return ComputeWorldSpacePosition(pos, UNITY_MATRIX_I_VP);
}

float3 ClipToViewDirWS(float4 pos)
{
	// returns a vector pointing towards the camera.
    return GetWorldSpaceViewDir(ClipToWorld(pos));
}

float3 ClipToNormalizedViewDirWS(float4 pos)
{
    return normalize(ClipToViewDirWS(pos));
}
