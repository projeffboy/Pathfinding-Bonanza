using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityGraph {
    public List<Vector3> Vertices = new List<Vector3>();
    // public Vector3[][] adjMatrix = new Vector3[][];
    public Dictionary<Vector3, List<Vector3>> AdjList
        = new Dictionary<Vector3, List<Vector3>>();

    public void AddVertex(Vector3 v) {
        Vertices.Add(v);
        AdjList.Add(v, new List<Vector3>());
    }

    public void PrintVertices() {
        foreach (Vector3 v in Vertices) {
            Debug.Log(v);
        }
    }

    public void AddEdge(Vector3 v1, Vector3 v2) {
        bool v1Exists = false;
        bool v2Exists = false;
        foreach (Vector3 v in Vertices) {
            if (v1 == v) {
                v1Exists = true;
            }
            if (v2 == v) {
                v2Exists = true;
            }
        }

        if (v1Exists && v2Exists) {
            AdjList[v1].Add(v2);
            AdjList[v2].Add(v1);
        }
    }

    public void PrintEdges() {
        foreach (Vector3 v in Vertices) {
            Debug.Log(AdjList[v].Count);
        }
    }

    public Vector3[] AStar() {
        return new Vector3[5];
    }
}
