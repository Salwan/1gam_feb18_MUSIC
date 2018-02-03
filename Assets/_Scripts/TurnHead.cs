using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnHead : MonoBehaviour {

	public float delay {
		get { return m_delay; }
		set {
			m_delay = value;
		}
	}

	public float speed {
		get { return m_speed; }
		set { 
			m_speed = value;
		}
	}

	private float m_angle = 0.0f;
	private float m_delay = 0.0f;
	private float m_speed = 0.0f;
	private bool m_update = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(m_delay > 0.0f) {
			m_delay -= Time.deltaTime;
		} else {
			if(m_angle > -360.0f) {
				m_angle -= Time.deltaTime * m_speed;
			} else {
				m_angle = -360.0f;
			}
			transform.localRotation = Quaternion.Euler(0.0f, 0.0f, m_angle);
		}
	}
}
