using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

[DisableAutoCreation]
public class PathfindingSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    private NativeArray<float3> navMeshVertices;
    private NativeMultiHashMap<int, int> neighbourIndices;

    protected override void OnCreate()
    {
        base.OnCreate();

        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        //todo: remove on build
        NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;

        NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();

        List<Vector3> navMeshVerticesList = navMeshTriangulation.vertices.ToList();
        List<int> navMeshIndicesList = navMeshTriangulation.indices.ToList();
        SimplifyMeshTopology(navMeshVerticesList, navMeshIndicesList, 1);
        navMeshTriangulation.vertices = navMeshVerticesList.ToArray();
        navMeshTriangulation.indices = navMeshIndicesList.ToArray();

        navMeshVertices = new NativeArray<float3>(navMeshTriangulation.vertices.Length, Allocator.Persistent);
        for (int i = 0; i < navMeshTriangulation.vertices.Length; i++)
        {
            navMeshVertices[i] = navMeshTriangulation.vertices[i];
        }
        NativeArray<int> navMeshIndices = new NativeArray<int>(navMeshTriangulation.indices.Length, Allocator.Temp);
        for (int i = 0; i < navMeshTriangulation.indices.Length; i++)
        {
            navMeshIndices[i] = navMeshTriangulation.indices[i];
        }

        neighbourIndices = GetVerticesNeighbours(navMeshVertices, navMeshIndices);

        navMeshIndices.Dispose();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent ECS = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent();

        NativeArray<float3> navMeshVertices = this.navMeshVertices;
        NativeMultiHashMap<int, int> neighbourIndices = this.neighbourIndices;

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref PathfindingParamsData pathfindingParamsData) =>
            {
                NativeArray<PathNode> path = CalculatePath(pathfindingParamsData.startPosition, pathfindingParamsData.endPosition, navMeshVertices, neighbourIndices);
                DynamicBuffer<PathNode> dynamicBuffers = ECS.SetBuffer<PathNode>(entityInQueryIndex, entity);
                dynamicBuffers.AddRange(path);
                ECS.RemoveComponent<PathfindingParamsData>(entityInQueryIndex, entity);
            }
            ).Schedule();

        endSimulationEntityCommandBuffer.AddJobHandleForProducer(Dependency);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        navMeshVertices.Dispose();
        neighbourIndices.Dispose();
    }


    private static void SimplifyMeshTopology(List<Vector3> vertices, List<int> indices, float weldThreshold)
    {
        // Put vertex indices into buckets based on their position
        Dictionary<Vector3Int, List<int>> vertexBuckets = new Dictionary<Vector3Int, List<int>>(vertices.Count);
        Dictionary<int, int> shiftedIndices = new Dictionary<int, int>(indices.Count);
        List<Vector3> uniqueVertices = new List<Vector3>();
        int weldThresholdMultiplier = Mathf.RoundToInt(1 / weldThreshold);

        // Heuristic for skipping indices that relies on the fact that the first time a vertex index appears on the indices array, it will always be the highest-numbered
        // index up to that point (e.g. if there's a 5 in the indices array, all the indices to the left of it are in the range [0, 4])
        int minRepeatedIndex = 0;

        for (int i = 0; i < vertices.Count; ++i)
        {
            Vector3Int currentVertex = Vector3Int.RoundToInt(vertices[i] * weldThresholdMultiplier);
            if (vertexBuckets.TryGetValue(currentVertex, out List<int> indexRefs))
            {
                indexRefs.Add(i);
                shiftedIndices[i] = shiftedIndices[indexRefs[0]];
                if (minRepeatedIndex == 0)
                {
                    minRepeatedIndex = i;
                }
            }
            else
            {
                indexRefs = new List<int> { i };
                vertexBuckets.Add(currentVertex, indexRefs);
                shiftedIndices[i] = uniqueVertices.Count;
                uniqueVertices.Add(vertices[i]);
            }
        }

        // Walk indices array and replace any repeated vertex indices with their corresponding unique one
        for (int i = 0; i < indices.Count; ++i)
        {
            var currentIndex = indices[i];
            if (currentIndex < minRepeatedIndex)
            {
                // Can't be a repeated index, skip.
                continue;
            }

            indices[i] = shiftedIndices[currentIndex];
        }

        vertices.Clear();
        vertices.AddRange(uniqueVertices);
    }


    private static NativeMultiHashMap<int, int> GetVerticesNeighbours(NativeArray<float3> navMeshVertices, NativeArray<int> navMeshIndices)
    {
        NativeMultiHashMap<int, int> neighbourIndices = new NativeMultiHashMap<int, int>(0, Allocator.Persistent);
        for (int i = 0; i < navMeshVertices.Length; i++)
        {
            NativeArray<int> vertexNeighbours = GetIndicesOfVertexNeighbours(i, navMeshIndices);
            for (int ii = 0; ii < vertexNeighbours.Length; ii++)
            {
                neighbourIndices.Add(i, vertexNeighbours[ii]);
            }
        }
        return neighbourIndices;
    }

    private static NativeArray<int> GetIndicesOfVertexNeighbours(int vertexIndex, NativeArray<int> navMeshIndices)
    {
        NativeList<int> neighboursIndices = new NativeList<int>(Allocator.Temp);

        int neighbourIndex1;
        int neighbourIndex2;

        for (int i = 0; i < navMeshIndices.Length; i++)
        {
            if (vertexIndex == navMeshIndices[i])
            {
                neighbourIndex1 = -1;
                neighbourIndex2 = -1;
                int remainder = i % 3;
                switch (remainder)
                {
                    case 0:
                        neighbourIndex1 = navMeshIndices[i + 1];
                        neighbourIndex2 = navMeshIndices[i + 2];
                        break;
                    case 1:
                        neighbourIndex1 = navMeshIndices[i - 1];
                        neighbourIndex2 = navMeshIndices[i + 1];
                        break;
                    case 2:
                        neighbourIndex1 = navMeshIndices[i - 2];
                        neighbourIndex2 = navMeshIndices[i - 1];
                        break;
                    default:
                        Debug.LogError("Impossible remainder " + remainder);
                        break;
                }
                if (ValidAndUniqueInt(neighboursIndices, neighbourIndex1))
                {
                    neighboursIndices.Add(neighbourIndex1);
                }
                if (ValidAndUniqueInt(neighboursIndices, neighbourIndex2))
                {
                    neighboursIndices.Add(neighbourIndex2);
                }
            }
        }
        return neighboursIndices.AsArray();
    }
    private static bool ValidAndUniqueInt(NativeList<int> list, int value)
    {
        return !list.Contains(value) && value != -1;
    }


    private static NativeArray<PathNode> CalculatePath(float3 startPosition, float3 endPositon, NativeArray<float3> nodePositions, NativeMultiHashMap<int, int> neighbourIndices)
    {
        NativeArray<PathNode> pathNodes = CreatePathNodes(nodePositions, CalculateDistanceCost(startPosition, endPositon));

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        int startNodeIndex = CalculateIndex(startPosition, pathNodes);
        int endNodeIndex = CalculateIndex(endPositon, pathNodes);

        PathNode startNode = pathNodes[startNodeIndex];
        startNode.gCost = 0;
        pathNodes[startNode.index] = startNode;

        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodes);

            //check if path found
            if (currentNodeIndex == endNodeIndex)
            {
                break;
            }

            //remove current node from openList...
            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }

            //... and add to closedList
            closedList.Add(currentNodeIndex);

            bool found = neighbourIndices.TryGetFirstValue(currentNodeIndex, out int neighbourIndex, out NativeMultiHashMapIterator<int> iterator);
            //while (found)
            for (int i = 0; i < 999 && found; i++)
            {
                PathNode neighbourNode = pathNodes[neighbourIndex];

                // check if node already searched
                if (!closedList.Contains(neighbourNode.index))
                {
                    float tentativeGCost = pathNodes[currentNodeIndex].gCost + CalculateDistanceCost(pathNodes[currentNodeIndex].position, neighbourNode.position);
                    // add neighbour to openList if gCost is lower
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
                }
                found = neighbourIndices.TryGetNextValue(out neighbourIndex, ref iterator);
            }
        }

        PathNode endNode = pathNodes[endNodeIndex];
        NativeList<PathNode> path = new NativeList<PathNode>(Allocator.Temp);
        if (endNode.prevIndex == -1)
        {
            // Didn't find a path!
        }
        else
        {
            // Found a path
            PathNode node = endNode;
            while (node.prevIndex != -1)
            {
                path.Add(node);
                PathNode prevNode = pathNodes[node.prevIndex];
                node = prevNode;
            }
            path.Add(node);
            ReversePathNodeNativeArray(path);
        }

        closedList.Dispose();
        openList.Dispose();

        return path;
    }

    private static NativeArray<PathNode> CreatePathNodes(NativeArray<float3> vertices, float hCost)
    {
        NativeArray<PathNode> pathNodes = new NativeArray<PathNode>(vertices.Length, Allocator.Temp);
        for (int i = 0; i < vertices.Length; i++)
        {
            pathNodes[i] =
                new PathNode
                {
                    position = vertices[i],
                    index = i,
                    gCost = int.MaxValue,
                    hCost = hCost,
                    prevIndex = -1,
                }
                ;
        }
        return pathNodes;
    }

    private static float CalculateDistanceCost(float3 aPosition, float3 bPosition)
    {
        return math.distancesq(aPosition, bPosition);
    }

    private static int CalculateIndex(float3 position, NativeArray<PathNode> pathNodes)
    {
        int closestIndex = 0;
        for (int i = 0; i < pathNodes.Length; i++)
        {
            if (math.distancesq(pathNodes[i].position, position) < math.distancesq(pathNodes[closestIndex].position, position))
            {
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private static int GetLowestCostFNodeIndex(NativeList<int> inList, NativeArray<PathNode> pathNodes)
    {
        PathNode lowestCostPathNode = pathNodes[inList[0]];
        for (int i = 1; i < inList.Length; i++)
        {
            if (pathNodes[inList[i]].FCost < lowestCostPathNode.FCost)
            {
                lowestCostPathNode = pathNodes[inList[i]];
            }
        }
        return lowestCostPathNode.index;
    }

    private static void ReversePathNodeNativeArray(NativeArray<PathNode> pathNodes)
    {
        PathNode temp;
        if (pathNodes.Length == 0) return;
        int start = 0;
        int end = pathNodes.Length-1;

        while (start < end)
        {
            temp = pathNodes[start];
            pathNodes[start] = pathNodes[end];
            pathNodes[end] = temp;
            start++;
            end--;
        }
    }
}