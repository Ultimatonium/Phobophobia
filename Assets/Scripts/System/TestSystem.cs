using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
public class TestSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;
        Entities
            .WithAll<TestTag>().ForEach(
                (ref Translation translation, ref Rotation rotation) =>
                {
                    translation.Value.z += 5 * dT;
                    rotation.Value = math.mul(rotation.Value, quaternion.RotateZ(10 * dT));
                }
            )
            .Schedule();
    }
}
