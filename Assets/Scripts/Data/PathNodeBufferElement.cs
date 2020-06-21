using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct PathNodeBufferElement : IBufferElementData
{
    public PathNode pathNode;
}
