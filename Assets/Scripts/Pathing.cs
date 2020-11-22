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
    [HideInInspector]
    public PlanningStats StatsScript;
    [HideInInspector]
    public int Id; // originally gonna prioritize agents with ids but nah

    private VisibilityGraph Graph;

    private float Speed = 1.5f;
    private GameObject Target;
    private List<Vector2> Path;
    private Color color;
    private int nthNodeInPath = 0; // not counting source node
    private bool finished = false;
    private bool impasse = false;
    private float waitingTimer = 0;
    private int replans = 0;
    private float cooldown = 5;

    void Start() {
        color = GetComponent<SpriteRenderer>().color;

        Recalculate(true);
        StatsScript.PathsPlanned++;
    }

    void Update() {
        if (Path.Count == 0) {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0) {
                Recalculate(true);
                StatsScript.PathsPlanned++;
                cooldown = 5;
            }

            return;
        }

        if (finished || impasse) {
            waitingTimer -= Time.deltaTime;

            if (impasse && Path.Count > nthNodeInPath) {
                float step = Time.deltaTime * Speed;

                // give the agents a chance to collide again
                transform.position = Vector2.MoveTowards(
                    transform.position, -Path[nthNodeInPath], step / 16
                );
            }

            if (waitingTimer <= 0) {
                Recalculate(finished);
                StatsScript.PathsPlanned++;
                nthNodeInPath = 0;
                finished = false;
                impasse = false;
            }
        } else if (Path.Count > nthNodeInPath) {
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
        } else { // when Path.Count == nthNodeInPath
            finished = true;
            waitingTimer = 1f;
            StatsScript.ReachedPlans++;
        }
    }

    private void Recalculate(bool changeTarget) {
        List<GameObject> agents = new List<GameObject>();
        agents.Add(gameObject);


        // Source to Target
        if (changeTarget || Target != null) {
            Destroy(Target);
            Target = SpawnAgentsScript.Spawn(false, agents, color);
            replans = 0; // can't forget this
        }
        var edge = DrawVisibilityGraphScript.DrawVisibilityEdge(
            transform.position, Target.transform.position,
            Color.green, false, true, false, true
        );
        Path = new List<Vector2>();
        if (edge != null) {
            Path.Add(edge[1]);
        }

        if (Path.Count > 0) { // if there's a direct path between two points, no need to even compute A*, just walk the path!
            return;
        }

        // Clone a fresh new graph
        Graph = new VisibilityGraph();
        Graph.Vertices = OriginalGraph.ClonedVertices();
        Graph.AdjList = OriginalGraph.ClonedAdjList();
        Graph.SetSource(transform.position);
        Graph.SetTarget(Target.transform.position);

        // Debug.DrawLine(transform.position, Target, color, 100);

        // Graph.PrintNumEdges();

        // Edges from source/target to boundaries/obstacles
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
            // Debug.Log(Time.realtimeSinceStartup);
            float planningTime = Time.realtimeSinceStartup;
            Path = Graph.AStar();
            planningTime = Time.realtimeSinceStartup - planningTime;
            StatsScript.TotalPlanningTime += planningTime;
            // Debug.Log(Time.realtimeSinceStartup);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Agent")) {
            impasse = true;
            waitingTimer = 0.5f;
            replans++;

            if (replans >= 3) { // new plan
                impasse = false;
                finished = true;
                waitingTimer = 1f;
                StatsScript.PathsPlanned++;
            } else {
                StatsScript.Replannings++;
            }
        }
    }
}