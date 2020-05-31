using Unity.Entities;

[GenerateAuthoringComponent]
public struct SpawnData : IComponentData
{
    public Entity enemy;
    public float spawnFrequency;
    public float timeUntilSpawn;
    public int spawnCount;
    public int spawnIncrease;
}
