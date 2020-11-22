using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanningStats : MonoBehaviour {
    public Text PathsPlannedText;
    public Text ReplanningsText;
    public Text ReachedPlansText;

    [HideInInspector]
    public int PathsPlanned = 0;
    [HideInInspector]
    public int Replannings = 0;
    [HideInInspector]
    public int ReachedPlans = 0;

    void Start() {
        UpdateTexts();
    }

    public void UpdateTexts() {
        PathsPlannedText.text = "Paths planned: " + PathsPlanned;
        ReplanningsText.text = "Replannings: " + Replannings;
        ReachedPlansText.text = "Reached plans: " + ReachedPlans;
    }
}
