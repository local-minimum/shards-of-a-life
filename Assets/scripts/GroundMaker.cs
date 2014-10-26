using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GroundMaker : MonoBehaviour {

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

	public Vector2 uvScale = Vector2.one;

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
	private List<Vector3> _vertices = new List<Vector3>();
	private List<Vector3> _topEdge = new List<Vector3>();
	private List<int> _tris = new List<int>();

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

	public void Build(TerrainType tType, float length, float stepOrder) {
		stepChaos = stepOrder;
		Build(tType, length);
	}

	public void Build(TerrainType tType, float length, float stepOrder, float amplitude) {
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
		Build(tType, length, stepOrder);
	}

	public void Build() {
		_ySeed = Random.value * 100f;
		_vertices.Clear();
		_tris.Clear();
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
			_renders += 1f;
			_yStats += _vertices[_vertices.Count - 3].y > _vertices[1].y ? 1f : 0f;
			Vector2[] uvs = new Vector2[_vertices.Count];
			for (int i=0; i < uvs.Length; i++) {
				uvs[i].x = _vertices[i].x * uvScale.x;
				uvs[i].y = _vertices[i].y * uvScale.y;
			}
			Mesh mesh = GetComponent<MeshFilter>().mesh;
			mesh.Clear();
			mesh.vertices = _vertices.ToArray();
			mesh.uv = uvs;
			mesh.triangles = _tris.ToArray();
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
			for (int i=0; i<2; i++)
				_tris.Add(_tris.Count);
		} else {
			_vertices.Add (topPt);
			_tris.Add(_tris.Count);
		}
		if (_vertices.Count > 2) {
			_vertices.Add (topPt);
			_vertices.Add(botPt);
			_vertices.Add (_vertices[_vertices.Count - 5]);
			
			for (int i=0; i<3; i++)
				_tris.Add(_tris.Count);
			
			if (topPt.x + stepSize <= groundSectionLength) {
				_vertices.Add (botPt);
				_vertices.Add (topPt);
				for (int i=0; i<2; i++)
					_tris.Add(_tris.Count);
			}
		}
	}

	private void _buildOrdered() {
		float x = 0f;
		Vector3 topPt = Vector3.zero;
		Vector3 botPt = topPt - Vector3.up * thickness;
		_topEdge.Add(topPt);

		while (x <= groundSectionLength) {

			if (Random.value < stepChaos || x + stepSize > groundSectionLength) {
				Vector3 topR = new Vector3(x, topPt.y);
				Vector3 botR = new Vector3(x, botPt.y);

				_topEdge.Add(topR);
				_vertices.Add(botPt);
				_vertices.Add(topPt);
				_vertices.Add(topR);
				_vertices.Add(topR);
				_vertices.Add(botR);
				_vertices.Add(botPt);
				for (int i=0; i<6; i++)
					_tris.Add (_tris.Count);

				if (x + stepSize < groundSectionLength) {
					topPt = new Vector3(x, (Mathf.PerlinNoise(x, _ySeed) - _perlinMean) * orderedAmplitude + topPt.y);
					botPt = topPt - Vector3.up * thickness;
				}
			}


			x += stepSize;
		}

		//Debug.Log(_tris.Count);
		//Debug.Log(_vertices[_vertices.Count - 1]);
	}

	private void _buildGroundSlopedAndAccelerated(bool sloped) {
		float x = 0f;
		float slope = 0f;
		//float y = 0f;
		float acceleration = 0f;

		while (x <= groundSectionLength) {
			if (_vertices.Count == 0) {
				_buildGroundIteration(Vector3.zero);
			} else if (Random.value < stepChaos || x + stepSize > groundSectionLength) {
				/*
				if (_vertices.Count == 2) {
					y = _vertices[1].y;
				} else {
					y = _vertices[_vertices.Count - 4].y;
				}
				*/
				if (sloped) {
					slope += (Mathf.PerlinNoise(x, _ySeed) - _perlinMean) * slopedAmplitude * stepSize;
				} else {
					acceleration += (Mathf.PerlinNoise(x, _ySeed) - _perlinMean) * acceleratedAmplitude * stepSize;
					slope += acceleration;
				}
				_buildGroundIteration(new Vector3(x, slope));

			}
			x += stepSize;
		}
	}

	private void _buildGroundBasic() {
		float x = 0f;

		while (x <= groundSectionLength) {
			if (Random.value < stepChaos || x + stepSize > groundSectionLength) {
				_buildGroundIteration(new Vector3(x, (Mathf.PerlinNoise(x, _ySeed) - _perlinMean) * basicAmplitude));
			}
			x += stepSize;
		}

	}
}
