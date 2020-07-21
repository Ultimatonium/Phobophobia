using UnityEngine;
using Unity.Entities;

public class Gameloop : SystemBase
{
    protected override void OnUpdate()
    {
        Entity gameStateEntity = GetEntityQuery(typeof(GameStateData)).GetSingletonEntity();

        Entities.WithAll<BaseTag>().ForEach((in HealthData healthData) =>
        {
            if (healthData.health <= 0)
            {
                EntityManager.SetComponentData(gameStateEntity, new GameStateData { gameState = GameState.GameOver });
            }
        }
        ).WithoutBurst().Run();
    }
}
