using UnityEngine;
using Unity.Entities;

public class Gameloop : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<BaseTag>().ForEach((in HealthData healthData) =>
        {
            if (healthData.health <= 0)
            {
                Debug.Log("Game over");
            }
        }
        ).Schedule();
    }
}
