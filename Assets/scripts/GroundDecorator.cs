using UnityEngine;
using System.Collections.Generic;

public class GroundDecorator : MonoBehaviour {

	public float houseProbability = 0.9f;
	private List<House> houses = new List<House>();
	public bool debug = true;

	GroundStitcher _stitcher;
	// Use this for initialization
	void Start () {
		_stitcher = gameObject.GetComponent<GroundStitcher>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.D))
			Decorate();

		if (debug) {
			Vector3 lhs = Vector3.zero;
			foreach (Vector3 rhs in _stitcher.groundSurface()) {
				if (!_stitcher.groundInvalidSegment) 
					Debug.DrawLine(_stitcher.groundSurfaceTransform.position + lhs, _stitcher.groundSurfaceTransform.position + rhs);
				lhs = rhs;
			}
		}
	}

	void Decorate() {
		Vector3 lhs = Vector3.zero;
		bool notInit = true;
		List<float> houseBaseWidths = new List<float>();
		int nHouse = 0;
		int i = -1;

		foreach (Vector3 rhs in _stitcher.groundSurface()) {
			i++;
			if (notInit) {
				lhs = rhs;
				notInit = false;
				continue;
			}
			if (_stitcher.groundInvalidSegment)
				continue;

			if (_stitcher.groundSurfaceType == GroundMaker.TerrainType.Ordered & 
			    (rhs - lhs).magnitude > CityPlanner.houseWidthMin &
			    Random.value < houseProbability) {
				if (nHouse < houses.Count) {
					CityPlanner.PrepareFoundation(houses[nHouse], lhs, _stitcher.groundSurfaceTransform);
				} else {
					houses.Add(CityPlanner.PrepareFoundation(lhs, _stitcher.groundSurfaceTransform));
				}
				nHouse++;
				houseBaseWidths.Add(rhs.x - lhs.x);
			} else if (_stitcher.groundSurfaceType != GroundMaker.TerrainType.Ordered) {

			}
			lhs = rhs;

		}

		while (houses.Count > nHouse) {
			House h = houses[houses.Count - 1];
			houses.Remove(h);
			Destroy(h.gameObject);
		}
		CityPlanner.Architect(houses, houseBaseWidths);

	}
}
