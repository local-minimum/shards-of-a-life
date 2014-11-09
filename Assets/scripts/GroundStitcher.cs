using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroundStitcher : MonoBehaviour {

	public bool debug = true;
	public int segments = 10;

	public Transform levelTarget;
	public Vector3 levelTargetOffset;

	private List<GroundMaker> _segments = new List<GroundMaker>();

	//TODO: This needs a better interface later on...
	public List<float> terrainAccProbability = new List<float>();
	public List<GroundMaker.TerrainType> terrainType = new List<GroundMaker.TerrainType>();
	public List<float> terrainSegmentLengthMin = new List<float>();
	public List<float> terrainSegmentLengthMax = new List<float>();
	public List<float> terrainSegmentStepSizeMin = new List<float>();
	public List<float> terrainSegmentStepSizeMax = new List<float>();
	public List<float> terrainSegmentStepChaosMin = new List<float>();
	public List<float> terrainSemgentStepChaosMax = new List<float>();
	public List<float> terrainSegmentAmplitudeMax = new List<float>();
	public List<float> terrainSegmentAmplitudeMin = new List<float>();

	private int idGS = 0;
	private bool _outsideSurfaceSegment = true;
	public IEnumerable<Vector3> groundSurface() {
//		Debug.Log(idGS);
		if (idGS != 0)
			yield break;
//		int i = 0;

		while(idGS < _segments.Count) {
			_outsideSurfaceSegment = true;
			foreach (Vector3 pt in _segments[idGS].surface) { 
//				Debug.Log(i);
				yield return pt;
				_outsideSurfaceSegment = false;
//				i++;
			}
			idGS++;
		}
		_outsideSurfaceSegment = true;
		idGS = 0;
	}

	public IEnumerable<Mesher> interactables {
		get {
			foreach (GroundMaker gm in _segments) {
				if (gm.interactable)
					yield return gm;
			}
		}
	}

	public GroundMaker.TerrainType groundSurfaceType {
		get {
			return _segments[idGS].terrain;
		}
	}

	public Transform groundSurfaceTransform {
		get {
			return _segments[idGS].transform;
		}
	}

	public bool groundInvalidSegment {
		get {
			return _outsideSurfaceSegment;
		}
	}
	// Use this for initialization
	void Start () {
	
	}

	public void Generate() {
		GroundMaker gm = null;
		Vector3 nextAnchor = Vector3.zero;
		while (_segments.Count > segments) {
			GroundMaker segment = _segments[_segments.Count - 1];
			_segments.Remove(segment);
			Destroy(segment.gameObject);
		}

		for (int i=0; i<segments; i++) {
			if (_segments.Count < i + 1) {
				GameObject go = new GameObject();
				go.name = string.Format("Terrain Ground {0}", i);
				gm = go.AddComponent<GroundMaker>();
				_segments.Add(gm);
			} else
				gm = _segments[i];

			gm.debug = debug;

			float terrainP = terrainAccProbability.Max() * Random.value;
			int idT = 0;
			while (terrainP > terrainAccProbability[idT] && idT < terrainAccProbability.Count)
				idT++;

			gm.stepSize = Random.Range(terrainSegmentStepSizeMin[idT], terrainSegmentStepSizeMax[idT]);
			gm.Build(terrainType[idT], 
			         Random.Range(terrainSegmentLengthMin[idT], terrainSegmentLengthMax[idT]),
			         Random.Range(terrainSegmentStepChaosMin[idT], terrainSemgentStepChaosMax[idT]), 
			         Random.Range(terrainSegmentAmplitudeMin[idT], terrainSegmentAmplitudeMax[idT]));
			//nextAnchor -= gm.anchorLeft;
			gm.transform.position = nextAnchor - gm.anchorLeft;
			nextAnchor = gm.anchorRight + gm.transform.localPosition;

		}
		levelTarget.position = nextAnchor + levelTargetOffset;
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
