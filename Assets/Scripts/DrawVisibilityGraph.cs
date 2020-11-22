using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawVisibilityGraph : MonoBehaviour {
    public float BoundaryTolerance = 0.75f;
    public LineRenderer Boundaries;
    public LineRenderer Obstacle1;
    public LineRenderer Obstacle2;
    public LineRenderer Obstacle3;
    public LineRenderer Obstacle4;

    [HideInInspector]
    public VisibilityGraph Graph;

    private float WaitStart = 0.05f;
    private Vector3[] boundaryReflexPts;
    private List<LineRenderer> obstacles;
    private Vector3[] obstaclePts;

    void Start() {
        Invoke("Initialize", WaitStart); // must wait for obstacles to spawn first
    }

    private void Initialize() {
        obstacles = new List<LineRenderer>();
        obstacles.Add(Obstacle1);
        obstacles.Add(Obstacle2);
        obstacles.Add(Obstacle3);
        if (Random.value > 0.5f) { // so we have 3-4 obstacles
            obstacles.Add(Obstacle4);
        } else {
            Destroy(Obstacle4.gameObject);
        }

        obstaclePts = new Vector3[6];
        obstacles[0].GetPositions(obstaclePts); // obstacles 1-4 have the same local points though and this returns local points

        Color colorAAA = new Color(0.66f, 0.66f, 0.66f);

        int[] boundaryReflexPtsIdx = {
            0, 3, 5, 8, 9, 12, 13, 16, 18,
            21, 22, 25, 27, 30, 31, 34, 35, 38, 41
        }; // boundary linerenderer vertices
        int iters = boundaryReflexPtsIdx.Length;
        boundaryReflexPts = new Vector3[iters];
        int count = 0;
        foreach (int idx in boundaryReflexPtsIdx) {
            boundaryReflexPts[count] = Boundaries.GetPosition(idx);
            count++;
        }

        /* ADD VERTICES TO VISIBILITY GRAPH */
        
        Graph = new VisibilityGraph();

        foreach (LineRenderer obstacle in obstacles) {
            foreach (Vector3 obstaclePt in obstaclePts) {
                Graph.AddVertex(
                    obstacle.transform.TransformPoint(obstaclePt)
                );
            }
        }
        foreach (Vector3 pt in boundaryReflexPts) {
            Graph.AddVertex(pt);
        }
        var vertices = new List<Vector2>(Graph.Vertices);
        // Debug.Log(VisibilityGraph.Vertices.Count);
        // VisibilityGraph.PrintVertices();

        /* DRAWING LINE SEGMENTS */

        // Between obstacle adjcent reflex points
        foreach (LineRenderer obstacle in obstacles) {
            int obstaclePtIdx = 0;
            Vector3 obstaclePtPrev = obstaclePts[5];

            foreach (Vector3 obstaclePt in obstaclePts) {
                if (obstaclePtIdx != 4) { // 4th point is not reflex
                    Vector3 obstaclePtWorld
                        = obstacle.transform.TransformPoint(obstaclePt);
                    Vector3 obstaclePtWorldPrev
                        = obstacle.transform.TransformPoint(obstaclePtPrev);

                    Debug.DrawRay(
                        obstaclePtWorldPrev,
                        obstaclePtWorld - obstaclePtWorldPrev,
                        Color.red, 1000
                    );
                    Graph.AddEdge(obstaclePtWorldPrev, obstaclePtWorld);

                    obstaclePtPrev = obstaclePt;
                }

                obstaclePtIdx++;
            }
        }

        // Between obstacles (bitangent)
        for (int i = 0; i < obstacles.Count - 1; i++) {
            for (int j = i + 1; j < obstacles.Count; j++) {
                for (int k = 0; k < 6; k++) {
                    for (int l = 0; l < 6; l++) {
                        Vector3 obstaclePtWorld
                            = obstacles[i].transform.TransformPoint(
                                obstaclePts[k]
                            );

                        Vector3 obstaclePtWorldToCompare
                            = obstacles[j].transform.TransformPoint(
                                obstaclePts[l]
                            );

                        DrawVisibilityEdge(
                            obstaclePtWorld, obstaclePtWorldToCompare,
                            Color.yellow, true, true
                        );
                    }
                }
            }
        }

        // Boundaries to Boundaries
        for (int i = 0; i < iters; i++) {
            for (int j = i + 1; j < iters; j++) {
                DrawVisibilityEdge(
                    boundaryReflexPts[i],
                    boundaryReflexPts[j],
                    colorAAA,
                    true,
                    true
                );
            }
        }
        DrawVisibilityEdge( // close the loop
            boundaryReflexPts[iters - 1],
            boundaryReflexPts[0],
            colorAAA,
            true,
            true
        );

        // Boundaries to obstacles
        for (int i = 0; i < iters; i++) {
            foreach (LineRenderer obstacle in obstacles) {
                int obstaclePtIdx = 0;

                foreach (Vector3 obstaclePt in obstaclePts) {
                    if (obstaclePtIdx != 4) { // 4th point is not reflex
                        Vector3 obstaclePtWorld
                            = obstacle.transform.TransformPoint(obstaclePt);

                        DrawVisibilityEdge(
                            boundaryReflexPts[i],
                            obstaclePtWorld,
                            colorAAA,
                            true,
                            true
                        );
                    }

                    obstaclePtIdx++;
                }
            }
        }

        // VisibilityGraph.PrintEdges();
    }

    public Vector3[] DrawVisibilityEdge(
        Vector3 from, Vector3 to,
        Color color,
        bool bitangence = false, bool considerBoundaries = false,
        bool addEdge = true, bool hideLine = false
    ) {

        /* FORWARDS RAYCAST */

        bool hitCollider = false;

        RaycastHit2D hit = Physics2D.Raycast(
            from,
            to - from,
            considerBoundaries
            ? Vector3.Distance(to, from) + BoundaryTolerance : Mathf.Infinity,
            1, -0.5f, 0.5f
        );
        if (hit.collider != null) {
            hitCollider = true;
        }
        if (
            !considerBoundaries
            && hit.collider.gameObject.CompareTag("Boundaries")
        ) {
            hitCollider = false;
        }
        /* NOT AS GOOD CODE
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            from,
            to - from,
            considerBoundaries
            ? Vector3.Distance(to, from) + BoundaryTolerance : Mathf.Infinity
        );
        foreach (RaycastHit2D hit in hits) {
            if (!hit.collider.gameObject.CompareTag("Agent")) {
                if (hit.collider != null) {
                    hitCollider = true;
                }
                if (
                    !considerBoundaries
                    && hit.collider.gameObject.CompareTag("Boundaries")
                ) {
                    hitCollider = false;
                }

                break;
            }
        }
        */

        /* BACKWARDS RAYCAST */

        bool backwardsHitCollider = false;
        RaycastHit2D backwardsHit;
        if (bitangence) {
            backwardsHit = Physics2D.Raycast(
                from,
                -(to - from),
                considerBoundaries ? BoundaryTolerance : Mathf.Infinity,
                1, -0.5f, 0.5f
            );
            
            if (backwardsHit.collider != null) {
                backwardsHitCollider = true;
            }

            if (
                !considerBoundaries
                && backwardsHit.collider.gameObject.CompareTag("Boundaries")
            ) {
                backwardsHitCollider = false;
            }

            /* NOT AS GOOD CODE
            RaycastHit2D[] backwardsHits = Physics2D.RaycastAll(
                from,
                -(to - from),
                considerBoundaries ? BoundaryTolerance : Mathf.Infinity
            );

            foreach (RaycastHit2D backwardsHit in backwardsHits) {
                if (!backwardsHit.collider.gameObject.CompareTag("Agent")) {
                    if (backwardsHit.collider != null) {
                        backwardsHitCollider = true;
                    }

                    if (
                        !considerBoundaries
                        && backwardsHit.collider.gameObject
                            .CompareTag("Boundaries")
                    ) {
                        backwardsHitCollider = false;
                    }

                    break;
                }
            }
            */
        }

        /* FINAL EVALUATION */

        if (!hitCollider && !backwardsHitCollider) {
            if (!hideLine) {
                Debug.DrawRay(
                    from,
                    to - from,
                    color, 1000
                );
            }
            if (addEdge) {
                Graph.AddEdge(from, to);
            }

            return new Vector3[2] {from, to};

            // Debug.Log(obstaclePtIdx + ", " + hit.collider);
        }

        return null;
    }

    public void DrawFromBoundaries(
        Vector2 source, Vector2 target, Color color, VisibilityGraph graph
    ) {
        Vector3[] sourceAndTarget = {source, target};

        foreach (Vector3 boundaryReflexPt in boundaryReflexPts) {
            foreach (Vector3 pt in sourceAndTarget) {
                Vector3[] vertices = DrawVisibilityEdge(
                    boundaryReflexPt, pt, color, false, true, false, true
                );

                if (vertices != null) {
                    graph.AddEdge(vertices[0], vertices[1]);
                }
            }
        }
    }

    public void DrawFromObstacles(
        Vector2 source, Vector2 target, Color color, VisibilityGraph graph
    ) {
        Vector3[] sourceAndTarget = {source, target};
        foreach (Vector3 pt in sourceAndTarget) {
            foreach (LineRenderer obstacle in obstacles) {
                int obstaclePtIdx = 0;
                Vector3 obstaclePtPrev = obstaclePts[5];

                foreach (Vector3 obstaclePt in obstaclePts) {
                    if (obstaclePtIdx != 4) { // 4th point is not reflex
                        Vector3 obstaclePtWorld
                            = obstacle.transform.TransformPoint(obstaclePt);

                        // Between obstacle point and source/target
                        Vector3[] vertices = DrawVisibilityEdge(
                            pt, obstaclePtWorld,
                            Color.red, false, true, false, true
                        );

                        if (vertices != null) {
                            graph.AddEdge(vertices[0], vertices[1]);
                        }

                        // Between obstacle adjcent reflex points
                        Vector3 obstaclePtWorldPrev
                            = obstacle.transform.TransformPoint(obstaclePtPrev);
                        Debug.DrawRay(
                            obstaclePtWorldPrev,
                            obstaclePtWorld - obstaclePtWorldPrev,
                            Color.red, 1000
                        );

                        obstaclePtPrev = obstaclePt;
                    }

                    obstaclePtIdx++;
                }
            }
        }
    }
}
