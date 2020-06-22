using Unity.Collections;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct MoveData : IComponentData
{
    public float speed;
}
