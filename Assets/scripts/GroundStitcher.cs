using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroundStitcher : MonoBehaviour {

	public bool debug = true;
	public int segments = 10;

	private List<GroundMaker> _segments = new List<GroundMaker>();

	// Use this for initialization
	void Start () {
	
		/*
		List<float> stats = new List<float>();
		for (int i=0; i<100000; i++) {
			stats.Add(Mathf.PerlinNoise(Random.value * 1000f, Random.value * 1000f));
		}
		Debug.Log(stats.Sum() / stats.Count);
		*/
		Generate();
		//Generate();
	}

	public void Generate() {
		GroundMaker gm = null;
		Vector3 nextAnchor = Vector3.zero;

		for (int i=0; i<segments; i++) {
			if (_segments.Count < i + 1) {
				GameObject go = new GameObject();
				go.name = string.Format("Terrain Ground {0}", i);
				gm = go.AddComponent<GroundMaker>();
				_segments.Add(gm);
			} else
				gm = _segments[i];

			gm.debug = debug;
			gm.Build(GroundMaker.TerrainType.Sloped, 
			         Random.Range(10f, 40f), Random.Range(0.1f, 1f), Random.Range(1f, 10f));
			nextAnchor -= gm.anchorLeft;
			gm.transform.position = nextAnchor;
			nextAnchor += gm.anchorRight;

		}
	}
	
	// Update is called once per frame
	void Update () {
		if (debug && Input.GetKeyDown(KeyCode.R))
			Generate();
	}

	public GroundMaker[] GetSegments() {
		return _segments.ToArray();
	}
}
