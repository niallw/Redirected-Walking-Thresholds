using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour {
    private GameObject textContainer;
    private Text questionText;
    private Text optionText;
	private string[] options;
	private int bufferSize;
	private int selectedIndex;

	// Use this for initialization
	void Start () {
        textContainer = GameObject.FindGameObjectWithTag("Text container");
        questionText = GameObject.FindGameObjectWithTag("Question text").GetComponent<UnityEngine.UI.Text>();
        optionText = GameObject.FindGameObjectWithTag("Option text").GetComponent<UnityEngine.UI.Text>();
		selectedIndex = 0;
	}

	void Update(){
		if (options != null)
			ChangeOptionText(options, 5);
	}

	public void IncrementSelection(){
		selectedIndex++;
		if (selectedIndex >= bufferSize)
			selectedIndex = 0;
	}

	public void DecrementSelection(){
		selectedIndex--;
		if (selectedIndex < 0)
			selectedIndex = bufferSize - 1;
	}

	public string GetSelection(){
		return options[selectedIndex];
	}

	public void Reset(){
		selectedIndex = 0;
	}

	public void ChangeQuestionText(string text, int fontSize){
		questionText.fontSize = fontSize;
		questionText.text = text;
	}

	public void ChangeOptionText(string[] optionArray, int fontSize){
		bufferSize = optionArray.Length;
		options = optionArray;
		optionText.fontSize = fontSize;
		optionText.text = BuildOptionText(optionArray);
	}

	public void SetOptions(string[] array){
		options = array;
		bufferSize = array.Length;
	}

	public void SetTextPosition(Vector3 destination){
		textContainer.transform.position = destination;
	}

	private string BuildOptionText(string[] optionArray){
		string optionsString = "";

		for (int i = 0; i < bufferSize; i++){
			if (i == selectedIndex)
				optionsString += "<size=7><b>" + optionArray[i] + "</b></size>";
			else
				optionsString += optionArray[i];

			if (i < bufferSize - 1)
				optionsString += " | ";
		}

		return optionsString;
	}
}
