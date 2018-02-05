using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public static readonly int Score_Hit = 100;
	public static readonly int Score_Perfect = 250;
	public static readonly int Score_Miss = 5;
	public static readonly int Score_Early = 20;

	public GameObject m_startText;
	public Maestro m_maestro; // TODO: this should be created inside GameController
	public GameObject m_footerGradient;
	public Text m_scoreText;
	public Transform m_loseHealthbar;
	public float m_loseAtPercentage = 0.5f;

	public int score {
		get { return m_score; }
		set { 
			m_score = value;
			string sc = m_score.ToString();
			while(sc.Length < 8) {
				sc = "0" + sc;
			}
			m_scoreText.text = sc;
		}
	}

	public float health {
		get { return m_health; }
		set { 
			m_health = value;
			m_loseHealthbar.localScale = new Vector3(1.0f - m_health, 1.0f, 1.0f);
			if(m_health <= 0.0f) {
				EndGame_Lost();
			}
		}
	}
	
	private int m_score;
	private float m_health;

	private enum GState {
		Init,
		Start,
		Playing,
		End,
	}

	private GState m_state;

	void Awake() {
		m_state = GState.Init;
		m_score = 0;
	}

	void Start() {
		Assert.IsTrue(m_footerGradient != null);
		m_maestro.tickSignal.connect(OnTickTock);
		m_maestro.finishedSignal.connect(EndGame_Finished);
		health = 1.0f;
		StartGame();
	}

	void StartGame() {
		m_state = GState.Start;
		m_startText.SetActive(true);
		m_startText.transform.localScale = new Vector3(2.0f, 2.0f, 1.0f);
		iTween.ScaleTo(m_startText, new Vector3(1.0f, 1.0f, 1.0f), 0.4f);
		iTween.MoveTo(m_startText, iTween.Hash(
			"position", new Vector3(12.0f, 4.0f, 0.0f), "time", 0.5f, "delay", 1.0f, "easeType", iTween.EaseType.easeInCirc,
			"oncomplete", "PlayGame", "oncompletetarget", gameObject
		));
		GameData.score = 0;
		GameData.health = 1.0f;
		GameData.finished = false;
	}

	void PlayGame() {
		m_state = GState.Playing;
		m_maestro.Begin();
	}

	void EndGame_Lost() {
		GameData.score = score;
		GameData.health = health;
		GameData.finished = false;
		GameData.outcome = "Lost!";
		GameData.oneword = "Next Time!";
		GameData.winLevel = 0;
		m_state = GState.End;
		m_maestro.End();
		SceneManager.LoadScene("GameOver");
	}

	void EndGame_Finished() {
		GameData.score = score;
		GameData.health = health;
		GameData.finished = true;
		GameData.outcome = "You've Done It!";
		string oneword = "Great!";
		GameData.winLevel = 1;
		if(score >= m_maestro.estimateAllScore) {
			oneword = "Awesome!";
			GameData.winLevel = 2;
		} else if(score >= m_maestro.estimatePerfectScore) {
			oneword = "Perfection!";
			GameData.winLevel = 3;
		}
		GameData.oneword = oneword;
		m_state = GState.End;
		m_maestro.End();
		SceneManager.LoadScene("GameOver");
	}

	// Bigtick is the first tick in a 4 tick bar
	void OnTickTock(bool bigtick) {
		m_footerGradient.GetComponent<TweenScaleOnTick>().OnTickTock(bigtick);
		score += bigtick? 2 : 1;
	}

	public void AddScore(int howmuch) {
		score += howmuch;
	}

	public void OnMiss() {
		health -= m_maestro.damageOfMissClicks / m_loseAtPercentage;
	}
}
