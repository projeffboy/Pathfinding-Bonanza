using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanningStats : MonoBehaviour {
    public Text PathsPlannedText;
    public Text ReplanningsText;
    public Text ReachedPlansText;
    public Text SecondsText;

    [HideInInspector]
    public int PathsPlanned = 0;
    [HideInInspector]
    public int Replannings = 0;
    [HideInInspector]
    public int ReachedPlans = 0;
    
    private int seconds = 0;

    void Start() {
        UpdateTexts();
        StartCoroutine(UpdateTimer());
    }

    public void UpdateTexts() {
        PathsPlannedText.text = "Paths planned: " + PathsPlanned;
        ReplanningsText.text = "Replannings: " + Replannings;
        ReachedPlansText.text = "Reached plans: " + ReachedPlans;
        SecondsText.text = "Seconds elapsed: " + seconds;
    }

    IEnumerator UpdateTimer() {
        while (true) {
            yield return new WaitForSeconds(1);
            seconds++;
        }
    }
}
