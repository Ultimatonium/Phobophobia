using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayVFX : StateMachineBehaviour
{
    public string[] vfxNames;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OneShot(animator.gameObject.transform.root.gameObject, vfxNames);   
    }

    public static void OneShot(GameObject rootGameObject, string[] vfxNames)
    {
        ParticleSystem[] particleSystems = rootGameObject.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystems.Length; i++)
        {
            for (int ii = 0; ii < vfxNames.Length; ii++)
            {

                if (particleSystems[i].gameObject.name.Contains(vfxNames[ii]))
                {
                    particleSystems[i].Stop();
                    particleSystems[i].Play();
                }
            }
        }
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
