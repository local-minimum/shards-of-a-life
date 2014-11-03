using UnityEngine;
using System.Collections.Generic;

public class CameraFly : MonoBehaviour {

	public float speed = 1f;

	private GroundStitcher gs;
	private bool flying = false;
	public Vector3 filmingPerspective = Vector3.forward * -10f;

	// Use this for initialization
	void Start () {
		gs = GameObject.FindObjectOfType<GroundStitcher>();
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(KeyCode.F))
			Fly();

	}

	void Fly() {
		if (!flying) {
			StartCoroutine(_Fly());
		}
	}

	private IEnumerator<WaitForSeconds> _Fly() {

		flying = true;
		
		Vector3 from = Vector3.zero + filmingPerspective;

		foreach (Vector3 surf in gs.groundSurface()) {

			float segStart = Time.timeSinceLevelLoad;
			Vector3 to = surf + gs.groundSurfaceTransform.position;
			to = (to + from) / 2f;
			float segTot = (to - from).magnitude / speed;
			float segEnd = segStart + segTot;

			if (from.x == to.x)
				continue;
			bool firstPass=true;
			while (Time.timeSinceLevelLoad < segEnd) {
				transform.position = Vector3.Lerp(from, to, (Time.timeSinceLevelLoad - segStart) / segTot) +
					filmingPerspective;
				if (firstPass) {
					firstPass=false;
					continue;
				}
				yield return new WaitForSeconds(0.02f);
			}
			from = to;
		}
		flying = false;
	}
}
