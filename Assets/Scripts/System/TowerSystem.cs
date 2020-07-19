﻿using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public class TowerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        NativeArray<Entity> targets = GetEntityQuery(typeof(EnemyTag),typeof(BaseTag)).ToEntityArray(Allocator.TempJob);
        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();
        ComponentDataFromEntity<HealthData> healthDatas = GetComponentDataFromEntity<HealthData>();
        ComponentDataFromEntity<EnemyTag> enemyTags = GetComponentDataFromEntity<EnemyTag>();
        BufferFromEntity<HealthModifierBufferElement> healthModifiersBuffers = GetBufferFromEntity<HealthModifierBufferElement>();
        float dt = Time.DeltaTime;
        Entities.WithAny<TowerTag,EnemyTag>().ForEach((Entity entity, Animator animator, Transform transform, ref RangeAttackData rangeAttackData, ref Rotation rotation, in LocalToWorld localToWorld) =>
        {
            rangeAttackData.timeUntilShoot -= dt;
            if (rangeAttackData.timeUntilShoot < 0)
            {
                rangeAttackData.timeUntilShoot = CadenceToFrequency(rangeAttackData.cadence);

                Entity target = GetLowestTargetInRange(entity, targets, rangeAttackData.range, positions, healthDatas, enemyTags);
                //Debug.Log(target);

                if (target != Entity.Null)
                {
                    healthModifiersBuffers[target].Add(new HealthModifierBufferElement { value = -rangeAttackData.damage });
                    rotation.Value = quaternion.LookRotation(positions[target].Value - positions[entity].Value, localToWorld.Up);
                    rotation.Value.value.x = 0;
                    rotation.Value.value.z = 0;
                    DrawShot(positions[entity].Value, positions[target].Value);
                    animator.SetBool("isAttacking", true);
                    transform.rotation = rotation.Value;
                } else
                {
                    animator.SetBool("isAttacking", false);
                }
            }
        }
        //).Schedule();
        ).WithoutBurst().Run(); //fix DrawShot

        Dependency.Complete();
        targets.Dispose();
        DrawRange();
    }

    private static Entity GetLowestTargetInRange(Entity source, NativeArray<Entity> targets, float towerRange, ComponentDataFromEntity<Translation> positions, ComponentDataFromEntity<HealthData> healthDatas, ComponentDataFromEntity<EnemyTag> enemyTags)
    {
        Entity target = Entity.Null;
        for (int i = 0; i < targets.Length; i++)
        {
            if (enemyTags.Exists(source) && enemyTags.Exists(targets[i])) continue;
            //Debug.Log((math.distancesq(positions[enemies[i]].Value, positions[tower].Value) + "|" + (towerRange * towerRange)));
            if (math.distancesq(positions[targets[i]].Value, positions[source].Value) < towerRange * towerRange)
            {
                //Debug.Log("Tower");
                if (target == Entity.Null) target = targets[i];
                if (healthDatas[target].health > healthDatas[targets[i]].health)
                {
                    target = targets[i];
                }
            }
        }
        return target;
    }

    private void DrawShot(float3 towerPosition, float3 target)
    {
        GameObject shot = new GameObject();
        LineRenderer lineRenderer = shot.AddComponent<LineRenderer>();
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.black;
        lineRenderer.SetPosition(0, towerPosition);
        lineRenderer.SetPosition(1, target);
        lineRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.1f;
        GameObject.Destroy(shot, 0.1f);
    }

    private static float CadenceToFrequency(float cadence)
    {
        const float minInSec = 60;
        return 1 / (cadence / minInSec);
    }

    private void DrawRange()
    {
        NativeArray<Entity> towers = GetEntityQuery(typeof(RangeAttackData)).ToEntityArray(Allocator.TempJob);
        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();
        ComponentDataFromEntity<RangeAttackData> towerDatas = GetComponentDataFromEntity<RangeAttackData>();
        for (int i = 0; i < towers.Length; i++)
        {
            DrawCircle(positions[towers[i]].Value, towerDatas[towers[i]].range, Color.green);
        }
        towers.Dispose();
    }

    private static void DrawCircle(Vector3 center, float radius, Color color)
    {
        Vector3 prevPos = center + new Vector3(radius, 0, 0);
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)(i + 1) / 30.0f * Mathf.PI * 2.0f;
            Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPos, newPos, color);
            prevPos = newPos;
        }
    }
}
