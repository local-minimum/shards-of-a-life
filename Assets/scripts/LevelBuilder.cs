using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class LevelBuilder : MonoBehaviour {

	public enum BuildOrder {X, X_REV, Y, Y_REV, Z, Z_REV, EDITOR, RANDOM};

//	public string buildTag = "Templater";
	public GameObject BuildTemplateLvl;
	public GameObject BuildTemplatePl;
	public GameObject[] players;

	public bool hideTemplate = true;

	public bool collisionDress = true;
	public List<float> collisionScope = new List<float>();

	public BuildOrder buildOrder = BuildOrder.X;

	public static float interPieceDelay = 0.05f;
	public static float interOrderDelay = 0.1f;
	public bool debug = false;
	
	[Range(1f, 10f)]
	public static float fillFactor = 6.5f;

	[Range(0f, 10000f)]
	public static float initialShake = 30f;

	[Range(0.01f, 1f)]
	public static float templateLowerScale = 0.25f;

	[Range(1f, 10f)]
	public static float templateUpperScale = 1.5f;

	[Range(0f, 1f)]
	public static float templateColorVariation = 0.2f;

	public bool renderInEditor {
		get {
			return _renderInEditor;
		}

		set {
			_renderInEditor = value;
		}
	}

	public bool ready {
		get {
			return _ready;
		}
	}

	[SerializeField]
	private bool _renderInEditor = false;

	private bool _ready = false;

	private List<Mesher> _templates = new List<Mesher>();
	private bool[] _templateReady;

	public List<GUIText> introTexts = new List<GUIText>();

	private GroundStitcher _groundStitcher;
	private GroundDecorator _groundDecorator;

	// Use this for initialization
	void Awake () {
		_groundStitcher = gameObject.GetComponent<GroundStitcher>();
		_groundDecorator = gameObject.GetComponent<GroundDecorator>();
	}

	private void _SetTemplates() {
//		_templates.AddRange(GameObject.FindGameObjectsWithTag(buildTag));
		foreach (GameObject t in GameObject.FindGameObjectsWithTag("TemplateLevel"))
			Destroy(t.gameObject);

		_templates.Clear();
		_templates.AddRange(_groundStitcher.interactables);
		_templates.AddRange(_groundDecorator.interactables);

		_templateReady = new bool[_templates.Count];

		foreach (Mesher m in _templates)
			m.renderer.enabled = false;

		if (debug)
			Debug.Log (string.Format("Found {0} interactables", _templates.Count));

		switch (buildOrder) {
		case BuildOrder.RANDOM:
			for (int i = 0; i < _templates.Count; i++) {
				Mesher m = _templates[i];
				int randomIndex = Random.Range(i, _templates.Count);
				_templates[i] = _templates[randomIndex];
				_templates[randomIndex] = m;
			}
			break;
		case BuildOrder.X:
			_templates.Sort((A, B) => A.transform.position.x.CompareTo(B.transform.position.x));
			break;
		case BuildOrder.X_REV:
			_templates.Sort((A, B) => B.transform.position.x.CompareTo(A.transform.position.x));
			break;
		case BuildOrder.Y:
			_templates.Sort((A, B) => A.transform.position.y.CompareTo(B.transform.position.y));
			break;
		case BuildOrder.Y_REV:
			_templates.Sort((A, B) => B.transform.position.y.CompareTo(A.transform.position.y));
			break;
		case BuildOrder.Z:
			_templates.Sort((A, B) => A.transform.position.z.CompareTo(B.transform.position.z));
			break;
		case BuildOrder.Z_REV:
			_templates.Sort((A, B) => B.transform.position.z.CompareTo(A.transform.position.z));
			break;
		default:
			break;
		}
	}

	static bool InTriangle(Vector3 v, Vector3 v1, Vector3 v2, Vector3 v12) {
		float a = -(Vector3.Cross(v, v1).z / v12.z);
		float b = (Vector3.Cross(v, v2).z / v12.z);
		return (a > 0 && b > 0 && a + b < 1);
	}

	static bool InTriangle(Vector3 v, Vector3 v1, Vector3 v2) {
		return LevelBuilder.InTriangle(v, v1, v2, Vector3.Cross(v1, v2));
	}

	static Vector3 RndTriPt(Vector3 v1, Vector3 v2) {
		float a1 = Random.value;
		float a2 = Random.value;
		Vector3 pt = a1 * v1 + a2 * v2;
		if (!InTriangle(pt, v1, v2)) {
			pt = (pt - (v1 + v2)) * -1f;
		}
		return pt;
	}

	private IEnumerator<WaitForSeconds> DressTemplate(Mesher template, int idT) {
		_templateReady[idT] = false;
		IEnumerator<WaitForSeconds> ie = DressTemplate(template, 1f);
		while (ie.MoveNext()) {
			yield return ie.Current;
		}
		_templateReady[idT] = true;
	}

	public IEnumerator<WaitForSeconds> DressTemplate(Mesher template, float globalScale) {
		
		float scaler = BuildTemplateLvl.GetComponent<CircleCollider2D>().radius * 2f;
		Mesh m = template.GetComponent<MeshFilter>().mesh;
		bool nonCollidable = template.gameObject.layer == LayerMask.NameToLayer("Non-collidables");

		if (debug)
			Debug.Log (string.Format("Dressing {0}", template, scaler));
		
		if (!m) {
			Debug.LogError(string.Format("Template {0} lacks mesh", template));

		} else {
			float Aref = scaler * BuildTemplateLvl.transform.lossyScale.magnitude;
			//Debug.Log(string.Format("Builder {0}x{1} scale {2}", refV.x, refV.y, BuildTemplate.transform.lossyScale.magnitude));
			List<Vector3> verts = template.vertices;
			int[] tris = template.GenerateTris();
			bool[] allDone = new bool[tris.Count() / 3];
			
			if (!template.rigidbody2D) {
				template.gameObject.AddComponent<Rigidbody2D>();
				template.rigidbody2D.isKinematic = true;
			}
			
			
			for (int i=0, l = tris.Length; i<l; i+=3) {
				StartCoroutine(DressTri(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]], template.gameObject, Aref, nonCollidable, allDone, i/3, globalScale, BuildTemplateLvl));
			}
			
			while (allDone.Any(e => !e))
				yield return new WaitForSeconds(interOrderDelay);
		}
	}

	public static IEnumerator<WaitForSeconds> DressTri(Vector3 v0, Vector3 v1, Vector3 v2, GameObject template,
	                                              float Aref, bool nonCollidable, bool[] isDone, int idDone, float globalScale, GameObject buildTemplate) {
		v1 -= v0;
		v2 -= v0;
		Vector3 v12 = Vector3.Cross(v1, v2);
		//Area of the quadrilateral
		float A2 = v12.magnitude * template.transform.lossyScale.magnitude;
		//Debug.Log(string.Format("Tris-size {0}, object scale {1}", v12.magnitude, template.transform.lossyScale.magnitude));
		//Random create points
		//if (debug)
		//	Debug.Log (string.Format("Dressing {0}: tri {1} will around get {2} items", template, i/3, Mathf.CeilToInt(Aref/A2*fillFactor)));
		
		int y = -1;
		for (int j=0, l2=Mathf.CeilToInt(A2/Aref*fillFactor); j<l2; j++) {
			if (y < 0)
				y = interPieceDelay < Time.deltaTime ? Mathf.RoundToInt(Time.deltaTime / interPieceDelay) : 1;
			
			//CREATE OBJECT & PLACE
			//TODO: Rnd and place low prob piece of player
			GameObject o = (GameObject) Instantiate(buildTemplate, Vector3.zero, template.transform.rotation);
			
			//SCALE
			float scale = Random.Range(templateLowerScale, templateUpperScale) * globalScale;
			o.transform.localScale = new Vector3(scale * o.transform.localScale.x, scale * o.transform.localScale.y, 1f);
			
			//POSITION
			o.transform.parent = template.transform;
			o.transform.localPosition = RndTriPt(v1, v2) + v0;

			
			//JOINT
			SpringJoint2D j2d = o.GetComponent<SpringJoint2D>();
			if (j2d) {
				j2d.anchor = Vector2.zero;
				j2d.connectedAnchor = new Vector2(o.transform.localPosition.x, o.transform.localPosition.y);
				j2d.connectedBody = template.rigidbody2D;
			}

			//PHYSICS
			if (nonCollidable) {
				o.layer = LayerMask.NameToLayer("Non-collidables");
			} else {
				o.layer = LayerMask.NameToLayer("Colliders");
			}
			
			//MATERIAL
			o.renderer.material.color = template.renderer.material.color +
				Color.white * template.renderer.material.color.grayscale * 
					Random.Range(-templateColorVariation, templateColorVariation);
//			o.renderer.material.SetColor("_Emission", template.renderer.material.color);
			
			//SHAKE
			if (o.rigidbody)
				o.rigidbody2D.AddForce(new Vector2(Random.value, Random.value) * initialShake);
			
			//NEXT
			if (y == 0)
				yield return new WaitForSeconds(interPieceDelay);
			y--;
		}
		
		isDone[idDone] = true;
	}

	private void _ReadyPlayers() {
		foreach (GameObject pl in players) {
			foreach (MeshFilter m in pl.GetComponentsInChildren<MeshFilter>()) {
				float btRef = 0.5f;
				Mesh mesh = m.mesh;
				bool[] isDone = new bool[mesh.triangles.Count() / 3];

				for (int i=0, l = mesh.triangles.Count(); i<l; i+=3) {
					StartCoroutine(DressTri(mesh.vertices[mesh.triangles[i]], mesh.vertices[mesh.triangles[i + 1]], mesh.vertices[mesh.triangles[i + 2]],
					                        m.gameObject, btRef, true, isDone, i / 3, 0.35f, BuildTemplatePl));
				}
				//TODO: Make something useful
				//StartCoroutine(DressTemplate(t.gameObject, 0.4f));
				//t.rigidbody2D.isKinematic = true;
			}
			if(pl.rigidbody2D)
				pl.rigidbody2D.isKinematic = false;
		}
		foreach (GUIText t in introTexts) {
			iTween.FadeTo(t.gameObject, 0f, 1f);
			Destroy(t, 1f);
		}

	}

	private void _ReadyMobs() {
		foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy")) {
			if (e.rigidbody2D)
				e.rigidbody2D.isKinematic = false;
		}
	}

	private IEnumerator<WaitForSeconds> _DressLevel() {
		float delay = 1f;
		for (int i=0; i<_templates.Count; i++) {
			StartCoroutine(DressTemplate(_templates[i], i));
			if (i < _templates.Count - 1) {
				switch (buildOrder) {
				case BuildOrder.X:
				case BuildOrder.X_REV:
					delay = Mathf.Abs(_templates[i].transform.position.x - _templates[i + 1].transform.position.x);
					break;
				case BuildOrder.Y:
				case BuildOrder.Y_REV:
					delay = Mathf.Abs(_templates[i].transform.position.y - _templates[i + 1].transform.position.y);
					break;
				case BuildOrder.Z:
				case BuildOrder.Z_REV:
					delay = Mathf.Abs(_templates[i].transform.position.z - _templates[i + 1].transform.position.z);
					break;
				default:
					delay = 1f;
					break;
				}
				yield return new WaitForSeconds(delay * interOrderDelay);
			}
		}

		while (_templateReady.Any(e => !e))
			yield return new WaitForSeconds(interPieceDelay);

		_ReadyPlayers();
		_ReadyMobs();
		_ready = true;

	}

	// Update is called once per frame
	public void Start () {
//		if (Input.GetKeyDown(KeyCode.S)) {

		_groundStitcher.Generate();
		_groundDecorator.Decorate();
		_SetTemplates();
		StartCoroutine(_DressLevel());
//		}

	}
}
