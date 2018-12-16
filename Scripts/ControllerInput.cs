/// <summary>
/// Controller input script
/// TODO: documentation
/// Author: Niall Williams
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerInput : MonoBehaviour {
	private float INPUT_BUFFER = 0.3f; // After pressing any button, the user must wait this many seconds before another button press is registered
	private RDWController myRDWController;
	private SteamVR_TrackedObject trackedObj;
	private SteamVR_Controller.Device device;
	private Vector2 touchpad;
	public int response;
	public bool lookingForAnswer = false;
	public bool userHasResponded = false;
	private DateTime trackpadPressTime = DateTime.MinValue;
	private bool inputBufferActive = false;
	public int questionNumber = 0;
	private float triggerAxis;

	private SteamVR_Controller.Device Controller{
		get { return SteamVR_Controller.Input((int)trackedObj.index); }
	}

	void Awake(){
		trackedObj = GetComponent<SteamVR_TrackedObject>();
		myRDWController = GameObject.FindGameObjectWithTag("Experiment controller").GetComponent<RDWController>();
		response = 0;
		Debug.Log(myRDWController.controllerInput.response);
	}

	// Update is called once per frame
	void Update(){
		// Debug.Log("ddd: " + (DateTime.Now - trackpadPressTime).TotalSeconds);
		if (trackpadPressTime != DateTime.MinValue)
			inputBufferActive = !((DateTime.Now - trackpadPressTime).TotalSeconds > INPUT_BUFFER);

		device = SteamVR_Controller.Input((int)trackedObj.index);
		if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad)){
			touchpad = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);

			if (touchpad.x < -0.5f && !inputBufferActive){
				myRDWController.textController.DecrementSelection();
				inputBufferActive = true;
				trackpadPressTime = DateTime.Now;
				// myRDWController.textController.ChangeOptionText("<b>SMALLER</b> | GREATER", 5);

				// response = -1;
				// myRDWController.Q1Response = -1;
				// // myRDWController.userResponseReceived = false;
				// myRDWController.buttonPressed = true;
				// lookingForAnswer = false;
				// userHasResponded = true;
			}
			else if (touchpad.x > 0.5f && !inputBufferActive){ 
				myRDWController.textController.IncrementSelection();
				inputBufferActive = true;
				trackpadPressTime = DateTime.Now;
				// myRDWController.textController.ChangeOptionText("SMALLER | <b>GREATER</b>", 5);

				// response = 1;
				// myRDWController.Q1Response = 1;
				// // myRDWController.userResponseReceived = false;
				// myRDWController.buttonPressed = true;
				// lookingForAnswer = false;
				// userHasResponded = true;
			}
		}
		else{
			// myRDWController.buttonPressed = false;
			response = 0;
			// myRDWController.Q1Response = null;
		}

		if (!myRDWController.orientingInProgress && myRDWController.intermissionRunning){
			// The user has selected their response
			triggerAxis = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
			if ((triggerAxis - 1 == 0.0f) && myRDWController.intermissionRunning && !inputBufferActive){
				inputBufferActive = true;
				trackpadPressTime = DateTime.Now;
				switch (questionNumber){
					case 0:
						myRDWController.Q1Response = myRDWController.textController.GetSelection();
						myRDWController.textController.Reset();
						questionNumber++;
						break;
					case 1:
						myRDWController.Q2Response = myRDWController.textController.GetSelection();
						myRDWController.textController.Reset();
						questionNumber++;
						break;
					case 2:
						myRDWController.Q3Response = myRDWController.textController.GetSelection();
						myRDWController.textController.Reset();
						questionNumber = 0;
						break;
				}
			}
		}
	}

	public IEnumerator GetUserResponse(){
		while (response != 0) { 
			yield return null; 
		}
	}
}
