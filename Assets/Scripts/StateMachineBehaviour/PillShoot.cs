using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PillShoot : StateMachineBehaviour
{
    public string[] shotProjectileName;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = EntityHelper.GetEntityOfGameObject(animator.gameObject, entityManager);
        AttackTargetData attackTargetData = entityManager.GetComponentData<AttackTargetData>(entity);
        Translation targetPosition = entityManager.GetComponentData<Translation>(attackTargetData.target);

        ParticleSystem[] particleSystems = animator.gameObject.transform.root.gameObject.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystems.Length; i++)
        {
            for (int ii = 0; ii < shotProjectileName.Length; ii++)
            {

                if (particleSystems[i].gameObject.name.Contains(shotProjectileName[ii]))
                {
                    particleSystems[i].gameObject.transform.LookAt(targetPosition.Value);
                    Debug.DrawLine(particleSystems[i].gameObject.transform.position, targetPosition.Value, Color.red, 1);

                    particleSystems[i].Stop();
                    particleSystems[i].Play();
                }
            }
        }
    }
}
