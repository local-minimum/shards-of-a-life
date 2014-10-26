using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class DestructionWave : MonoBehaviour {

	public LayerMask destructLayers;

	[Range(0f, 10f)]
	public float destroyF = 0.1f;
	public float startX = -20f;

	[Range(0f, 10f)]
	public float speed = 1.4f;

	public GameObject deathEffect;

	public Level level;

	private HashSet<GameObject> _destructables = new HashSet<GameObject>();

	private void _Death(GameObject o) {
		Debug.Log(string.Format("Death to {0}", o));
		if (o.renderer.isVisible) {
			GameObject de = (GameObject) Instantiate(deathEffect);
			de.transform.position = o.transform.position;
			de.particleSystem.Play();
			Destroy(o, 0.3f);
			Destroy(de, 2f);
		} else {
			Destroy(o, 0.3f);
		}
	}

	// Update is called once per frame
	void Update () {
		if (level.started && Random.value < destroyF * Time.deltaTime && _destructables.Count > 0) {
			List<GameObject> l = _destructables.ToList();
			GameObject o = l[Random.Range(0, l.Count - 1)];
			if (o) {
				_destructables.Remove(o);
				_Death(o);
			}

		}

		if (Application.isEditor) {

			Vector3 v = new Vector3(speed * level.progressDistance + startX, 5f, 1f);
			if ((transform.position - v).magnitude > 0.01f)
				transform.position = v;
		} else {
			transform.position = new Vector3(speed * level.progressDistance + startX, 5f, 1f);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {

		if ((destructLayers.value & 1<<other.gameObject.layer) != 0) {
			_destructables.Add(other.gameObject);
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		_destructables.Remove(other.gameObject);
	}
}
