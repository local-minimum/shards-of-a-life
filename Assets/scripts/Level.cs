using UnityEngine;
using System.Collections;

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

	public GameObject levelTarget;

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
	}

	private void _Go() {
		_startTime = Time.timeSinceLevelLoad;
		_pauseTime = 0f;
		_started = true;
		_startX = _builder.players[0].transform.position.x;
		_endX = levelTarget.transform.position.x;
		_distance = _endX - _startX;
	}

	public void GameOver() {
		Application.LoadLevel(Application.loadedLevel);
	}
	
}
