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

        Entities.WithAll<EnemyTag>().ForEach((NavMeshAgent agent, ref RangeAttackData rangeAttackData, in Translation translation) =>
        {
            if (math.distancesq(positions[target].Value, translation.Value) < rangeAttackData.range * rangeAttackData.range)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
            }
        }
        ).WithoutBurst().Run();
    }
}
