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
    private NativeMultiHashMap<int,int> neighbourIndices;

    void Start()
    {
        NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();

        navMeshVertices = new NativeArray<float3>(navMeshTriangulation.vertices.Length, Allocator.Temp);
        for (int i = 0; i < navMeshTriangulation.vertices.Length; i++)
        {
            navMeshVertices[i] = navMeshTriangulation.vertices[i];
        }

        navMeshIndices = new NativeArray<int>(navMeshTriangulation.indices.Length, Allocator.Temp);
        for (int i = 0; i < navMeshTriangulation.indices.Length; i++)
        {
            navMeshIndices[i] = navMeshTriangulation.indices[i];
        }

        GetPath(new float3(1, 1, 1), new float3(42, 1, 33));
    }

    public NativeArray<PathNode> GetPath(float3 start, float3 end)
    {
        NativeArray<PathNode> pathNodes = new NativeArray<PathNode>(navMeshVertices.Length, Allocator.Temp);
        for (int i = 0; i < pathNodes.Length; i++)
        {
            pathNodes[i] =
            new PathNode
            {
                position = navMeshVertices[i],
                index = i,
                gCost = int.MaxValue,
                hCost = CalculateDistanceCost(start, end),
                prevIndex = -1,
            };
            NativeArray<int> vertexNeighbours = GetIndicesOfVertexNeighbours(i);
            neighbourIndices = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
            for (int ii = 0; ii < vertexNeighbours.Length; ii++)
            {
                neighbourIndices.Add(i, ii);
                Debug.Log("add " + i + " " + ii);
            }
        }

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        int startNodeIndex = CalculateIndex(start);
        int endNodeIndex = CalculateIndex(end);
        //Debug.Log(startNodeIndex + " " + endNodeIndex);
        //Debug.DrawLine(pathNodes[startNodeIndex].position, pathNodes[endNodeIndex].position, Color.red, 100);

        PathNode startNode = pathNodes[CalculateIndex(startNodeIndex)];
        startNode.gCost = 0;
        pathNodes[startNode.index] = startNode;

        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodes);
            Debug.Log(currentNodeIndex);

            if (currentNodeIndex == endNodeIndex)
            {
                Debug.Log("found path");
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

            bool found = neighbourIndices.TryGetFirstValue(currentNodeIndex, out int neighbourIndex, out NativeMultiHashMapIterator<int> iterator);
            Debug.Log(found);
            while (found)
            {

                PathNode neighbourNode = pathNodes[neighbourIndex];
                if (closedList.Contains(neighbourNode.index))
                {
                    // Already searched this node
                    continue;
                }

                float tentativeGCost = pathNodes[currentNodeIndex].gCost + CalculateDistanceCost(pathNodes[currentNodeIndex].position, neighbourNode.position);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.prevIndex = currentNodeIndex;
                    neighbourNode.gCost = tentativeGCost;
                    pathNodes[neighbourNode.index] = neighbourNode;

                    if (!openList.Contains(neighbourNode.index))
                    {
                        openList.Add(neighbourNode.index);
                    }
                }
                neighbourIndices.TryGetNextValue(out neighbourIndex, ref iterator);
            }
        }

        closedList.Dispose();
        openList.Dispose();

        return new NativeArray<PathNode>();
    }

    private NativeArray<int> GetIndicesOfVertexNeighbours(int vertexIndex)
    {
        //NativeList<int> neighboursIndices = new NativeList<int>(Allocator.Temp);
        NativeHashMap<int, float3> neighboursIndices = new NativeHashMap<int, float3>(1,Allocator.Temp);
        //NativeMultiHashMap<float3, int> neighboursIndices = new NativeMultiHashMap<float3, int>();

        int neighbourIndex1 = 0;
        int neighbourIndex2 = 0;

        for (int i = 0; i < navMeshIndices.Length; i++)
        {
            if(navMeshIndices[i] == vertexIndex)
            {
                int remainder = navMeshIndices[i] % 3;
                switch (remainder)
                {
                    case 0:
                        neighbourIndex1 = navMeshIndices[i] + 1;
                        neighbourIndex2 = navMeshIndices[i] + 2;
                        break;
                    case 1:
                        neighbourIndex1 = navMeshIndices[i] - 1;
                        neighbourIndex2 = navMeshIndices[i] + 1;
                        break ;
                    case 2:
                        neighbourIndex1 = navMeshIndices[i] - 2;
                        neighbourIndex2 = navMeshIndices[i] - 1;
                        break;
                    default:
                        Debug.LogError("Impossible remainder " + remainder);
                        break;
                }
                neighboursIndices.TryAdd(neighbourIndex1, navMeshVertices[neighbourIndex1]);
                neighboursIndices.TryAdd(neighbourIndex2, navMeshVertices[neighbourIndex2]);
            }
        }

        return neighboursIndices.GetKeyArray(Allocator.Temp);
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
        public float3 position;

        public int index;

        public float gCost;
        public float hCost;
        public float fCost { get { return gCost + hCost; } }

        public int prevIndex;


    }
}
