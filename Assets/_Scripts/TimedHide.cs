using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TimedHide : MonoBehaviour {

	private float m_hideTime;
	private MeshRenderer m_meshRenderer;

	// Use this for initialization
	void Start () {
		m_hideTime = 0.0f;
		m_meshRenderer = GetComponent<MeshRenderer>();
		Assert.IsTrue(m_meshRenderer != null);
		m_meshRenderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(m_hideTime > 0.0f) {
			m_hideTime -= Time.deltaTime;
			if(m_hideTime <= 0.0f) {
				m_hideTime = 0.0f;
				m_meshRenderer.enabled = false;
			}
		}
	}

	public void ShowAndHide(float hide_after) {
		//if(m_hideTime > 0.0f) {
	//		Debug.Log("Called ShowAndHide() before hide_after reached zero. Are you sure? Feels like an error.");
	//	}
		m_meshRenderer.enabled = true;
		m_hideTime = hide_after;
	}
}
