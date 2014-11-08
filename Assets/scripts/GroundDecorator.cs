using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroundDecorator : MonoBehaviour {

	public float houseProbability = 0.9f;
	private List<House> houses = new List<House>();
	private List<Plant> plants = new List<Plant>();

	public bool debug = true;
	public float plantBaseMaxSlope = 0.3f;
	public float plantProbability = 0.1f;

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
		List<Vector3> plantBaseDirections = new List<Vector3>();

		int nHouses = 0;
		int nPlants = 0;
		int i = -1;

		foreach (Vector3 rhs in _stitcher.groundSurface()) {
			i++;
			if (notInit || _stitcher.groundInvalidSegment || lhs.x >= rhs.x) {
				lhs = rhs;
				if (notInit)
					notInit = false;
				continue;
			}

			if (_stitcher.groundSurfaceType == GroundMaker.TerrainType.Ordered && 
			    (rhs.x - lhs.x) > CityPlanner.instance.houseWidthMin &&
			    Random.value < houseProbability) {
				//Produce a house
				if (nHouses < houses.Count) {
					CityPlanner.PrepareFoundation(houses[nHouses], lhs, _stitcher.groundSurfaceTransform);
				} else {
					houses.Add(CityPlanner.PrepareFoundation(lhs, _stitcher.groundSurfaceTransform));
				}
				nHouses++;
				houseBaseWidths.Add(rhs.x - lhs.x);
			} else if (_stitcher.groundSurfaceType != GroundMaker.TerrainType.Ordered &&
				Mathf.Abs(Vector3.Dot((rhs - lhs).normalized, Vector3.up)) < plantBaseMaxSlope &&
			    Random.value < plantProbability) {

				//Produce a plant/shrub/tree
				if (nPlants < plants.Count) {
					Gardener.Seed(plants[nPlants], lhs, _stitcher.groundSurfaceTransform);
				} else {
					plants.Add(Gardener.Seed(lhs, _stitcher.groundSurfaceTransform));
				}
				nPlants++;
				plantBaseDirections.Add(rhs - lhs);
			}
			lhs = rhs;

		}

		while (houses.Count > nHouses) {
			House h = houses[houses.Count - 1];
			houses.Remove(h);
			Destroy(h.gameObject);
		}

		while (plants.Count > nPlants) {
			Plant p = plants[plants.Count - 1];
			plants.RemoveAt(plants.Count - 1);
			Destroy(p.gameObject);
		}

		CityPlanner.Architect(houses, houseBaseWidths);
		Gardener.Germinate(plants, plantBaseDirections);
	}
}
