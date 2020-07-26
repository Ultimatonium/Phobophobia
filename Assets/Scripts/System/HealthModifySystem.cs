using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class HealthModifySystem : SystemBase
{
    /*
    EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    */

    protected override void OnUpdate()
    {
        //EntityCommandBuffer.Concurrent ECS = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent();

        ComponentDataFromEntity<EnemyTag> enemyTags = GetComponentDataFromEntity<EnemyTag>();
        ComponentDataFromEntity<BaseTag> baseTags = GetComponentDataFromEntity<BaseTag>();
        ComponentDataFromEntity<TowerTag> towerTags = GetComponentDataFromEntity<TowerTag>();
        ComponentDataFromEntity<PlayerTag> playerTag = GetComponentDataFromEntity<PlayerTag>();
        ComponentDataFromEntity<HealthData> healthData = GetComponentDataFromEntity<HealthData>();

        //NativeList<Entity> entities = new NativeList<Entity>(Allocator.TempJob);

        Entities.ForEach((Entity entity, Animator animator, HealthBar healthBar, int entityInQueryIndex, ref HealthData healthDataChange, ref DynamicBuffer<HealthModifierBufferElement> healthModifiers) =>
        {
            for (int i = 0; i < healthModifiers.Length; i++)
            {
                healthDataChange.health += healthModifiers[i].value;
                //entities.Add(entity);
                healthBar.SetHealth(healthData[entity].health, healthData[entity].maxHealth);
                /*
                if (baseTags.Exists(entity))
                {
                    HUD.Instance.SetBaseHealth(healthData[entity].health, healthData[entity].maxHealth);
                }
                if (playerTag.Exists(entity))
                {
                    HUD.Instance.SetPlayerHealth(healthData[entity].health, healthData[entity].maxHealth);
                }
                */
                animator.SetTrigger("getHit");
            }
            healthModifiers.Clear();
            //ECS.SetBuffer<HealthModifierBufferElement>(entityInQueryIndex, entity);
        }
        ).WithoutBurst().Run();

        //endSimulationEntityCommandBuffer.AddJobHandleForProducer(Dependency);
        //Dependency.Complete();

        /*
        for (int i = 0; i < entities.Length; i++)
        {
            if (HUD.Instance != null)
            {
                
            }
        }

        entities.Dispose();
        */
    }
}
