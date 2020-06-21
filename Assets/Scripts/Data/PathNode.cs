using Unity.Entities;
using Unity.Mathematics;

public struct PathNode : IBufferElementData
{
    public float3 position;

    public int index;

    public float gCost;
    public float hCost;
    public float FCost { get { return gCost + hCost; } }

    public int prevIndex;
}