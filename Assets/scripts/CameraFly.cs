using UnityEngine;
using System.Collections.Generic;

public class CameraFly : MonoBehaviour {

	private GroundStitcher gs;

	// Use this for initialization
	void Start () {
		gs = GameObject.FindObjectOfType<GroundStitcher>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
