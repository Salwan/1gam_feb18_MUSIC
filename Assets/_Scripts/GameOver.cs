using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour {

	public Text m_outcomeText;
	public Text m_scoreLabel;
	public Text m_scoreText;
	public Text m_onewordText;
	public Button m_playAgainButton;
	public AudioClip m_winSound;
	public AudioClip m_loseSound;
	public AudioClip m_allClicksSound;
	public AudioClip m_perfectSound;

	public int score {
		get { return GameData.score; }
		set { 
			string sc = value.ToString();
			while(sc.Length < 8) {
				sc = "0" + sc;
			}
			m_scoreText.text = sc;
		}
	}

	private int m_scoreCounter;
	private float m_scoreTime;
	private float m_scoreTimeTotal;
	private AudioSource[] m_audioSources;

	void Awake() {
		m_scoreCounter = 0;
		m_scoreTime = 0.0f;
		m_scoreTimeTotal = 3.0f;
	}

	// Use this for initialization
	void Start () {
		m_audioSources = GetComponents<AudioSource>();
		if(GameData.finished) {
			m_audioSources[0].clip = m_winSound;
		} else {
			m_audioSources[0].clip = m_loseSound;
		}
		m_audioSources[0].Play();
		m_outcomeText.text = GameData.outcome;
		m_onewordText.text = GameData.oneword;
		// Outcome text
		m_outcomeText.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		m_outcomeText.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		iTween.ColorTo(m_outcomeText.gameObject, iTween.Hash(
			"a", 1.0f,
			"time", 0.5f
		));
		iTween.ScaleTo(m_outcomeText.gameObject, iTween.Hash(
			"scale", new Vector3(1.0f, 1.0f, 1.0f),
			"time", 1.0f,
			"easeType", iTween.EaseType.easeOutElastic
		));
		// Score label
		m_scoreLabel.color = new Color(m_scoreLabel.color.r, m_scoreLabel.color.g, m_scoreLabel.color.b, 0.0f);
		m_scoreLabel.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
		iTween.FadeTo(m_scoreLabel.gameObject, iTween.Hash(
			"time", 0.5f, 
			"delay", 0.25f,
			"alpha", 1.0f
		));
		iTween.ScaleTo(m_scoreLabel.gameObject, iTween.Hash(
			"time", 1.0f,
			"delay", 0.25f,
			"scale", Vector3.one,
			"easeType", iTween.EaseType.easeOutElastic
		));
		// Score text
		m_scoreText.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		m_scoreText.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
		iTween.FadeTo(m_scoreText.gameObject, iTween.Hash(
			"time", 0.5f, 
			"delay", 1.0f,
			"alpha", 1.0f
		));
		iTween.ScaleTo(m_scoreText.gameObject, iTween.Hash(
			"time", 1.0f,
			"delay", 1.0f,
			"scale", Vector3.one,
			"easeType", iTween.EaseType.easeOutCirc
		));
		iTween.ScaleTo(m_scoreText.gameObject, iTween.Hash(
			"time", 0.25f,
			"delay", m_scoreTimeTotal,
			"scale", new Vector3(1.25f, 1.25f, 1.0f),
			"easeType", iTween.EaseType.easeOutExpo
		));
		iTween.ScaleTo(m_scoreText.gameObject, iTween.Hash(
			"time", 0.25f,
			"delay", m_scoreTimeTotal + 0.25f,
			"scale", new Vector3(1.0f, 1.0f, 1.0f),
			"easeType", iTween.EaseType.easeOutExpo
		));
		// One word
		Color owc = m_onewordText.color;
		owc.a = 0.0f;
		m_onewordText.color = owc;
		m_onewordText.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
		iTween.FadeTo(m_onewordText.gameObject, iTween.Hash(
			"time", 0.5f, 
			"delay", 3.0f,
			"alpha", 1.0f
		));
		iTween.ScaleTo(m_onewordText.gameObject, iTween.Hash(
			"time", 1.0f,
			"delay", 3.0f,
			"scale", Vector3.one,
			"easeType", iTween.EaseType.easeOutBounce
		));
		if(GameData.winLevel == 2) {
			m_audioSources[1].clip = m_allClicksSound;
			m_audioSources[1].PlayDelayed(3.25f);
		} else if(GameData.winLevel == 3) {
			m_audioSources[1].clip = m_perfectSound;
			m_audioSources[1].PlayDelayed(3.25f);
		}

		// Button
		Vector3 pos = m_playAgainButton.transform.position;
		float tx = pos.x;
		pos.x = 1000.0f;
		m_playAgainButton.transform.position = pos;
		iTween.MoveTo(m_playAgainButton.gameObject, iTween.Hash(
			"time", 1.0f,
			"delay", 1.5f,
			"x", tx,
			"easeType", iTween.EaseType.easeOutCubic
		));

	}
	
	// Update is called once per frame
	void Update () {
		if(m_scoreTime < m_scoreTimeTotal) {
			float alpha = m_scoreTime / m_scoreTimeTotal;
			score = (int)((float)GameData.score * alpha);
			m_scoreTime += Time.deltaTime;
		}
	}

	public void PlayAgain() {
		SceneManager.LoadScene("Main");
	}
}
