using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.Find("RespawnVFXPortal").GetComponent<ParticleSystem>().Play();
        PlayerController playerController = animator.gameObject.GetComponent<PlayerController>();
        animator.gameObject.transform.position = playerController.spawnPostion;
        animator.gameObject.transform.rotation = playerController.spawnRotation;
        playerController.characterCam.transform.parent = null;
        animator.gameObject.transform.position += new Vector3(0, 5, 0);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController playerController = animator.gameObject.GetComponent<PlayerController>();
        float maxHealth = playerController.entityManager.GetComponentData<HealthData>(playerController.player).maxHealth;
        playerController.entityManager.SetComponentData(playerController.player, new HealthData { health = maxHealth, maxHealth = maxHealth });
        animator.ResetTrigger("die");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetTrigger("land");
    }
}
