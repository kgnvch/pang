using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
	This is the script handles fading in and out text on a canvas.
*/
public class FadeText : MonoBehaviour
{
	public CanvasGroup canvas;
	public float showSpeed, hideDelay;
	private bool isStarted = false;
	
	void Start()
	{
		if (!isStarted) {
			ShowAndHideCanvas ();
		}
	}
	
	public void ShowAndHideCanvas(Action nextCallback=null)
	{
		if (isStarted)
			return;
		isStarted = true;
		canvas.gameObject.SetActive (true);
		StartCoroutine (_ShowAndHideCanvas (nextCallback));
	}
	
	private IEnumerator _ShowAndHideCanvas(Action nextCallback)
	{
		canvas.alpha = 0;
		while (canvas.alpha < 1) {
			canvas.alpha += Time.deltaTime * showSpeed;
			yield return null;
		}
		yield return new WaitForSeconds (hideDelay);
		while (canvas.alpha > 0) {
			canvas.alpha -= Time.deltaTime * showSpeed;
			yield return null;
		}
		canvas.gameObject.SetActive (false);
		if (nextCallback != null) {
			nextCallback ();
		}
		yield break;
	}
}
