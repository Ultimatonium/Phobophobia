using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class GetTargetSystem : SystemBase
{
    //EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        //entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        //EntityCommandBuffer.Concurrent ECB = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        NativeArray<Entity> targets = GetEntityQuery(new EntityQueryDesc
        {
            Any = new ComponentType[] { typeof(EnemyTag), typeof(BaseTag) }
        }).ToEntityArray(Allocator.TempJob);
        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();
        ComponentDataFromEntity<HealthData> healthDatas = GetComponentDataFromEntity<HealthData>();
        ComponentDataFromEntity<EnemyTag> enemyTags = GetComponentDataFromEntity<EnemyTag>();
        ComponentDataFromEntity<BaseTag> baseTags = GetComponentDataFromEntity<BaseTag>();
        ComponentDataFromEntity<TowerTag> towerTags = GetComponentDataFromEntity<TowerTag>();

        Entities.WithAny<TowerTag, EnemyTag>().ForEach((Entity entity, int entityInQueryIndex, ref AttackTargetData attackTargetData, in RangeAttackData rangeAttackData) =>
        {
            attackTargetData.target = GetLowestTargetInRange(entity, targets, rangeAttackData.range, positions, healthDatas, enemyTags, baseTags, towerTags);

            /*
            if (target != Entity.Null)
            {
                //ECB.AddComponent(entityInQueryIndex, entity, new AttackTargetData { target = target });
                //EntityManager.AddComponent(entity, typeof(AttackTargetData));
                //EntityManager.SetComponentData(entity, new AttackTargetData { target = target });
                attackTargetData.target = target;
            }
            */
        }
        ).Run();
        targets.Dispose();
        //entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }

    private static Entity GetLowestTargetInRange(Entity source, NativeArray<Entity> targets, float towerRange, ComponentDataFromEntity<Translation> positions, ComponentDataFromEntity<HealthData> healthDatas, ComponentDataFromEntity<EnemyTag> enemyTags, ComponentDataFromEntity<BaseTag> baseTags, ComponentDataFromEntity<TowerTag> towerTags)
    {
        Entity target = Entity.Null;
        for (int i = 0; i < targets.Length; i++)
        {
            if (enemyTags.Exists(source) && enemyTags.Exists(targets[i])) continue;
            if (towerTags.Exists(source) && baseTags.Exists(targets[i])) continue;
            //Debug.Log("Find target");
            //Debug.Log((math.distancesq(positions[enemies[i]].Value, positions[tower].Value) + "|" + (towerRange * towerRange)));
            if (math.distancesq(positions[targets[i]].Value, positions[source].Value) < towerRange * towerRange)
            {
                //Debug.Log("Tower");
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
