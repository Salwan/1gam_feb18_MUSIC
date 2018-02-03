using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public GameObject m_startText;

	private enum GState {
		Start,
		Playing,
		End,
	}

	private GState m_state;

	void Awake() {
		m_state = GState.Start;
	}

	void Start() {
		StartGame();
	}

	void StartGame() {
		m_startText.SetActive(true);
		m_startText.transform.localScale = new Vector3(2.0f, 2.0f, 1.0f);
		iTween.FadeTo(m_startText, iTween.Hash("alpha", 0.0f, "time", 0.25f, "amount", 1.0f));
		iTween.ScaleTo(m_startText, new Vector3(1.0f, 1.0f, 1.0f), 0.4f);
		iTween.MoveTo(m_startText, iTween.Hash(
			"position", new Vector3(12.0f, 4.0f, 0.0f), "time", 0.5f, "delay", 1.0f, "easeType", iTween.EaseType.easeInCirc
		));
	}
}
