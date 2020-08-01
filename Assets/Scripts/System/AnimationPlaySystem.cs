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
                case SetterType.NONE:
                    break;
                case SetterType.Float:
                    Debug.LogError(animationData.setterType + " is not implemented");
                    break;
                case SetterType.Int:
                    Debug.LogError(animationData.setterType + " is not implemented");
                    break;
                case SetterType.Bool:
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
                case SetterType.Trigger:
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
            animationData.parameter = AnimationParameter.NONE;
        }
        ).WithoutBurst().Run();
    }
}
