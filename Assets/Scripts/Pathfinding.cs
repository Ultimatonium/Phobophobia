using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.AI;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using System.IO;
using TMPro;
using System;
using UnityEngine.Profiling;

public class Pathfinding : MonoBehaviour
{
    private NativeArray<float3> navMeshVertices;
    private NativeArray<int> navMeshIndices;

    public GameObject start;
    public GameObject end;
    public GameObject textMeshPrefab;

    NativeArray<PathNode> pathNodes;
    private NativeMultiHashMap<int,int> neighbourIndices;

    void Start()
    {
        return;
        float startTime = Time.realtimeSinceStartup;
        NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
        Debug.Log("original" + navMeshTriangulation.vertices.Length);

        List<Vector3> navMeshVerticesList = navMeshTriangulation.vertices.ToList();
        List<int> navMeshIndicesList = navMeshTriangulation.indices.ToList();
        
        //Debug.Log("pre" + navMeshVerticesList.Count());
        //Debug.Log("pre" + navMeshIndicesList.Count());
        SimplifyMeshTopology(navMeshVerticesList, navMeshIndicesList, 1);
        //Debug.Log("post" + navMeshVerticesList.Count());
        //Debug.Log("post" + navMeshIndicesList.Count());
        navMeshTriangulation.vertices = navMeshVerticesList.ToArray();
        navMeshTriangulation.indices = navMeshIndicesList.ToArray();

        GetComponent<MeshFilter>().mesh.vertices = navMeshTriangulation.vertices;
        GetComponent<MeshFilter>().mesh.triangles = navMeshTriangulation.indices;
        //Debug.Log(navMeshTriangulation.vertices.Length);
        //Debug.Log(navMeshTriangulation.indices.Length);
        //Debug.Log(GetComponent<MeshFilter>().mesh.GetTopology(0));

        navMeshVertices = new NativeArray<float3>(navMeshTriangulation.vertices.Length, Allocator.Persistent);
        for (int i = 0; i < navMeshTriangulation.vertices.Length; i++)
        {
            navMeshVertices[i] = navMeshTriangulation.vertices[i];
            //writeTextMesh(navMeshVertices[i], "vertex" + i + navMeshVertices[i]);
        }

        navMeshIndices = new NativeArray<int>(navMeshTriangulation.indices.Length, Allocator.Persistent);
        for (int i = 0; i < navMeshTriangulation.indices.Length; i++)
        {
            navMeshIndices[i] = navMeshTriangulation.indices[i];
            //writeTextMesh(navMeshVertices[navMeshIndices[i]], "index triangle" + i + "|" + "index vertex" + navMeshIndices[i] + ":" + navMeshVertices[navMeshIndices[i]].ToString());
        }
        //CondenseMesh(ref navMeshVertices, ref navMeshIndices);

        pathNodes = CreatePathNodes(navMeshVertices, CalculateDistanceCost(start.transform.position, end.transform.position));
        neighbourIndices = GetNodeNeighbours(pathNodes);

        //DrawNeighbour(258, Color.black, float.MaxValue);
        for (int i = 0; i < pathNodes.Length; i++)
        {
            //DrawNeighbour(pathNodes[i].index, Color.black, float.MaxValue);
        }

        /*
        Debug.Log(navMeshTriangulation.vertices[0]);
        Debug.Log(navMeshTriangulation.vertices[1]);
        Debug.Log(navMeshTriangulation.vertices[2]);
        Debug.Log(navMeshTriangulation.vertices[3]);
        Debug.Log(navMeshTriangulation.vertices[4]);
        Debug.Log(navMeshTriangulation.vertices[5]);
        */

        /*
        for (int i = 0; i < 160; i+=3)
        {
            Debug.Log("index " + i + " " 
                + navMeshTriangulation.indices[i] + "("+ navMeshTriangulation.vertices[navMeshTriangulation.indices[i]]+")"
                + navMeshTriangulation.indices[i+1] + "(" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i+1]] + ")"
                + navMeshTriangulation.indices[i+2] + "(" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i+2]] + ")");
            Debug.DrawLine(navMeshTriangulation.vertices[navMeshTriangulation.indices[i]], navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 1]],Color.green,50);
            Debug.DrawLine(navMeshTriangulation.vertices[navMeshTriangulation.indices[i+1]], navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 2]], Color.green,50);
            Debug.DrawLine(navMeshTriangulation.vertices[navMeshTriangulation.indices[i+2]], navMeshTriangulation.vertices[navMeshTriangulation.indices[i]], Color.green,50);
            TextMesh textMeshObject1 = Instantiate(textMeshPrefab, navMeshTriangulation.vertices[navMeshTriangulation.indices[i]], textMeshPrefab.transform.rotation).GetComponent<TextMesh>();
            textMeshObject1.text = navMeshTriangulation.indices[i] + ":" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i]].ToString();
            textMeshObject1.name = textMeshObject1.text;
            TextMesh textMeshObject2 = Instantiate(textMeshPrefab, navMeshTriangulation.vertices[navMeshTriangulation.indices[i+1]], textMeshPrefab.transform.rotation).GetComponent<TextMesh>();
            textMeshObject2.text = navMeshTriangulation.indices[i+1] + ":" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i+1]].ToString();
            textMeshObject2.name = textMeshObject2.text;
            TextMesh textMeshObject3 = Instantiate(textMeshPrefab, navMeshTriangulation.vertices[navMeshTriangulation.indices[i+2]], textMeshPrefab.transform.rotation).GetComponent<TextMesh>();
            textMeshObject3.text = navMeshTriangulation.indices[i+2] + ":" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i+2]].ToString();
            textMeshObject3.name = textMeshObject3.text;
        }
        */

        Debug.Log("start time: " + (Time.realtimeSinceStartup - startTime));

    }

    private void CondenseMesh(ref NativeList<float3> verticies, ref NativeList<int> indicies)
    {
        NativeList<float3> condensedVerticies = new NativeList<float3>(0, Allocator.Temp);
        NativeList<int> condensedIndicies = new NativeList<int>(0, Allocator.Temp);

        

        condensedIndicies.Dispose();
        condensedVerticies.Dispose();
    }

    private bool first = true;
    private void Update()
    {
        return;
        DrawHighlightedNeighbour();
        if (first)
        {
            Profiler.BeginSample("find path");
            float startTime = Time.realtimeSinceStartup;
            GetPath(start.transform.position, end.transform.position);
            Debug.Log("find path time " + (Time.realtimeSinceStartup - startTime));
            first = false;
            Profiler.EndSample();
        }
    }

    private void DrawHighlightedNeighbour()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, float.MaxValue))
        {
            int index = CalculateIndex(raycastHit.point);
            DrawNeighbour(index, Color.red, 0);
        }
    }

    private void DrawNeighbour(int index, Color color, float duration)
    {
        bool found = neighbourIndices.TryGetFirstValue(index, out int neighbourIndex, out NativeMultiHashMapIterator<int> iterator);
        while (found)
        {
            Debug.DrawLine(pathNodes[index].position, pathNodes[neighbourIndex].position, color, duration);
            found = neighbourIndices.TryGetNextValue(out neighbourIndex, ref iterator);
        }
    }

    private void OnDestroy()
    {
        return;
        navMeshVertices.Dispose();
        navMeshIndices.Dispose();
        pathNodes.Dispose();
        neighbourIndices.Dispose();
    }

    private NativeMultiHashMap<int, int> GetNodeNeighbours(NativeArray<PathNode> pathNodes)
    {
        neighbourIndices = new NativeMultiHashMap<int, int>(0, Allocator.Persistent);
        for (int i = 0; i < pathNodes.Length; i++)
        {
            NativeArray<int> vertexNeighbours = GetIndicesOfVertexNeighbours(i);
            for (int ii = 0; ii < vertexNeighbours.Length; ii++)
            {
                neighbourIndices.Add(i, vertexNeighbours[ii]);
                /*
                if (i == 459)
                {
                Debug.Log("add " + i + " " + vertexNeighbours[ii] + " of " + vertexNeighbours.Length);
                }*/
            }
        }
        return neighbourIndices;
    }

    private NativeArray<PathNode> CreatePathNodes(NativeArray<float3> vertices, float hCost)
    {
        //NativeArray<PathNode> pathNodes = new NativeArray<PathNode>(vertices.Length, Allocator.Persistent);
        NativeList<PathNode> pathNodes = new NativeList<PathNode>(vertices.Length, Allocator.Persistent);
        for (int i = 0; i < vertices.Length; i++)
        {
            /*
            if (!PathNodesContainsSamePosition(pathNodes,vertices[i]))
            {
            */
            //pathNodes[i] =
            pathNodes.Add(
            new PathNode
            {
                position = navMeshVertices[i],
                index = i,
                gCost = int.MaxValue,
                hCost = hCost,
                prevIndex = -1,
            }
                );
            //}
        }
        return pathNodes;
    }

    private bool PathNodesContainsSamePosition(NativeArray<PathNode> pathNodes, float3 position)
    {
        for (int i = 0; i < pathNodes.Length; i++)
        {
            if (EqualFloat3(pathNodes[i].position, position, 0.1f))
            {
                return true;
            }
        }
        return false;
    }

    private void paintVertexFromTriangleIndex(int index)
    {
        //TextMeshPro textMeshObject = Instantiate(textMeshPrefab, navMeshVertices[navMeshIndices[index]], textMeshPrefab.transform.rotation).GetComponent<TextMeshPro>();
        /*
        foreach (TextMeshPro item in FindObjectsOfType<TextMeshPro>())
        {
            if (item.transform.position == textMeshObject.transform.position)
            {
                textMeshObject.transform.position += new Vector3(0, 0, -1);
            }
        }
        */
        //textMeshObject.text = "index triangle: " + index + "|" + "index vertex: " + navMeshIndices[index] + ":" + navMeshVertices[navMeshIndices[index]].ToString();
        //textMeshObject.name = textMeshObject.text;
        writeTextMesh(navMeshVertices[navMeshIndices[index]], "index triangle: " + index + "|" + "index vertex: " + navMeshIndices[index] + ":" + navMeshVertices[navMeshIndices[index]].ToString());
    }

    private void writeTextMesh(float3 position, string text)
    {
        writeTextMesh(position, text, textMeshPrefab.GetComponent<TextMeshPro>().fontSize, textMeshPrefab.GetComponent<TextMeshPro>().color);
    }

    private void writeTextMesh(float3 position, string text, float fontSize, Color color)
    {
        TextMeshPro textMeshObject = Instantiate(textMeshPrefab, position, textMeshPrefab.transform.rotation).GetComponent<TextMeshPro>();
        /*
        foreach (TextMeshPro item in FindObjectsOfType<TextMeshPro>())
        {
            if (item.transform.position == textMeshObject.transform.position)
            {
                textMeshObject.transform.position += new Vector3(0, 0, -1);
            }
        }
        */
        textMeshObject.text = text;
        textMeshObject.name = textMeshObject.text;
        textMeshObject.fontSize = fontSize;
        textMeshObject.color = color;
    }

    public NativeArray<PathNode> GetPath(float3 start, float3 end)
    {
        

        


                //Debug.Log("230" + pathNodes[230].position);
        //Debug.Log("320" + pathNodes[320].position);
        //if((pathNodes[230].position).Equals((pathNodes[320].position)))
        /*
        if(EqualFloat3(pathNodes[230].position, pathNodes[320].position,0.1f))
        {
            Debug.Log("same");
        } else
        {
            Debug.Log("not same");
        }*/

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        int startNodeIndex = CalculateIndex(start);
        int endNodeIndex = CalculateIndex(end);
        Debug.Log("start/end:" + startNodeIndex + " " + endNodeIndex);
        Debug.DrawLine(pathNodes[startNodeIndex].position, pathNodes[endNodeIndex].position, Color.blue, 100);

        //PathNode startNode = pathNodes[CalculateIndex(startNodeIndex)];
        PathNode startNode = pathNodes[startNodeIndex];
        startNode.gCost = 0;
        pathNodes[startNode.index] = startNode;

        openList.Add(startNode.index);

        int order = -1;
        while (openList.Length > 0)
        {
            order++;
            int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodes);
            //writeTextMesh(pathNodes[currentNodeIndex].position, "Order:" + order, 10, Color.black);
            //Debug.Log(currentNodeIndex);

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

            NativeArray<int> nativeArrays = neighbourIndices.GetKeyArray(Allocator.Temp);
            for (int i = 0; i < nativeArrays.Length; i++)
            {
                //Debug.Log(nativeArrays[i]);
                if (nativeArrays[i] == currentNodeIndex)
                {
                    //Debug.Log("found I");
                }
            }
            bool found = neighbourIndices.TryGetFirstValue(currentNodeIndex, out int neighbourIndex, out NativeMultiHashMapIterator<int> iterator);
            //Debug.Log(found);
            //while (found)
            for (int i = 0; i < 999 && found; i++)
            {
                /*
                if (currentNodeIndex == 56)
                {
                    Debug.Log("found " + neighbourIndex);
                }
                */
                PathNode neighbourNode = pathNodes[neighbourIndex];
                if (closedList.Contains(neighbourNode.index))
                {
                    // Already searched this node
                    found = neighbourIndices.TryGetNextValue(out neighbourIndex, ref iterator);
                    continue;
                }
                /*
                if (neighbourNode.position.y > 1)
                {
                    //ignore jump
                    continue;
                }
                */
                if (!closedList.Contains(neighbourNode.index))
                {

                    float tentativeGCost = pathNodes[currentNodeIndex].gCost + CalculateDistanceCost(pathNodes[currentNodeIndex].position, neighbourNode.position);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.prevIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        pathNodes[neighbourNode.index] = neighbourNode;

                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                            Debug.DrawLine(pathNodes[currentNodeIndex].position, neighbourNode.position, Color.red, 10);
                            //Debug.Log(currentNodeIndex + " " + neighbourNode.index);
                        }
                    }
                }
                found = neighbourIndices.TryGetNextValue(out neighbourIndex, ref iterator);
                //Debug.Log(found);
            }
        }

        PathNode endNode = pathNodes[endNodeIndex];
        if (endNode.prevIndex == -1)
        {
            // Didn't find a path!
            Debug.Log("Didn't find a path!");
        }
        else
        {
            // Found a path
            Debug.Log("found a path");
            drawPath(pathNodes, endNode);
            //NativeList<int2> path = CalculatePath(pathNodeArray, endNode);
            /*
            foreach (int2 pathPosition in path) {
                Debug.Log(pathPosition);
            }
            */
            //path.Dispose();
        }

        closedList.Dispose();
        openList.Dispose();

        return new NativeArray<PathNode>();
    }

    private void drawPath(NativeArray<PathNode> pathNodes, PathNode endNode)
    {
        if (endNode.prevIndex == -1)
        {
            return;
        } else
        {
            PathNode node = endNode;
            while(node.prevIndex != -1)
            {
                PathNode prevNode = pathNodes[node.prevIndex];
                Debug.DrawLine(node.position, prevNode.position, Color.green, 500);
                node = prevNode;
            }    
        }
    }

    private bool EqualFloat3(float3 a, float3 b, float tolerance)
    {
        return math.abs(a.x - b.x) < tolerance 
            && math.abs(a.y - b.y) < tolerance 
            && math.abs(a.z - b.z) < tolerance;
    }

    private bool EqualFloat(float a, float b, float tolerance)
    {
        return math.abs(a - b) < tolerance;
    }

    private NativeMultiHashMap<int, int> CollectVertexNeighbours()
    {
        NativeMultiHashMap<int, int> vertexNeighbours = new NativeMultiHashMap<int, int>(0,Allocator.Persistent);

        for (int i = 0; i < navMeshIndices.Length; i += 3)
        {
            vertexNeighbours.Add(i, i + 1);
            vertexNeighbours.Add(i, i + 2);
            vertexNeighbours.Add(i + 1, 1);

        }

        return vertexNeighbours;
    }

    private NativeArray<int> GetIndicesOfVertexNeighbours(int vertexIndex)
    {
        NativeList<int> neighboursIndices = new NativeList<int>(Allocator.Temp);
        //NativeHashMap<int, float3> neighboursIndices = new NativeHashMap<int, float3>(0,Allocator.Temp);
        //NativeMultiHashMap<float3, int> neighboursIndices = new NativeMultiHashMap<float3, int>();

        int neighbourIndex1;
        int neighbourIndex2;

        //int outi = 0;
        for (int i = 0; i < navMeshIndices.Length; i++)
        {
            /*
            if (vertexIndex == 459 && navMeshIndices[i] == 552 || vertexIndex == 552)
            {
                Debug.Log(vertexIndex + "(" + pathNodes[vertexIndex].position + ")" + " pre mod " + i + " " + navMeshIndices[i] + "(" + pathNodes[navMeshIndices[i]].position + ")");
            }*/
            //if (EqualFloat3(pathNodes[navMeshIndices[i]].position, pathNodes[vertexIndex].position, 0.1f))
            if (vertexIndex == navMeshIndices[i]) 
            {
                neighbourIndex1 = -1;
                neighbourIndex2 = -1;
                int remainder = i % 3;
                switch (remainder)
                {
                    case 0:
                        neighbourIndex1 = TryGetNavMeshIndices(i + 1);
                        neighbourIndex2 = TryGetNavMeshIndices(i + 2);
                        break;
                    case 1:
                        neighbourIndex1 = TryGetNavMeshIndices(i - 1);
                        neighbourIndex2 = TryGetNavMeshIndices(i + 1);
                        break;
                    case 2:
                        neighbourIndex1 = TryGetNavMeshIndices(i - 2);
                        neighbourIndex2 = TryGetNavMeshIndices(i - 1);
                        break;
                    default:
                        Debug.LogError("Impossible remainder " + remainder);
                        break;
                }
                //Debug.Log(i + " " + neighbourIndex1 + " " + neighbourIndex2);
                //neighboursIndices.Add(neighbourIndex1);
                //neighboursIndices.Add(neighbourIndex2);
                /*
                if (vertexIndex == 2 && i == 73)
                {
                    Debug.Log(i % 3);
                    Debug.Log(remainder);
                    Debug.Log("loop" + i + " " + neighbourIndex1 + " " + neighbourIndex2);
                }
                */
                if (ValidAndUniqueInt(neighboursIndices, neighbourIndex1)) {
                    neighboursIndices.Add(neighbourIndex1);
                }
                if (ValidAndUniqueInt(neighboursIndices, neighbourIndex2)) {
                    neighboursIndices.Add(neighbourIndex2);
                }
                /*
                neighboursIndices.TryAdd(neighbourIndex1, navMeshVertices[neighbourIndex1]);
                neighboursIndices.TryAdd(neighbourIndex2, navMeshVertices[neighbourIndex2]);
                */
                //if (EqualFloat3(pathNodes[navMeshIndices[i]].position, pathNodes[258].position, 0.1f))
                /*
                if (neighbourIndex1 == 260 || neighbourIndex2 == 260)
                {
                    Debug.Log("found for " + TryGetNavMeshIndices(i));
                    Debug.Log(neighbourIndex1 + " " + neighbourIndex2);
                    writeTextMesh(pathNodes[neighbourIndex1].position, "neighbour1 " + neighbourIndex1, 10, Color.red);
                    writeTextMesh(pathNodes[neighbourIndex2].position, "neighbour2 " + neighbourIndex2, 10, Color.red);
                    Debug.DrawLine(pathNodes[TryGetNavMeshIndices(i)].position, pathNodes[neighbourIndex1].position, Color.green, 999);
                    Debug.DrawLine(pathNodes[neighbourIndex1].position, pathNodes[neighbourIndex2].position, Color.green, 999);
                    Debug.DrawLine(pathNodes[neighbourIndex2].position, pathNodes[TryGetNavMeshIndices(i)].position, Color.green, 999);
                }
                */
            }
            //outi = i;
        }
        /*
        if (vertexIndex == 2)
        {
            for (int i = 0; i < neighboursIndices.Length; i++)
            {
                Debug.Log(neighboursIndices[i]);
            }
        }
        */
        return neighboursIndices.AsArray();
    }

    private bool ValidAndUniqueInt(NativeList<int> list, int value)
    {
        return !list.Contains(value) && value != -1;
    }

    private int TryGetNavMeshIndices(int index)
    {
        if (index < navMeshIndices.Length)
        {
            return navMeshIndices[index];
        } else
        {
            Debug.LogWarning("out of range" + index + "/" + navMeshIndices.Length);
            return navMeshIndices[0];
        }
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
        //Debug.Log(lowestCostPathNode.index);
        return lowestCostPathNode.index;
    }

    /**
     * https://forum.unity.com/threads/navmesh-calculatetriangulation-produces-inaccurate-meshes.293894/
     * PedroCoriAG
     */
    public static void SimplifyMeshTopology(List<Vector3> vertices, List<int> indices, float weldThreshold)
    {
        Profiler.BeginSample("SimplifyNavMeshTopology");
        float startTime = Time.realtimeSinceStartup;

        int startingVerts = vertices.Count;

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

        Debug.Log($"Finished simplifying mesh topology. Time: {Time.realtimeSinceStartup - startTime}. initVerts: {startingVerts}, endVerts: {vertices.Count}");
        Profiler.EndSample();
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
