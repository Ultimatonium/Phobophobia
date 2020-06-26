using Unity.Entities;

[GenerateAuthoringComponent]
public struct TowerData : IComponentData
{
    public float range;
    public float damage;
    public float cadence;
    public float timeUntilShoot;
}
