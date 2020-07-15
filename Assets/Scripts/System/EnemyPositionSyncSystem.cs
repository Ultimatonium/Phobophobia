using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EnemyPositionSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Transform transform, ref Translation translation) =>
        {
            translation.Value = transform.position;
        }
        ).WithoutBurst().Run();
    }
}
