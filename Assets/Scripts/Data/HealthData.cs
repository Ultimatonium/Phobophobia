using Unity.Entities;

[GenerateAuthoringComponent]
public struct HealthData : IComponentData
{
    public float maxHealth;
    public float health;
    public Tag tag;
}
