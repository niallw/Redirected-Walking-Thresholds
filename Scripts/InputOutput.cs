/// <summary>
/// Input output. A basic script for reading from and writing to a text file. 
/// 
/// Author: Tabitha C. Peck
/// Date: March 9, 2017
/// 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class InputOutput : MonoBehaviour {

	public string myInputFile;  //set the input file in Unity

	public string myOutputFile; //set the output file in Unity

	private StreamReader inputStream;
	private StreamWriter outputStream;

	// Use this for initialization
	void Awake () {
		//get the full path name
		//add subfolder to myInputFile name in Unity if you want to store in a subfolder
		if (myInputFile != "") {
			var fullPath = Path.GetFullPath (myInputFile);
			Debug.Log (fullPath);
			// Open the read from file
			if (File.Exists (fullPath)) {
				inputStream = File.OpenText (fullPath);
			} else {
				throw new System.ArgumentException ("File does not exist", fullPath);
			}
		}
			
		if (myOutputFile != "") {
			//if the output file doesn't exist, create it
			if (!File.Exists (myOutputFile)) {
				// Create a file to write to.
				outputStream = File.CreateText (myOutputFile);
			} else {
				outputStream = File.AppendText (myOutputFile); //append text to not overwrite previous data
			}
		}
	}

	/*return the line beginning with id
	 * 
	 * Often used to pull participant setup data 
	 * where the input file is formatted with id
	 * at the beginning of each line.
	 * 
	 * Close the inputStream after getting this line
     */
	public string GetLine(string id){
		if (inputStream != null) {
			string s = "";
			while ((s = inputStream.ReadLine ()) != null) {
				if (s.StartsWith (id)) {
					inputStream.Close ();
					return s;
				}
			}
			throw new System.ArgumentException ("No line in file beginning with", id);
		}
		return "";
	}

	/// <summary>
	/// Writes the data to the outstream that was specified when starting the script
	/// </summary>
	/// <param name="data">Data.</param> The string to be written to the output file
	/// <param name="newLine">If set to <c>true</c> new line.</param> pass the parameter false 
	/// if you do not want a new line
	public void WriteData(String data, bool newLine = true){
		if (outputStream != null) {
			if (newLine) {
				outputStream.WriteLine (data);
			} else {
				outputStream.Write (data);
			}
			outputStream.Flush(); //push data to output file
		}
	}
					
	void OnApplicationQuit() {
		//make sure to close the streams if they haven't yet been closed
		if (outputStream != null) {
			outputStream.Close ();
		}
		if (inputStream != null) {
			inputStream.Close ();
		}
	}
}
