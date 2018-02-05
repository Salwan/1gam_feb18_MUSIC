using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameController : MonoBehaviour {

	public GameObject m_startText;
	public Maestro m_maestro; // TODO: this should be created inside GameController
	public GameObject m_footerGradient;

	private enum GState {
		Init,
		Start,
		Playing,
		End,
	}

	private GState m_state;

	void Awake() {
		m_state = GState.Init;
	}

	void Start() {
		Assert.IsTrue(m_footerGradient != null);
		m_maestro.tickSignal.connect(OnTickTock);
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
	}

	void PlayGame() {
		m_state = GState.Playing;
		m_maestro.Begin();
	}

	void EndGame() {
		m_state = GState.End;
		m_maestro.End();
	}

	// Bigtick is the first tick in a 4 tick bar
	void OnTickTock(bool bigtick) {
		m_footerGradient.GetComponent<TweenScaleOnTick>().OnTickTock(bigtick);
	}
}
