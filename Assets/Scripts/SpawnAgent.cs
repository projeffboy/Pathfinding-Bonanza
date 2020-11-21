using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAgent : MonoBehaviour {
    public Settings settings;
    private float waitStart = 0.5f;

    void Start() {
        if (settings.IsRandom) {
            Invoke("Initialize", waitStart);
        }
    }

    private void Initialize() {
        GetComponent<CircleCollider2D>().enabled = true;
        RandomSpawn();
    }

    private void OnTriggerEnter2D() {
        RandomSpawn();
    }

    private void RandomSpawn() {
        transform.localPosition = new Vector2(
            Random.Range(-7f, 2f),
            Random.Range(-2f, 1.4f)
        );
    }
}
