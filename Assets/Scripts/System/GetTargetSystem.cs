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
            attackTargetData.target = Entity.Null;
            for (int i = 0; i < targets.Length; i++)
            {
                if (!(enemyTags.Exists(entity) && baseTags.Exists(targets[i])
                  || enemyTags.Exists(entity) && playerTags.Exists(targets[i])
                  || towerTags.Exists(entity) && enemyTags.Exists(targets[i])
                  )) continue;
                if (math.distancesq(positions[targets[i]].Value, positions[entity].Value) < rangeAttackData.range * rangeAttackData.range)
                {
                    /*set target if empty*/
                    if (attackTargetData.target == Entity.Null) attackTargetData.target = targets[i];
                    /*set lowest health target*/
                    if (healthDatas[attackTargetData.target].health > healthDatas[targets[i]].health)
                    {
                        attackTargetData.target = targets[i];
                    }
                    /*focus base*/
                    if (baseTags.Exists(targets[i])) {
                        attackTargetData.target = targets[i];
                    }
                }
            }
        }
        ).WithoutBurst().Run();

        targets.Dispose();
    }
}
