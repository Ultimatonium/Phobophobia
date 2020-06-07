using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.AI;
using System.Linq;
using Unity.Entities.UniversalDelegates;

public class Pathfinding : MonoBehaviour
{
    private NativeArray<float3> navMeshVertices;
    private NativeArray<int> navMeshIndices;

    void Start()
    {
        NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();

        navMeshVertices = new NativeArray<float3>(navMeshTriangulation.vertices.Length, Allocator.TempJob);
        for (int i = 0; i < navMeshTriangulation.vertices.Length; i++)
        {
            navMeshVertices[i] = navMeshTriangulation.vertices[i];
        }

        navMeshIndices = new NativeArray<int>(navMeshTriangulation.indices.Length, Allocator.TempJob);
        for (int i = 0; i < navMeshTriangulation.indices.Length; i++)
        {
            navMeshIndices[i] = navMeshTriangulation.indices[i];
        }
    }

    public NativeArray<PathNode> GetPath(float3 start, float3 end)
    {
        NativeArray<PathNode> pathNodes = new NativeArray<PathNode>(navMeshVertices.Length, Allocator.Temp);
        for (int i = 0; i < pathNodes.Length; i++)
        {
            pathNodes[i] =
            new PathNode
            {
                x = navMeshVertices[i].x,
                y = navMeshVertices[i].z,
                index = i,
                gCost = int.MaxValue,
                hCost = CalculateDistanceCost(start, end),
                prevIndex = -1
            };
        }

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        int startNodeIndex = CalculateIndex(start);
        int endNodeIndex = CalculateIndex(end);

        PathNode startNode = pathNodes[CalculateIndex(startNodeIndex)];
        startNode.gCost = 0;
        pathNodes[startNode.index] = startNode;

        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodes);

            if (currentNodeIndex == endNodeIndex)
            {
                // Reached our destination!
                break;
            }

            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }

            closedList.Add(currentNodeIndex);

            //todo: get neighbours and add to open list


        }

        closedList.Dispose();
        openList.Dispose();

        return new NativeArray<PathNode>();
    }

    private NativeArray<int> GetIndicesOfVertexNeighbours(float3 vertex)
    {
        NativeList<int> neighboursIndices = new NativeList<int>(Allocator.Temp);
        //NativeHashMap<float3, int> neighboursIndices = new NativeHashMap<float3, int>();

        for (int i = 0; i < navMeshVertices.Length; i++)
        {
            if(navMeshVertices[i].Equals(vertex))
            {

            }
        }

        return neighboursIndices.ToArray(Allocator.Temp);
    }

    private int CalculateIndex(float3 pos)
    {
        int closestIndex = 0;
        for (int i = 0; i < navMeshVertices.Length; i++)
        {
            if (math.distancesq(navMeshVertices[i], pos) < math.distancesq(navMeshVertices[closestIndex], pos))
            {
                closestIndex = i;
            }
        }
        return closestIndex;
    }
    private static float CalculateDistanceCost(float3 aPosition, float3 bPosition)
    {
        return math.distance(aPosition, bPosition);
    }

    private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
    {
        PathNode lowestCostPathNode = pathNodeArray[openList[0]];
        for (int i = 1; i < openList.Length; i++)
        {
            if (pathNodeArray[openList[i]].fCost < lowestCostPathNode.fCost)
            {
                lowestCostPathNode = pathNodeArray[openList[i]];
            }
        }
        return lowestCostPathNode.index;
    }

    public struct PathNode
    {
        public float x;
        public float y;

        public int index;

        public float gCost;
        public float hCost;
        public float fCost { get { return gCost + hCost; } }

        public int prevIndex;

    }
}
