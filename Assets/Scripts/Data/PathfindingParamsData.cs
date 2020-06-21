using Unity.Entities;
using Unity.Mathematics;

public struct PathfindingParamsData : IComponentData
{
    public float3 startPosition;
    public float3 endPosition;
}
