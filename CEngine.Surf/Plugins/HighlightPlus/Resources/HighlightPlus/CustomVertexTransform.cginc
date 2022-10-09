#ifndef CUSTOM_VERTEX_TRANSFORM_INCLUDED
#define CUSTOM_VERTEX_TRANSFORM_INCLUDED

float4 ComputeVertexPosition(float4 vertex) {
    // Add here any custom vertex transform
    float4 pos = UnityObjectToClipPos(vertex);
    return pos;
}
		
#endif
