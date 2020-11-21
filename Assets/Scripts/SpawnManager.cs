using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public Settings settings;
    public GameObject obstacles;

    void Start() {
        if (settings.IsRandom) {
            foreach (
                Transform obstacle
                in obstacles.GetComponentsInChildren<Transform>()
            ) {
                RandomSpawn(obstacle);
            }
        }
    }

    /*
    private void OnTriggerEnter2D() {
        RandomSpawn();
    }
    */

    private void RandomSpawn(Transform obstacle) {
        if (obstacle == obstacles.transform) {
            return;
        }

        obstacle.localPosition = new Vector2(
            Random.Range(-7f, 2f),
            Random.Range(-2f, 1.4f)
        );

        RaycastHit2D hit;
        Vector2[] directions = {
            Vector2.up,
            Vector2.up + Vector2.right,
            Vector2.right,
            Vector2.right + Vector2.down,
            Vector2.down,
            Vector2.down + Vector2.left,
            Vector2.left,
            Vector2.left + Vector2.up
        };
        foreach (Vector2 dir in directions) {
            hit = Physics2D.Raycast(
                obstacle.TransformPoint(new Vector2(0.55f, -0.23f)),
                dir,
                1
            );
            Debug.DrawRay(
                obstacle.TransformPoint(new Vector2(0.55f, -0.23f)),
                dir.normalized,
                Color.green, 10, false
            );

            if (hit.collider != null) {
                Debug.Log(hit.collider);
                RandomSpawn(obstacle);
                break;
            }
        }
    }
}
