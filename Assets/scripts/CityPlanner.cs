using UnityEngine;
using System.Collections.Generic;


public class CityPlanner : MonoBehaviour {

	private static CityPlanner _instance;

	public static CityPlanner instance {
		get {
			if (_instance == null)
				_instance = GameObject.FindObjectOfType<CityPlanner>();
			return _instance;
		}
	}

	public Color segmentColor;
	public Color floorColorA;
	public Color floorColorB;

	public float stepNorm = 0.1f;
	public float stepNoise = 0.025f;

	public float houseWidthMin = 4f;
	public float houseWidthMax = 14f;

	public int minSegments = 3;
	public int maxSegments = 10;

	public float minSpacing = 0.5f;
	public float maxSpacing = 1f;
	private int houseId = 0;

	private static void Name(GameObject GO, Transform parentTransform) {
		GO.name = string.Format("House {0} (Block {1})", instance.houseId, parentTransform.name);
		instance.houseId++;
	}

	public static House PrepareFoundation(Vector3 pos, Transform parentTransform) {
		GameObject GO = new GameObject();
		GO.transform.parent = parentTransform;
		GO.transform.localPosition = pos;
		Name (GO, parentTransform);
		return GO.AddComponent<House>();

	}

	public static void PrepareFoundation(House h, Vector3 pos, Transform parentTransform) {
		Name(h.gameObject, parentTransform);
		h.transform.parent = parentTransform;
		h.transform.localPosition = pos;
	}


	public static void Architect(List<House> houses, List<float> houseWidths) {

		float _y = Random.Range(0f, 1000f);
		float _x = Random.Range(0f, 1000f);
		float _heightVar = (float) (instance.maxSegments - instance.minSegments);
		float _xStep = Random.Range(instance.stepNorm - instance.stepNoise,
		                            instance.stepNorm + instance.stepNoise);

		int i = 0;
//		House prevH = null;

		foreach (House h in houses) {
			Vector3 p = h.transform.localPosition;
			float w = Random.Range(instance.houseWidthMin, 
			                       houseWidths[i] < instance.houseWidthMax ? 
			                       houseWidths[i] : instance.houseWidthMax); 
			p.x += Random.Range(0, houseWidths[i] - w);
			h.baseWidth = w;
			h.baseDirection = Vector3.right;
			h.segmentColor = instance.segmentColor;
			h.interactableColor = Color.Lerp(instance.floorColorA, 
			                                 instance.floorColorB, Random.value);
			//h.transform.localPosition = p;
			if (h.debug)
				h.setupComponensForDebug();
			h.Generate(
				instance.minSegments + 
				Mathf.RoundToInt(_heightVar * 
			                 Mathf.PerlinNoise(_x + _xStep * h.transform.position.x, _y)),
			w);
			i++;
		}
	}
}
