using UnityEngine;
using System.Collections.Generic;


public static class CityPlanner : System.Object {


	public static float stepNorm = 0.1f;
	public static float stepNoise = 0.025f;

	public static float houseWidthMin = 4f;
	public static float houseWidthMax = 14f;

	public static int minSegments = 3;
	public static int maxSegments = 10;

	public static float minSpacing = 0.5f;
	public static float maxSpacing = 1f;
	private static int houseId = 0;

	private static void Name(GameObject GO, Transform parentTransform) {
		GO.name = string.Format("House {0} (Block {1})", houseId, parentTransform.name);
		houseId++;
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
		float _heightVar = (float) (maxSegments - minSegments);
		float _xStep = Random.Range(stepNorm - stepNoise, stepNorm + stepNoise);

		int i = 0;
		House prevH = null;

		foreach (House h in houses) {
			Vector3 p = h.transform.localPosition;
			float w = Random.Range(houseWidthMin, houseWidths[i] < houseWidthMax ? houseWidths[i] : houseWidthMax); 
			p.x += Random.Range(0, houseWidths[i] - w);
			//h.transform.localPosition = p;
			if (h.debug)
				h.setupComponensForDebug();
			h.Generate(
				minSegments + Mathf.RoundToInt(_heightVar * Mathf.PerlinNoise(_x + _xStep * h.transform.position.x, _y)),
			w);
			i++;
		}
	}
}
