using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class DestoryEnemySystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent ECS = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent();

        Entity target = GetEntityQuery(typeof(BaseTag)).GetSingletonEntity();
        float3 targetPosition = EntityManager.GetComponentData<Translation>(target).Value;

        Entities.WithAll<EnemyTag>().ForEach((int entityInQueryIndex, ref Entity entity, in Translation translation) =>
        {
            if (math.distancesq(targetPosition, translation.Value) < 0.1f)
            {
                ECS.DestroyEntity(entityInQueryIndex, entity);
            }
        }
        ).Schedule();
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(Dependency);
    }
}
