using UnityEngine;

public class AttachCharacterCam : StateMachineBehaviour
{

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController playerController = animator.gameObject.GetComponent<PlayerController>();
        animator.gameObject.transform.position = playerController.spawnPostion;
        animator.gameObject.transform.rotation = playerController.spawnRotation;
        playerController.characterCam.transform.parent = animator.transform.gameObject.transform;
        GameObject.Find("RespawnVFXPortal").GetComponent<ParticleSystem>().Stop();
    }
}
