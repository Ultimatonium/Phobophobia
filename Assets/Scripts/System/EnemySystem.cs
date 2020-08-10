using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class EnemySystem : SystemBase
{
    private float timer = 0f;
    private float footstepSpeed = 0.3f;

    private void PlayFootstepAudio(Transform transform)
    {
      if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 10f))
      {
        switch(hitInfo.collider.tag)
        {
          case "Metal":
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemies/Footsteps/Metal", transform.gameObject);
            break;
          case "Sand":
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemies/Footsteps/Sand", transform.gameObject);
            break;
          case "Wood":
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemies/Footsteps/Wood", transform.gameObject);
            break;
          default: //If there is no tag, use what probably is most adequate.
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemies/Footsteps/Sand", transform.gameObject);
            break;
        }
      }
    }

    protected override void OnUpdate()
    {
        Entity baseTarget = GetEntityQuery(typeof(BaseTag)).GetSingletonEntity();

        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();

        Entities.WithAll<EnemyTag>().ForEach((NavMeshAgent agent, Transform transform, ref RangeAttackData rangeAttackData, ref AnimationPlayData animationData, in Translation translation, in HealthData healthData, in AttackTargetData attackTargetData) =>
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

            /*50% chance to keep focusing the base*/
            if (UnityEngine.Random.Range(0,1) == 0)
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

              if(timer > footstepSpeed)
              {
                PlayFootstepAudio(transform);
                timer = 0f;
              }

              timer += UnityEngine.Time.deltaTime;
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
