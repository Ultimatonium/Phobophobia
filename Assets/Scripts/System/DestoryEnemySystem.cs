using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class DestoryEnemySystem : SystemBase
{
    BeginSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent ECS = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent();

        Entity target = GetEntityQuery(typeof(BaseTag)).GetSingletonEntity();
        float3 targetPosition = EntityManager.GetComponentData<Translation>(target).Value;

        Entities.WithAll<EnemyTag>().ForEach((int entityInQueryIndex, ref Entity entity, in Translation translation, in HealthData healthData) =>
        {
            if (math.distancesq(targetPosition, translation.Value) < 0.1f
            || healthData.health < 0)
            {
                ECS.DestroyEntity(entityInQueryIndex, entity);
            }
        }
        ).Schedule();
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(Dependency);
    }
}
