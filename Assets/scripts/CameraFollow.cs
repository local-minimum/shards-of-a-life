using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	[Range(-5f, -20f)]
	public float zDist = -10f;
	[Range(-4f, 4f)]
	public float yDist = 1f;
	public float xLeaning = 1f;
	public float yCap = 5f;

	public GameObject player;
	public Level level;

	private float _rightX;

	public float X {
		get {
			return _rightX;
		}
	}

	public bool follow = true;

	public GameObject[] hurtCams;

	// Use this for initialization
	void Start () {
		_rightX = player.transform.position.x;
		//_player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
		if (!follow)
			return;

		Vector3 p = player.transform.position;
		p.z += zDist;
		p.y += yDist;
		if (p.x < _rightX - xLeaning) {
			p.x = _rightX - xLeaning;
		} else if (p.x > level.endX) {
			p.x = Mathf.Lerp(p.x, level.endX, 0.99f);
		} else if (p.x > _rightX) {
			_rightX = p.x;
		}
		if (p.y > yCap)
			p.y = Mathf.Lerp(p.y, yCap, 0.6f);

		transform.position = p;
	}

	public void HurtEffect() {
		foreach (GameObject g in hurtCams) {
			g.transform.localPosition = Vector3.zero;
			g.SetActive(true);
		}
		//iTween.CameraFadeFrom(
	}

	public void Reset() {
		_rightX = player.transform.position.x;
		follow = true;
	}
}
