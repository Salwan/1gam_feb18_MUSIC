using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameStartScreen : MonoBehaviour {

	public Text m_devText;
	public Text m_titleText;
	public Button m_startButton;

	private bool m_bDone;

	// Use this for initialization
	void Start () {
		m_bDone = false;
		m_devText.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		Color c = m_titleText.color;
		c.a = 0.0f;
		m_titleText.color = c;
		Vector3 v = m_startButton.transform.position;
		m_startButton.transform.position = new Vector3(1000.0f, v.y, v.z);

		iTween.FadeTo(m_devText.gameObject, 1.0f, 1.0f);
		iTween.FadeTo(m_titleText.gameObject, iTween.Hash(
			"alpha", 1.0f,
			"time", 1.0f,
			"delay", 1.0f
		));
		/*iTween.ScaleTo(m_startButton.gameObject, iTween.Hash(
			"scale", Vector3.one,
			"time", 0.5f,
			"delay", 2.5f,
			"easeType", iTween.EaseType.easeOutQuad
		));*/
		iTween.MoveTo(m_startButton.gameObject, iTween.Hash(
			"position", v,
			"time", 1.0f,
			"delay", 2.5f,
			"easeType", iTween.EaseType.easeOutCirc
		));
		Invoke("TitlePulseOut", 3.0f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Play() {
		m_bDone = true;
		iTween.Stop();
		SceneManager.LoadScene("Main");
	}

	void TitlePulseOut() {
		if(m_bDone) return;
		iTween.ScaleTo(m_titleText.gameObject, iTween.Hash(
			"scale", new Vector3(1.1f, 1.1f, 1.0f),
			"time", 2.0f,
			"easeType", iTween.EaseType.easeInOutQuad,
			"oncomplete", "TitlePulseIn",
			"oncompletetarget", gameObject
		));
	}

	void TitlePulseIn() {
		if(m_bDone) return;
		iTween.ScaleTo(m_titleText.gameObject, iTween.Hash(
			"scale", Vector3.one,
			"time", 2.0f,
			"easeType", iTween.EaseType.easeInOutQuad,
			"oncomplete", "TitlePulseOut",
			"oncompletetarget", gameObject
		));
	}
}
