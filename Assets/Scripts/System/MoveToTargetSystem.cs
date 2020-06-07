using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public class MoveToTargetSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;
        Entities.ForEach((ref TargetData targetData, ref MoveData moveData, ref Translation translation, ref Rotation rotation) =>
        {
            float3 dir = math.normalize(targetData.targetPosition - translation.Value);
            rotation.Value = quaternion.LookRotation(dir, new float3(0,1,0));
            translation.Value += dir * moveData.speed * dT;
        }
        ).Schedule();
    }
}
