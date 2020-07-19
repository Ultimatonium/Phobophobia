using Unity.Entities;

[GenerateAuthoringComponent]
public struct AttackTargetData : IComponentData
{
    public Entity target;
}
