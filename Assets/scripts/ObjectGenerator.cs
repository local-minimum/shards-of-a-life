﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class ObjectGenerator : Mesher {

	/// <summary>
	/// Toggle some extra debugging while developing
	/// </summary>
	public bool debug = true;

	/// <summary>
	/// The name of the material for the mesh created.
	/// </summary>
	public string materialName = "Diffuse";

	/// <summary>
	/// The base rotation (set via base direction)
	/// </summary>
	private Vector3 _baseDirection = Vector3.right;

	/// <summary>
	/// The parent of the current segment (null if root)
	/// </summary>
	private ObjectGenerator _foundation = null;

	/// <summary>
	/// The width of the base of the segment (only settable for the root and -1 if not specified/to be randomized).
	/// </summary>
	private float _baseWidth = -1f;

	/// <summary>
	/// All depenent structures as keys and the lookup index for their base vertices as value
	/// </summary>
	private Dictionary<ObjectGenerator, int> _superStructures = new Dictionary<ObjectGenerator, int>();

	/// <summary>
	/// Resolving lookup index for the two vertex indices of the base
	/// </summary>
	protected List<int[]> _superStructureSlots = new List<int[]>();

	/// <summary>
	/// The number of superstructures(i.e. children allowed for a specific slot
	/// </summary>
	private List<int> _superStructureSlotsCapacity = new List<int>();

	/// <summary>
	/// The segment's depth level in the structure.
	/// </summary>
	private int _segmentLevel = 0;

	/// <summary>
	/// If the mesh has been generated
	/// </summary>
	protected bool _built = false;

	public Color segmentColor;
	public Color interactableColor;

	// Adds the needed features to draw the mesh
	protected void Start () {
		setupComponensForDebug();
	}

	public void setupComponensForDebug() {
		if (!gameObject.GetComponent<MeshRenderer>()) {
			gameObject.AddComponent<MeshRenderer>();
			gameObject.renderer.material = new Material(Shader.Find(materialName));
			gameObject.AddComponent<MeshFilter>();
		}

	}

	/// <summary>
	/// Attempts to find a slot to add a super structure (i.e. child).
	/// Typical use:
	/// <c>
	/// if (child.foundation.addSuperStructure(this)) ...
	/// </c>
	/// </summary>
	/// <returns><c>true</c>, if super structure was added, <c>false</c> otherwise.</returns>
	/// <param name="superStructure">Super structure (i.e. child).</param>
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

	/// <summary>
	/// Add a new anchoring/slot to add children with unlimited capacity
	/// </summary>
	/// <param name="indices">Indices.</param>
	protected void addAnchorage(int[] indices) {
		addAnchorage(indices, -1);
	}

	/// <summary>
	/// Adds the anchoring/slot to add children with specified capacity
	/// </summary>
	/// <param name="indices">Indices.</param>
	/// <param name="capacity">Capacity.</param>
	protected void addAnchorage(int[] indices, int capacity) {
		_superStructureSlots.Add(indices);
		_superStructureSlotsCapacity.Add(capacity);
	}

	/// <summary>
	/// Makes sure that current section follows building norms relative to
	/// its foundation.
	/// 
	/// Safety first!
	/// 
	/// Basically sets current sections origin vertex to appropriate position according
	/// to its parents anchorage position.
	/// 
	/// NOTE: Probably this does not work with scaling!
	/// </summary>
	protected void Align() {
		if (foundation) {
			transform.localPosition = anchorage[0];
		} else {
			Debug.LogWarning("Trying to align root");
		}
	}

	/// <summary>
	/// Gets the anchorage (base vetrices in parent's coordinate system)
    /// of the current section from its parent.
	/// </summary>
	/// <value>The anchorage.</value>
	public Vector3[] anchorage {
		get {
			if (foundation)
				return foundation.getAnchorage(this);
			else
				return null;
		}
	}

	/// <summary>
	/// Gets or sets (only allowed for the root) the width of the base.
	/// TODO: Make <c>_baseWidth</c> serialized and expose in editor to be able to do away with
	/// <c>rootBaseWidth</c> in plant etc.
	/// </summary>
	/// <value>The width of the base.</value>
	public float baseWidth {
		get {
			if (foundation) {
				return baseDirection.magnitude;
			} else if (_baseWidth > 0f)
				return _baseWidth;
			else {
				Debug.LogError("No valid base width set or obtainable");
				return -999f;
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
	

	/// <summary>
	/// Gets or sets (only allowed for root) the base direction.
	/// </summary>
	/// <value>The base direction.</value>
	public Vector3 baseDirection {
		get {
			if (foundation) {
				Vector3[] anchor = anchorage;
				return (anchor[1] - anchor[0]);
			} else {
				return _baseDirection;
			}
		}

		set {
			if (foundation)
				Debug.LogWarning("Can't set base direction on other things than the root");
			else
				_baseDirection = value;
		}
	}

	/// <summary>
	/// Build the mesh of the segment.
	/// </summary>
	public void Build() {
		GenerateVertices();

		if (debug) {
			Mesh mesh = GetComponent<MeshFilter>().mesh;
			
			if (built)
				mesh.Clear();
			
			mesh.vertices = vertices.ToArray();
			mesh.uv = GenerateUv();
			mesh.triangles = GenerateTris();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		
		}
		if (foundation)
			Align();

		if (foundation) {
			interactableColor = foundation.interactableColor;
			segmentColor = foundation.segmentColor;
		}

		_built = true;

	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="ObjectGenerator"/> is built.
	/// </summary>
	/// <value><c>true</c> if built; otherwise, <c>false</c>.</value>
	public bool built {
		
		get {
			return _built;
		}
	}

	/// <summary>
	/// Gets or sets the foundation.
	/// If foundation is set, the current segment is realigned to its foundation.
	/// </summary>
	/// <value>The foundation.</value>
	public ObjectGenerator foundation {
		get {
			return _foundation;
		}
		
		set {
			_foundation = value;
			if (_foundation.addSuperStructure(this)) {
				_segmentLevel = _foundation._segmentLevel + 1;
				Align();
			} else {
				Debug.LogError("Could not add myself to foundation");
				destroyAllSuperstructures();
				Destroy(gameObject);
			}
		}
	}
	

	/// <summary>
	/// Generates the vertices.
	/// </summary>
	protected abstract void GenerateVertices();

	/// <summary>
	/// Gets the anchorage of a child, typically used by the child.
	/// </summary>
	/// <returns>The anchorage.</returns>
	/// <param name="superStructure">Super structure.</param>
	public Vector3[] getAnchorage(ObjectGenerator superStructure) {
		if (_superStructures.ContainsKey(superStructure)) {
			int[] indices = _superStructureSlots[_superStructures[superStructure]];
			List<Vector3> anchor = new List<Vector3>();
			foreach (int i in indices)
				anchor.Add(vertices[i]);
//			Debug.Log(string.Format("{0} {1}", anchor[0], anchor[1]));
			return anchor.ToArray();
//			return _vertices.Where((v, index) => indices.Contains(index)).OrderBy((Vector3 v, int i) => indices[i]).ToArray();			
		} else {
			Debug.LogError(string.Format("{0} is not a known superstucture of {1}", superStructure, this));
			return new Vector3[2]{Vector3.zero, Vector3.zero};
		}
	}

	public IEnumerable<ObjectGenerator> interactables {

		get {
			if (interactable)
				yield return this;

			foreach (ObjectGenerator super in _superStructures.Keys) {
				foreach (ObjectGenerator interA in super.interactables)
					yield return interA;
			}
		}
	}

	/// <summary>
	/// Gets the segment depth level.
	/// </summary>
	/// <value>The segment level.</value>
	public int segmentLevel {
		get {
			return _segmentLevel;
		}
	}

	/// <summary>
	/// Gets maximum depth level present under a segment
	/// </summary>
	/// <value>The max level.</value>
	public int segmentsMaxLevel {
		get {
			if (_superStructures.Count > 0)
				return _superStructures.Keys.Select(superStructure => superStructure.segmentsMaxLevel).Max();
			else
				return _segmentLevel;
		}
	}

	/// <summary>
	/// Removes the super structure.
	/// </summary>
	/// <param name="superStructure">Super structure.</param>
	public void removeSuperStructure(ObjectGenerator superStructure) {
		int anchorPos = _superStructures[superStructure];
		if (_superStructureSlotsCapacity[anchorPos] >= 0)
			_superStructureSlotsCapacity[anchorPos]++;
		_superStructures.Remove(superStructure);
	}

	/// <summary>
	/// Removes the last super structure slot and all superstructures connected at it
	/// </summary>
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

	/// <summary>
	/// Removes all super structure slots.
	/// </summary>
	public void removeAllSuperStructureSlots() {
		destroyAllSuperstructures();
		_superStructures.Clear();
		_superStructureSlots.Clear();
		_superStructureSlotsCapacity.Clear();
	}

	/// <summary>
	/// Destroies all superstructures.
	/// </summary>
	public void destroyAllSuperstructures() {
		ObjectGenerator[] O = _superStructures.Keys.ToArray();
		foreach (ObjectGenerator o in O) {
			removeSuperStructure(o);
			Destroy(o.gameObject);
		}
	}

	/// <summary>
	/// Gets the super structures.
	/// </summary>
	/// <value>The super structures.</value>
	public List<ObjectGenerator> superStructures {
		get {
			return _superStructures.Keys.ToList();
		}
	}

	/// <summary>
	/// Gets the super structure slots.
	/// </summary>
	/// <value>The super structure slots.</value>
	public int superStructureSlots {
		get {
			return _superStructureSlots.Count;
		}
	}

	/// <summary>
	/// Gets the number of free super structure slots.
	/// </summary>
	/// <value>The number of free super structure slots or <c>999</c> if infinite.</value>
	public int superStructureSlotsFree {
		get {
			return _superStructureSlotsCapacity.Any(c => c < 0) ? 
				999 : _superStructureSlotsCapacity.Select(c => c < 0 ? 0 : c).Sum();
		}
	}

	/// <summary>
	/// Updates the anchorage.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="pos">Position.</param>
	/// <param name="capacity">Capacity.</param>
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