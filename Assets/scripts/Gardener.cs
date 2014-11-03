using UnityEngine;
using System.Collections.Generic;

public static class Gardener : System.Object {

	public static int minRootAnchorPositions = 4;
	public static int maxRootAnchorPositions = 10;

	
	public static float segmentHeightMin = 1f;
	public static float segmentHeightMax = 10f;

	public static float segmentLengthVariationMin = 0f;
	public static float segmentLengthVariationMax = 0.5f;

	public static float segmentParallelRoughnessVariationMin = 0f;
	public static float segmentParallelRoughnessVariationMax = 0.5f;

	public static float segmentRoughnessMin = 0f;
	public static float segmentRoughnessMax = 0.5f;

	public static float segmentRoughnessFrequencyMin = 0f;
	public static float segmentRoughnessFrequencyMax = 10f;

	public static float segmentTopHVariationMin = 0f;
	public static float segmentTopHVariationMax = 0.5f;

	private static int plantId = 0;

	private static void Name(GameObject GO, Transform parentTransform) {
		GO.name = string.Format("Plant {0} (Field {1})", plantId, parentTransform.name);
		plantId++;
	}

	public static Plant Seed(Vector3 pos, Transform parentTransform) {
		GameObject GO = new GameObject();
		GO.transform.parent = parentTransform;
		GO.transform.localPosition = pos;
		Name(GO, parentTransform);
		return GO.AddComponent<Plant>();
	}

	public static void Seed(Plant plant, Vector3 pos, Transform parentTransform) {
		Name (plant.gameObject, parentTransform);
		plant.transform.parent = parentTransform;
		plant.transform.localPosition = pos;
	}

	public static void Germinate(List<Plant> plants, List<Vector3> baseDirections) {
		
//		float _y = Random.Range(0f, 1000f);
//		float _x = Random.Range(0f, 1000f);
//		float _heightVar = (float) (maxSegments - minSegments);
		//float _xStep = Random.Range(stepNorm - stepNoise, stepNorm + stepNoise);
		
		int i = 0;
		//Plant prevP = null;
		
		foreach (Plant p in plants) {

			if (p.debug)
				p.setupComponensForDebug();

			p.segmentHeight = Random.Range(segmentHeightMin, segmentHeightMax);
			p.segmentLengthVariation = Random.Range(segmentLengthVariationMin, segmentLengthVariationMax);
			p.segmentParallelRoughnessVariation = Random.Range(segmentParallelRoughnessVariationMin, segmentParallelRoughnessVariationMax);
			p.segmentRoughness = Random.Range(segmentRoughnessMin, segmentRoughnessMax);
			p.segmentRoughnessFrequency = Random.Range(segmentRoughnessFrequencyMin, segmentRoughnessFrequencyMax);
			p.segmentTopHVariation = Random.Range(segmentTopHVariationMin, segmentTopHVariationMax);
			p.baseDirection = baseDirections[i];
			p.Build(Random.Range(minRootAnchorPositions, maxRootAnchorPositions));
			i++;
		}
	}
}
