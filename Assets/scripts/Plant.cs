using UnityEngine;
using System.Collections;

public class Plant : ObjectGenerator {
	public int childAnchorLossMax = 4;
	public int childAnchorLossMin = 2;
	private int _anchorPositions = 0;

	public float rootBaseWidth = 1f;
	public float segmentHeight = 5f;

	public int rootAnchorsMin = 6;
	public int rootAnchorsMax = 8;

	public float leafSizeVariationMin = 0.9f;
	public float leafSizeVariationMax = 1.5f;
	public float leafBaseOffsetVariation = 0.05f;

	public float segmentTopHVariation = 0.15f;
	public float segmentHeightVariation = 0.1f;

	public float childHeightLossMin = 0.4f;
	public float childHeightLossMax = 0.8f;


	private Vector3[] V;

	// Update is called once per frame
	void Update () {
		if (debug && Input.GetKeyDown(KeyCode.P) && !foundation) {
			Build(Random.Range(rootAnchorsMin, rootAnchorsMax));
		}
		/*
		if (debug & !foundation) {
			for (int i=0;i<_superStructureSlots.Count;i++)
				Debug.DrawLine(_vertices[_superStructureSlots[i][0]], Vector3.Cross(_vertices[_superStructureSlots[i][1]] - _vertices[_superStructureSlots[i][0]], Vector3.forward *-1).normalized + _vertices[_superStructureSlots[i][0]]);
		}*/
	}

	private bool _TestAddAnchor(Vector3 A, Vector3 B) {
		/*
		Debug.Log("===");
		Debug.Log((A-B).magnitude);
		Debug.Log(baseDirection.magnitude);*/
		return (_anchorPositions > 0 & Random.value < 0.8f &
		        (superStructureSlots <= _anchorPositions ? 
		        	true : Random.value / (superStructureSlots - _anchorPositions) < 0.25f) &
		        (A - B).magnitude < baseDirection.magnitude * 1.1f &
		        Vector3.Dot((A - B).normalized, Vector3.right) < 0.8f);
	}

	protected override void GenerateVertices ()
	{

		int T = _anchorPositions > 0 ? _anchorPositions * 2 : 2;
		_vertices.Clear();
//		Debug.Log(_anchorPositions);
		if (_anchorPositions < 1) {
			V = new Vector3[4];
			int N = V.Length;
			Vector3 B = baseDirection * Random.Range(leafSizeVariationMin, leafSizeVariationMax);
			Vector3 O = Vector3.Cross(B, Vector3.forward) * Random.Range(leafSizeVariationMin, 
			                                                                  leafSizeVariationMax);
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

		} else {

	//		Debug.Log(N);
			Vector3 B = baseDirection;
			Vector3 O = Vector3.Cross(B, Vector3.forward * (foundation ? 1:-1));
			
			
			Vector3 upTarget = Vector3.RotateTowards(O,
			                                         (Vector3.up + Vector3.right * Random.Range(-0.1f, 0.1f)).normalized,
			                                         Mathf.PI * 1f/6f, 0f).normalized * segmentHeight;
			
			float lateralSteps = 1.5f * upTarget.magnitude / B.magnitude + 1f;// (float) N / 2 - 3;
			
			V = new Vector3[4 + 2 * (int) lateralSteps];
			int N = V.Length;

			if (foundation) {
				V[0] = anchorage[1] - anchorage[0];
				V[1] = Vector3.zero;
			} else {
				V[1] = Vector3.zero;
				V[0] = Vector3.right * rootBaseWidth * Random.Range(0.8f, 1.2f);
				baseDirection = V[0];
			}

			for (int i=0; i<2; i++) {
				V[N / 2 + 1 - i] = V[i] + B * Random.Range(-segmentTopHVariation, segmentTopHVariation)
				                                    + upTarget * Random.Range(1f - segmentHeightVariation,
					                          1f + segmentHeightVariation); 
			}

			for (int i=1; i<lateralSteps; i++) {
				V[1 + i] = Vector3.Lerp(V[1], V[N / 2], 
				                               ((float) i + 1f) / (lateralSteps + 3f) + 
				                               1f / (lateralSteps + 2f) * Random.Range(-0.4f, 0.4f)) +
					B * Random.Range(-0.25f, 0.25f);
				V[N - i] = Vector3.Lerp(V[0], V[N/2 + 1], 
				                               ((float) i + 1f) / (lateralSteps + 3f) + 
				                               1f / (lateralSteps + 2f) * Random.Range(-0.4f, 0.4f)) +
					B * Random.Range(-0.25f, 0.25f);

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
						addAnchorage(new int[] {_vertices.Count - 2, _vertices.Count - 1}, 1);
					if (i == N - 3 & 
					    _TestAddAnchor(_vertices[_vertices.Count - 3], _vertices[_vertices.Count - 2])) {
						addAnchorage(new int[] {_vertices.Count - 3, _vertices.Count - 2}, 1);
					}
				} else {
					_vertices.Add(_vertices[_vertices.Count - 3]);
					_vertices.Add(V[2 + i / 2]);
					_vertices.Add(_vertices[_vertices.Count - 4]);
					if (_TestAddAnchor(_vertices[_vertices.Count - 3], _vertices[_vertices.Count - 2]))
						addAnchorage(new int[] {_vertices.Count - 3, _vertices.Count - 2}, 1);

				}
				                    
			}

		}
	}

	public void Build() {
	
		removeAllSuperStructureSlots();
		base.Build();

		renderer.material.color = _anchorPositions == 0 ? Color.green : Color.grey;
		Vector3 wp = transform.position;
		if (_anchorPositions == 0)
			wp.z = 0f;
		else
			wp.z = 1f;
		transform.position = wp;

		/*
		if (_anchorPositions > 0) 
			destroyAllSuperstructures();
		else {
			while (superStructureSlots > _anchorPositions)
				removeLastSuperStructureSlot();
			for(int i=0; i<_anchorPositions; i++) {
				int childMinA = _anchorPositions - Random.Range(0, segmentAnchorLossMax);
				int[] A = new int[] {i * 2, i * 2 + 1}; //TODO: Is this OK?
				int newCapacity = Random.Range(childMinA < 0 ? 0 : childMinA, _anchorPositions);
				if (i < superStructureSlots)
					addAnchorage(A, newCapacity);
				else 
					updateAnchorage(i, A, newCapacity);

			}
		}
		*/
		/*

		//Updating existing children
		foreach (ObjectGenerator o in superStructures) {
			updateChild((Plant) o);
			o.Build();
		}
		*/
		//Populating lacking children
		int i = 0;
		while (_anchorPositions > 0 & superStructureSlotsFree > 0) {
			i++;
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
		/*
		Debug.Log("--");
		Debug.Log(_superStructureSlots.Count);
		Debug.Log(_anchorPositions);
		Debug.Log(superStructureSlotsFree);
		*/
	}

	public string getName() {
		return _anchorPositions > 0 ? 
			string.Format("Branch Lvl {0} Capacity {1}", segmentLevel, _anchorPositions) :
				string.Format("Leaf Lvl {0}", segmentLevel);

	}

	public void Build(int anchorPositions) {
		_anchorPositions  = anchorPositions;
		Build();
	}

	private void updateChild(Plant child) {
		child._anchorPositions = _anchorPositions - Random.Range(childAnchorLossMin, 
		                                                         childAnchorLossMax);
		child._anchorPositions = child._anchorPositions < 0 ? 0 : child._anchorPositions;
		child.segmentHeight = segmentHeight * Random.Range(childHeightLossMax, childHeightLossMin);
	}
}
