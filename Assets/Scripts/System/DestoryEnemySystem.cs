using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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

        Entities.WithAll<EnemyTag>().ForEach((Entity entity, int entityInQueryIndex, Transform transform, Animator animator, in HealthData healthData) =>
        {
            if (healthData.health < 0)
            {
                //ECS.DestroyEntity(entityInQueryIndex, entity);
                animator.SetBool("isDying", true);
                EntityManager.DestroyEntity(entity);                
                Object.Destroy(transform.gameObject, 3f);
            }
        }
        ).WithoutBurst().WithStructuralChanges().Run();

        endSimulationEntityCommandBuffer.AddJobHandleForProducer(Dependency);
    }
}
