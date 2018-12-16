/// <summary>
/// Make the object this script is attached to always face the participant
///
/// Author: Niall Williams
/// Date: June 18, 2018
/// </summary>
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {
    private SteamVR_Camera player;
    [Tooltip("How far from the participant you want the object to be")]
	public float distanceFromPlayer; 

	// Use this for initialization
	void Start(){
        player = SteamVR_Render.Top();
	}
	
	// Update is called once per frame
	void FixedUpdate(){
        transform.position = player.transform.position + (player.transform.forward * distanceFromPlayer);
		transform.LookAt(player.transform.position, Vector3.up);
        transform.Rotate(Vector3.forward, -player.transform.eulerAngles.z);
	}
}
