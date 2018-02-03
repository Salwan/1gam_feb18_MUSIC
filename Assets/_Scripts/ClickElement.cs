using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickElement : MonoBehaviour {

	private enum HitState {
		Perfect,
		Hit,
		Miss,
		Early,
	}

	public GameObject m_turnHeadPrefab;
	public float m_clickAngleOffset; // Detection of clicks starts at 360.0 - m_clickAngleOffset and ends 360 + m_ClickAngleOffset
	public AudioClip m_hitSound;
	public AudioClip m_missSound;

	private List<GameObject> m_turnHeads;
	private ArcElement m_arcElement;
	private float m_arcSpeed = 90.0f;
	private float m_clickAngle = 0.0f;
	private AudioSource m_audioSource;
	private bool m_bDone = false;

	void Awake() {
		m_arcElement = GetComponentInChildren<ArcElement>();
		if(m_arcElement == null) {
			Debug.LogError("Click Element couldn't find Arc Element component in children");
		}
		m_audioSource = GetComponent<AudioSource>();
	}

	// Use this for initialization
	void Start () {
		// Spawn 1 turn head (always)
		m_turnHeads = new List<GameObject>();
		GameObject th = Instantiate(m_turnHeadPrefab, transform.position, Quaternion.identity, transform) as GameObject;
		TurnHead turnhead = th.GetComponent<TurnHead>();
		turnhead.delay = 0.0f;
		turnhead.speed = m_arcSpeed;
		m_turnHeads.Add(th);
	}
	
	// Update is called once per frame
	void Update () {
		if(!m_bDone) {
			m_arcElement.arcAngle += Time.deltaTime * m_arcSpeed;
			if(m_arcElement.arcAngle >= 360.0f - m_clickAngleOffset) {
				m_clickAngle += Time.deltaTime * m_arcSpeed;
				if(m_clickAngle > m_clickAngleOffset * 2.0f) {
					Debug.Log("Angle = " + m_clickAngle.ToString());
					Miss();
				}
			}
		}
	}

	void OnMouseDown() {
		if(!m_bDone) {
			if(m_clickAngle == 0.0f) {
				Miss(true);
			} else if(m_clickAngle >= m_clickAngleOffset * 0.75f && m_clickAngle <= m_clickAngleOffset * 1.25f) {
				Hit(1.0f);
			} else {
				Hit(0.5f);
			}
		}
	}

	void Hit(float accuracy) {
		if(accuracy > 0.5f) {
			Debug.Log("> PERFECT !!");
			EndOfLife(HitState.Perfect);
		} else {
			Debug.Log("> HIT !!"); 
			EndOfLife(HitState.Hit);
		}
		// TEMP: to test restart
		//m_arcElement.arcAngle = 0.0f;
		m_clickAngle = 0.0f;
	}

	// Miss Early: when clicking too early
	// Miss !Early: when not clicking at all
	void Miss(bool early = false) {
		// Show miss transition and text
		if(early) {
			Debug.Log("> TOO EARLY :(");
			EndOfLife(HitState.Early);
		} else {
			Debug.Log("> MISS !");
			EndOfLife(HitState.Miss);
		}
		// TEMP: to test restart
		//m_arcElement.arcAngle = 0.0f;
		m_clickAngle = 0.0f;
	}

	void EndOfLife(HitState state) {
		// TODO: initiate feedback then schedule kill all involved objects
		// TODO: sound effects should be played by game controller since they should play regardless of active state or destroy
		switch(state) {
			case HitState.Early:
				break;
			case HitState.Hit:
				m_audioSource.clip = m_hitSound;
				m_audioSource.Play();
				break;
			case HitState.Miss:
				m_audioSource.clip = m_missSound;
				m_audioSource.Play();
				break;
			case HitState.Perfect:
				// TODO: Perfect should have a special sound!
				m_audioSource.clip = m_hitSound;
				m_audioSource.Play();
				break;
		}
		m_bDone = true;
		Destroy(gameObject, 1.0f);
	}
}
