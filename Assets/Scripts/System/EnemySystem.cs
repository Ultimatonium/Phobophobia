using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class EnemySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entity baseTarget = GetEntityQuery(typeof(BaseTag)).GetSingletonEntity();

        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();

        Entities.WithAll<EnemyTag>().ForEach((NavMeshAgent agent, ref RangeAttackData rangeAttackData, ref AnimationPlayData animationData, in Translation translation, in HealthData healthData, in AttackTargetData attackTargetData) =>
        {
            /*set agent stop distance*/
            agent.stoppingDistance = rangeAttackData.range;

            /*set agent target*/
            if (attackTargetData.target != Entity.Null)
            {
                agent.SetDestination(positions[attackTargetData.target].Value);
            }
            else
            {
                agent.SetDestination(positions[baseTarget].Value);
            }

            /*set walking animation*/
            animationData.setterType = AnimationSetterType.Bool;
            animationData.parameter = AnimationParameter.isWalking;
            if (agent.velocity == Vector3.zero)
            {
                animationData.boolValue = false;
            }
            else
            {
                animationData.boolValue = true;
            }

            /*stop if dead*/
            if (healthData.health <= 0)
            {
                agent.isStopped = true;
            }
        }
        ).WithoutBurst().Run();
    }
}
