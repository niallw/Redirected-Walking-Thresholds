/// <summary>
/// TODO: DOCUMENTATION!!!
///
/// Author: Niall Williams
/// Date: May 25, 2018
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class RotationExperiment : MovementExperiment {
    // Values are copied from here 10 times to build the actual array of gains for the trials
    // The HMD already accounts for the 1-to-1 "rotation" of the world, so that's why our gains are centered
    // around 0 instead of 1.
	private float[] GAIN_TEMPLATE = {-0.4f, -0.3f, -0.2f, -0.1f, 0.0f, 0.1f, 0.2f, 0.3f, 0.4f}; 
	// private float[] GAIN_TEMPLATE = {-0.4f, -0.3f, -0.2f}; 

    // Get this at the beginning of the program so that you don't have to get it every frame
    private Transform playerTransformRef; // The player's transform
    // private Transform playerTransformRef; // The player's transform
    private Transform worldTransformRef;
    private Transform targetTransformRef; // The experiment target's transform
    private Transform spawnTransformRef; // The player's spawn position transform
    private int practiceTrialCount = 0; // Starts at 1 instead of 0 because the first trial is always a practice trial TODO: change comment
    private DateTime trialCompleteStartTime; // Time at which the user completed the trial.
                                             // Exists because we want the user to stay at that position for 3s.
    private GameObject reorientTarget;
    private float reorientTargetAngle = 0.0f;
    private float reorientedTotal;
    private bool reorientTargetRotated = false;
    private Vector3 directionPrevious;

    public override void Initialize(){
        myRDWController  = GetComponent<RDWController>();
        playerReference = myRDWController.player;
        playerTransformRef = playerReference.transform;
        worldReference = myRDWController.world;
        worldTransformRef = worldReference.transform;
        targetReference = myRDWController.target;
        targetTransformRef = targetReference.transform;
        spawnPos = myRDWController.spawnTransform.position + new Vector3(0.0f, .5f, 0.0f); // Eye level on the mushroom
        reorientTarget = GameObject.FindGameObjectWithTag("Reorient target");
        
        // INTERMISSION_FADE_TIME = myRDWController.INTERMISSION_FADE_TIME;
        reorientTarget.SetActive(false);
        myRDWController.leftArrow.SetActive(false);
        myRDWController.rightArrow.SetActive(false);
        BuildGainArrays();

        ResetPlayerPosition();
    }

    public override IEnumerator RunTrial(){
        trialInProgress = true;
        if (practiceTrialCount >= RDWController.NUM_PRACTICE_TRIALS){
            isPracticeTrial = false;
        }

        myRDWController.targetIndicator.SetActive(true);
        myRDWController.completedIndicator.SetActive(userReachedTrialCondition);

        // Store the data to be logged
        myRDWController.rotationDirection = rotationDirection[0];
        myRDWController.gainApplied = gains[0];
        myRDWController.distractorPresent = distractors[0];
        myRDWController.distractorDirection = distractorDirection[0];
        
        // Set up the distractor
        if (!userStartedMovement){
            // Set up the target indicator
            if (rotationDirection[0] < 0)
                myRDWController.rightArrow.SetActive(true);
            else
                myRDWController.leftArrow.SetActive(true);
        }

        Vector3 playerToTarget = (targetTransformRef.position - playerTransformRef.position).normalized;
        playerToTarget.y = 0;
        playerToTarget = playerToTarget.normalized;
        Vector3 temp1 = playerTransformRef.forward;
        temp1.y = 0;
        temp1 = temp1.normalized;
        float dot = Vector3.Dot(playerToTarget, temp1);
        
        if (failedTrial){
            ResetTrial();
            yield return null;
        }

        if (dot > 0.3f && !userStartedMovement){
            userStartedMovement = true;
            // Remove the direction indicator
            myRDWController.leftArrow.SetActive(false);
            myRDWController.rightArrow.SetActive(false);

            // Make the distractor start moving
            if (!myRDWController.distractorController.initialized){
                myRDWController.distractorController.Initialize(distractors[0], distractorDirection[0], rotationDirection[0]);
            }
        }

        if (dot > 0.995f){ 
            userReachedTrialCondition = true;
            // Set the timer for when the user has looked at the target
            if (trialCompleteStartTime == DateTime.MinValue) 
                trialCompleteStartTime = DateTime.Now;
            myRDWController.completedIndicator.GetComponent<SpriteRenderer>().color = new Color(0.2777234f, 0.4530669f, 0.745283f, 1f); // Blue

            // When the user has maintained a "trial complete" status for the right amount of time, move to the next trial
            if ((DateTime.Now - trialCompleteStartTime).TotalSeconds >= 0.25f){
                myRDWController.audioController.PlaySound();
                myRDWController.trialEndTime = DateTime.Now;
                trialCompleteStartTime = DateTime.MinValue; // Hack to prevent the script from moving on to the next gain while the user was looking at the target during the fade to intermission
                ResetTrial();
                yield return null;
            }
        }
        else{
            trialCompleteStartTime = DateTime.MinValue; // Reset the timer when the user looks away from the target
            myRDWController.completedIndicator.GetComponent<SpriteRenderer>().color = new Color(0.8207547f, 0.3213332f, 0.3213332f, 1f); // Red
        }
        
        yield return null;
    }

    public override void ResetTrial(){
        userStartedMovement = false;
        userReachedTrialCondition = false;
        myRDWController.trialStartLogged = false;
        myRDWController.distractorController.Reset();
        myRDWController.leftArrow.SetActive(false); // Just turn them both off, so I don't have to check which one was on in the first place
        myRDWController.rightArrow.SetActive(false);
        myRDWController.completedTrialCount++;
        trialInProgress = false;
        myRDWController.intermissionRunning = true; //TODO: implement intermission support 
        reorientedTotal = 0.0f;
        if (practiceTrialCount < RDWController.NUM_PRACTICE_TRIALS){
            practiceTrialCount++;
            return;    
        }
    
        if (failedTrial && !isPracticeTrial){
            failedTrial = false;
            Debug.Log("USER FAILED");
            gains.Add(gains[0]);
            distractors.Add(distractors[0]);
            distractorDirection.Add(distractorDirection[0]);
			rotationDirection.Add(rotationDirection[0]);
        }
        gains.RemoveAt(0);
        distractors.RemoveAt(0);
        distractorDirection.RemoveAt(0);
		rotationDirection.RemoveAt(0);
    }

    public override IEnumerator Intermission(){
            yield return null;
    }

    public override IEnumerator ReorientUser(){
        reorientTarget.SetActive(true);
        myRDWController.orientingInProgress = true;
        Transform reorientTargetTransform = reorientTarget.transform;
        if (reorientTargetAngle == 0.0f)
            reorientTargetAngle = myRDWController.totalTurnedReal;
        if (!reorientTargetRotated){
            reorientTargetTransform.position = playerTransformRef.position + playerTransformRef.forward * 2f;
            reorientTargetTransform.rotation = Quaternion.LookRotation(playerTransformRef.forward);
            reorientTargetTransform.RotateAround(playerTransformRef.position, Vector3.up, reorientTargetAngle);
            reorientTargetRotated = true;
        }

        myRDWController.targetIndicator.SetActive(true);
        myRDWController.completedIndicator.SetActive(true);

        Vector3 playerToReorientTarget = (reorientTargetTransform.position - playerTransformRef.position).normalized;
        playerToReorientTarget.y = 0;
        playerToReorientTarget = playerToReorientTarget.normalized;
        Vector3 temp1 = playerTransformRef.forward;
        temp1.y = 0;
        temp1 = temp1.normalized;
        float dot = Vector3.Dot(playerToReorientTarget, temp1);
        
        if (Vector3.SignedAngle(playerTransformRef.forward, reorientTargetTransform.position-playerTransformRef.position, Vector3.up) < 0){
            myRDWController.rightArrow.SetActive(true);
            myRDWController.leftArrow.SetActive(false);
        }
        else{
            myRDWController.rightArrow.SetActive(false);
            myRDWController.leftArrow.SetActive(true);
        }

        if (dot > 0.995f){ //TODO: HOw much precision to use? FIXME: uncomment this so it works like normal again. was testing but got sick of walking to the hmd all day
            // Set the timer for when the user has looked at the target
            if (trialCompleteStartTime == DateTime.MinValue) 
                trialCompleteStartTime = DateTime.Now;
            myRDWController.completedIndicator.GetComponent<SpriteRenderer>().color = new Color(0.2777234f, 0.4530669f, 0.745283f, 1f); // Blue
            myRDWController.leftArrow.SetActive(false);
            myRDWController.rightArrow.SetActive(false);

            // When the user has maintained a "trial complete" status for the right amount of time, move to the next trial
            if ((DateTime.Now - trialCompleteStartTime).TotalSeconds >= 0.75f){
                myRDWController.targetIndicator.SetActive(false);
                reorientTarget.SetActive(false);
                myRDWController.userWasReoriented = true;
                // myRDWController.orientingInProgress = false;
                yield return null;
            }
        }
        else{ //TODO: FIXME: uncomment this so it works like normal again. was testing but got sick of walking to the hmd all day
            trialCompleteStartTime = DateTime.MinValue; // Reset the timer when the user looks away from the target
            myRDWController.completedIndicator.GetComponent<SpriteRenderer>().color = new Color(0.8207547f, 0.3213332f, 0.3213332f, 1f); // Red
        }
        
        yield return null;
    }

    /// <summary>
    /// Build the arrays of information to dictate the settings for each trial. These arrays contain
    /// the gain to apply, which direction the user must turn, if there is a distractor
    /// present in the trial, and which direction it is coming from.
    /// </summary>
    public override void BuildGainArrays(){
        gains = new List<float>();
        distractors = new List<bool>();
        distractorDirection = new List<int>();
        rotationDirection = new List<int>();

        // Build the arrays with the appropriate amount of trials. 10 trials per gain
        foreach (float gain in GAIN_TEMPLATE){
            // Trials without distractors. Regular 10 per gain (no distractors)
            for (int i = 0; i < TRIALS_PER_GAIN; i++){
                gains.Add(gain);
                distractors.Add(false);
                distractorDirection.Add(0); // Dummy value
                if (i % 2 == 0)
                    rotationDirection.Add(1);
                else
                    rotationDirection.Add(-1);
            }
            // Add the trials with distractors
            if (distractorsPresent){
                List<int> tempList = new List<int>();
                for (int i = 0; i < DISTRACTORS_PER_GAIN; i++){
                    if (i % 2 == 0)
                        tempList.Add(1);
                    else
                        tempList.Add(-1);
                }
                tempList = RandomShuffle(tempList);

                for (int i = 0; i < DISTRACTORS_PER_GAIN; i++){
                    gains.Add(gain);
                    distractors.Add(true);
                    distractorDirection.Add(tempList[i]);
                    // L->R for 50% of distractors, R->L for the other 50%
                    if (i % 2 == 0)
                        rotationDirection.Add(1);
                    else
                        rotationDirection.Add(-1);
                }
            }
        }

        ShuffleTrials();
    }
}