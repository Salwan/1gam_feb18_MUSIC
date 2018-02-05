using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using System;

public class Maestro : MonoBehaviour {

	// First label in Audacity must always be: tempo=value
	enum ESync {
		Tempo,
		Pause
	};
	struct SyncEntry {
		public double time;
		public string tag;
		public int value;
		public ESync type;
	};

	public AudioClip m_musicClip;
	public TextAsset m_musicLabels;
	public Signal<bool> m_tickSignal;

	private bool m_bEnabled;
	private AudioSource m_audioSource;
	private GameObject m_tick;
	private GameObject m_tock;
	private List<SyncEntry> m_syncInfo;
	private int m_currentSyncId;
	private int m_nextSyncId;
	private int m_currentTempo;
	private double m_playHead;
	private double m_bpmPeriod;	// Time period for 1 tick at current bpm
	private double m_bpmTick;	// Times current tick to figure out when to fire event
	private int m_tickType;  	// Loops 0 1 2 3, big tick on 0, small tock on 1 2 3 (tick type)
	private bool m_bTicking;		// Enables/disables ticking, will be used based on 'pause' label in Audacity

	public int tempo {
		get { return m_currentTempo; }
		set { 
			m_currentTempo = value; 
			m_bpmPeriod = 60.0 / (double)m_currentTempo;
			m_bpmTick = 0.0; // start first big tick immediately following sync on tempo
			m_tickType = 0;
			if(m_currentTempo > 0 && !m_bTicking) {
				// resuming ticking
				m_bTicking = true;
			} else if(m_currentTempo == 0 && m_bTicking) {
				// stopping ticking
				m_bTicking = false;
			}
			//OnResync();
		}
	}

	public Signal<bool> tickSignal {
		get { return m_tickSignal; }
	}

	void Awake() {
		m_bEnabled = false;
		m_audioSource = GetComponent<AudioSource>();
		m_tick = transform.Find("tick").gameObject;
		m_tock = transform.Find("tock").gameObject;
		m_syncInfo = new List<SyncEntry>();
		m_playHead = 0.0;
		m_currentTempo = 0;
		m_bpmPeriod = 0.0;
		m_bpmTick = 0.0;
		m_tickType = 0;
		m_bTicking = false;
		m_tickSignal = new Signal<bool>();
	}
	
	void Start() {
		Assert.IsTrue(m_tick != null && m_tock != null);
		// Initialize and build sync information from labels file here
		ParseSyncInfo(m_musicLabels);
	}	
	
	void Update() {
		Assert.IsTrue(m_syncInfo.Count > 0);
		if(!m_bEnabled) {
			return;
		}
		if(m_playHead == 0.0) {
			m_nextSyncId = 0;
			Assert.IsTrue(m_syncInfo[m_nextSyncId].type == ESync.Tempo);
			tempo = m_syncInfo[0].value;
		} else {
			int current_sync_id = 0;
			for(int i = 0; i < m_syncInfo.Count; ++i) {
				SyncEntry se = m_syncInfo[i];
				if(se.type == ESync.Tempo) {
					if(se.time > m_playHead) {
						m_nextSyncId = i;
						break;
					} else {
						current_sync_id = i;
					}
				}
			}
			if(current_sync_id > m_currentSyncId) {
				// New sync occured
				//Debug.Log("Resync ID = " + current_sync_id);
				tempo = m_syncInfo[current_sync_id].value;
				OnResync();
			}
			m_currentSyncId = current_sync_id;
		}
		m_playHead += Time.deltaTime;
		if(m_bTicking) {
			m_bpmTick -= Time.deltaTime;
			while(m_bpmTick <= 0.0) {
				m_bpmTick += m_bpmPeriod;
				Tick(m_tickType == 0);
				m_tickType  = (m_tickType + 1) % 4;
			}
		}
	}

	public void Begin() {
		// Starts music and sync timers
		m_audioSource.clip = m_musicClip;
		m_audioSource.Play();
		m_bEnabled = true;
	}

	public void End() {
		// Stops music and sync timers
		m_audioSource.Stop();
		m_bEnabled = false;
	}

	void ParseSyncInfo(TextAsset text_asset) {
		//StreamReader reader = new StreamReader("path");
		//reader.ReadToEnd();
		//reader.Close();
		string text = text_asset.text;
		StringReader reader = new StringReader(text);
		string nextline = "";
		while(nextline != null) {
			nextline = reader.ReadLine();
			if(nextline != null && nextline.Length > 0) {
				// Format: double double string
				// both doubles are the same value (Audacity exported)
				// string formats: name, name=integer
				string[] elements = nextline.Split(new Char[] {' ', '\t'});
				//Debug.Log("How many elements = " + elements.Length.ToString());
				Assert.IsTrue(elements.Length >= 3);
				if(elements.Length < 3) {
					continue;
				}
				SyncEntry se = new SyncEntry();
				se.time = double.Parse(elements[0]);
				string[] tag = elements[2].Split('=');
				se.tag = tag[0];
				se.value = 0;
				if(tag.Length > 1) {
					se.value = int.Parse(tag[1]);
				}
				switch(se.tag) {
					case "tempo":
						se.type = ESync.Tempo;
						break;
					case "pause":
						se.type = ESync.Pause;
						break;
				}
				//Debug.Log("Parsed Entry @ " + se.time.ToString() + " seconds: " + se.tag + "(" + se.value + ")");
				m_syncInfo.Add(se);
			}
		}
		//Debug.Log("DONE! number of entries = " + m_syncInfo.Count);
	}

	void Tick(bool bigtick) {
		// TICK TOCK!
		m_tickSignal.emit(bigtick);
		if(bigtick) {
			m_tick.GetComponent<TimedHide>().ShowAndHide(0.1f);
			//Debug.Log("TICK");
		} else {
			m_tock.GetComponent<TimedHide>().ShowAndHide(0.1f);
			//Debug.Log("TOCK");
		}
	}

	void OnResync() {
		Debug.Log("RESYNC to " + tempo.ToString());
	}
}
