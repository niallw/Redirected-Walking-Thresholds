using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generic class for a movement experiment
/// </summary>
public abstract class MovementExperiment : MonoBehaviour {

	protected RDWController myRDWController; // Reference to experiment controller
    protected SteamVR_Camera playerReference; // The player (camera) that is being tracked TODO: probably remove this since I just put the vr camera inside a generic objkect tagged as player
	protected GameObject worldReference;
    protected GameObject targetReference; // The object the player must look at to finish the trial
    protected GameObject spawnReference; // Player spawns here
	protected static Vector3 spawnPos; // Where the user begins a trial
	protected static Vector3 target; // Win condition for the trial

	// Experiment settings
	public bool isPracticeTrial = true;
    public bool trialInProgress = false;
    public List<float> gains; // Holds the shuffled list of gains to test for each trial
    public List<bool> distractors; // Denotes when a distractor will be present in the trial
    public List<int> distractorDirection; // Direction of the distractor when present
                                             // True = Left to right FIXME: this is an int list not a bool list
                                             // False = Right to left
    public List<int> rotationDirection; // Direction the user will turn his/her head
                                        // True = Left to right FIXME: this is an int list not a bool list
                                        // False = Right to left
    protected static bool distractorsPresent = true; //TODO: set this variable via the config file per user
    protected static bool opticalFlowPresent;
    protected static int TRIALS_PER_GAIN = 8; // How many times each gain is tested
    protected static int DISTRACTORS_PER_GAIN = 8; 
    protected static float MAX_TRIAL_LENGTH = 5.0f; // Trial is failed if it is longer than this TODO: change htis value to be appropriate. I picked a random number
    public bool failedTrial = false; // If the user failed the most recent trial
    protected int failCount = 0; // Amount of trials the user has failed
    protected DateTime trialStartTime = DateTime.MinValue;
	protected float INTERMISSION_FADE_TIME;
	public bool userStartedMovement = false; // Turns to true and stays at true when the user begins moving
	protected bool userReachedTrialCondition = false;

	// public abstract void ApplyGain(); //TODO: Will probably delete this. Don't know. Life's a lot easier if I let the controller handle applying gains, but it's not really in the spirit of amazingly-structured code. I think it's okay being handled in the controller, though..

	public abstract void Initialize();

	public abstract void BuildGainArrays();

	public abstract IEnumerator RunTrial();
	// public abstract void RunTrial();

	public abstract void ResetTrial();
	// public abstract IEnumerator ResetTrial();

	public abstract IEnumerator Intermission();

	public abstract IEnumerator ReorientUser();

	/// <summary>
	/// Move the player back to the start position of the experiment
	/// </summary>
	public void ResetPlayerPosition(){
		worldReference.SetActive(true);
		Transform temp = playerReference.transform;
		// Reset the world to origin to make the math easier
		worldReference.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 1f));
		worldReference.transform.position = new Vector3(0f, 0f, 0f);

		// Translate the world appropriately
		Vector3 translationDiff = (worldReference.transform.position - temp.position) + spawnPos;
        worldReference.transform.Translate(-translationDiff, Space.World);
		// 51.672 TODO: explain magic number
		worldReference.transform.RotateAround(temp.position, Vector3.up, -51.672f);

		// // Rotate the world appropriately
		// float angleDiff = Vector3.SignedAngle(new Vector3(0f, 0f, 1f), temp.forward, Vector3.up);
		float angleDiff = Vector3.SignedAngle(targetReference.transform.forward, temp.forward, Vector3.up);
		// Debug.Log("angle diff: " + angleDiff);
		// Debug.Log("dir: " + rotationDirection[0]);
		// Make the user face the target no matter where he/she is. Then rotate the world for the next experiment. Makes the math easier this way
		worldReference.transform.RotateAround(temp.position, Vector3.up, angleDiff);
		worldReference.transform.RotateAround(temp.position, Vector3.up, 90 * rotationDirection[0]);
		// worldReference.transform.RotateAround(playerReference.transform.position, Vector3.up, angleDiff);
		// worldReference.transform.RotateAround(playerReference.transform.position, Vector3.up, 90 * rotationDirection[0]);

		// Keep the world at the correct height below the user's head
		float playerHeight = temp.position.y;
		worldReference.transform.Translate(new Vector3(0f, (-playerHeight), 0f));
		// worldReference.transform.Translate(new Vector3(0f, (-playerHeight * 0.75f), 0f));
	}

	/// <summary>
	/// Shuffle the arrays holding the information dictating the trial settings
	/// </summary>
	protected void ShuffleTrials(){
		System.Random rand = new System.Random();

		// Fisher-Yates shuffle
		for (int i = gains.Count - 1; i > 0; i--){
			int j = rand.Next(0, i + 1);

			// Must keep the shuffle the same for all arrays!
			float tempGain = gains[i];
			gains[i] = gains[j];
			gains[j] = tempGain;
			bool tempDistract = distractors[i];
			distractors[i] = distractors[j];
			distractors[j] = tempDistract;
			int tempDistractDir = distractorDirection[i];
			distractorDirection[i] = distractorDirection[j];
			distractorDirection[j] = tempDistractDir;
			if (rotationDirection.Count > 0){ // This list is empty for translation and curvature experiments
				int tempDirection = rotationDirection[i];
				rotationDirection[i] = rotationDirection[j];
				rotationDirection[j] = tempDirection;
			}
		}
	}

	/// <summary>
	/// Shuffle the provided list. Need this to have the distractor direction be independent of
	/// the user's rotation direction.
	/// </summary>
	/// <param name="array">The array of distractor directions that will be shuffled</param>
	/// <returns>A shuffled array</returns>
	protected List<int> RandomShuffle(List<int> array){
		System.Random rand = new System.Random();

		for (int i = array.Count - 1; i > 0; i--){
			int j = rand.Next(0, i + 1);

			int temp = array[i];
			array[i] = array[j];
			array[j] = temp;
		}
		
		return array;
	}

	/// <summary>
	/// Get the gain to be applied (first in the queue)
	/// </summary>
	/// <returns>Gain to be applied</returns>
    public float GetCurrentGain(){
        return gains[0];
    }

	/// <summary>
	/// Covers the user's vision with a blank screen so that we can start the intermission process
	/// </summary>
	public void StartIntermission(){

	}
}
