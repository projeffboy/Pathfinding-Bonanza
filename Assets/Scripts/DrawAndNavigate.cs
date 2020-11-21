using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawAndNavigate : MonoBehaviour {
    public Settings settings;
    public float BoundaryTolerance = 0.5f;
    public float Speed = 0.5f;
    public LineRenderer Boundaries;
    public LineRenderer Obstacle1;
    public LineRenderer Obstacle2;
    public LineRenderer Obstacle3;
    public LineRenderer Obstacle4;

    public Vector3[] Path;

    private float waitStart = 0.05f;
    private Vector3 target;
    private VisibilityGraph graph;

    void Start() {
        Invoke("Initialize", waitStart);
    }

    private void Initialize() {
        // Vector3 source = transform.localPosition;
        target = new Vector2(1.74f, -0.18f);

        List<LineRenderer> obstacles = new List<LineRenderer>();
        obstacles.Add(Obstacle1);
        obstacles.Add(Obstacle2);
        obstacles.Add(Obstacle3);
        if (!settings.IsRandom || Random.value > 0.5f) {
            obstacles.Add(Obstacle4);
        } else {
            Destroy(Obstacle4.gameObject);
        }

        Vector3[] obstaclePts = new Vector3[6];
        obstacles[0].GetPositions(obstaclePts); // obstacles 1-4 have the same local points though and this returns local points

        Color colorAAA = new Color(0.66f, 0.66f, 0.66f);

        int[] boundaryReflexPtsIdx = {
            0, 3, 5, 8, 9, 12, 13, 16, 18,
            21, 22, 25, 27, 30, 31, 34, 35, 38, 41
        };
        int iters = boundaryReflexPtsIdx.Length;
        Vector3[] boundaryReflexPts = new Vector3[iters];
        int count = 0;
        foreach (int idx in boundaryReflexPtsIdx) {
            boundaryReflexPts[count] = Boundaries.GetPosition(idx);
            count++;
        }

        /* ADD VERTICES TO VISIBILITY GRAPH */
        graph = new VisibilityGraph();
        // graph.AddVertex(source);
        // graph.AddVertex(target);
        foreach (LineRenderer obstacle in obstacles) {
            foreach (Vector3 obstaclePt in obstaclePts) {
                graph.AddVertex(obstacle.transform.TransformPoint(obstaclePt));
            }
        }
        foreach (Vector3 pt in boundaryReflexPts) {
            graph.AddVertex(pt);
        }

        /* DRAWING LINE SEGMENTS */

        // Source to target
        // Path = DrawVisibilityEdge(source, target, Color.green);

        // Source, target to obstacles
        /*
        Vector3[] sourceAndTarget = {source, target};
        foreach (Vector3 pt in sourceAndTarget) {
            foreach (LineRenderer obstacle in obstacles) {
                int obstaclePtIdx = 0;
                Vector3 obstaclePtPrev = obstaclePts[5];

                foreach (Vector3 obstaclePt in obstaclePts) {
                    if (obstaclePtIdx != 4) {
                        Vector3 obstaclePtWorld
                            = obstacle.transform.TransformPoint(obstaclePt);

                        DrawVisibilityEdge(pt, obstaclePtWorld, Color.red);

                        // Between obstacle adjcent reflex points
                        Vector3 obstaclePtWorldPrev
                            = obstacle.transform.TransformPoint(obstaclePtPrev);
                        Debug.DrawRay(
                            obstaclePtWorldPrev,
                            obstaclePtWorld - obstaclePtWorldPrev,
                            Color.red, 1000, false
                        );

                        obstaclePtPrev = obstaclePt;
                    }

                    obstaclePtIdx++;
                }
            }
        }
        */

        // Between obstacle adjcent reflex points
        foreach (LineRenderer obstacle in obstacles) {
            int obstaclePtIdx = 0;
            Vector3 obstaclePtPrev = obstaclePts[5];

            foreach (Vector3 obstaclePt in obstaclePts) {
                if (obstaclePtIdx != 4) {
                    Vector3 obstaclePtWorld
                        = obstacle.transform.TransformPoint(obstaclePt);
                    Vector3 obstaclePtWorldPrev
                        = obstacle.transform.TransformPoint(obstaclePtPrev);

                    Debug.DrawRay(
                        obstaclePtWorldPrev,
                        obstaclePtWorld - obstaclePtWorldPrev,
                        Color.red, 1000, false
                    );

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
                            Color.yellow, true
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

        DrawVisibilityEdge(
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
                    if (obstaclePtIdx != 4) {
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

        // Boundaries to Source, target
        /*
        for (int i = 0; i < iters; i++) {
            foreach (Vector3 pt in sourceAndTarget) {
                DrawVisibilityEdge(
                    boundaryReflexPts[i],
                    pt,
                    colorAAA,
                    true,
                    true
                );
            }
        }
        */
    }

    private Vector3[] DrawVisibilityEdge(
        Vector3 from, Vector3 to,
        Color color, bool bitangence = false, bool considerBoundaries = false
    ) {
        bool hitCollider = false;
        RaycastHit2D hit = Physics2D.Raycast(
            from,
            to - from,
            considerBoundaries
            ? Vector3.Distance(to, from) + BoundaryTolerance : Mathf.Infinity
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

        bool backwardsHitCollider = false;
        RaycastHit2D backwardsHit;
        if (bitangence) {
            backwardsHit = Physics2D.Raycast(
                from,
                -(to - from),
                considerBoundaries ? BoundaryTolerance : Mathf.Infinity
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
        }

        if (
            !hitCollider && !backwardsHitCollider
        ) {
            Debug.DrawRay(
                from,
                to - from,
                color, 1000, false
            );
            graph.AddEdge(from, to);

            return new Vector3[2] {from, to};

            // Debug.Log(obstaclePtIdx + ", " + hit.collider);
        }

        return null;
    }

    /*
    void Update() {
        if (Time.time > waitStart + 0.1f && Path != null) {
            float step = Time.deltaTime * Speed;
            
            transform.position = Vector3.MoveTowards(
                transform.position, Path[1], step
            );
        }
    }
    */
}
