using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObstacle : MonoBehaviour {
    public Settings settings;
    public float MinX;
    public float MaxX;
    public float MinY;
    public float MaxY;

    void Start() {
        if (settings.IsRandom) {
            RandomSpawn();
        }
    }

    private void RandomSpawn() {
        transform.localPosition = new Vector2(
            Random.Range(MinX, MaxX),
            Random.Range(MinY, MaxY)
        );
    }

    /*
    Random.Range(-7f, 2f),
    Random.Range(-2f, 1.4f)
    */
}
