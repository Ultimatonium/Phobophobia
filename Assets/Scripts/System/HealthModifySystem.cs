using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class HealthModifySystem : SystemBase
{
    protected override void OnUpdate()
    {
        ComponentDataFromEntity<EnemyTag> enemyTags = GetComponentDataFromEntity<EnemyTag>();

        Entities.ForEach((Entity entity, Transform transform, ref HealthData healthData, ref DynamicBuffer<HealthModifierBufferElement> healthModifiers, ref AnimationPlayData animationData, in CombatStatusData combatStatus) =>
        {
            if (combatStatus.status != CombatStatus.Blocking)
            {
                for (int i = 0; i < healthModifiers.Length; i++)
                {
                    healthData.health += healthModifiers[i].value;

                    if(enemyTags.Exists(entity))
                      FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemies/Hitsound/Hitsound", transform.gameObject); //Event Macro - Cooldown: 240 ms!

                    animationData.setterType = AnimationSetterType.Trigger;
                    animationData.parameter = AnimationParameter.hitted;
                }
            }

            healthModifiers.Clear();
        }
        ).WithoutBurst().Run();

        Entities.ForEach((HealthBar healthBar, ref HealthData healthData) =>
        {
            healthBar.SetHealth(healthData.health, healthData.maxHealth);
        }
        ).WithoutBurst().Run();
    }
}
