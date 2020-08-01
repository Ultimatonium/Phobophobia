using Unity.Entities;

[GenerateAuthoringComponent]
public struct CombatStatusData : IComponentData
{
    public CombatStatus status;
}
