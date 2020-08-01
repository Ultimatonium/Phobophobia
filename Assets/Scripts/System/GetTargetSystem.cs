using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class GetTargetSystem : SystemBase
{
    [ReadOnly] private ComponentDataFromEntity<Translation> positions;
    [ReadOnly] private ComponentDataFromEntity<HealthData> healthDatas;
    [ReadOnly] private ComponentDataFromEntity<EnemyTag> enemyTags;
    [ReadOnly] private ComponentDataFromEntity<BaseTag> baseTags;
    [ReadOnly] private ComponentDataFromEntity<TowerTag> towerTags;
    [ReadOnly] private ComponentDataFromEntity<PlayerTag> playerTags;

    protected override void OnUpdate()
    {
        NativeArray<Entity> targets = GetEntityQuery(new EntityQueryDesc
        {
            Any = new ComponentType[] { typeof(EnemyTag), typeof(BaseTag), typeof(PlayerTag) }
        }).ToEntityArray(Allocator.TempJob);

        positions = GetComponentDataFromEntity<Translation>();
        healthDatas = GetComponentDataFromEntity<HealthData>();
        enemyTags = GetComponentDataFromEntity<EnemyTag>();
        baseTags = GetComponentDataFromEntity<BaseTag>();
        towerTags = GetComponentDataFromEntity<TowerTag>();
        playerTags = GetComponentDataFromEntity<PlayerTag>();

        Entities.WithAny<TowerTag, EnemyTag>().ForEach((Entity entity, ref AttackTargetData attackTargetData, in RangeAttackData rangeAttackData) =>
        {
            //attackTargetData.target = GetLowestTargetInRange(entity, targets, rangeAttackData.range, positions, healthDatas, enemyTags, baseTags, towerTags, playerTags);
            attackTargetData.target = Entity.Null;
            for (int i = 0; i < targets.Length; i++)
            {
                if (!(enemyTags.Exists(entity) && baseTags.Exists(targets[i])
                  || enemyTags.Exists(entity) && playerTags.Exists(targets[i])
                  || towerTags.Exists(entity) && enemyTags.Exists(targets[i])
                  )) continue;
                if (math.distancesq(positions[targets[i]].Value, positions[entity].Value) < rangeAttackData.range * rangeAttackData.range)
                {
                    if (attackTargetData.target == Entity.Null) attackTargetData.target = targets[i];
                    if (healthDatas[attackTargetData.target].health > healthDatas[targets[i]].health)
                    {
                        attackTargetData.target = targets[i];
                    }
                }
            }
        }
        ).WithoutBurst().Run();

        targets.Dispose();
    }

    private static Entity GetLowestTargetInRange(Entity source, NativeArray<Entity> targets, float towerRange
        , ComponentDataFromEntity<Translation> positions
        , ComponentDataFromEntity<HealthData> healthDatas
        , ComponentDataFromEntity<EnemyTag> enemyTags
        , ComponentDataFromEntity<BaseTag> baseTags
        , ComponentDataFromEntity<TowerTag> towerTags
        , ComponentDataFromEntity<PlayerTag> playerTags
        )
    {
        Entity target = Entity.Null;
        for (int i = 0; i < targets.Length; i++)
        {
            if (!(enemyTags.Exists(source) && baseTags.Exists(targets[i])
              || enemyTags.Exists(source) && playerTags.Exists(targets[i])
              || towerTags.Exists(source) && enemyTags.Exists(targets[i])
              )) continue;
            if (math.distancesq(positions[targets[i]].Value, positions[source].Value) < towerRange * towerRange)
            {
                if (target == Entity.Null) target = targets[i];
                if (healthDatas[target].health > healthDatas[targets[i]].health)
                {
                    target = targets[i];
                }
            }
        }
        return target;
    }
}
