using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor( typeof( GroundMaker ) )]
public class GroundMakerEditor : Editor {
	
	
	GroundMaker m_Instance;
	//PropertyField[] m_fields;
	
	
	public void OnEnable()
	{
		m_Instance = target as GroundMaker;
		//m_fields = ExposeProperties.GetProperties( m_Instance );
	}
	
	public override void OnInspectorGUI () {
		
		if ( m_Instance == null )
			return;
		
		this.DrawDefaultInspector();
		
		//ExposeProperties.Expose( m_fields );
		GroundMaker.TerrainType t = (GroundMaker.TerrainType) EditorGUILayout.EnumPopup("Mode", m_Instance.terrain);
		if (t != m_Instance.terrain)
			m_Instance.terrain = t;
	}
}