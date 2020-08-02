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
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = false;
                        World.GetExistingSystem<AttackTargetSystem>().Enabled = true;
                        World.GetExistingSystem<HealthModifySystem>().Enabled = true;
                        World.GetExistingSystem<MoneySystem>().Enabled = true;
                        Object.FindObjectOfType<Spawner>().enabled = true;
                        sceneHandler.TryUnloadScene("UIIngameESC");
                        break;
                    case GameState.Pause:
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        World.GetExistingSystem<AttackTargetSystem>().Enabled = false;
                        World.GetExistingSystem<HealthModifySystem>().Enabled = false;
                        World.GetExistingSystem<MoneySystem>().Enabled = false;
                        Object.FindObjectOfType<Spawner>().enabled = false;
                        sceneHandler.LoadSceneAdditive("UIIngameESC");
                        break;
                    case GameState.GameOver:
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
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
