using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[DisableAutoCreation]
public class MoveToTargetSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;
        Entities.ForEach((ref TargetData targetData, ref MoveData moveData, ref Translation translation, ref Rotation rotation, ref DynamicBuffer<PathNode> pathNodes) =>
        {
            float3 dir;
            if (pathNodes.Length > 0 && EqualFloat3(translation.Value, pathNodes[0].position, 0.5f))
            {
                pathNodes.RemoveAt(0);
            }
            if (pathNodes.Length > 0)
            {
                dir = math.normalize(pathNodes[0].position - translation.Value);
            }
            else
            {
                dir = math.normalize(targetData.targetPosition - translation.Value);
            }
            rotation.Value = quaternion.LookRotation(dir, new float3(0, 1, 0));
            translation.Value += dir * moveData.speed * dT;
        }
        ).Schedule();
    }
    private static bool EqualFloat3(float3 a, float3 b, float tolerance)
    {
        return math.abs(a.x - b.x) < tolerance
            && math.abs(a.y - b.y) < tolerance
            && math.abs(a.z - b.z) < tolerance;
    }
}
