using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Mesher : MonoBehaviour {

	/// <summary>
	/// The mesh vertices (used to create mech and report back child's base).
	/// </summary>
	public List<Vector3> vertices = new List<Vector3>();
	
	/// <summary>
	/// The uv scaling for the main material
	/// </summary>
	public Vector2 uvScale = Vector2.one;

	public bool interactable = true;

	/// <summary>
	/// Generates the tris for the mesh
	/// </summary>
	/// <returns>The tris.</returns>
	public int[] GenerateTris() {
		return Enumerable.Range(0, vertices.Count).ToArray();
	}
	
	/// <summary>
	/// Generates the uvs for the mesh.
	/// </summary>
	/// <returns>The uv.</returns>
	public Vector2[] GenerateUv() {
		Vector2[] uvs = new Vector2[vertices.Count];
		for (int i=0; i < uvs.Length; i++) {
			uvs[i].x = vertices[i].x * uvScale.x;
			uvs[i].y = vertices[i].y * uvScale.y;
		}
		return uvs;
	}

	/*Should be good but no need for it now
	public IEnumerable<Vector3[]> meshInTris {
		get {
			for (int i=0, l=_vertices.Count; i<l;i+=3)
				yield return _vertices.GetRange(i, 3).ToArray();
		}
	}*/
}
