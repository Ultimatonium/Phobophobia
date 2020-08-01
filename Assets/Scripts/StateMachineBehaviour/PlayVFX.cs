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
}
