using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class House : ObjectGenerator {

	public float lateralNoise = 0.05f;
	public float offsetNoise = 1f;
	public float foundationNoise = .2f;
	public float foundationMin = 2f;
	public float sectionMin = 3f;
	public float sectionMax = 5f;

	public bool isFloorSection = false;
	
	public House roof {
		get {
			if (superStructure)
				return superStructure.roof;
			else
				return this;
		}
	}
	
	public House superStructure {
		get {
			if (superStructures.Count > 0)
				return (House) superStructures[0];
			else
				return null;
		}

	}


	private void _TearDown(int currentHeight, int maxHeight) {
		currentHeight += 1;
		if (superStructure)
			superStructure._TearDown(currentHeight, maxHeight);

		if (currentHeight > maxHeight) {
			if (foundation) 
				foundation.removeSuperStructure(this);
			Destroy(this.gameObject);

		}
	}

	public void setMaxHeight(int maxHeight) {
		_TearDown(0, maxHeight);
	}


	override protected void GenerateVertices ()
	{
		vertices.Clear();
		Vector3 z = Vector3.zero;
		float sectionHeight = Random.Range(sectionMin, sectionMax);
		if (foundation && !((House) foundation).isFloorSection && Random.value < 0.85f) {
			isFloorSection = true;
			sectionHeight = 0.1f * sectionHeight;
			z = Vector3.forward * -1f;	
		} else {
			isFloorSection = false;
			z = foundation != null && ((House) foundation).isFloorSection ? Vector3.forward : Vector3.zero;
		}

		interactable = isFloorSection;

		Vector3 ptLL = z;

		if (foundation)
			ptLL.x += Random.Range(-offsetNoise, offsetNoise);

		Vector3 ptLR = ptLL + baseDirection.normalized * baseWidth;
//		Debug.Log(string.Format("{0}  {1}  {2}", name, segmentLevel, baseWidth));

		if (foundation)
			ptLR.x += Random.Range(-lateralNoise, lateralNoise);

		if (ptLR.x - ptLL.x < foundationMin) {
			Debug.Log(string.Format("{0}  {1}  {2} vs {3}", name, segmentLevel, baseWidth, ptLR.x - ptLL.x));
			ptLR.x = ptLL.x + foundationMin;
		}

		Vector3 ptUL = ptLL + Vector3.up * sectionHeight + Vector3.right * Random.Range(-lateralNoise, lateralNoise) * sectionHeight;
		Vector3 ptUR = ptLR + Vector3.up * (ptUL.y - ptLL.y) +  Vector3.right * Random.Range(-lateralNoise, lateralNoise) * sectionHeight;

		if (ptUR.x - ptUL.x < foundationMin)
			ptUR.x = ptUL.x + foundationMin;

		vertices.Add(ptLL);
		vertices.Add(ptUL);
		vertices.Add(ptUR);
		vertices.Add(ptLL);
		vertices.Add(ptUR);
		vertices.Add(ptLR);


		if (superStructureSlots == 0)
			addAnchorage(new int[] {1, 2}, 1);

	}


	/// <summary>
	/// Build the current segment.
	/// </summary>
	/// <param name="cascade">If set to <c>true</c> cascade to superstructures, else only align it.</param>
	public void Build(bool cascade) {

		base.Build();

		renderer.material.color = isFloorSection ? interactableColor : segmentColor;

		//		Debug.Log(string.Format("Built {0}", this));
		
		if (superStructure) {
			if (cascade) {
				superStructure.Build(cascade);
			} else {
				superStructure.Align();
			}
		}
	}

	/// <summary>
	/// Build this section and all sections supported by it.
	/// </summary>
	public void Build() {
		Build(true);
	}

	/// <summary>
	/// Build section ontop of specified foundation.
	/// </summary>
	/// <param name="foundation">Foundation.</param>
	public void Build(House foundation) {
		this.foundation = foundation;
		Build(true);
	}

	/// <summary>
	/// Build section ontop of specified foundation and cascade building
	/// according to parameter
	/// </summary>
	/// <param name="foundation">Foundation.</param>
	/// <param name="cascade">If set to <c>true</c> cascade to all superstructures.</param>
	public void Build(House foundation, bool cascade) {
		this.foundation = foundation;
		Build(cascade);
	}

	private void _Generate(int currentHeigh, int sections) {
		if (currentHeigh < sections) {
			currentHeigh++;
//			Debug.Log(string.Format("Building level {0}", currentHeigh));
			GameObject go = new GameObject();
			go.name = name;
			go.transform.parent = transform;
			House h = go.AddComponent<House>();
			h.foundation = this;
			h.Start();
			h.Build(false);
			h._Generate(currentHeigh, sections);
		}

	}

	public void Generate(int sections) {
//		Debug.Log("Checking current structure");
		setMaxHeight(sections);
//		Debug.Log("Rebuilding current structure");
		Build();
//		Debug.Log("Adding new structure");
		roof._Generate(segmentsMaxLevel, sections);
	}

	public void Generate(int sections, float width) {
		baseWidth = width;
		Generate(sections);
	}
}
