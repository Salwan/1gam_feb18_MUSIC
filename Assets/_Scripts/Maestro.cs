using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using System;

public class Maestro : MonoBehaviour {

	// First label in Audacity must always be: tempo=value
	enum ELabel {
		None,
		Tempo,
		Pause,
		Pattern,
	};
	struct SSyncEntry {
		public double time;
		public string tag;
		public int value;
		public ELabel type;
	};
	struct SPatternEntry {
		public double time;
		public int pattern;
	};

	public AudioClip m_musicClip;
	public TextAsset m_musicLabels;
	public Signal<bool> m_tickSignal;
	public Signal m_syncSignal;
	public Signal m_pauseSignal;
	public Signal m_finishedSignal;

	public int totalClicksCount {
		get { return m_totalClicksCount; }
	}

	public int estimatePerfectScore {
		get { return m_totalClicksCount * 250; }
	}

	public int estimateAllScore {
		get { return m_totalClicksCount * 100; }
	}

	public float damageOfMissClicks {
		get { return 1.0f / (float)m_totalClicksCount; }
	}

	private bool m_bEnabled;
	private AudioSource m_audioSource;
	private GameObject m_tick;
	private GameObject m_tock;
	private List<SSyncEntry> m_syncInfo;
	private List<SPatternEntry> m_patternList;
	private int m_currentSyncId;
	private int m_nextSyncId;
	private int m_currentTempo;
	private double m_playHead;
	private double m_bpmPeriod;	// Time period for 1 tick at current bpm
	private double m_bpmTick;	// Times current tick to figure out when to fire event
	private int m_tickType;  	// Loops 0 1 2 3, big tick on 0, small tock on 1 2 3 (tick type)
	private bool m_bTicking;		// Enables/disables ticking, will be used based on 'pause' label in Audacity
	private int m_totalClicksCount;	// Total number of clicks based on patterns

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

	public Signal finishedSignal {
		get { return m_finishedSignal; }
	}

	void Awake() {
		m_bEnabled = false;
		m_audioSource = GetComponent<AudioSource>();
		m_tick = transform.Find("tick").gameObject;
		m_tock = transform.Find("tock").gameObject;
		m_syncInfo = new List<SSyncEntry>();
		m_patternList = new List<SPatternEntry>();
		m_playHead = 0.0;
		m_currentTempo = 0;
		m_bpmPeriod = 0.0;
		m_bpmTick = 0.0;
		m_tickType = 0;
		m_bTicking = false;
		m_tickSignal = new Signal<bool>();
		m_syncSignal = new Signal();
		m_pauseSignal = new Signal();
		m_finishedSignal = new Signal();
		m_totalClicksCount = 0;
	}
	
	void Start() {
		Assert.IsTrue(m_tick != null && m_tock != null);
		// Initialize and build sync information from labels file here
		ParseLabelsInfo(m_musicLabels);
		InitPatternator();
	}	
	
	void Update() {
		Assert.IsTrue(m_syncInfo.Count > 0);
		if(!m_bEnabled) {
			return;
		}
		if(m_playHead == 0.0) {
			m_nextSyncId = 0;
			Assert.IsTrue(m_syncInfo[m_nextSyncId].type == ELabel.Tempo);
			tempo = m_syncInfo[0].value;
		} else {
			int current_sync_id = 0;
			for(int i = 0; i < m_syncInfo.Count; ++i) {
				SSyncEntry se = m_syncInfo[i];

				if(se.time > m_playHead) {
					m_nextSyncId = i;
					break;
				} else {
					current_sync_id = i;
				}
			}
			if(current_sync_id > m_currentSyncId) {
				// New sync occured
				//Debug.Log("Resync ID = " + current_sync_id);
				tempo = m_syncInfo[current_sync_id].value;
				if(tempo == 0) {
					OnPause(); 
				} else {
					OnResync();
				}
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
		// Patternator
		UpdatePatternator();
	}

	public void Begin() {
		// Starts music and sync timers
		m_audioSource.clip = m_musicClip;
		m_audioSource.loop = false;
		m_audioSource.Play();
		Invoke("SignalEnd", m_musicClip.length);
		m_bEnabled = true;
	}

	void SignalEnd() {
		m_finishedSignal.emit();
	}

	public void End() {
		// Stops music and sync timers
		m_audioSource.Stop();
		m_bEnabled = false;
	}

	void ParseLabelsInfo(TextAsset text_asset) {
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
				double time = double.Parse(elements[0]);
				string[] tag = elements[2].Split('=');
				string tag0 = tag[0];
				int value = 0;
				if(tag.Length > 1) {
					value = int.Parse(tag[1]);
				}
				ELabel type = ELabel.None;
				switch(tag0) {
					case "tempo":
						type = ELabel.Tempo;
						break;
					case "pause":
						type = ELabel.Pause;
						break;
					case "pattern":
						type = ELabel.Pattern;
						break;
					default:
						type = ELabel.None;
						break;
				}
				if(type == ELabel.Tempo || type == ELabel.Pause) {
					SSyncEntry se = new SSyncEntry();
					se.type = type;
					se.time = time;
					se.tag = tag0;
					se.value = value;
					//Debug.Log("Parsed Entry @ " + se.time.ToString() + " seconds: " + se.tag + "(" + se.value + ")");
					m_syncInfo.Add(se);
				} else {
					SPatternEntry pe = new SPatternEntry();
					pe.time = time;
					pe.pattern = value;
					m_patternList.Add(pe);
				}
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
		Debug.Log("Maestro: RESYNC Tempo=" + tempo.ToString());
		m_syncSignal.emit();
	}

	void OnPause() {
		Debug.Log("Maestro: PAUSE ticking");
		m_pauseSignal.emit();
	}
	
	///////////////////////////////////////////// Patternator
	enum EElement {
		None,
		Click,		// Type 0
		Line,		// 1
		ZigZag,		// 2
		Ladder,		// 3
		UpDown, 	// 4
	};
	struct SActionElement {
		public EElement type;
		public double time;
		public float x;
		public float y;
	};
	public Bounds m_boundary;
	public GameObject m_clickElementPrefab;
	public GameObject m_connectorPrefab;

	private float m_reactionTime = 4.0f; // Time between visual element and actual click expected event
	private List<SActionElement> m_actions; 
	private int m_nextActionId;
	private int m_currentActionId;

	void InitPatternator() {
		Assert.IsTrue(m_patternList != null && m_patternList.Count > 0);
		m_actions = new List<SActionElement>();
		m_reactionTime = 4.0f;
		m_totalClicksCount = 0;
		int quadrant = 0; // used to avoid randomly overlapping simple clicks
		foreach(SPatternEntry pe in m_patternList) {
			// Support for type 0 first
			Assert.IsTrue(pe.time > m_reactionTime); // Just in case
			if(pe.time > m_reactionTime) {
				SActionElement ae = new SActionElement();
				ae.time = pe.time - m_reactionTime;
				// Simple Click
				if(pe.pattern == 0) {
					m_totalClicksCount += 1;
					ae.type = EElement.Click;
					int x_side = quadrant < 2? x_side = 1 : x_side = -1;
					int y_side = quadrant == 1 || quadrant == 3? y_side = -1 : y_side = 1;
					ae.x = UnityEngine.Random.Range(0.5f * x_side, m_boundary.max.x * x_side);
					ae.y = UnityEngine.Random.Range(0.5f * y_side, m_boundary.max.y * y_side);
					quadrant = (quadrant + 1) % 4;
				} else if(pe.pattern == 1) {
					m_totalClicksCount += 4;
					ae.type = EElement.Line;
					// Left
					ae.x = m_boundary.min.x;
					ae.y = m_boundary.center.y;
				} else if(pe.pattern == 2) {
					m_totalClicksCount += 4;
					ae.type = EElement.ZigZag;
					// Left-top
					ae.x = m_boundary.min.x;
					ae.y = m_boundary.max.y;
				} else if(pe.pattern == 3) {
					m_totalClicksCount += 4;
					ae.type = EElement.Ladder;
					// Left-bottom
					ae.x = m_boundary.min.x;
					ae.y = m_boundary.min.y;
				} else if(pe.pattern == 4) {
					m_totalClicksCount += 4;
					ae.type = EElement.UpDown;
					// Right-Top
					ae.x = m_boundary.max.x;
					ae.y = m_boundary.max.y;
				} else {
					// Should never get here
					Debug.LogError("Shouldn't be here");
					continue;
				}
				m_actions.Add(ae); // TEMP: remove compare once all patterns implemented
			}
		}
	}

	void UpdatePatternator() {
		int current_action_id = m_currentActionId;
		for(int i = 0; i < m_actions.Count; ++i) {
			SActionElement ae = m_actions[i];
			if(ae.time > m_playHead) {
				m_nextActionId = i;
				break;
			} else {
				current_action_id = i;
			}
		}
		if(current_action_id > m_currentActionId) {
			// Action happened: current_action_id
			SActionElement ae = m_actions[current_action_id];
			if(ae.type == EElement.Click) {
				Debug.Log("Summon: CLICK");
				GameObject go = Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity);
				go.GetComponent<ClickElement>().Initialize(new Vector3(ae.x, ae.y, 0.0f), m_reactionTime);				
			} else if(ae.type == EElement.Line) {
				Debug.Log("Summon: LINE");
				GameObject[] go = {
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity)
				};  	
				float w = m_boundary.size.x;
				float y = UnityEngine.Random.Range(m_boundary.min.y, m_boundary.max.y);
				for(int i = 0; i < 4; ++i) {
					float delay = (float)m_bpmPeriod * (float)i;
					float x = m_boundary.min.x + (w * (float)i / 3.0f);
					go[i].GetComponent<ClickElement>().Initialize(new Vector3(x, y, 0.0f), m_reactionTime, delay);
				}	
			} else if(ae.type == EElement.ZigZag) {
				Debug.Log("Summon: ZIGZAG");
				GameObject[] go = {
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity)
				};
				GameObject line = Instantiate(m_connectorPrefab, new Vector3(0.0f, 0.0f, 2.5f), Quaternion.identity);
				line.transform.localPosition = new Vector3(0.0f, 0.0f, 15.0f);
				LineRenderer lrendr = line.GetComponent<LineRenderer>();
				lrendr.positionCount = 4;
				Assert.IsTrue(lrendr != null);
				float w = m_boundary.size.x - 1.0f;
				float h = m_boundary.size.y;
				for(int i = 0; i < 4; ++i) {
					float delay = (float)m_bpmPeriod * (float)i;
					float side = i == 1 || i == 3? -1.0f : 1.0f;
					float x = (side * (m_boundary.max.x - 1.0f)) - 0.5f;
					float y = m_boundary.max.y - (h * (float)i / 3.0f) - 0.5f;
					Vector3 pos = new Vector3(x, y, 0.0f);
					go[i].GetComponent<ClickElement>().Initialize(pos, m_reactionTime, delay);
					lrendr.SetPosition(i, pos);
				}
				GameObject.Destroy(line, m_reactionTime + ((float)m_bpmPeriod * 4.0f));
			} else if(ae.type == EElement.Ladder) {
				Debug.Log("Summon: LADDER");
				GameObject[] go = {
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity)
				};
				GameObject line = Instantiate(m_connectorPrefab, new Vector3(0.0f, 0.0f, 2.5f), Quaternion.identity);
				line.transform.localPosition = new Vector3(0.0f, 0.0f, 15.0f);
				LineRenderer lrendr = line.GetComponent<LineRenderer>();
				lrendr.positionCount = 4;
				Assert.IsTrue(lrendr != null);
				float w = m_boundary.size.x;
				float h = m_boundary.size.y;
				for(int i = 0; i < 4; ++i) {
					float delay = (float)m_bpmPeriod * (float)i;
					float x = m_boundary.min.x + (w * (float)i / 3.0f);
					float y = m_boundary.min.y + (h * (float)i / 3.0f);
					Vector3 pos = new Vector3(x, y, 0.0f);
					go[i].GetComponent<ClickElement>().Initialize(pos, m_reactionTime, delay);
					lrendr.SetPosition(i, pos);
				}
				GameObject.Destroy(line, m_reactionTime + ((float)m_bpmPeriod * 4.0f));
			} else if(ae.type == EElement.UpDown) {
				GameObject[] go = {
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity),
					Instantiate(m_clickElementPrefab, transform.position, Quaternion.identity)
				};
				GameObject line = Instantiate(m_connectorPrefab, new Vector3(0.0f, 0.0f, 2.5f), Quaternion.identity);
				line.transform.localPosition = new Vector3(0.0f, 0.0f, 15.0f);
				LineRenderer lrendr = line.GetComponent<LineRenderer>();
				lrendr.positionCount = 4;
				Assert.IsTrue(lrendr != null);
				float w = m_boundary.size.x - 1.0f;
				for(int i = 0; i < 4; ++i) {
					float delay = (float)m_bpmPeriod * (float)i;
					float x = (m_boundary.max.x - 0.5f) - (w * (float)i / 3.0f);
					float side = i == 1 || i == 3? -1.0f : 1.0f;
					float yy = (side * (m_boundary.max.y - 1.0f)) - 0.5f;
					Vector3 pos = new Vector3(x, yy, 0.0f);
					go[i].GetComponent<ClickElement>().Initialize(pos, m_reactionTime, delay);
					lrendr.SetPosition(i, pos);
				}
				GameObject.Destroy(line, m_reactionTime + ((float)m_bpmPeriod * 4.0f));
			}
		}
		m_currentActionId = current_action_id;
	}

	////////////////////////
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(m_boundary.center, m_boundary.size);
	}
}
