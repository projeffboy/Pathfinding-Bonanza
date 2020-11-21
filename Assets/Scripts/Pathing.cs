using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing : MonoBehaviour {
    [HideInInspector]
    public SpawnAgents SpawnAgentsScript;

    private Vector2 Target;

    void Start() {
        List<GameObject> agents = new List<GameObject>();
        agents.Add(gameObject);

        Color color = GetComponent<SpriteRenderer>().color;

        Target = SpawnAgentsScript.Spawn(false, agents, color)
            .transform.position;
        
        // Debug.DrawLine(transform.position, Target, color, 100);
    }
}