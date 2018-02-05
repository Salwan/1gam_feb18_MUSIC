using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MusicElement : MonoBehaviour {

	private AudioSource m_audioSource;

	public AudioClip m_musicClip;
	public TextAsset m_musicLabels;

	private GameObject m_tick;
	private GameObject m_tock;
	
	void Start() {
		m_audioSource = GetComponent<AudioSource>();
		m_tick = transform.Find("tick").gameObject;
		m_tock = transform.Find("tock").gameObject;
		Assert.IsTrue(m_tick != null && m_tock != null);
		m_tick.SetActive(false);
		m_tock.SetActive(false);

		// Initialize and build sync information from labels file here
		Debug.Log(m_musicLabels.text);
	}	
	
	void Update() {

	}

	public void Begin() {
		// Starts music and sync timers
		m_audioSource.clip = m_musicClip;
		m_audioSource.Play();
	}

	public void End() {
		// Stops music and sync timers
		m_audioSource.Stop();
	}
}
