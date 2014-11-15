using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Character : MonoBehaviour {

	public float forceFactor = 80f;
	public float jumpFactor = 40f;
	public float maxLateral = 2f;

	private float _lastContact = 0f;
	private float _graceGrounded = 0.3f;

	private float _decayLateral = -0.25f;
	private float _airLateral = 0.5f;
	private bool _jumped = false;
	private Animator _anim;
	private bool _movingLeft = true;

	public Level level;
	private HashSet<GameObject> _selfParts = new HashSet<GameObject>();
	private List<GameObject> _selfShards = new List<GameObject>();
	private Dictionary<GameObject, float> _hurters = new Dictionary<GameObject, float>();

	public float reHurtDelay = 1f;
	public float upForceWalking = 2f;
	private float _deadShard = 0.04f;
	public int health {
		
		get {
			return _selfShards.Count(e => e.renderer.material.color.a > _deadShard);
		}
	}

	public bool grounded {

		get {
			//Debug.Log(string.Format("{0}, {1}<{2}", !_jumped, Time.timeSinceLevelLoad - _lastContact, _graceGrounded));
			return !_jumped && Time.timeSinceLevelLoad - _lastContact < _graceGrounded;
		}

	}
	// Use this for initialization
	void Start () {
		foreach (Transform t in gameObject.GetComponentsInChildren<Transform>()) {
			if (t.renderer)
				t.renderer.enabled = false;
			_selfParts.Add(t.gameObject);
		}
		_anim = gameObject.GetComponent<Animator>();
	}

	void Update() {
		if (!level.started)
			return;

		if (Input.GetButtonDown("Jump") && grounded) {
			_jumped = true;
			_anim.SetBool("jumping", true);
			rigidbody2D.AddForce(Vector2.up * forceFactor * jumpFactor);
		}

		float t = level.sinceLevelLoaded - reHurtDelay;
		GameObject[] keys = _hurters.Where(e => e.Value < t).Select(kvp => kvp.Key).ToArray();
		//Debug.Log(string.Format("{0} H, {1} k {2} t {3} T", _hurters.Count, keys.Length, t, level.sinceLevelLoaded));
		foreach(GameObject key in keys) {
			//Debug.Log(string.Format("{0}=>{1}", key, _hurters[key]));
			_hurters.Remove(key);
		}
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (!level.started)
			return;

		float f = grounded ? 1f : _airLateral;

		rigidbody2D.AddForce(new Vector2(Input.GetAxis("Horizontal"), 0.05f) * forceFactor * f);
		if (Input.GetButtonDown("Horizontal")) {
		} else {
			rigidbody2D.AddForce(Vector2.right * rigidbody2D.velocity.x * _decayLateral * f + Vector2.up * upForceWalking);
		}
		if ((Mathf.Sign(Input.GetAxis("Horizontal")) == 1f) != _movingLeft)
			_Flip();

		if (Mathf.Abs(rigidbody2D.velocity.x) > maxLateral)
			rigidbody2D.velocity = new Vector2(maxLateral * Mathf.Sign(rigidbody2D.velocity.x), rigidbody2D.velocity.y);

		_anim.SetFloat("speed", Mathf.Abs(rigidbody2D.velocity.x));
		_anim.SetFloat("vSpeed", rigidbody2D.velocity.y);

	}

	void OnCollisionEnter2D(Collision2D coll) {
		//Debug.Log(string.Format("{0}<{1}", coll.transform.position.y, transform.position.y));
		if (coll.transform.position.y < transform.position.y) {
			_jumped = false;
			_anim.SetBool("jumping", false);
			_lastContact = Time.timeSinceLevelLoad;
			if (rigidbody2D.velocity.y < -5f)
				Hurt(Random.Range(3, 6));
		}
	}

	void OnCollisionStay2D(Collision2D coll) {
		if (grounded)
			_lastContact = Time.timeSinceLevelLoad;
	}

	private void _Flip() {
		Vector3 s = transform.localScale;
		s.x *= -1;
		_movingLeft = !_movingLeft;
		transform.localScale = s;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "PlayerStoppers")
			level.LevelEndDeath();
		else if (other.tag == "Enemy") {
			if (!_hurters.ContainsKey(other.gameObject)) {
				Hurt(Random.Range(1,4));
				_hurters.Add(other.gameObject, level.sinceLevelLoaded);
			}
		} else if (other.tag == "LevelTarget") {
			level.LevelEndTarget();
		}

		//Potentially make timeout for when to allow same to re-hurt
	}

	public void Hurt() {
		Hurt (1);
	}
	
	public void Hurt(int amount) {
		if (_selfShards.Count == 0) {
			foreach (Transform t in gameObject.GetComponentsInChildren<Transform>()) {
				if (!_selfParts.Contains(t.gameObject) && t.renderer)
					_selfShards.Add(t.gameObject);
			}
			//Debug.Log(_selfShards.Count);
		}

		for (int i=0; i<amount; i++) {
			int h = health;
			//Debug.Log(h);
			if (h == 0) {
				level.LevelEndDeath();
				return;
			} else {
				int j = Random.Range(0, health);
				GameObject o = _selfShards.Where(e => e.renderer.material.color.a > _deadShard).ToArray()[j];
				Color c = o.renderer.material.color;
				c.a = _deadShard;
				o.renderer.material.color = c;
			}
		}
	}
}
