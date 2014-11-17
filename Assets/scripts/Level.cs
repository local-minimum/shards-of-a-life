using UnityEngine;
using System.Collections.Generic;

public class Level : MonoBehaviour {

	bool _started = false;

	public bool started {
		get {
			return _started;
		}
	}

	float _progress = 0f;

	public float progress {
		get {
			return _started ? _progress : 0f;
		}
	}

	public Transform levelStartPosition;
	public Character player;
	public GameObject levelTarget;
	public Transform lightCycler;

	public GameObject lightMorning;
	public GameObject lightNoon;
	public GameObject lightEvening;
	public GameObject lightMidnight;

	public Camera mainCam;

	private float _startX = 0f;
	private float _distance = 0f;
	public float progressDistance {
		get {
			return _started ? _progress * _distance : 0f;
		}
	}

	public float distance {
		get {
			return _distance;
		}
	}

	float _startTime = 0f;
	float _pauseTime = 0f;

	public float sinceLevelLoaded {

		get {
			return _started ? Time.timeSinceLevelLoad - _startTime - _pauseTime : 0f;
		}
	}

	private float _endX = 0f;
	public float endX {
		get {
			return _endX;
		}
	}

	private CameraFollow cf;

	private LevelBuilder _builder;

	// Use this for initialization
	void Start () {
		_builder = gameObject.GetComponent<LevelBuilder>();
		cf = Camera.main.GetComponent<CameraFollow>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!_started && _builder.ready)
			_Go();
		else if (_started) {
			_progress = Mathf.Clamp01((cf.X - _startX) / _distance);
		}

		if (_started) {
			if (_progress < 0.5f)
				lightCycler.rotation = Quaternion.Lerp(lightMorning.transform.rotation, lightNoon.transform.rotation, 2f * _progress);
			else
				lightCycler.rotation = Quaternion.Lerp(lightNoon.transform.rotation, lightEvening.transform.rotation, 2f * _progress - 2f);
		}
	}

	private void _Go() {
		_startTime = Time.timeSinceLevelLoad;
		_pauseTime = 0f;
		_started = true;
		_startX = _builder.players[0].transform.position.x;
		_endX = levelTarget.transform.position.x;
		_distance = _endX - _startX;
	}

	public void LevelEndDeath() {
		StartCoroutine( LoadNextLevel());
	}

	public void LevelEndTarget() {
		StartCoroutine( LoadNextLevel());
	}

	private IEnumerator<WaitForSeconds> LoadNextLevel() {
//		Application.LoadLevel(Application.loadedLevel);
		cf.follow = false;
		_started = true;
		float startT = sinceLevelLoaded;
		float curT = startT;
		Vector3 camTarget1 = mainCam.transform.position + Vector3.forward * -5f;
		Vector3 camTarget2 = levelStartPosition.transform.position;
		camTarget2.z = camTarget1.z;
		Vector3 camTarget3 = camTarget2;
		camTarget3.z = mainCam.transform.position.z;
		while (curT - 1f < startT) {
			mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, camTarget1, (curT - startT) / 1f);
			curT = sinceLevelLoaded;
			yield return new WaitForSeconds(0.1f);
		}
		player.transform.position = levelStartPosition.position;

		_builder.Start();
		while (!_builder.ready)
			yield return new WaitForSeconds(0.1f);

		curT = sinceLevelLoaded;
		startT = curT;
		while (curT - 1f < startT) {
			mainCam.transform.position = Vector3.Lerp(camTarget2, camTarget3, (curT - startT) / 1f);
			curT = sinceLevelLoaded;
			yield return new WaitForSeconds(0.1f);
		}

		cf.Reset();
		_Go();
	}
}
