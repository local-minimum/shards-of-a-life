using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Mob : MonoBehaviour {

	public float[] startDirectionP;
	public float[] directionTransLeftP;
	public float[] directionTransStillP;
	public float[] directionTransRightP;

	public float force;
	public float jumpForce;
	public float maxLateral;
	public float jumpiness = 0.0001f;
	public float reRollFreq = 0.5f;

	private bool _alive = true;

	private float _direction;
	private int _state;
	private float [][] _model;
	private float _prevX;

	private bool _jumped = false;
	private float _lastContact = 0f;
	private float _graceGrounded = 0.2f;
	private float _walkUpForce = 10f;
	public bool grounded {
		
		get {
			//Debug.Log(string.Format("{0}, {1}<{2}", !_jumped, Time.timeSinceLevelLoad - _lastContact, _graceGrounded));
			return !_jumped && Time.timeSinceLevelLoad - _lastContact < _graceGrounded;
		}
		
	}

	// Use this for initialization
	void Start () {
		startDirectionP = _SanityCheck(startDirectionP);
		directionTransLeftP = _SanityCheck(directionTransLeftP);
		directionTransStillP = _SanityCheck(directionTransStillP);
		directionTransRightP = _SanityCheck(directionTransRightP);
		_model = new float[3][];
		_model[0] = directionTransLeftP;
		_model[1] = directionTransStillP;
		_model[2] = directionTransRightP;
		_state = _Roll(startDirectionP);
		UpdateDirection();
		StartCoroutine(StateSwapper());

	}

	private static float[] _SanityCheck(float[] p) {
		float[] ret = new float[3];
		float t = p.Sum();
		for (int i=0, l=p.Length <= 3 ? p.Length : 3; i < l; i ++)
			ret[i] = p[i] / t;
		return ret;
	}

	private static int _Roll(float[] p) {
		float v = Random.value;
		for (int i=0; i < p.Length; i++) {
			if (v < p[i])
				return i;
			else
				v -= p[i];
		}
		return p.Length - 1;
	}

	private void UpdateDirection() {
		if (_state == 0)
			_direction = -1f;
		else if (_state == 1)
			_direction = 0f;
		else
			_direction = 1f;
	}

	IEnumerator<WaitForSeconds> StateSwapper() {
		while (_alive) {
			//Updating model
			_state = _Roll(_model[_state]);
			UpdateDirection();
			yield return new WaitForSeconds(reRollFreq);
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (rigidbody2D.isKinematic)
			return;

		//Physics
		rigidbody2D.AddForce(_direction * force * Vector2.right + Vector2.up * _walkUpForce);
		Debug.Log(Mathf.Abs(transform.position.x - _prevX) / maxLateral);
		if (Mathf.Abs(transform.position.x - _prevX) / maxLateral < jumpiness && grounded) {
			rigidbody2D.AddForce(Vector2.up * jumpForce);
			Debug.Log("jumped");
			_jumped = true;
		}

		if (Mathf.Abs(rigidbody2D.velocity.x) > maxLateral)
			rigidbody2D.velocity = new Vector2(maxLateral * Mathf.Sign(rigidbody2D.velocity.x), rigidbody2D.velocity.y);

		//Graphics
		if (Mathf.Sign(_direction) != Mathf.Sign (transform.localScale.x)) {
			if (_direction == 0f) {
				//Make rest
			} else {
				//Make sure walks
				Vector3 s = transform.localScale;
				s.x = s.x * -1f;
				transform.localScale = s;
			}
		}

		//NEXT TURN
		_prevX = transform.position.x;
	}

	void OnCollisionEnter2D(Collision2D coll) {
		//Debug.Log(string.Format("{0}<{1}", coll.transform.position.y, transform.position.y));
		if (coll.transform.position.y < transform.position.y) {
			_jumped = false;
			_lastContact = Time.timeSinceLevelLoad;
		}
	}

	void OnCollisionStay2D(Collision2D coll) {
		if (grounded)
			_lastContact = Time.timeSinceLevelLoad;
	}

}
