using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAgents : MonoBehaviour {
    public GameObject point;

    void Start() {
        float randX = Random.Range(-7.25f, 5.25f);
        float randY = Random.Range(-5.61f, 3.69f);

        Instantiate(point, new Vector2(randX, randY), Quaternion.identity);

    }
}
