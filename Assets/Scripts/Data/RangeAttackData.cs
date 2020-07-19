using Unity.Entities;

[GenerateAuthoringComponent]
public struct RangeAttackData : IComponentData
{
    public float range;
    public float damage;
    public float cadence;
    public float timeUntilShoot;
}
