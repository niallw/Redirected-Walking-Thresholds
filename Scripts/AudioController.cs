using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AudioController : MonoBehaviour {
    private AudioSource trialSuccess;

	void Awake(){
        trialSuccess = GameObject.FindGameObjectWithTag("Trial complete sound").GetComponent<AudioSource>();
		// var fullAmbientPath = Path.GetFullPath(ambientFileName);
        // Debug.Log(fullAmbientPath);
        // ambientNoise = GetComponentInChildren<AudioSource>();
        // string audioPath = "file:///ambient_noise.wav";
        // string audioPath = "file:///C:/Users/niwilliams/Documents/Thresholds/Assets/Scripts/ambient_noise.wav";
        // WWW www = new WWW(fullAmbientPath);
        // while(www.GetAudioClip().loadState != AudioDataLoadState.Loaded) {
        //     //Debug.Log("Waiting to load audio clip " + clipName);
        // }
        // ambientNoise.clip = www.GetAudioClip(false, false, AudioType.WAV);
        // if (ambientNoise != null)
        //     ambientNoise.Play();
	}

    public void PlaySound(){
        trialSuccess.Play();
    }
	
	// Update is called once per frame
	void Update () {
	}
}
