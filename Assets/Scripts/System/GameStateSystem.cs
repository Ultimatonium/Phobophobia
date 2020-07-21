using Unity.Entities;
using UnityEngine;
using Unity.Collections;

public class GameStateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((SceneHandler sceneHandler, ref GameStateData gameStateData) =>
        {
            if (gameStateData.gameState != gameStateData.prevGameState)
            {
                switch (gameStateData.gameState)
                {
                    case GameState.None:
                        break;
                    case GameState.Running:
                        World.GetExistingSystem<AttackTargetSystem>().Enabled = true;
                        Object.FindObjectOfType<Spawner>().enabled = true;
                        sceneHandler.TryUnloadScene("UIIngameESC");
                        break;
                    case GameState.Pause:
                        World.GetExistingSystem<AttackTargetSystem>().Enabled = false;
                        Object.FindObjectOfType<Spawner>().enabled = false;
                        sceneHandler.LoadSceneAdditive("UIIngameESC");
                        break;
                    case GameState.GameOver:
                        sceneHandler.TryUnloadScene("UIIngameMain");
                        sceneHandler.LoadScene("UIIngameEndscreen");
                        DestroyAllEntities();
                        World.GetExistingSystem<GameStateSystem>().Enabled = false;
                        break;
                    default:
                        Debug.LogWarning("gameState " + gameStateData.gameState + " not handled");
                        break;
                }
                gameStateData.prevGameState = gameStateData.gameState;
            }
        }
        ).WithStructuralChanges().WithoutBurst().Run();
    }

    private void DestroyAllEntities()
    {
        NativeArray<Entity> entities = EntityManager.GetAllEntities();
        for (int i = 0; i < entities.Length; i++)
        {
            EntityManager.DestroyEntity(entities[i]);
        }
        entities.Dispose();
    }
}
