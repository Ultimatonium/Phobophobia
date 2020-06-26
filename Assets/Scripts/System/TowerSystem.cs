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
        BufferFromEntity<FloatBufferElement> healthModifiersBuffers = GetBufferFromEntity<FloatBufferElement>();
        float dt = Time.DeltaTime;
        Entities.ForEach((Entity entity, ref TowerData towerData) =>
        {
            towerData.timeUntilShoot -= dt;
            if (towerData.timeUntilShoot < 0)
            {
                towerData.timeUntilShoot = CadenceToFrequency(towerData.cadence);

                for (int i = 0; i < enemies.Length; i++)
                {
                    if (math.distancesq(positions[enemies[i]].Value, positions[entity].Value) < towerData.range * towerData.range)
                    {
                        healthModifiersBuffers[enemies[i]].Add(new FloatBufferElement { value = -towerData.damage });
                    }
                }
            }
        }
        ).Schedule();
        Dependency.Complete();
        enemies.Dispose();
        DrawRange();
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
