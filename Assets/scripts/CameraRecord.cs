using UnityEngine;
using System.Collections.Generic;

public class CameraRecord : MonoBehaviour {

	public string fileName = "capture_{0}_{1}.jpg";
	public int leadingZeros = 3;
	public KeyCode recordKey = KeyCode.C;
	public float fps = 14f;

	private int recording = 0;
	private string intFormat;
	private int curFrame = 0;
	private bool isRecording = false;

	// Use this for initialization
	void Start () {
		intFormat = new string('0', leadingZeros);
		while (System.IO.File.Exists(string.Format(fileName, recording, curFrame.ToString(intFormat))))
			recording++;
		Debug.Log(string.Format("Will start at recording {0}", recording));
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(recordKey)) {
			if (!isRecording) {
				Debug.Log(string.Format("Recording {0}", recording));
				isRecording = true;
				StartCoroutine(record());
			} else {
				Debug.Log("Recording complete");
				isRecording = false;
				curFrame = 0;
				recording ++;
			}
		}
	}

	IEnumerator<WaitForSeconds> record() {
		while (isRecording) {
			Application.CaptureScreenshot(string.Format(fileName, recording, curFrame.ToString(intFormat)));
			curFrame ++;
			yield return new WaitForSeconds(1f / fps);
		}
	}
}
