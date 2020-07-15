using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class TowerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        NativeArray<Entity> enemies = GetEntityQuery(typeof(EnemyTag)).ToEntityArray(Allocator.TempJob);
        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();
        ComponentDataFromEntity<HealthData> healthDatas = GetComponentDataFromEntity<HealthData>();
        BufferFromEntity<HealthModifierBufferElement> healthModifiersBuffers = GetBufferFromEntity<HealthModifierBufferElement>();
        float dt = Time.DeltaTime;
        Entities.ForEach((Entity entity, Animator animator, Transform transform, ref TowerData towerData, ref Rotation rotation, in LocalToWorld localToWorld) =>
        {
            towerData.timeUntilShoot -= dt;
            if (towerData.timeUntilShoot < 0)
            {
                towerData.timeUntilShoot = CadenceToFrequency(towerData.cadence);

                Entity target = GetLowestTargetInRange(entity, enemies, towerData.range, positions, healthDatas);

                if (target != Entity.Null)
                {
                    healthModifiersBuffers[target].Add(new HealthModifierBufferElement { value = -towerData.damage });
                    rotation.Value = quaternion.LookRotation(positions[target].Value - positions[entity].Value, localToWorld.Up);
                    rotation.Value.value.x = 0;
                    rotation.Value.value.z = 0;
                    DrawShot(positions[entity].Value, positions[target].Value);
                    animator.SetBool("IsAttacking", true);
                    transform.rotation = rotation.Value;
                } else
                {
                    animator.SetBool("IsAttacking", false);
                }
            }
        }
        //).Schedule();
        ).WithoutBurst().Run(); //fix DrawShot

        Dependency.Complete();
        enemies.Dispose();
        DrawRange();
    }

    private static Entity GetLowestTargetInRange(Entity tower, NativeArray<Entity> enemies, float towerRange, ComponentDataFromEntity<Translation> positions, ComponentDataFromEntity<HealthData> healthDatas)
    {
        Entity target = Entity.Null;
        for (int i = 0; i < enemies.Length; i++)
        {
            //Debug.Log((math.distancesq(positions[enemies[i]].Value, positions[tower].Value) + "|" + (towerRange * towerRange)));
            if (math.distancesq(positions[enemies[i]].Value, positions[tower].Value) < towerRange * towerRange)
            {
                //Debug.Log("Tower");
                if (target == Entity.Null) target = enemies[i];
                if (healthDatas[target].health > healthDatas[enemies[i]].health)
                {
                    target = enemies[i];
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
        NativeArray<Entity> towers = GetEntityQuery(typeof(TowerData)).ToEntityArray(Allocator.TempJob);
        ComponentDataFromEntity<Translation> positions = GetComponentDataFromEntity<Translation>();
        ComponentDataFromEntity<TowerData> towerDatas = GetComponentDataFromEntity<TowerData>();
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
