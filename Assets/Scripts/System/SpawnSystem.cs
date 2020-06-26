using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;


public class SpawnSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;
    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeArray<Entity> enemies = GetEntityQuery(typeof(EnemyTag)).ToEntityArray(Allocator.TempJob);
        if (enemies.Length < 100020)
        {
            Unity.Mathematics.Random randomGen = new Unity.Mathematics.Random((uint)(new System.Random().NextDouble() * uint.MaxValue));
            EntityCommandBuffer.Concurrent ECS = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent();

            NativeArray<Entity> spawns = GetEntityQuery(typeof(SpawnTag)).ToEntityArray(Allocator.TempJob);
            Entity target = GetEntityQuery(typeof(BaseTag)).GetSingletonEntity();
            Entity spawner = GetEntityQuery(typeof(SpawnData)).GetSingletonEntity();
            SpawnData spawnData = EntityManager.GetComponentData<SpawnData>(spawner);

            int randomSpawn = UnityEngine.Random.Range(0, spawns.Length);

            float3 spawnPosition = EntityManager.GetComponentData<Translation>(spawns[randomSpawn]).Value;
            float spawnScale = EntityManager.GetComponentData<NonUniformScale>(spawns[randomSpawn]).Value.x;
            float3 targetPosition = EntityManager.GetComponentData<Translation>(target).Value;

            spawns.Dispose();

            spawnData.timeUntilSpawn -= Time.DeltaTime;

            NativeArray<SpawnData> spawnDatas = new NativeArray<SpawnData>(1, Allocator.TempJob);
            spawnDatas[0] = spawnData;

            if (spawnData.timeUntilSpawn < 0)
            {
                Job.WithCode(() =>
                {
                    SpawnData spawnDataJob = spawnDatas[0];
                    float spawnHalfScale = spawnScale / 2;

                    for (int i = 0; i < spawnDataJob.spawnCount; i++)
                    {
                        spawnDataJob.timeUntilSpawn = spawnDataJob.spawnFrequency;

                        float3 spawnPositionMod = new float3(randomGen.NextFloat() * spawnScale - spawnHalfScale, 0, randomGen.NextFloat() * spawnScale - spawnHalfScale);
                        Entity enemy = ECS.Instantiate(i, spawnDataJob.enemy);
                        ECS.SetComponent(i, enemy, new Translation { Value = spawnPosition + spawnPositionMod });
                        ECS.SetComponent(i, enemy, new TargetData { targetPosition = targetPosition });
                        ECS.AddComponent(i, enemy, new PathfindingParamsData { startPosition = spawnPosition, endPosition = targetPosition });
                        ECS.AddBuffer<PathNode>(i, enemy);
                        ECS.AddBuffer<FloatBufferElement>(i, enemy);
                    }
                    spawnDataJob.spawnCount += spawnDataJob.spawnIncrease;
                    spawnDatas[0] = spawnDataJob;
                }).Schedule();
            }

            CompleteDependency();
            EntityManager.SetComponentData(spawner, spawnDatas[0]);
            spawnDatas.Dispose();
            endSimulationEntityCommandBuffer.AddJobHandleForProducer(Dependency);
        }
        enemies.Dispose();
    }
}

