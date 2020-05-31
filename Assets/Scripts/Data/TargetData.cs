using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TargetData : IComponentData
{
    public float3 targetPosition;
}
