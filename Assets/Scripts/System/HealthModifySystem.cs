using Unity.Entities;

public class HealthModifySystem : SystemBase
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

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref HealthData healthData, ref DynamicBuffer<HealthModifierBufferElement> healthModifiers) => {
            for (int i = 0; i < healthModifiers.Length; i++)
            {
                healthData.health += healthModifiers[i].value;
            }
            ECS.SetBuffer<HealthModifierBufferElement>(entityInQueryIndex, entity);
        }
        ).Schedule();
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(Dependency);
    }
}
