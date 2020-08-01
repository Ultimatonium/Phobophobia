using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class EnemySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entity baseTarget = GetEntityQuery(typeof(BaseTag)).GetSingletonEntity();
        /*
        NativeArray<Entity> targets = GetEntityQuery(new EntityQueryDesc
        {
            Any = new ComponentType[] { typeof(PlayerTag), typeof(BaseTag) }
        }).ToEntityArray(Allocator.TempJob);*/

        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();

        Entities.WithAll<EnemyTag>().ForEach((NavMeshAgent agent, /*Animator animator,*/ ref RangeAttackData rangeAttackData, ref AnimationPlayData animationData, in Translation translation, in HealthData healthData, in AttackTargetData attackTargetData) =>
        {
            if (attackTargetData.target != Entity.Null)
            {
                agent.SetDestination(positions[attackTargetData.target].Value);
            }
            else
            {
                agent.SetDestination(positions[baseTarget].Value);
            }

            animationData.setterType = SetterType.Bool;
            animationData.parameter = AnimationParameter.isWalking;
            if (agent.velocity == Vector3.zero)
            {
                animationData.boolValue = false;
                //animator.SetBool("isWalking", false);

            }
            else
            {
                animationData.boolValue = true;
                //animator.SetBool("isWalking", true);

            }
            /*
            if (math.distancesq(positions[target].Value, translation.Value) < rangeAttackData.range * rangeAttackData.range)
            {
                //agent.isStopped = true;
                animator.SetBool("isWalking", false);
            }
            else
            {
                //agent.isStopped = false;
                animator.SetBool("isWalking", true);
            }
            */

            if (healthData.health <= 0)
            {
                agent.isStopped = true;
            }
        }
        ).WithoutBurst().Run();
    }
}
