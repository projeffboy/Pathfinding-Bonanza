using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing : MonoBehaviour {
    [HideInInspector]
    public SpawnAgents SpawnAgentsScript;
    [HideInInspector]
    public DrawVisibilityGraph DrawVisibilityGraphScript;
    [HideInInspector]
    public VisibilityGraph OriginalGraph;
    
    private VisibilityGraph Graph;

    private float Speed = 1.5f;
    private GameObject Target;
    private List<Vector2> Path;
    private Color color;
    private int nthNodeInPath = 0; // not counting source node
    private float WaitForASecond = 0;
    private bool waiting = false;

    void Start() {
        color = GetComponent<SpriteRenderer>().color;

        Recalculate();
    }

    void Update() {
        if (Path.Count == 0) {
            return;    
        }

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
        } else if (WaitForASecond >= 1) {
            if (!waiting) {
                waiting = true;
                Recalculate();
            }
        } else {
            WaitForASecond += Time.deltaTime;
        }
    }

    private void Recalculate() {
        if (Target != null) {
            Destroy(Target);
        }

        List<GameObject> agents = new List<GameObject>();
        agents.Add(gameObject);


        // Source to Target
        Target = SpawnAgentsScript.Spawn(false, agents, color);
        var edge = DrawVisibilityGraphScript.DrawVisibilityEdge(
            transform.position, Target.transform.position,
            Color.green, false, true, false, true
        );
        Path = new List<Vector2>();
        if (edge != null) {
            Path.Add(edge[1]);
        }

        nthNodeInPath = 0;
        waiting = false;

        if (Path.Count > 0) {
            return;
        }

        Graph = new VisibilityGraph();
        Graph.Vertices = OriginalGraph.ClonedVertices();
        Graph.AdjList = OriginalGraph.ClonedAdjList();
        Graph.SetSource(transform.position);
        Graph.SetTarget(Target.transform.position);

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