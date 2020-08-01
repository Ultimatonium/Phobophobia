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
            Entity enemyEntity = EntityHelper.GetEntityOfGameObject(item, entityManager);
            if (enemyEntity != Entity.Null)
            {
                entityManager.GetBuffer<HealthModifierBufferElement>(enemyEntity).Add(new HealthModifierBufferElement { value = -playerController.damage });
            }
        }
    }


}
