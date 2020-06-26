using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public class DrawPathSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref DynamicBuffer<PathNode> pathNodes) =>
        {
            if (pathNodes.Length > 0)
            {
                for (int i = 1; i < pathNodes.Length; i++)
                {
                    Debug.DrawLine(pathNodes[i - 1].position, pathNodes[i].position, Color.green);
                }
            }
        }
        ).Run();
    }
}
