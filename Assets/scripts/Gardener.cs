using UnityEngine;
using System.Collections.Generic;

public class Gardener : MonoBehaviour {

	private static Gardener _instance;

	public static Gardener instance {

		get {
			if (_instance==null)
				_instance = GameObject.FindObjectOfType<Gardener>();
			return _instance;
		}
	}

	public Color segmentColor;
	public Color leafColorA;
	public Color leafColorB;

	public int minRootAnchorPositions = 4;
	public int maxRootAnchorPositions = 7;

	
	public float segmentHeightMin = 1f;
	public float segmentHeightMax = 10f;

	public float segmentLengthVariationMin = 0f;
	public float segmentLengthVariationMax = 0.5f;

	public float segmentParallelRoughnessVariationMin = 0f;
	public float segmentParallelRoughnessVariationMax = 0.5f;

	public float segmentRoughnessMin = 0f;
	public float segmentRoughnessMax = 0.5f;

	public float segmentRoughnessFrequencyMin = 0f;
	public float segmentRoughnessFrequencyMax = 10f;

	public float segmentTopHorisontalVarMin = 0f;
	public float segmentTopHorisontalVarMax = 0.5f;

	public float rootBaseMin = 0.5f;
	public float rootBaseMax = 1f;

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

			p.segmentColor = instance.segmentColor;
			p.interactableColor = Color.Lerp(instance.leafColorA, instance.leafColorB, Random.value);
			p.segmentLengthVariation = Random.Range(instance.segmentLengthVariationMin, instance.segmentLengthVariationMax);
			p.segmentParallelRoughnessVariation = Random.Range(instance.segmentParallelRoughnessVariationMin,
			                                                   instance.segmentParallelRoughnessVariationMax);
			p.segmentRoughness = Random.Range(instance.segmentRoughnessMin, instance.segmentRoughnessMax);
			p.segmentRoughnessFrequency = Random.Range(instance.segmentRoughnessFrequencyMin,
			                                           instance.segmentRoughnessFrequencyMax);
			p.segmentTopHorisontalVar = Random.Range(instance.segmentTopHorisontalVarMin,
			                                      instance.segmentTopHorisontalVarMax);
			p.baseDirection = baseDirections[i] * Random.Range(instance.rootBaseMin, instance.rootBaseMax);
			if (p.baseDirection.magnitude > baseDirections[i].magnitude)
				p.baseDirection = baseDirections[i];
			p.segmentHeight = p.baseDirection.magnitude * Random.Range(instance.segmentHeightMin, instance.segmentHeightMax);

			p.Build(Random.Range(instance.minRootAnchorPositions, instance.maxRootAnchorPositions));
			i++;
		}
	}
}
