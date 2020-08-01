using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class HealthModifySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref HealthData healthData, ref DynamicBuffer<HealthModifierBufferElement> healthModifiers, ref AnimationPlayData animationData, in CombatStatusData combatStatus) =>
        {
            if (combatStatus.status != CombatStatus.Blocking)
            {
                for (int i = 0; i < healthModifiers.Length; i++)
                {
                    healthData.health += healthModifiers[i].value;

                    animationData.setterType = AnimationSetterType.Trigger;
                    animationData.parameter = AnimationParameter.hitted;
                }
            }
            healthModifiers.Clear();
        }
        ).Schedule();

        Entities.ForEach((HealthBar healthBar, ref HealthData healthData) =>
        {
            healthBar.SetHealth(healthData.health, healthData.maxHealth);
        }
        ).WithoutBurst().Run();
    }
}
