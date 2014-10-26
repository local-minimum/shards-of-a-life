using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class ObjectGenerator : MonoBehaviour {

	public bool debug = true;
	public string materialName = "Diffuse";

	private Vector3 _baseRotation = Vector3.right; //TODO: Make interface for this

	private ObjectGenerator _foundation = null;

	private float _baseWidth = -1f;
	private float _baseElevation = 0f; //TODO: Make interface to stuff can grow out of slopes

	private Dictionary<ObjectGenerator, int> _superStructures = new Dictionary<ObjectGenerator, int>();
	protected List<int[]> _superStructureSlots = new List<int[]>();
	private List<int> _superStructureSlotsCapacity = new List<int>();

	protected List<Vector3> _vertices = new List<Vector3>();
	public Vector2 uvScale = Vector2.one;

	private int _segmentLevel = 0;
	protected bool _built = false;

	// Use this for initialization
	protected void Start () {
		
		if (!gameObject.GetComponent<MeshRenderer>()) {
			gameObject.AddComponent<MeshRenderer>();
			gameObject.renderer.material = new Material(Shader.Find(materialName));
			gameObject.AddComponent<MeshFilter>();
		}
	}

	public bool addSuperStructure(ObjectGenerator superStructure) {
		if (_superStructureSlotsCapacity.Where(capacity => capacity >= 0).Sum() == 0) {
			Debug.LogError("Tried to add superstructure to full segment");
			return false;
		}

		int pos = Enumerable.Range(0, _superStructureSlots.Count)
			.Where(index => _superStructureSlotsCapacity[index] != 0)
				.OrderBy(x => Random.value).First();
		if (_superStructureSlotsCapacity[pos] > 0)
			_superStructureSlotsCapacity[pos]--;

		_superStructures.Add(superStructure, pos);
		return true;
	}

	protected void addAnchorage(int[] indices) {
		addAnchorage(indices, -1);
	}

	protected void addAnchorage(int[] indices, int capacity) {
		_superStructureSlots.Add(indices);
		_superStructureSlotsCapacity.Add(capacity);
	}

	/// <summary>
	/// Makes sure that current section follows building norms relative to
	/// its foundation.
	/// 
	/// Safety first!
	/// </summary>
	protected void Align() {
		if (foundation) {
			transform.localPosition = anchorage[0];
		} else {
			Debug.LogWarning("Trying to align root");
		}
	}

	public Vector3[] anchorage {
		get {
			if (foundation)
				return foundation.getAnchorage(this);
			else
				return null;
		}
	}

	public float baseWidth {
		get {
			if (foundation) {
				Vector3[] anchor = anchorage;
				return Vector3.Magnitude(anchor[1] - anchor[0]);
			} else if (_baseWidth > 0f)
				return _baseWidth;
			else {
				_baseWidth = Random.Range(4f, 10f);
				return _baseWidth;
			}
		}
		
		set {
			if (foundation) {
				Debug.LogWarning("Base Width not applicable above foundation section");
			} else {
				_baseWidth = value;
			}
		}
	}

	public float baseElevation {
		get {
			if (foundation) {
				Vector3[] anchor = anchorage;
				return anchor[1].y - anchor[0].y; 
			} else
				return _baseElevation;
		}
		
		set {
			
			if (foundation) {
				Debug.LogWarning("Base Elevation not applicable above foundation section");
			} else {
				_baseElevation = value;
			}
		}
	}

	public Vector3 baseDirection {
		get {
			if (foundation) {
				Vector3[] anchor = anchorage;
				return (anchor[0] - anchor[1]);
			} else {
				return _baseRotation;
			}
		}

		set {
			if (foundation)
				Debug.LogWarning("Can't set base direction on other things than the root");
			else
				_baseRotation = value;
		}
	}

	public void Build() {
		GenerateVertices();
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		if (built)
			mesh.Clear();
		
		mesh.vertices = _vertices.ToArray();
		mesh.uv = GenerateUv();
		mesh.triangles = GenerateTris();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		

		if (foundation)
			Align();
		
		_built = true;

	}

	public bool built {
		
		get {
			return _built;
		}
	}

	public ObjectGenerator foundation {
		get {
			return _foundation;
		}
		
		set {
			_foundation = value;
			_foundation.addSuperStructure(this);
			_segmentLevel = _foundation._segmentLevel + 1;
			Align();
		}
	}


	protected int[] GenerateTris() {
		return Enumerable.Range(0, _vertices.Count).ToArray();
	}

	protected Vector2[] GenerateUv() {
		Vector2[] uvs = new Vector2[_vertices.Count];
		for (int i=0; i < uvs.Length; i++) {
			uvs[i].x = _vertices[i].x * uvScale.x;
			uvs[i].y = _vertices[i].y * uvScale.y;
		}
		return uvs;
	}

	protected abstract void GenerateVertices();

	public Vector3[] getAnchorage(ObjectGenerator superStructure) {
		if (_superStructures.ContainsKey(superStructure)) 
			return _vertices.Where((v, index) => _superStructureSlots[_superStructures[superStructure]].Contains(index)).ToArray();			
		else {
			Debug.LogError(string.Format("{0} is not a known superstucture of {1}", superStructure, this));
			return new Vector3[2]{Vector3.zero, Vector3.zero};
		}
	}

	public int segmentLevel {
		get {
			return _segmentLevel;
		}
	}
	
	public int segmentsMaxLevel {
		get {
			if (_superStructures.Count > 0)
				return _superStructures.Keys.Select(superStructure => superStructure.segmentsMaxLevel).Max();
			else
				return _segmentLevel;
		}
	}

	public void removeSuperStructure(ObjectGenerator superStructure) {
		int anchorPos = _superStructures[superStructure];
		if (_superStructureSlotsCapacity[anchorPos] >= 0)
			_superStructureSlotsCapacity[anchorPos]++;
		_superStructures.Remove(superStructure);
	}

	public void removeLastSuperStructureSlot() {
		int pos = superStructureSlots - 1;
		ObjectGenerator[] O = _superStructures.Keys.ToArray();
		foreach (ObjectGenerator o in O) {
			if (_superStructures[o] == pos) {
				removeSuperStructure(o);
				Destroy(o.gameObject);
			}
		}
		_superStructureSlots.RemoveAt(pos);
		_superStructureSlotsCapacity.RemoveAt(pos);

	}

	public void removeAllSuperStructureSlots() {
		destroyAllSuperstructures();
		_superStructures.Clear();
		_superStructureSlots.Clear();
		_superStructureSlotsCapacity.Clear();
	}

	public void destroyAllSuperstructures() {
		ObjectGenerator[] O = _superStructures.Keys.ToArray();
		foreach (ObjectGenerator o in O) {
			removeSuperStructure(o);
			Destroy(o.gameObject);
		}
	}

	public List<ObjectGenerator> superStructures {
		get {
			return _superStructures.Keys.ToList();
		}
	}

	public int superStructureSlots {
		get {
			return _superStructureSlots.Count;
		}
	}

	public int superStructureSlotsFree {
		get {
			return _superStructureSlotsCapacity.Any(c => c < 0) ? 
				999 : _superStructureSlotsCapacity.Select(c => c < 0 ? 0 : c).Sum();
		}
	}

	public void updateAnchorage(int index, int[] pos, int capacity) {
		_superStructureSlots[index] = pos;
		foreach (KeyValuePair<ObjectGenerator, int> kvp in _superStructures) {
			if (capacity != 0) {
				removeSuperStructure(kvp.Key);
				Destroy(kvp.Key.gameObject);
			} else {
				capacity--;
			}
		}
		_superStructureSlotsCapacity[index] = capacity;
	}
}