using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing : MonoBehaviour {
    public float Speed = 1.5f;
    [HideInInspector]
    public SpawnAgents SpawnAgentsScript;
    [HideInInspector]
    public DrawVisibilityGraph DrawVisibilityGraphScript;
    [HideInInspector]
    public VisibilityGraph Graph;

    private GameObject Target;
    private List<Vector2> Path;
    private Color color;
    private int nthNodeInPath = 0; // not counting source node

    void Start() {
        color = GetComponent<SpriteRenderer>().color;

        Graph.SetSource(transform.position);

        Recalculate();
    }

    void Update() {
        if (Path.Count > nthNodeInPath) {
            float step = Time.deltaTime * Speed;
            
            //Debug.Log(transform.position + ", " + Path[1]);
            transform.position = Vector2.MoveTowards(
                transform.position, Path[nthNodeInPath], step
            );

            if (
                ((Vector2)transform.position - Path[nthNodeInPath])
                    .sqrMagnitude < 0.0001f
            ) {
                nthNodeInPath++;
            }
        }
    }

    private void Recalculate() {
        if (Target != null) {
            Destroy(Target);
            Graph.RemoveVertex(Target.transform.position);
        }

        List<GameObject> agents = new List<GameObject>();
        agents.Add(gameObject);

        Target = SpawnAgentsScript.Spawn(false, agents, color);
        Graph.SetTarget(Target.transform.position);

        // Source to Target
        var edge = DrawVisibilityGraphScript.DrawVisibilityEdge(
            transform.position, Target.transform.position,
            Color.green, false, true, false, true
        );
        Path = new List<Vector2>();
        if (edge != null) {
            Path.Add(edge[1]);
        }

        if (Path.Count > 0) {
            return;
        }

        // Debug.DrawLine(transform.position, Target, color, 100);

        // Graph.PrintNumEdges();

        DrawVisibilityGraphScript.DrawFromBoundaries(
            transform.position, Target.transform.position, color, Graph
        );

        DrawVisibilityGraphScript.DrawFromObstacles(
            transform.position, Target.transform.position, color, Graph
        );

        // Graph.PrintNumEdges();

        /*
        if (Graph.AdjList.ContainsKey(transform.position)) {
            Debug.Log(Graph.AdjList[transform.position].Count);
        } else {
            Debug.Log("Nada");
        }
        */

        if (
            Graph.AdjList.ContainsKey(transform.position)
            && Graph.AdjList[transform.position].Count > 0
            && Graph.AdjList.ContainsKey(Target.transform.position)
            && Graph.AdjList[Target.transform.position].Count > 0
        ) {
            // Debug.Log("A Star time.");
            Path = Graph.AStar();
        }
    }
}