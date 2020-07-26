using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class EnemySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entity target = GetEntityQuery(typeof(BaseTag)).GetSingletonEntity();
        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();

        Entities.WithAll<EnemyTag>().ForEach((NavMeshAgent agent, Animator animator, ref RangeAttackData rangeAttackData, in Translation translation, in HealthData healthData) =>
        {
            if (math.distancesq(positions[target].Value, translation.Value) < rangeAttackData.range * rangeAttackData.range)
            {
                agent.isStopped = true;
                animator.SetBool("isWalking", false);
            }
            else
            {
                agent.isStopped = false;
                animator.SetBool("isWalking", true);
            }

            if (healthData.health <= 0)
            {
                agent.isStopped = true;
            }
        }
        ).WithoutBurst().Run();
    }
}
