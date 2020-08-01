using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct AnimationPlayData : IComponentData
{
    public float floatValue;
    public int intValue;
    public bool boolValue;
    public AnimationSetterType setterType;
    public AnimationParameter parameter;
}
