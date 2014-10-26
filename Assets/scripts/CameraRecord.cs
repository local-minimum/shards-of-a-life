using UnityEngine;
using System.Collections.Generic;

public class CameraRecord : MonoBehaviour {

	public string fileName = "capture_{0}.jpg";
	public int leadingZeros = 3;
	public string recordKey = "r";
	public float fps = 14f;

	private string intFormat;
	private int curFrame = 0;
	private bool recording = false;

	// Use this for initialization
	void Start () {
		intFormat = new string('0', leadingZeros);

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(recordKey)) {
			if (!recording) {
				Debug.Log("Recording");
				recording = true;
				StartCoroutine(record());
			} else {
				Debug.Log("Recording complete");
				recording = false;
				curFrame = 0;
			}
		}
	}

	IEnumerator<WaitForSeconds> record() {
		while (recording) {
			Application.CaptureScreenshot(string.Format(fileName, curFrame.ToString(intFormat)));
			curFrame ++;
			yield return new WaitForSeconds(1f / fps);
		}
	}
}
