using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Mesher : MonoBehaviour {

	/// <summary>
	/// The mesh vertices (used to create mech and report back child's base).
	/// </summary>
	protected List<Vector3> _vertices = new List<Vector3>();
	
	/// <summary>
	/// The uv scaling for the main material
	/// </summary>
	public Vector2 uvScale = Vector2.one;
	/// <summary>
	/// Generates the tris for the mesh
	/// </summary>
	/// <returns>The tris.</returns>
	protected int[] GenerateTris() {
		return Enumerable.Range(0, _vertices.Count).ToArray();
	}
	
	/// <summary>
	/// Generates the uvs for the mesh.
	/// </summary>
	/// <returns>The uv.</returns>
	protected Vector2[] GenerateUv() {
		Vector2[] uvs = new Vector2[_vertices.Count];
		for (int i=0; i < uvs.Length; i++) {
			uvs[i].x = _vertices[i].x * uvScale.x;
			uvs[i].y = _vertices[i].y * uvScale.y;
		}
		return uvs;
	}
}
