/// <summary>
/// Script that controls distactor behavior
/// 
/// Author: Niall Williams
/// Date: June 18, 2018
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistractorController : MonoBehaviour {
	private float speed = 50.0f;
	private RDWController myRDWController;
	[HideInInspector]
	public float timeCounter;
	[HideInInspector]
	public bool initialized = false;
	[HideInInspector]
	public int direction;

	/// <summary>
	/// Set up the distractor at the start of each trial
	/// </summary>
	/// <param name="distractorPresent">If the trial will have a distractor or not</param>
	/// <param name="distractorDirection">The direction the distractor will walk</param>
	/// <param name="rotationDirection">The direction the participant will turn</param>
	public void Initialize(bool distractorPresent, int distractorDirection, int rotationDirection){
		myRDWController = GameObject.FindGameObjectWithTag("Experiment controller").GetComponent<RDWController>();
		gameObject.SetActive(true);
		transform.forward = myRDWController.targetTransform.forward;
		timeCounter = 0.0f;
		SetStartingPosition(distractorPresent, distractorDirection, rotationDirection);
		initialized = true;	
	}
	
	// Update is called once per frame
	void Update(){
		if (initialized && (timeCounter < 190.0f)){
			timeCounter += (Time.deltaTime * speed);
			transform.RotateAround(myRDWController.spawnTransform.position, Vector3.up, (Time.deltaTime * speed * direction));
		}
		else if (timeCounter > 190.0f){
			gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Set the distractor's starting position depending on the participant's turning direction
	/// </summary>
	/// <param name="distractorPresent">If the trial will have a distractor or not</param>
	/// <param name="distractorDirection">The direction the distractor will walk</param>
	/// <param name="rotationDirection">The direction the participant will turn</param>
	private void SetStartingPosition(bool distractorPresent, int distractorDirection, int rotationDirection){
		Vector3 RTLpos = GameObject.FindGameObjectWithTag("R to L distractor").transform.position;
		Vector3 LTRpos = GameObject.FindGameObjectWithTag("L to R distractor").transform.position;
		Vector3 centerPos = GameObject.FindGameObjectWithTag("Center distractor").transform.position;
		direction = distractorDirection;

		if (!distractorPresent){
			transform.position = new Vector3(-10.0f, -10.0f, -10.0f); // Under the floor :)
		}
		else if (distractorDirection != rotationDirection){
			transform.position = centerPos;
			transform.Rotate(0.0f, (90.0f * direction), 0.0f);
		}
		else{
			transform.position = (direction < 0) ? RTLpos : LTRpos;	
		}
	}

	/// <summary>
	/// Reset the distractor data for the next trial
	/// </summary>
	public void Reset(){
		gameObject.SetActive(false);
        initialized = false;
        timeCounter = 0.0f;
	}
}
