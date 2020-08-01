using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class AttackTargetSystem : SystemBase
{
    protected override void OnUpdate()
    {
        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();
        ComponentDataFromEntity<TowerTag> towerTags = GetComponentDataFromEntity<TowerTag>();
        ComponentDataFromEntity<EnemyTag> enemyTags = GetComponentDataFromEntity<EnemyTag>();

        BufferFromEntity<HealthModifierBufferElement> healthModifiersBuffers = GetBufferFromEntity<HealthModifierBufferElement>();
        float dt = Time.DeltaTime;

        Entities.WithAny<TowerTag, EnemyTag>().ForEach((Entity entity, Transform transform, ref RangeAttackData rangeAttackData, ref Rotation rotation, ref AnimationPlayData animationData, in LocalToWorld localToWorld, in AttackTargetData attackTarget) =>
        {
            if (attackTarget.target != Entity.Null)
            {
                rangeAttackData.timeUntilShoot -= dt;
                if (rangeAttackData.timeUntilShoot < 0)
                {
                    rangeAttackData.timeUntilShoot = CadenceToFrequency(rangeAttackData.cadence);

                    healthModifiersBuffers[attackTarget.target].Add(new HealthModifierBufferElement { value = -rangeAttackData.damage });
                    rotation.Value = quaternion.LookRotation(positions[attackTarget.target].Value - positions[entity].Value, localToWorld.Up);
                    rotation.Value.value.x = 0;
                    rotation.Value.value.z = 0;

                    if (enemyTags.Exists(entity))
                    {
                        animationData.setterType = AnimationSetterType.Trigger;
                        animationData.parameter = AnimationParameter.attack;
                    }
                    if (towerTags.Exists(entity))
                    {
                        animationData.setterType = AnimationSetterType.Bool;
                        animationData.parameter = AnimationParameter.isAttacking;
                        animationData.boolValue = true;
                    }

                    transform.rotation = rotation.Value;
                }
            }
            else
            {
                rangeAttackData.timeUntilShoot = 0;
                if (towerTags.Exists(entity))
                {
                    animationData.setterType = AnimationSetterType.Bool;
                    animationData.parameter = AnimationParameter.isAttacking;
                    animationData.boolValue = false;
                }
            }
        }
        ).WithoutBurst().Run();

    }

    private static float CadenceToFrequency(float cadence)
    {
        const float minInSec = 60;
        return 1 / (cadence / minInSec);
    }
}
