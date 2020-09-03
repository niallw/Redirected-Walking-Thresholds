/// <summary>
/// TODO: DOCUMENTATION!!!
/// TODO: tooltips: good example: https://github.com/mahdiazmandian/The-Redirected-Walking-Toolkit/blob/master/Assets/RDW%20Toolkit/Scripts/Redirection/RedirectionManager.cs
/// TODO: hide in inspector
///
/// Author: Niall Williams
/// Date: May 24, 2018
/// </summary>

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class RDWController : MonoBehaviour {
    public InputOutput IOscript; // Script to handle reading and logging participant data
    [HideInInspector]
    public SteamVR_Camera player; // The player (camera) that is being tracked
    [HideInInspector]
    public ControllerInput controllerInput; // The controller script
    [HideInInspector]
    public GameObject distractor; // Distractor object
    [HideInInspector]
    public DistractorController distractorController; // Distractor script
    [HideInInspector]
    public GameObject world; // The virtual world that will be moved around the player
    [HideInInspector]
    public GameObject target; // The object the player must look at to finish the trial
    [HideInInspector]
    public GameObject spawn; // Player spawns here
    [HideInInspector]
    public GameObject targetIndicator; // Circle that tells the user when the trial is done
    [HideInInspector]
    public GameObject completedIndicator;
    [HideInInspector]
    public IntermissionController intermissionController; // Controls the white screen that serves as intermission between trials
    [HideInInspector]
    public AudioController audioController;
    [HideInInspector]
    public TextController textController;
    [HideInInspector]
    public GameObject textContainer;
    [HideInInspector]
    public Text questionText;
    [HideInInspector]
    public Text optionText;
    [HideInInspector]
    public GameObject leftArrow;
    [HideInInspector]
    public GameObject rightArrow;
    private GameObject fovMask;

    // Get this at the beginning of the program so that you don't have to get it every frame
    [HideInInspector]
    public Transform playerTransform; // The player's transform
    private float CALIBRATED_EYE_HEIGHT;
    [HideInInspector]
    public Transform worldTransform; // The world's transform
    [HideInInspector]
    public Transform targetTransform; // The experiment target's transform
    [HideInInspector]
    public Transform spawnTransform; // The player's spawn position transform
    [HideInInspector]
    public Transform targetIndicatorTransform;

    // public KeyCode FAILED_TRIAL_KEY = KeyCode.F; // Experiment runner can press this button if the user
                                                 // failed the trial in a way not handled automatically by the code
    [Tooltip("Press this key once the user is looking straight ahead with normal posture")]
    public KeyCode CALIBRATE_USER_KEY = KeyCode.Space;
    [Tooltip("Press this key during intermission (black screen) to pause the experiment after the user responds to the questions on screen.")]
    public KeyCode INITIATE_BREAK = KeyCode.B; // Press this button to give the participant a break after they respond to the questions

    // The gains that will be added to the motion on each frame
    [Tooltip("Rotation gain being applied")]
    public float g_r = 0.0f; // Rotation gains
    [Tooltip("Translation gain being applied")]
    public float g_t = 0.0f; // Translation gains
    [Tooltip("Curvature gain being applied")]
    public float g_c = 0.045f; // Curvature gains 

    // Rotation
    private Vector3 directionPrevious;
    private float currentHeadRotationAngle; // The angle the user's head turned between the current and previous frame

    // Translation
    private Vector3 positionPrevious; // The previous position of the user
    private Vector3 currentTranslation;
    private float distanceTraveled; // Distance traveled since previous frame

    // Curvature
    [Tooltip("Minimum distance the user needs to walk for the curvature gain to be applied")]
    public static float MIN_DISTANCE = 0.001f; // Participant needs to move this distance since last frame to apply curvature
    private static float deg_rad = 180.0f / Mathf.PI; // Only calculate once

    // Experiment settings/data
    [Tooltip("0: Rotation\n1: Translation\n2: Curvature")]
    public int gainToTest = 0; // 0 - rotation
                               // 1 - translation
                               // 2 - curvature
    [HideInInspector]
	public static int NUM_PRACTICE_TRIALS = 3; 
    [HideInInspector]
    public int completedTrialCount = 0;
    [HideInInspector]
    public int lastTrialCompletedCount = -1; // Hack so we don't record the user response many times (Since the button press lasts more than 1 frame)
    public bool buttonPressed = false; // If the user is pressing down on the controller trackpad
    [HideInInspector]
    public MovementExperiment myMovementExperiment;
    private static float MAX_TRIAL_LENGTH = 200.0f; // Trial is failed if it lasts longer than this TODO: change htis value to be appropriate. I picked a random number
    private bool failedTrial = false; // If the user failed the most recent trial //TODO: maybe remove all these failed trial variabels. wait for pilot to decide
    private int failCount = 0; // Amount of trials the user has failed
    // [HideInInspector]
    public bool intermissionRunning = false; // Intermission is the blank screen that repositions the user and collects their response
    // [HideInInspector]
    public bool calibrationRunning = false;
    // [HideInInspector]
    public bool breakRunning = false;
    [HideInInspector]
    public bool userResponseReceived = false;
    // [HideInInspector]
    private float INTERMISSION_FADE_TIME = 0.5f;
    public bool promptShowing = false;
    public bool trialStartLogged = false;
    public bool trialEndLogged = false;
    public int numQuestionsAnswered = 0;
    public bool fortyFOVActive;
    public bool eightyFiveFOVActive;
    private string activatedFOV; // Which FOV is active (40, 85, or 110)
    public bool userWasReoriented = false;
    public bool userReadyToBegin = false;
    public bool orientingInProgress = false;

    // Log data
    private DateTime trialStartTime = DateTime.MinValue;
    [HideInInspector]
    public DateTime trialEndTime = DateTime.MinValue;
    [HideInInspector]
    public int rotationDirection;
    [HideInInspector]
    public float gainApplied;
    [HideInInspector]
    public bool distractorPresent;
    [HideInInspector]
    public int distractorDirection;
    [HideInInspector]
    public float totalTurnedReal = 0.0f;
    [HideInInspector]
    public float totalTurnedVirtual = 0.0f;
    [HideInInspector]
    public string Q1Response = null; 
    [HideInInspector]
    public string Q2Response = null; 
    [HideInInspector]
    public string Q3Response = null;

    void Start(){
        leftArrow = GameObject.FindGameObjectWithTag("Indicator left");
        rightArrow = GameObject.FindGameObjectWithTag("Indicator right");
        IOscript = GetComponent<InputOutput>();
        player = SteamVR_Render.Top();
        playerTransform = player.transform;

        controllerInput = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetComponent<ControllerInput>();

        distractor = GameObject.FindGameObjectWithTag("Distractor");
        distractorController = distractor.GetComponent<DistractorController>();
        fovMask = GameObject.FindGameObjectWithTag("FOV restrictor");

        world = GameObject.FindGameObjectWithTag("World");
        targetIndicator = GameObject.FindGameObjectWithTag("Target indicator");
        completedIndicator = GameObject.FindGameObjectWithTag("Completed indicator");
        targetIndicatorTransform = targetIndicator.transform;
        targetIndicator.SetActive(false); // Target indicator is invisible until calibration TODO: maybe can set it to inactive in unity, and then get rid of this line?
        
        worldTransform = world.transform;
        intermissionController = GetComponent<IntermissionController>();
        audioController = GetComponent<AudioController>();
        textController = GetComponent<TextController>();
        positionPrevious = playerTransform.position;
        directionPrevious = playerTransform.TransformDirection(Vector3.forward);

        // Set FOV restrictors
        if (fortyFOVActive && !eightyFiveFOVActive){
            GameObject.FindGameObjectWithTag("85 FOV").SetActive(false);
            activatedFOV = "40";
        }            
        else if (!fortyFOVActive && eightyFiveFOVActive){
            GameObject.FindGameObjectWithTag("40 FOV").SetActive(false);
            activatedFOV = "85";
        }
        else{
            GameObject.FindGameObjectWithTag("85 FOV").SetActive(false);
            GameObject.FindGameObjectWithTag("40 FOV").SetActive(false);
            activatedFOV = "110";
        }
        
        IOscript.WriteData("BLOCK_START");

        // Orient the user correctly for the start of the experiment according to the gain being tested
        switch (gainToTest){
            case 0:
                myMovementExperiment = GetComponent<RotationExperiment>();
                spawn = GameObject.FindGameObjectWithTag("Rotation start");
                target = GameObject.FindGameObjectWithTag("Target");
                spawnTransform = spawn.transform;
                targetTransform = target.transform;
                break;
            case 1:
                myMovementExperiment = GetComponent<TranslationExperiment>();
                spawn = GameObject.FindGameObjectWithTag("Translation start");
                target = GameObject.FindGameObjectWithTag("Rotation start");
                spawnTransform = spawn.transform;
                targetTransform = target.transform;
                break;
            case 2:
                myMovementExperiment = GetComponent<CurvatureExperiment>();
                spawn = GameObject.FindGameObjectWithTag("Translation start");
                target = GameObject.FindGameObjectWithTag("Rotation start");
                spawnTransform = spawn.transform;
                targetTransform = target.transform;
                break;
            default:
                Debug.LogError("Please input a valid value for \'Gain To Test\' (0: Rotation | 1: Translation | 2: Curvature).");
                UnityEditor.EditorApplication.isPlaying = false;
                break;
        }

        myMovementExperiment.Initialize();
        calibrationRunning = true;
    }

    void FixedUpdate(){
        Debug.Log("Trial #" + (completedTrialCount-NUM_PRACTICE_TRIALS+1));
        if (calibrationRunning){
            world.SetActive(false);
            fovMask.SetActive(false);
            CalibrateUser();
        }
        // Calcuate user movements
        else{
            if (!world.activeSelf && myMovementExperiment.trialInProgress && !intermissionRunning){
                world.SetActive(true);
                fovMask.SetActive(true);
            }
            // Only apply the gain during real trials
            if (!intermissionRunning && !calibrationRunning){
                if (myMovementExperiment.trialInProgress && !myMovementExperiment.isPracticeTrial)
                    LogFrameData(playerTransform, worldTransform, (1.0f / Time.deltaTime));

                switch (gainToTest){
                    case 0:
                        SetRotation(myMovementExperiment.gains[0]); // Current trial's rotation gain
                        CurrentRotation(); // Calculate user's current head rotation angle since last frame
                        RotationGain(); // Apply gain
                        break;
                    case 1:
                        // if (!myMovementExperiment.isPracticeTrial) TODO: this
                        //     SetTranslation(myMovementExperiment.gains[0]); // Current trial's translation gain
                        // CurrentTranslation(); //calcuate user's current translation direction and distance since the previous frame
                        // TranslationGain();
                        break;
                    case 2:
                        // if (!myMovementExperiment.isPracticeTrial) TODO: this
                        //     SetCurvature(myMovementExperiment.gains[0]); // Current trial's translation gain
                        // CurvatureGain();
                        break;
                }
            }
        }
    }

    void Update(){
        if (!calibrationRunning && !breakRunning){
            // Only do trial stuff when we are not reorienting the user or collecting responses
            if (!intermissionRunning){
                textController.ChangeQuestionText(null, 0);
                if (trialStartTime == DateTime.MinValue)
                    trialStartTime = DateTime.Now;
                StartCoroutine(myMovementExperiment.RunTrial());

                if (myMovementExperiment.trialInProgress && !myMovementExperiment.isPracticeTrial){
                    if (!trialStartLogged){
                        LogTrialData(true);
                    }
                }
                else{
                    myMovementExperiment.trialInProgress = true;
                }
            }
            // Trial was finished, now we can begin intermission
            else if (intermissionRunning){
                Debug.Log("Intermission running");
                if (!promptShowing){
                    StartCoroutine(ShowPrompt());
                    promptShowing = true;
                }
                if (Input.GetKeyDown(INITIATE_BREAK)){
                    breakRunning = true;
                }
            }
        }
        else if (breakRunning && (lastTrialCompletedCount == completedTrialCount)){
            Debug.Log("Experiment paused");
            textController.ChangeQuestionText("Experiment paused", 5);
            textController.ChangeOptionText(new string[] {""}, 5);
            if (Input.GetKeyDown(INITIATE_BREAK)){
                textController.ChangeQuestionText(null, 0);
                calibrationRunning = true;
                intermissionRunning = false;
                promptShowing = false;
                CalibrateUser();
                breakRunning = false;
                orientingInProgress = false;

                if (!calibrationRunning){
                    myMovementExperiment.ResetPlayerPosition();
                    textController.ChangeQuestionText(null, 0);
                }
            }
        }
    }

    void LateUpdate(){
        // When the user has pressed left or right on the trackpad, and it is not the same trial that is being responded to
        if ((Q1Response != "") && (Q2Response != "") && (Q3Response != "") && (lastTrialCompletedCount != completedTrialCount) && (completedTrialCount > 0) && promptShowing){
            if (calibrationRunning){
                textController.ChangeQuestionText("The practice trials are now complete.\nPlease say when you are\nready to begin the experiment.", 3);
                textController.ChangeOptionText(new string[] {""}, 5);

                promptShowing = false;
                lastTrialCompletedCount = completedTrialCount;
                userResponseReceived = true;
                Q1Response = "";
                Q2Response = "";
                Q3Response = "";
                intermissionRunning = false;
                userWasReoriented = false;
            }
            else if (userWasReoriented && !calibrationRunning){
                if (completedTrialCount > NUM_PRACTICE_TRIALS){
                    if (!myMovementExperiment.isPracticeTrial)
                        LogTrialData(false);
                    userReadyToBegin = true;
                }

                lastTrialCompletedCount = completedTrialCount;
                userResponseReceived = true;
                Q1Response = "";
                Q2Response = "";
                Q3Response = "";
                intermissionRunning = false;
                userWasReoriented = false;

                if (!breakRunning){
                    myMovementExperiment.ResetPlayerPosition();
                    StartCoroutine(HidePrompt());
                    orientingInProgress = false;
                }
            }
            else{
                textController.ChangeQuestionText("", 3);
                textController.ChangeOptionText(new string[] {""}, 5);
                StartCoroutine(myMovementExperiment.ReorientUser());
            }
        }
        else if ((Q1Response != "") && (Q2Response == "") && (Q3Response == "")){
            textController.ChangeQuestionText("How confident are you in your \nresonse to the previous question?\n(1 = no confidence, 5 = high confidence)", 3);
            textController.ChangeOptionText(new string[] {"1","2","3","4","5"}, 5);
        }
        else if (Q2Response != "" && Q3Response == ""){
            textController.ChangeQuestionText("Did you see an animal in the scene?", 3);
            textController.ChangeOptionText(new string[] {"YES","NO"}, 5);
            userWasReoriented = false;
            if (completedTrialCount == NUM_PRACTICE_TRIALS){
                calibrationRunning = true;
            }
        }
    }

    private void CalibrateUser(){
        Debug.Log("Calibrating...");
        if (Input.GetKeyDown(CALIBRATE_USER_KEY)){
            targetIndicatorTransform.position = playerTransform.position + playerTransform.forward * 1;
            CALIBRATED_EYE_HEIGHT = playerTransform.position.y;

            // Adjust the height of the target to be level with the user's eyes
            Vector3 temp = targetTransform.position;
            temp.y = CALIBRATED_EYE_HEIGHT;
            targetTransform.position = temp;
            targetIndicator.SetActive(true);

            myMovementExperiment.ResetPlayerPosition();

            textController.ChangeQuestionText("", 3);
            textController.ChangeOptionText(new string[] {""}, 5);
            StartCoroutine(intermissionController.FadeTo(-1.0f, INTERMISSION_FADE_TIME)); // Remove the white screen
            calibrationRunning = false;
            fovMask.SetActive(true);
        }
    }

	private IEnumerator ShowPrompt(){
        yield return StartCoroutine(intermissionController.FadeTo(2.0f, INTERMISSION_FADE_TIME));
		switch (gainToTest){
			case 0:
                textController.ChangeQuestionText("Was the virtual movement \n<i>smaller</i> or <i>greater</i> than the physical movement?", 3);
                textController.ChangeOptionText(new string[] {"SMALLER", "GREATER"}, 5);
                controllerInput.questionNumber = 0; // Ensures the user cannot advance the questionNumber variable in the time before the question appears, 
                                                    // which would result in a mismatch between the question being displayed and the question response being saved
				break;
			case 1:
                questionText.text = "Was the walked distance more or less?"; //TODO: change this prompt
				break;
			case 2:
                questionText.text = "Was the walked distance more or less?"; //TODO: change this prompt
				break;
		}
        targetIndicator.SetActive(false);
        world.SetActive(false);
        fovMask.SetActive(false);
	}

	private IEnumerator HidePrompt(){
        world.SetActive(true);
        fovMask.SetActive(true);
        promptShowing = false;
        textController.ChangeQuestionText("", 3);
        textController.ChangeOptionText(new string[] {""}, 5);
		yield return StartCoroutine(intermissionController.FadeTo(-1.0f, INTERMISSION_FADE_TIME));
		intermissionRunning = false;
		targetIndicator.SetActive(true);
	}

    private void LogTrialData(bool isStartLog){
        string movement = "";
        double timeTaken = (trialEndTime-trialStartTime).TotalSeconds - INTERMISSION_FADE_TIME; // Hack because the code starts the timer when the fade begins, not when it ends
        string data = "";
        switch (gainToTest){
            case 0:
                movement = "rotation";
                if (isStartLog){
                    data = "TRIAL," + (completedTrialCount-NUM_PRACTICE_TRIALS+1) +
                           ",START_TIME," + trialStartTime +
                           ",MOVEMENT_TYPE," + movement +
                           ",ROTATION_DIRECTION," + rotationDirection +
                           ",GAIN_APPLIED," + gainApplied + 
                           ",DISTRACTOR_PRESENT," + distractorPresent +
                           ",DISTRACTOR_DIRECTION," + distractorDirection +
                           ",FOV_ACTIVE," + activatedFOV;
                }
                else{
                    data = "TRIAL," + (completedTrialCount-NUM_PRACTICE_TRIALS) +
                           ",END_TIME," + trialEndTime +
                           ",TOTAL_TRIAL_TIME," + timeTaken +
                           ",MOVEMENT_TYPE," + movement +
                           ",ROTATION_DIRECTION," + rotationDirection +
                           ",GAIN_APPLIED," + gainApplied + 
                           ",DISTRACTOR_PRESENT," + distractorPresent +
                           ",DISTRACTOR_DIRECTION," + distractorDirection +
                           ",TOTAL_TURNED_REAL," + totalTurnedReal +
                           ",TOTAL_TURNED_VIRTUAL," + totalTurnedVirtual +
                           ",FOV_ACTIVE," + activatedFOV +
                           ",Q1_RESPONSE," + Q1Response +
                           ",Q2_RESPONSE," + Q2Response +
                           ",Q3_RESPONSE," + Q3Response;
                }
                break;
            case 1:
                movement = "translation";
                break;
            case 2:
                movement = "curvature";
                break;
        }
        
        IOscript.WriteData(data);
        if (isStartLog)
            trialStartLogged = true;
        else
            trialEndLogged = true;
        trialStartTime = DateTime.MinValue;
        totalTurnedReal = 0.0f;
        totalTurnedVirtual = 0.0f;

        // Player is done
        if (myMovementExperiment.gains.Count == 0){
            Debug.Log("Trials all done!");
            IOscript.WriteData("BLOCK_END");
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    private void LogFrameData(Transform HMDPosition, Transform worldTransform, float fps){        
        float xPos = HMDPosition.position.x;                
        float yPos = HMDPosition.position.y;                
        float zPos = HMDPosition.position.z;                
        float pitch = HMDPosition.eulerAngles.x;
        float yaw = HMDPosition.eulerAngles.y;
        float roll = HMDPosition.eulerAngles.z;
        string worldPos = worldTransform.position.ToString("F6");
        string worldRotation = worldTransform.rotation.ToString("F6");
        string playerPos = playerTransform.position.ToString("F6");
        string playerRotation = playerTransform.rotation.ToString("F6");
        string data = "HMD_POSITION" + 
                      ",X_POSITION," + xPos + 
                      ",Y_POSITION," + yPos + 
                      ",Z_POSITION," + zPos + 
                      ",PITCH," + pitch + 
                      ",YAW," + yaw + 
                      ",ROLL," + roll +
                      ",WORLD_POSITION," + worldPos + 
                      ",WORLD_ROTATION," + worldRotation + 
                      ",PLAYER_POSITION," + playerPos + 
                      ",PLAYER_ROTATION," + playerRotation +
                      ",FPS," + fps +
                      ",TIME," + DateTime.Now.ToString("hh.mm.ss.ffffff");

        IOscript.WriteData(data);
    }

    /// <summary>
    /// Calcuate the user's head rotation since the last frame.
    /// current_head_rotation will be in degrees
    /// update the previous look direction
    /// </summary>
    private void CurrentRotation(){
        Vector3 directionCurrent = playerTransform.TransformDirection(Vector3.forward); //get the current forward direction as a unit vector
        directionCurrent.y = 0; // Don't rotate the world based on the pitch change

        // Get the angle between previous and current direction
        float r_real = Vector3.Angle(directionCurrent, directionPrevious); //this is in degrees
        Vector3 cross = Vector3.Cross(directionCurrent, directionPrevious);
        if (cross.y < 0) { //determine if the angle is negative TODO: Why not just always get abs(r_real)?
            r_real = -r_real;
        }

        currentHeadRotationAngle = r_real;
        directionPrevious = directionCurrent;
    }

    /// <summary>
    /// Updates the user's current translation and saves it in current_tanslation
    /// Updates the previous user position
    /// </summary>
    private void CurrentTranslation(){
        Vector3 positionCurrent = playerTransform.position; //get current camera position

        positionCurrent.y = 0; //we are not interested in y direction

        distanceTraveled = Vector3.Distance(positionCurrent, positionPrevious);
        currentTranslation = positionCurrent - positionPrevious; //get real translation over the frame

        positionPrevious = positionCurrent; //update previous position
    }

    /// <summary>
    /// Calculates the rotation gain for the current time step
    /// The function will update the previous rotation of the user
    /// </summary>
    private void RotationGain(){
        float r_virtual = currentHeadRotationAngle * g_r; //Apply the gain

        if ((Mathf.Abs(currentHeadRotationAngle) > 0.1f) && (Mathf.Abs(currentHeadRotationAngle) < 5.0f)){ //FIXME: this is a huge hack. theres a bug that rotates the world at the start of the program because it instantly changes the camera's forward direction from its direction in the scene (before running the program) to what direction the user is actualyl facing with the HMD.
            totalTurnedReal += (currentHeadRotationAngle);
            totalTurnedVirtual += (r_virtual);
            worldTransform.RotateAround(playerTransform.position, Vector3.up, r_virtual); //Do the rotation
        }
    }

    /// <summary>
    /// Calculates the translation gain for the current time step
    /// The function will update the previous position of the user
    /// </summary>
    private void TranslationGain(){
        //TODO: Make sure this ignores head bobbing. Head bobs side to side while walking, but we want to only scale the player's forward motion (i.e. don't move the world slightly side-to-side as their head bobs).
        // Tabitha Peck [9:03 AM] TODO: remove this ofc
        // The head bobs side to side as people walk. If you scale motion based on head movement the world is also scaled side to side.
        // It should only be scaled in the forward motion that the user is traveling
        Vector3 t_virtual = currentTranslation * g_t; //scale the translation based on scale numbers from steering algorithm.
        // if (currentTranslation != new Vector3(0.0f, 0.0f, 0.0f)){
        //     Debug.Log("BEFORE: " + currentTranslation);
        //     Debug.Log("AFTER: " + (currentTranslation * g_t));)
        // };
        worldTransform.Translate(-t_virtual); //translation of the world is opposite
    }

    /// <summary>
    /// Calculate and perform curvature gain around the current location of the user
    /// </summary>
    private void CurvatureGain(){
        if (distanceTraveled > MIN_DISTANCE){ // If the person has moved, apply curvature
            float cur = g_c * deg_rad * distanceTraveled; //TODO: ?
            worldTransform.RotateAround(playerTransform.position, Vector3.up, cur); //TODO: ??? I understand that this will rotate the world around the player along the y axis, but the documentation for RotateAround does not make sense to me: https://docs.unity3d.com/ScriptReference/Transform.RotateAround.html specifically I don't understand the "through point" part
        }
    }

    /// <summary>
    /// Set the rotation gain
    /// </summary>
    /// <param name="rotation">What you are updating the rotation gain to</param>
    public void SetRotation(float rotation){
        g_r = rotation;
    }

    /// <summary>
    /// Set the translation gain
    /// </summary>
    /// <param name="translation">What you are updating the translation gain to</param>
    public void SetTranslation(float translation){
        g_t = translation;
    }

    /// <summary>
    /// Set the curvature gain
    /// </summary>
    /// <param name="curvature">What you are updating the curvature gain to</param>
    public void SetCurvature(float curvature){
        g_c = curvature;
    }

    public override string ToString(){
        string answer = playerTransform.position.ToString("F4") + "," + playerTransform.rotation.ToString("F4")
            + "," + worldTransform.position.ToString("F4") + "," + worldTransform.rotation.ToString("F4")
            + "," + g_c + "," + g_r + "," + g_t;

        return answer;
    }
}
