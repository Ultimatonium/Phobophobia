using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;

public class PhoboDoesDmg : StateMachineBehaviour
{
    private HashSet<GameObject> hittedEnemies;
    private EntityManager entityManager;
    private PlayerController playerController;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hittedEnemies = new HashSet<GameObject>();
        playerController = animator.gameObject.GetComponent<PlayerController>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int pre = hittedEnemies.Count;
        hittedEnemies.UnionWith(playerController.enemies);
        int post = hittedEnemies.Count;
        if (pre != post)
        {
            PlayVFX.OneShot(animator.gameObject.transform.root.gameObject, new[] { "VFXCharacterFeathers" });
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerController = animator.gameObject.GetComponent<PlayerController>();
        foreach (GameObject item in hittedEnemies)
        {
            Entity enemyEntity = GetEntityOfGameObject(item);
            if (enemyEntity != Entity.Null)
            {
                entityManager.GetBuffer<HealthModifierBufferElement>(enemyEntity).Add(new HealthModifierBufferElement { value = -playerController.damage });
            }
        }
    }

    private Entity GetEntityOfGameObject(GameObject gameObject)
    {
        NativeArray<Entity> entities = entityManager.GetAllEntities();

        for (int i = 0; i < entities.Length; i++)
        {
            if (!entityManager.HasComponent<Transform>(entities[i])) continue;
            if (entityManager.GetComponentObject<Transform>(entities[i]).gameObject == gameObject)
            {
                return entities[i];
            }
        }

        entities.Dispose();

        return Entity.Null;
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
