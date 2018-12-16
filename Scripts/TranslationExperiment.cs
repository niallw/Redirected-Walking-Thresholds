/// <summary>
/// TODO: DOCUMENTATION!!!
///
/// Author: Niall Williams
/// Date: May 25, 2018
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslationExperiment : MovementExperiment {
    // Get this at the beginning of the program so that you don't have to get it every frame
    private Transform playerTransform; // The player's transform
    private Transform targetTransform; // The experiment target's transform
    private Transform spawnTransform; // The player's spawn position transform

    // Values are copied from here 10 times to build the actual array of gains for the trials
    private static float[] GAIN_TEMPLATE = {0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.1f, 1.2f, 1.3f, 1.4f};
    private Vector3 currentTranslation;
    private float distanceTraveled; // Distance traveled since previous frame

	// Use this for initialization
	void Start () {
        // player = GameObject.FindGameObjectWithTag("Player");
        // //TODO: Read in from file or that interface within unity to check which gain is beign tested!! Then update gainToTest accordingly
        // // Also read in if distractors and optical flow (trees) are present.

        // playerTransform = player.transform;

		// BuildGainArrays();
		// spawn = GameObject.FindGameObjectWithTag("Translation start");
		// target = GameObject.FindGameObjectWithTag("Rotation start");
		// targetTransform = target.transform;
		// spawnTransform = spawn.transform;
	}

    public override void Initialize(){
        Debug.Log("trrans");
        myRDWController = GetComponent<RDWController>();
        playerReference = myRDWController.player;
        targetReference = myRDWController.target;
        targetTransform = targetReference.transform;
        spawnPos = myRDWController.spawnTransform.position + new Vector3(0.0f, .5f, 0.0f); // Eye level on the mushroom
        // INTERMISSION_FADE_TIME = myRDWController.INTERMISSION_FADE_TIME;
        // spawnTransform = spawnPos.transform;
        BuildGainArrays();
        ResetPlayerPosition();
    }

	// Update is called once per frame
	void Update () {
		// Debug.Log(""
		// 		+ "CURRENT GAIN: " + gains[0]
        //         + " | DISTRACTOR: " + distractors[0]
        //         + " | DISTRACTOR DIR: " + distractorDirection[0]
        //         + " | GAIN COUNT tran: " + gains.Count
        //         + " | DIS PER GAIN: " + DISTRACTORS_PER_GAIN
        //         + " | START TIME: " + trialStartTime
        //         // + " | TIME ELAPSED: " + (DateTime.Now - trialStartTime).TotalSeconds
		// 		+ " | POS tran: " + playerTransform.position
        // );
	}

    public override IEnumerator RunTrial(){
    // public override void RunTrial(){
        Vector3 positionCurrent = playerTransform.position - new Vector3(0.0f, 0.5f, 0.0f); // Get player's feet position TODO: might change this when i get a HMD
        float distanceFromTarget = Vector3.Distance(positionCurrent, targetTransform.position);
		// Debug.Log("DIST tran: " + distanceFromTarget);

        if (distanceFromTarget <= 0.1f){ //TODO: HOw much precision to use?
            // yield return new WaitForSeconds(3);
            // Player is done
            if (gains.Count == 0){
                Debug.Log("Trials all done!");
            }
            // More gains to be tested
            else{
                ResetTrial();
            }
        }
        yield return null;
    }

    public override void ResetTrial(){
    // public override IEnumerator ResetTrial(){
        ResetPlayerPosition();

        if (failedTrial){
            gains.Add(gains[0]);
            distractors.Add(distractors[0]);
            distractorDirection.Add(distractorDirection[0]);
        }
        gains.RemoveAt(0);
        distractors.RemoveAt(0);
        distractorDirection.RemoveAt(0);

        trialStartTime = DateTime.MinValue;
        // yield return null;
    }

    public override IEnumerator Intermission(){
        yield return null;
    }

    public override IEnumerator ReorientUser(){
        yield return null;
    }

    public override void BuildGainArrays(){
        gains = new List<float>();
        distractors = new List<bool>();
        distractorDirection = new List<int>();

        // Build the arrays with the appropriate amount of trials. 10 trials per gain,
        foreach (float gain in GAIN_TEMPLATE){
            // Trials without distractors. Regular 10 per gain (no distractors)
            for (int i = 0; i < TRIALS_PER_GAIN; i++){
                gains.Add(gain);
                distractors.Add(false);
                distractorDirection.Add(-1); // Dummy value
            }
            // Add the trials with distractors
            if (distractorsPresent){
                for (int i = 0; i < DISTRACTORS_PER_GAIN; i++){
                    gains.Add(gain);
                    distractors.Add(true);
                    // L->R for 50% of distractors, R->L for the other 50%
                    if (i % 2 == 0)
                        distractorDirection.Add(1);
                    else
                        distractorDirection.Add(-1);
                }
            }
        }

        ShuffleTrials();
    }
}