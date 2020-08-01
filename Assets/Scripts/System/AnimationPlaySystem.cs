using Unity.Entities;
using UnityEngine;

public class AnimationPlaySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Animator animator, ref AnimationPlayData animationData) =>
        {
            switch (animationData.setterType)
            {
                case AnimationSetterType.NONE:
                    break;
                case AnimationSetterType.Float:
                    Debug.LogError(animationData.setterType + " is not implemented");
                    break;
                case AnimationSetterType.Int:
                    Debug.LogError(animationData.setterType + " is not implemented");
                    break;
                case AnimationSetterType.Bool:
                    switch (animationData.parameter)
                    {
                        case AnimationParameter.NONE:
                            break;
                        case AnimationParameter.isWalking:
                            animator.SetBool("isWalking", animationData.boolValue);
                            break;
                        case AnimationParameter.isRunning:
                            animator.SetBool("isRunning", animationData.boolValue);
                            break;
                        case AnimationParameter.isBlocking:
                            animator.SetBool("isBlocking", animationData.boolValue);
                            break;
                        case AnimationParameter.isAttacking:
                            animator.SetBool("isAttacking", animationData.boolValue);
                            break;
                        default:
                            Debug.LogError(animationData.parameter + " is not in " + animationData.setterType);
                            break;
                    }
                    break;
                case AnimationSetterType.Trigger:
                    switch (animationData.parameter)
                    {
                        case AnimationParameter.die:
                            animator.SetTrigger("die");
                            break;
                        case AnimationParameter.attack:
                            animator.SetTrigger("attack");
                            break;
                        case AnimationParameter.hitted:
                            animator.SetTrigger("hitted");
                            break;
                        default:
                            Debug.LogError(animationData.parameter + " is not in " + animationData.setterType);
                            break;
                    }
                    break;
                default:
                    break;
            }
            animationData.setterType = AnimationSetterType.NONE;
        }
        ).WithoutBurst().Run();
    }
}
