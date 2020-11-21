using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisibilityGraph {
    public List<Vector2> Vertices = new List<Vector2>();
    public Dictionary<Vector2, List<Vector2>> AdjList = new Dictionary<Vector2, List<Vector2>>();
	
	public Vector2 source;
	public Vector2 target;
	
	public void AddVertex(Vector2 v) {  
        Vertices.Add(v);
        AdjList.Add(v, new List<Vector2>());
    }
	
	public void PrintVertices() {
        foreach (Vector2 v in Vertices) {
            Debug.Log(v);
        }
    }

	public void AddEdge(Vector2 v1, Vector2 v2) {
		bool v1Exists = false;
        bool v2Exists = false;
        foreach (Vector2 v in Vertices) {
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
	
	public void AStar() {
		var Q = new Dictionary<Vector2, Tuple<float, Vector2, bool>>();
		// (float, Vector2, bool) = (weight, prev vertex, isVisited)
		Q.Add(source, Tuple.Create(0f, source, false));
		var archivedQ = new Dictionary<Vector2, Tuple<float, Vector2, bool>>();
		
		while(Q.Count != 0) {
			Vector2 u = Min(Q);
			foreach (Vector2 v in AdjList[u]) { // v is neighbor
				if (!(archivedQ.ContainsKey(v) && archivedQ[v].Item3)) {
					float w = Q[u].Item1 + Vector2.Distance(v, u);

					if (Q.ContainsKey(v)) {
						Vector2 prev = Q[v].Item2;
						if (w < Q[v].Item1) {
							prev = u;
						} else {
							w = Q[v].Item1;
						}


						Q[v] = Tuple.Create(w, prev, Q[v].Item3);
					} else {
						Q.Add(v, Tuple.Create(w, u, false));
					}
				}
			}
			
			Q[u] = Tuple.Create(Q[u].Item1, Q[u].Item2, true);
			archivedQ.Add(u, Q[u]);
			Q.Remove(u);
			
			if (Q.ContainsKey(target) && Min(Q) == target) {
				Console.WriteLine("Arrived at Destination!");
				break;
			}
		}
		
		foreach (KeyValuePair<Vector2, Tuple<float, Vector2, bool>> pair in Q) {
			archivedQ.Add(pair.Key, pair.Value);
		}
		
		Vector2 prevPathVertex = archivedQ[target].Item2;
		while (prevPathVertex != source) {
			Console.WriteLine(prevPathVertex);
			prevPathVertex = archivedQ[prevPathVertex].Item2;
		}
	}
	
	private Vector2 Min(Dictionary<Vector2, Tuple<float, Vector2, bool>> Q) {
		Vector2 u = new Vector2(0, 0);
		float smallestWeight = float.MaxValue;
		foreach (KeyValuePair<Vector2, Tuple<float, Vector2, bool>> pair in Q) {
			if (pair.Value.Item1 < smallestWeight) {
				smallestWeight = pair.Value.Item1;
				u = pair.Key;
			}
		}
		
		return u;
	}
}