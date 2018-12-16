/// <summary>
/// TODO: DOCUMENTATION!!!
/// TODO: tooltips: good example: https://github.com/mahdiazmandian/The-Redirected-Walking-Toolkit/blob/master/Assets/RDW%20Toolkit/Scripts/Redirection/RedirectionManager.cs
/// TODO: hide in inspector
///
/// Author: Niall Williams
/// Date: May 24, 2018
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntermissionController : MonoBehaviour {

	public GameObject container; // Object holding the plane
	public GameObject screen;

	private RDWController myRDWController;

	// Use this for initialization
	void Awake(){
		container = GameObject.FindGameObjectWithTag("Intermission screen");
		screen = GameObject.FindGameObjectWithTag("screen");
		myRDWController = GameObject.FindGameObjectWithTag("Experiment controller").GetComponent<RDWController>();
		screen.transform.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 1); // Set screen to black on startup
	}

	public IEnumerator FadeTo(float alphaVal, float fadeTime){
		float alpha = screen.transform.GetComponent<Renderer>().material.color.a;

		for (float i = 0.0f; i < 1.0f; i += Time.deltaTime/fadeTime){
			Color newScreenColor = new Color(0, 0, 0, Mathf.Lerp(alpha, alphaVal, i));
			screen.transform.GetComponent<Renderer>().material.color = newScreenColor;
			yield return null;
		}
		Debug.Log("here: " + screen.transform.GetComponent<Renderer>().material.color);
	}

	public void SetScreenPosition(Vector3 destination){
		container.transform.position = destination;
	}
	
	// Update is called once per frame
	void Update(){
		
	}
}
