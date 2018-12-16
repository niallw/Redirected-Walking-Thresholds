using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float speed = 1.0f;
	public float rotSpeed = 5.0f;

	private AudioSource audio;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.RightArrow)) {
			transform.Translate (new Vector3 (speed * Time.deltaTime, 0f, 0f));
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			transform.Translate (new Vector3 (-speed * Time.deltaTime, 0f, 0f));
		}
		if (Input.GetKey (KeyCode.UpArrow)) {

			transform.Translate (new Vector3 (0f, 0f, speed * Time.deltaTime));
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			transform.Translate (new Vector3 (0f, 0f, -speed * Time.deltaTime));
		}
		if (Input.GetKey (KeyCode.A)) {
			transform.Rotate (new Vector3 (0f, -rotSpeed * Time.deltaTime, 0f));
		}
		if (Input.GetKey (KeyCode.D)) {
			transform.Rotate (new Vector3 (0f, rotSpeed * Time.deltaTime, 0f));
		}
		if (Input.GetKey (KeyCode.W)) {
			transform.Rotate (new Vector3 (-rotSpeed * Time.deltaTime, 0f, 0f));
		}
		if (Input.GetKey (KeyCode.S)) {
			transform.Rotate (new Vector3 (rotSpeed * Time.deltaTime, 0f, 0f));
		}
	}
}
