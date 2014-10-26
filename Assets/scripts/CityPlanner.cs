using UnityEngine;
using System.Collections.Generic;


public class CityPlanner : MonoBehaviour {

	public bool debug = true;
	public float stepNorm = 0.1f;
	public float stepNoise = 0.025f;

	public float houseWidthMin = 4f;
	public float houseWidthMax = 10f;

	public int minSegments = 3;
	public int maxSegments = 10;

	public float minSpacing = 0.5f;
	public float maxSpacing = 1f;

	List<House> _houses = new List<House>();

	// Use this for initialization
	void Start () {
		foreach (GameObject h in GameObject.FindGameObjectsWithTag("House"))
			_houses.Add(h.GetComponent<House>());
	}
	
	// Update is called once per frame
	void Update () {
		if (debug && Input.GetKeyDown(KeyCode.B)) {
			float _y = Random.Range(0f, 1000f);
			float _x = Random.Range(0f, 1000f);
			float _xStep = Random.Range(stepNorm - stepNoise, stepNorm + stepNoise);

			int i = 0;
			House prevH = null;

			foreach (House h in _houses) {
				if (i > 0) {
					Vector3 p = h.transform.localPosition;
					p.x = prevH.transform.position.x + h.baseWidth + Random.Range(minSpacing, maxSpacing);
					h.transform.localPosition = p;
				}
				h.Generate(Random.Range(minSegments, maxSegments),
				           houseWidthMin + houseWidthMax * Mathf.PerlinNoise(_x + _xStep * i, _y));
				i++;
				prevH = h;
			}
		}
	}
}
