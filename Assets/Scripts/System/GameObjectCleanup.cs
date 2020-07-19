using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public class GameObjectCleanup : ComponentSystem
{
    protected override void OnUpdate()
    {
        var goes = Object.FindObjectsOfType<GameObjectEntity>();

        foreach (var goe in goes)
        {
            if (!EntityManager.Exists(goe.Entity))
                Object.Destroy(goe.gameObject);
        }
    }
}
