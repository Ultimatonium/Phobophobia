using Unity.Entities;
using UnityEngine;

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

        ComponentDataFromEntity<HealthData> healthDatas = GetComponentDataFromEntity<HealthData>();

        Entity bank = GetEntityQuery(typeof(MoneyData)).GetSingletonEntity();

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref AttackTargetData attackTarget) =>
        {
            if (attackTarget.target != Entity.Null)
            {
                if (healthDatas[attackTarget.target].health < 0)
                {
                    //ECS.RemoveComponent<AttackTargetData>(entityInQueryIndex, entity);
                    attackTarget.target = Entity.Null;
                }
            }
        }
        ).Schedule();

        Entities.WithAll<EnemyTag>().ForEach((Entity entity, int entityInQueryIndex, Transform transform/*, Animator animator*/, ref AnimationPlayData animationData, in HealthData healthData, in ValueData valueData) =>
        {
            if (healthData.health < 0)
            {
                //ECS.DestroyEntity(entityInQueryIndex, entity);
                animationData.setterType = SetterType.Trigger;
                animationData.parameter = AnimationParameter.die;
                //animator.SetBool("isDying", true);
                EntityManager.DestroyEntity(entity);                
                Object.Destroy(transform.gameObject, 3f);
                EntityManager.GetBuffer<MoneyModifierBufferElement>(bank).Add(new MoneyModifierBufferElement { value = valueData.money });
            }
        }
        ).WithoutBurst().WithStructuralChanges().Run();

        endSimulationEntityCommandBuffer.AddJobHandleForProducer(Dependency);
    }
}
