using Unity.Entities;

[GenerateAuthoringComponent]
public class CombatStatusData : IComponentData
{
    public CombatStatus status;
}
