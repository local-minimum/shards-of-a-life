using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class GroundMaker : Mesher {

	public enum TerrainType {Basic, Ordered, Sloped, Accelerated};

	[HideInInspector]
	[SerializeField]
	private bool _debug = false;

	public bool debug {
		get {
			return _debug;
		}

		set {
			if (value)
				_initMesh();
			_debug = value;

		}
	}

	[Range (1f, 1000f)]
	public float groundSectionLength = 20f;

	[Range (0f, 10f)]
	public float stepSize = 0.1f;

	[Range (0.5f, 10f)]
	public float thickness = 1f;

	[Range (0f, 10f)]
	public float slopedAmplitude = 1f;

	[Range (0f, 0.5f)]
	public float acceleratedAmplitude = 0.2f;

	[Range (0f, 10f)]
	public float basicAmplitude = 1f;

	[Range (0f, 10f)]
	public float orderedAmplitude = 1f;

	[Range (0f, 1f)]
	public float stepChaos = 0.1f;

	[HideInInspector]
	[SerializeField]
	private TerrainType _terrain = TerrainType.Basic;


	public TerrainType terrain {
		get {
			return _terrain;
		}
		set {
			_yStats = 0f;
			_renders = 0f;
			_terrain = value;
		}
	}

	public Vector3 anchorLeft {
		get {
			if (_topEdge.Count > 0) 
				return _topEdge[0];
			else 
				Debug.LogError(new KeyNotFoundException("No mesh initialized"));
			return Vector3.zero;
		}
	}

	public Vector3 anchorRight {
		get {
			if (_topEdge.Count > 0)
				return _topEdge[_topEdge.Count - 1];
			else
				Debug.LogError(new KeyNotFoundException("No mesh initialized"));
			return Vector3.zero;
		}
	}

	private float _yStats = 0f;
	private float _renders = 0f;
	private float _ySeed = 0f;
	private float _perlinMean = 0.4652489f;

	private List<Vector3> _topEdge = new List<Vector3>();
	public Vector3[] surface {
		get {
			return _topEdge.ToArray();
		}
	}

	// Use this for initialization
	void Start () {
		if (debug) {
			_initMesh();
			//TODO: Add a canvas to scene and add a redraw button
			/*
			Camera.main.gameObject.AddComponent<Button>();
			//Button b = GameObject.GetComponent<Button>();
			/*Text t = gameObject.AddComponent<Text>();
			t.text = "Redraw";
			RectTransform rt = b.GetComponent<RectTransform>();
			rt.offsetMin = Vector2.one * 0.01f;
					    */
			//Build();

		}
	}

	private void _initMesh() {
		if (!gameObject.GetComponent<MeshRenderer>()) {
			gameObject.AddComponent<MeshRenderer>();
			gameObject.renderer.material = new Material(Shader.Find("Diffuse"));
			gameObject.AddComponent<MeshFilter>();
		}

	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Build(float length) {
		groundSectionLength = length;
		Build();
	}

	public void Build(TerrainType tType) {
		terrain = tType;
		Build();
	}

	public void Build(TerrainType tType, float length) {
		terrain = tType;
		groundSectionLength = length;
		Build();
	}

	public void Build(TerrainType tType, float length, float chaos) {
		stepChaos = chaos;
		Build(tType, length);
	}

	public void Build(TerrainType tType, float length, float chaos, float amplitude) {
		switch (terrain) {
		case TerrainType.Basic:
			basicAmplitude = amplitude;
			break;
		case TerrainType.Sloped:
			slopedAmplitude = amplitude;
			break;
		case TerrainType.Accelerated:
			acceleratedAmplitude = amplitude;
			break;
		case TerrainType.Ordered:
			orderedAmplitude = amplitude;
			break;
		default:
			break;
		}
		Build(tType, length, chaos);
	}


	public void Build() {
		_ySeed = Random.value * 100f;
		_vertices.Clear();
		_topEdge.Clear();

		switch (terrain) {
		case TerrainType.Basic:
			_buildGroundBasic();
			break;
		case TerrainType.Sloped:
			_buildGroundSlopedAndAccelerated(true);
			break;
		case TerrainType.Accelerated:
			_buildGroundSlopedAndAccelerated(false);
			break;
		case TerrainType.Ordered:
			_buildOrdered();
			break;
		default:
			Debug.LogError(string.Format("Terrain {0} not implemented", terrain));
			break;
		}
		
		if (debug) {
			if (_vertices.Count < 6) {
				Debug.LogWarning(string.Format("{0} has no structure", gameObject.name));
				return;
			} else {
//				Debug.Log(stepChaos);
//				Debug.Log(string.Format("{0} has {1} triangles", gameObject.name, _vertices.Count / 3));
			}
			_renders += 1f;
			_yStats += _vertices[_vertices.Count - 3].y > _vertices[1].y ? 1f : 0f;

			Mesh mesh = GetComponent<MeshFilter>().mesh;
			mesh.Clear();
			mesh.vertices = _vertices.ToArray();
			mesh.uv = GenerateUv();
			mesh.triangles = GenerateTris();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			Debug.Log(string.Format("{2}: Ascending f={0} (N={1})", _yStats / _renders, _renders, terrain));
		}
		
	}
	
	private void _buildGroundIteration(Vector3 topPt) {
		_topEdge.Add(topPt);
		Vector3 botPt = topPt - Vector3.up * thickness;
		if (_vertices.Count == 0) {
			_vertices.Add(botPt);
			_vertices.Add(topPt);
		} else {
			_vertices.Add (topPt);

		}
		if (_vertices.Count > 2) {
			_vertices.Add (topPt);
			_vertices.Add(botPt);
			_vertices.Add (_vertices[_vertices.Count - 5]);

		
			_vertices.Add (botPt);
			_vertices.Add (topPt);
		}
	}

	private void _buildGroundFinalize() {
		while (_vertices.Count % 3 != 0) {
			//Debug.Log(_topEdge.Remove(_vertices[_vertices.Count - 1]));
			_vertices.RemoveAt(_vertices.Count - 1);
		}
	}

	private void _buildOrdered() {
		float x = 0f;
		Vector3 topPt = Vector3.zero;
		Vector3 botPt = topPt - Vector3.up * thickness;
//		_topEdge.Add(topPt);
//		Debug.Log(string.Format("{0}: Step size {1}", gameObject.name, stepSize));
//		Debug.Log(string.Format("{0}: Step chaos {1}", gameObject.name, stepChaos));
//		Debug.Log(string.Format("{0}: Section length {1}", gameObject.name, groundSectionLength));

		while (true) {

			x += stepSize * Random.Range(1f - stepChaos, 1f + stepChaos); // * Random.Range(1f - stepChaos, 1f + stepChaos * orderedAmplitude);

			if (x > groundSectionLength)
				break;

			Vector3 topR = new Vector3(x, topPt.y);
			Vector3 botR = new Vector3(x, botPt.y);

			_topEdge.Add(topPt);
			_topEdge.Add(topR);

			_vertices.Add(botPt);
			_vertices.Add(topPt);
			_vertices.Add(topR);
			_vertices.Add(topR);
			_vertices.Add(botR);
			_vertices.Add(botPt);

			topPt = new Vector3(topR.x, (Mathf.PerlinNoise(x, _ySeed) - _perlinMean) * orderedAmplitude + topPt.y);
			botPt = topPt - Vector3.up * thickness;


		}

		_buildGroundFinalize();
	}

	private void _buildGroundSlopedAndAccelerated(bool sloped) {
		float x = 0f;
		float elevation = 0f;
		//float y = 0f;
		float slope = 0f;

		while (x <= groundSectionLength) {
			if (_vertices.Count == 0) {
				_buildGroundIteration(Vector3.zero);
			} else {

				if (sloped) {
					elevation += (Mathf.PerlinNoise(x, _ySeed) - _perlinMean) * slopedAmplitude * stepSize;
				} else {
					slope += (Mathf.PerlinNoise(x, _ySeed) - _perlinMean) * acceleratedAmplitude * stepSize;
					elevation += slope;
				}
				_buildGroundIteration(new Vector3(x, elevation));

			}
			x += stepSize * Random.Range(1f - stepChaos, 1f + stepChaos);
		}
		_buildGroundFinalize();
	}

	private void _buildGroundBasic() {
		float x = 0f;

		while (x <= groundSectionLength) {
			_buildGroundIteration(new Vector3(x, (Mathf.PerlinNoise(x, _ySeed) - _perlinMean) * basicAmplitude));
			x += stepSize * Random.Range(1f - stepChaos, 1f + stepChaos);
		}
		_buildGroundFinalize();

	}
}
