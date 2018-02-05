using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenScaleOnTick : MonoBehaviour {

	public Vector3 m_startScale;
	public Vector3 m_tickScale;
	public Vector3 m_tockScale;
	
	public void OnTickTock(bool bigtick) {
		iTween.StopByName("tween_scaler_dn");
		Vector3 s = bigtick? m_tickScale : m_tockScale;
		transform.localScale = s;
		iTween.ScaleTo(gameObject, iTween.Hash(
			"name", "tween_scaler_dn",
			"scale", m_startScale,
			"time", 0.2f,
			"easeType", iTween.EaseType.easeInOutQuad
		));
	}
}
