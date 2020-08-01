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
        animator.ResetTrigger("dead");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetTrigger("landed");
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
