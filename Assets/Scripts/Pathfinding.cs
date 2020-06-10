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

public class Pathfinding : MonoBehaviour
{
    private NativeArray<float3> navMeshVertices;
    private NativeArray<int> navMeshIndices;
    private NativeMultiHashMap<int,int> neighbourIndices;

    public GameObject start;
    public GameObject end;
    public GameObject textMeshPrefab;

    NativeArray<PathNode> pathNodes;

    void Start()
    {
        NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
        GetComponent<MeshFilter>().mesh.vertices = navMeshTriangulation.vertices;
        GetComponent<MeshFilter>().mesh.triangles = navMeshTriangulation.indices;
        navMeshVertices = new NativeArray<float3>(navMeshTriangulation.vertices.Length, Allocator.Temp);
        for (int i = 0; i < navMeshTriangulation.vertices.Length; i++)
        {
            navMeshVertices[i] = navMeshTriangulation.vertices[i];
        }

        navMeshIndices = new NativeArray<int>(navMeshTriangulation.indices.Length, Allocator.Temp);
        for (int i = 0; i < navMeshTriangulation.indices.Length; i++)
        {
            navMeshIndices[i] = navMeshTriangulation.indices[i];
            paintVertexFromTriangleIndex(i);
        }

        /*
        Debug.Log(navMeshTriangulation.vertices[0]);
        Debug.Log(navMeshTriangulation.vertices[1]);
        Debug.Log(navMeshTriangulation.vertices[2]);
        Debug.Log(navMeshTriangulation.vertices[3]);
        Debug.Log(navMeshTriangulation.vertices[4]);
        Debug.Log(navMeshTriangulation.vertices[5]);
        */

        for (int i = 0; i < 160; i+=3)
        {
            /*
            Debug.Log("index " + i + " " 
                + navMeshTriangulation.indices[i] + "("+ navMeshTriangulation.vertices[navMeshTriangulation.indices[i]]+")"
                + navMeshTriangulation.indices[i+1] + "(" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i+1]] + ")"
                + navMeshTriangulation.indices[i+2] + "(" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i+2]] + ")");
            */
            Debug.DrawLine(navMeshTriangulation.vertices[navMeshTriangulation.indices[i]], navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 1]],Color.green,50);
            Debug.DrawLine(navMeshTriangulation.vertices[navMeshTriangulation.indices[i+1]], navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 2]], Color.green,50);
            Debug.DrawLine(navMeshTriangulation.vertices[navMeshTriangulation.indices[i+2]], navMeshTriangulation.vertices[navMeshTriangulation.indices[i]], Color.green,50);
            /*
            TextMesh textMeshObject1 = Instantiate(textMeshPrefab, navMeshTriangulation.vertices[navMeshTriangulation.indices[i]], textMeshPrefab.transform.rotation).GetComponent<TextMesh>();
            textMeshObject1.text = navMeshTriangulation.indices[i] + ":" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i]].ToString();
            textMeshObject1.name = textMeshObject1.text;
            TextMesh textMeshObject2 = Instantiate(textMeshPrefab, navMeshTriangulation.vertices[navMeshTriangulation.indices[i+1]], textMeshPrefab.transform.rotation).GetComponent<TextMesh>();
            textMeshObject2.text = navMeshTriangulation.indices[i+1] + ":" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i+1]].ToString();
            textMeshObject2.name = textMeshObject2.text;
            TextMesh textMeshObject3 = Instantiate(textMeshPrefab, navMeshTriangulation.vertices[navMeshTriangulation.indices[i+2]], textMeshPrefab.transform.rotation).GetComponent<TextMesh>();
            textMeshObject3.text = navMeshTriangulation.indices[i+2] + ":" + navMeshTriangulation.vertices[navMeshTriangulation.indices[i+2]].ToString();
            textMeshObject3.name = textMeshObject3.text;
            */

        }

        GetPath(start.transform.position, end.transform.position);
    }

    private void paintVertexFromTriangleIndex(int index)
    {
        TextMeshPro textMeshObject = Instantiate(textMeshPrefab, navMeshVertices[navMeshIndices[index]], textMeshPrefab.transform.rotation).GetComponent<TextMeshPro>();
        /*
        foreach (TextMeshPro item in FindObjectsOfType<TextMeshPro>())
        {
            if (item.transform.position == textMeshObject.transform.position)
            {
                textMeshObject.transform.position += new Vector3(0, 0, -1);
            }
        }
        */
        textMeshObject.text = navMeshIndices[index] + ":" + navMeshVertices[navMeshIndices[index]].ToString();
        textMeshObject.name = textMeshObject.text;
    }

    public NativeArray<PathNode> GetPath(float3 start, float3 end)
    {
        neighbourIndices = new NativeMultiHashMap<int, int>(0, Allocator.Temp);
        pathNodes = new NativeArray<PathNode>(navMeshVertices.Length, Allocator.Temp);
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
            for (int ii = 0; ii < vertexNeighbours.Length; ii++)
            {
                neighbourIndices.Add(i, vertexNeighbours[ii]);
                if (i == 463)
                {
                //Debug.Log("add " + i + " " + vertexNeighbours[ii]);
                }
            }
        }

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        int startNodeIndex = CalculateIndex(start);
        int endNodeIndex = CalculateIndex(end);
        Debug.Log(startNodeIndex + " " + endNodeIndex);
        Debug.DrawLine(pathNodes[startNodeIndex].position, pathNodes[endNodeIndex].position, Color.blue, 100);

        //PathNode startNode = pathNodes[CalculateIndex(startNodeIndex)];
        PathNode startNode = pathNodes[startNodeIndex];
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
                if (currentNodeIndex == 463)
                {
                    Debug.Log("found " + neighbourIndex);
                }
                PathNode neighbourNode = pathNodes[neighbourIndex];
                /*
                if (closedList.Contains(neighbourNode.index))
                {
                    // Already searched this node
                    continue;
                }*/
                if (!closedList.Contains(neighbourNode.index))
                {

                    //float tentativeGCost = pathNodes[currentNodeIndex].gCost + CalculateDistanceCost(pathNodes[currentNodeIndex].position, neighbourNode.position);
                    //if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.prevIndex = currentNodeIndex;
                        //neighbourNode.gCost = tentativeGCost;
                        pathNodes[neighbourNode.index] = neighbourNode;

                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                            Debug.DrawLine(pathNodes[currentNodeIndex].position, neighbourNode.position, Color.red, 10);
                            Debug.Log(currentNodeIndex + " " + neighbourNode.index);
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

    private NativeArray<int> GetIndicesOfVertexNeighbours(int vertexIndex)
    {
        //NativeList<int> neighboursIndices = new NativeList<int>(Allocator.Temp);
        NativeHashMap<int, float3> neighboursIndices = new NativeHashMap<int, float3>(0,Allocator.Temp);
        //NativeMultiHashMap<float3, int> neighboursIndices = new NativeMultiHashMap<float3, int>();

        int neighbourIndex1 = 0;
        int neighbourIndex2 = 0;

        for (int i = 0; i < navMeshIndices.Length; i++)
        {
            if(math.abs(pathNodes[navMeshIndices[i]].position).Equals(math.abs(pathNodes[vertexIndex].position)))
            {
                int remainder = navMeshIndices[i] % 3;
                switch (remainder)
                {
                    case 0:
                        neighbourIndex1 = navMeshIndices[i + 1];
                        neighbourIndex2 = navMeshIndices[i + 2];
                        break;
                    case 1:
                        neighbourIndex1 = navMeshIndices[i - 1];
                        neighbourIndex2 = navMeshIndices[i + 1];
                        break ;
                    case 2:
                        neighbourIndex1 = navMeshIndices[i - 2];
                        neighbourIndex2 = navMeshIndices[i - 1];
                        break;
                    default:
                        Debug.LogError("Impossible remainder " + remainder);
                        break;
                }
                //Debug.Log(i + " " + neighbourIndex1 + " " + neighbourIndex2);
                neighboursIndices.TryAdd(neighbourIndex1, navMeshVertices[neighbourIndex1]);
                neighboursIndices.TryAdd(neighbourIndex2, navMeshVertices[neighbourIndex2]);
                /*
                if (vertexIndex == 0)
                {
                    Debug.Log("found 0 for " + i);
                    Debug.Log(neighbourIndex1 + " " + neighbourIndex2);
                }*/
            }
        }
        for (int i = 0; i < neighboursIndices.GetKeyArray(Allocator.Temp).Length; i++)
        {
            if (vertexIndex == 0)
            {
            Debug.Log("search " + vertexIndex + " " + neighboursIndices.GetKeyArray(Allocator.Temp)[i]);
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
        //Debug.Log(lowestCostPathNode.index);
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
