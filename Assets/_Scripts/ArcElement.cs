using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcElement : MonoBehaviour {

	public float arcAngle = 0.0f; // 0.0: none, 180.0: half, 360.0: all
	public int faceCount = 36;
	public float radius = 3.0f;

	private MeshFilter m_meshFilter;
	
	void Awake() {
		m_meshFilter = GetComponent<MeshFilter>();
		transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f); // This is to start at 12 oclock, me avoiding angle juggling :P
	}
	// Use this for initialization
	void Start () {
		GenMesh();
	}

	void GenMesh() {
		Mesh mesh = new Mesh();
		mesh.MarkDynamic();
		m_meshFilter.mesh = mesh;
		
		int verts_count = faceCount + 2;
		Vector3[] verts = new Vector3 [verts_count];
		Vector2[] uv = new Vector2[verts_count];
		float angle_step = 360.0f / (float)faceCount;
		// First verts is the center
		verts[0] = new Vector3(0.0f, 0.0f, 0.0f); 
		uv[0] = new Vector2(0.5f, 0.5f);
		// Last vert is only used during arc changes
		verts[verts_count - 1] = new Vector3(0.0f, 0.0f, 0.0f);
		uv[verts_count - 1] = new Vector2(0.5f, 0.5f);
		float angle = 0.0f;
		for(int i = 0; i < faceCount; ++i) {
			float ax = -Mathf.Cos(angle * Mathf.Deg2Rad);
			float ay = Mathf.Sin(angle * Mathf.Deg2Rad);
			verts[i + 1] = new Vector3(radius * ax, radius * ay, 0.0f);
			uv[i + 1] = new Vector2((ax * 0.5f) + 0.5f, (ay * 0.5f) + 0.5f);
			angle += angle_step;
		}
		mesh.vertices = verts;
		mesh.uv = uv;

		int[] tris = new int[(faceCount + 1) * 3];
		// Last tri only used for arc changes
		tris[(faceCount * 3)] = 0;
		tris[(faceCount * 3) + 1] = 0;
		tris[(faceCount * 3) + 2] = 0;
		// Circle tris
		for(int i = 0; i < faceCount; ++i) {
			tris[(i * 3) + 0] = 0; // center vertex
			tris[(i * 3) + 1] = i + 1;
			if(i < faceCount - 1) {
				tris[(i * 3) + 2] = i + 2;
			} else {
				tris[(i * 3) + 2] = 1; // First vertex again
			}
		}
		mesh.triangles = tris;

		Vector3[] norms = new Vector3 [verts_count];
		for(int i = 0; i < verts_count; ++i) {
			norms[i] = -Vector3.forward;
		}
		mesh.normals = norms;
	}

	void UpdateArc() {
		// Enable rendering of all pies up to arc angle, disable the rest using tris
		Mesh mesh = m_meshFilter.mesh;
		int[] tris = mesh.triangles;
		int verts_count = faceCount + 2;
		float angle = 0.0f;
		float angle_step = 360.0f / (float)faceCount;
		int last_face = -1;
		for(int i = 0; i < faceCount; ++i) {
			if(angle + angle_step <= arcAngle) {
				// Enable
				tris[(i * 3) + 0] = 0; // center vertex
				tris[(i * 3) + 1] = i + 1;
				if(i < faceCount - 1) {
					tris[(i * 3) + 2] = i + 2;
				} else {
					tris[(i * 3) + 2] = 1; // First vertex again
				}
				last_face = i;
			} else {
				// Disable
				tris[(i * 3) + 0] = 0;
				tris[(i * 3) + 1] = 0;
				tris[(i * 3) + 2] = 0;
			}
			angle += angle_step;
		}

		// Calculate arc vertex (extra trailing vertex)
		float ax = -Mathf.Cos(arcAngle * Mathf.Deg2Rad);
		float ay = Mathf.Sin(arcAngle * Mathf.Deg2Rad);
		Vector3 arc_vert = new Vector3(radius * ax, radius * ay, 0.0f);
		Vector2 arc_uv = new Vector2((ax * 0.5f) + 0.5f, (ay * 0.5f) + 0.5f);

		Vector3[] verts = mesh.vertices;
		Vector2[] uvs = mesh.uv;
		verts[verts_count - 1] = arc_vert;
		uvs[verts_count - 1] = arc_uv;
		mesh.vertices = verts;
		mesh.uv = uvs;
		//mesh.vertices[verts_count - 1] = arc_vert;
		//mesh.uv[verts_count - 1] = arc_uv;

		// Use last pie vertex in combination with arc vertex to generate last triangle
		if(last_face > -1) {
			tris[(faceCount * 3)] = 0;
			tris[(faceCount * 3) + 2] = verts_count - 1;
			tris[(faceCount * 3) + 1] = last_face + 2;
		} else {
			tris[(faceCount * 3)] = 0;
			tris[(faceCount * 3) + 2] = 0;
			tris[(faceCount * 3) + 1] = 0;
		}

		// Update triangles
		mesh.triangles = tris;
	}
	
	// Update is called once per frame
	void Update () {
		arcAngle += Time.deltaTime * 90.0f;
		while(arcAngle > 360.0f) {
			arcAngle -= 360.0f;
		}
		UpdateArc();
	}
}
