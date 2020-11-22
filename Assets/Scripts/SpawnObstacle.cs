using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObstacle : MonoBehaviour {
    public float MinX;
    public float MaxX;
    public float MinY;
    public float MaxY;

    void Start() {
        RandomSpawn();
    }

    private void RandomSpawn() {
        // Randomize Location to an extent
        transform.localPosition = new Vector2(
            Random.Range(MinX, MaxX),
            Random.Range(MinY, MaxY)
        );

        // Randomize Scale to an extent
        transform.localScale += new Vector3(
            Random.Range(-0.375f, 0.375f),
            Random.Range(-0.375f, 0.375f),
            0
        );
    }
}
