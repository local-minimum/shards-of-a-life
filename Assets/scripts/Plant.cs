using UnityEngine;
using System.Collections;

public class Plant : ObjectGenerator {

	/// <summary>
	/// Maximum fewer number of children in child compared to current
	/// </summary>
	public int childAnchorLossMax = 4;

	/// <summary>
	/// Minimum fewer number of children in child compared to corrent
	/// </summary>
	public int childAnchorLossMin = 2;

	/// <summary>
	/// Current number of childrens as target
	/// </summary>
	private int _anchorPositions = 0;

	/// <summary>
	/// Root base width
	/// </summary>
	public float rootBaseWidth = 1f;

	/// <summary>
	/// The root base width variation.
	/// </summary>
	public float rootBaseWidthVariation = 0.2f;

	/// <summary>
	/// Segment height
	/// </summary>
	public float segmentHeight = 5f;

	/// <summary>
	/// The segment roughness frequency.
	/// </summary>
	[Range(1,10)]
	public float segmentRoughnessFrequency = 1.5f;

	/// <summary>
	/// The segment roughness.
	/// </summary>
	[Range(0, 0.5f)]
	public float segmentRoughness = 0.4f;

	[Range(0, 0.45f)]
	public float segmentParallelRoughnessVariation = 0.25f;

	/// <summary>
	/// Minimum number of children for root segment
	/// </summary>
	public int rootAnchorsMin = 6;

	/// <summary>
	/// Targeted maxiumum number of children for root segment
	/// </summary>
	public int rootAnchorsMax = 8;

	/// <summary>
	/// The leaf min length relative to base 
	/// </summary>
	public float leafLengthVariationMin = 0.9f;

	/// <summary>
	/// Leaf length max relative to base width
	/// </summary>
	public float leafLengthVariationMax = 1.5f;

	/// <summary>
	/// Offset along base axis relative to foundation that leaf may have
	/// </summary>
	public float leafBaseOffsetVariation = 0.05f;

	/// <summary>
	/// Horisontal offset relative to base width end can have.
	/// </summary>
	public float segmentTopHorisontalVar = 0.15f;

	/// <summary>
	/// Variation relative to expected length current segments extremes can have
	/// </summary>
	public float segmentLengthVariation = 0.1f;

	/// <summary>
	/// Minimum relative length child segment can have.
	/// </summary>
	public float childLengthLossMin = 0.4f;

	/// <summary>
	/// Maximum relative length child segment can have
	/// </summary>
	public float childLengthLossMax = 0.8f;

	/// <summary>
	/// Probability that next segment will have only leaf
	/// </summary>
	public float nonRootOnlyLeafP = 0.3f;

	/// <summary>
	/// How much downward child segment is allowed to have (-1 => only vertical up, 1 => allow vertical down)
	/// </summary>
	[Range(-1,1)]
	public float growthDownTolerance = 0.8f;

	/// <summary>
	/// The relative maximum allowed for child segment's base compared to current base
	/// </summary>
	[Range(0, 2)]
	public float growthChildBaseTolerance = 0.8f;

	/// <summary>
	/// The outer vertices (no duplicates) in clockwise order.
	/// </summary>
	private Vector3[] V;

	// This is just temporary for debuging
	void Update () {
		if (debug && Input.GetKeyDown(KeyCode.P) && !foundation) {
			Build(Random.Range(rootAnchorsMin, rootAnchorsMax));
		}

		if (debug && !foundation) {
			/*
			Debug.DrawLine(transform.position + _vertices[0], 
			               transform.position + _vertices[2]);

			Debug.DrawLine(transform.position,
			               transform.position + baseDirection, Color.blue);
*/
		}
	}

	/// <summary>
	/// Checks if
	/// <list type="enumerate">
	/// <item>Current allows for children (is leaf or not)</item>
	/// <item>Proability test based on relative distance of inputs to length of segments (further away, more probable)</item>
	/// <item>Not all positions used up, else probability of overshooting decaying with overshoot</item>
	/// <item>If distance between points is less than a set fraction of the base</item>
	/// <item>If the angle of poduced by the points is not aiming too far down</item>
	/// </list>
	/// </summary>
	/// <returns><c>true</c>, if <c>A<c/> and <c>B</c> fulfill critera, <c>false</c> otherwise.</returns>
	/// <param name="A">First vector in V</param>
	/// <param name="B">Second vector in V</param>
	private bool _TestAddAnchor(Vector3 A, Vector3 B) {
		/*
		Debug.Log("===");
		Debug.Log((A-B).magnitude);
		Debug.Log(baseDirection.magnitude);*/
		return (_anchorPositions > 0 &&
		        Random.value < Vector3.Lerp(A, B, 0.5f).magnitude / segmentHeight * 0.75f &&
		        (superStructureSlots < _anchorPositions ? 
		        	true : Random.value / (superStructureSlots - _anchorPositions) < 0.5f) &&
		        (A - B).magnitude < baseDirection.magnitude * growthChildBaseTolerance &&
		        Vector3.Dot((A - B).normalized, Vector3.right) < growthDownTolerance);
	}

	/// <summary>
	/// Generates the leaf vertices (rectangle)
	/// </summary>
	protected void GenerateLeafVertices() {
		V = new Vector3[4];
		Vector3 B = baseDirection * Random.Range(leafLengthVariationMin, leafLengthVariationMax);
		Vector3 O = Vector3.Cross(B, Vector3.forward) * Random.Range(leafLengthVariationMin, 
		                                                             leafLengthVariationMax);
		V[1] = B * Random.Range(-leafBaseOffsetVariation, leafBaseOffsetVariation);
		V[0] = V[1] - B;
		V[2] = V[0] + O;
		V[3] = V[1] + O;
		_vertices.Add(V[0]);
		_vertices.Add(V[1]);
		_vertices.Add(V[2]);
		_vertices.Add(V[1]);
		_vertices.Add(V[3]);
		_vertices.Add(V[2]);
	}

	/// <summary>
	/// Generates the segment vertices.
	/// This is basically a rectangle with its far end horisontally skewed and the length axis slightly
	/// rotated upwards.
	/// Vertices are put between the base and the far end that deviate orthogonally from the <c>Vector3.Lerp</c> between
	/// the two.
	/// All outward facing triangle parts are evaluated for potential extensions of the tree.
	/// </summary>
	protected void GenerateSegmentVertices() {

		Vector3 B = baseDirection;
		Vector3 O = Vector3.Cross(B, Vector3.forward * (foundation ? 1:-1));
		
		
		Vector3 upTarget = Vector3.RotateTowards(O,
		                                         (Vector3.up + Vector3.right * Random.Range(-0.1f, 0.1f)).normalized,
		                                         Mathf.PI * 1f/6f, 0f).normalized * segmentHeight;
		
		float lateralSteps = segmentRoughnessFrequency * upTarget.magnitude / B.magnitude + 1f;

		V = new Vector3[4 + 2 * (int) lateralSteps];
		int N = V.Length;
		int T = N - 2;
		if (foundation) {
			Vector3[] anchor = anchorage;
			V[0] = anchor[0] - anchor[1];
			V[1] = Vector3.zero;
		} else {
			V[1] = Vector3.zero;
			V[0] = Vector3.right * rootBaseWidth * Random.Range(1f-rootBaseWidthVariation, 1f+rootBaseWidthVariation);
			//baseDirection = V[0];
		}
		
		for (int i=0; i<2; i++) {
			V[N / 2 + 1 - i] = V[i] + B * Random.Range(-segmentTopHorisontalVar, segmentTopHorisontalVar)
				+ upTarget * Random.Range(1f - segmentLengthVariation,
				                          1f + segmentLengthVariation); 
		}
		
		for (int i=1; i<lateralSteps; i++) {
			V[1 + i] = Vector3.Lerp(V[1], V[N / 2], 
			                        ((float) i + 1f) / (lateralSteps + 3f) + 
			                        1f / (lateralSteps + 2f) * Random.Range(-segmentRoughness, segmentRoughness)) +
				B * Random.Range(-segmentParallelRoughnessVariation, segmentParallelRoughnessVariation);
			V[N - i] = Vector3.Lerp(V[0], V[N/2 + 1], 
			                        ((float) i + 1f) / (lateralSteps + 3f) + 
			                        1f / (lateralSteps + 2f) * Random.Range(-segmentRoughness, segmentRoughness)) +
				B * Random.Range(-segmentParallelRoughnessVariation, segmentParallelRoughnessVariation);
			
		}
		
		_vertices.Add(V[1]);
		_vertices.Add(V[1+1]);
		_vertices.Add(V[N-N]);
		
		for (int i=1; i<T; i++) {
			if (i % 2 == 1) {
				_vertices.Add(_vertices[_vertices.Count - 2]);
				_vertices.Add(V[N - 1 - i / 2]);
				_vertices.Add(_vertices[_vertices.Count - 3]);
				if (_TestAddAnchor(_vertices[_vertices.Count - 2], _vertices[_vertices.Count -1]))
					addAnchorage(new int[] {_vertices.Count - 1, _vertices.Count - 2}, 1);
				if (i == N - 3 && 
				    _TestAddAnchor(_vertices[_vertices.Count - 3], _vertices[_vertices.Count - 2])) {
					addAnchorage(new int[] {_vertices.Count - 2, _vertices.Count - 3}, 1);
				}
			} else {
				_vertices.Add(_vertices[_vertices.Count - 3]);
				_vertices.Add(V[2 + i / 2]);
				_vertices.Add(_vertices[_vertices.Count - 4]);
				if (_TestAddAnchor(_vertices[_vertices.Count - 3], _vertices[_vertices.Count - 2]))
					addAnchorage(new int[] {_vertices.Count - 2, _vertices.Count - 3}, 1);
				
			}
			
		}
	}

	/// <summary>
	/// Generates the vertices (either as a leaf or as a segment/branch/stem.
	/// </summary>
	protected override void GenerateVertices ()
	{
		_vertices.Clear();

		if (_anchorPositions < 1) {

			GenerateLeafVertices();

		} else {

			GenerateSegmentVertices();

		}
	}

	/// <summary>
	/// Creates the suitable mesh for the segment and a set of child game objects for the deeper
	/// parts of the tree.
	/// </summary>
	public void Build() {
	
		removeAllSuperStructureSlots();
		base.Build();

		renderer.material.color = _anchorPositions == 0 ? interactableColor : segmentColor;
		Vector3 wp = transform.position;
		if (_anchorPositions == 0)
			wp.z = 0f;
		else
			wp.z = .1f;
		transform.position = wp;

		if (_anchorPositions > 0) {
			while (superStructureSlotsFree > 0) {
				//Debug.Log(superStructureSlotsFree);
				GameObject go = new GameObject();
				Plant p = go.AddComponent<Plant>();
				go.transform.parent = transform;
				p.foundation = this;
				updateChild(p);
				p.Start();
				go.name = p.getName();
				p.Build();
			}
		}
	}

	/// <summary>
	/// Creates a name suggestions for the node in the tree based on its depth and capacity for children
	/// </summary>
	/// <returns>The name.</returns>
	public string getName() {
		return _anchorPositions > 0 ? 
			string.Format("Branch Lvl {0} Capacity {1}", segmentLevel, _anchorPositions) :
				string.Format("Leaf Lvl {0}", segmentLevel);

	}

	/// <summary>
	/// Convinience method for building but setting new number of targeted children first
	/// </summary>
	/// <param name="anchorPositions">Anchor positions.</param>
	public void Build(int anchorPositions) {
		_anchorPositions  = anchorPositions;
		Build();
	}

	/// <summary>
	/// Updates the child parameters based on current's parameters and decay settings.
	/// </summary>
	/// <param name="child">Child.</param>
	private void updateChild(Plant child) {
		if (foundation != null && Random.value < nonRootOnlyLeafP) {
			child._anchorPositions = 0;
		} else {
			child._anchorPositions = _anchorPositions - Random.Range(childAnchorLossMin, 
			                                                         childAnchorLossMax);
			child._anchorPositions = child._anchorPositions < 0 ? 0 : child._anchorPositions;
		}
		child.segmentHeight = segmentHeight * Random.Range(childLengthLossMax, childLengthLossMin);
	}
}
