using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
	This script handles the animations of the ui elements while playing.
*/
public class GameUIElemsAnims : MonoBehaviour
{	
	public float minScale, maxScale;
	public RectTransform[] buttons;
	public float pulseSpeed;
	
	public Transform livesParent;
	public float rotSpeed, dissolveSpeed;
	
	void OnEnable()
	{
		StartCoroutine (_Pulse ());
	}
	
	void Update()
	{
		foreach (Transform life in livesParent) {
			life.RotateAround(life.position, Vector3.up, Time.deltaTime * rotSpeed);
		}
	}
	
	public void ResetLives()
	{
		foreach (Transform life in livesParent)
			life.localScale = Vector3.one;
	}
	
	private IEnumerator _Pulse()
	{
		float currScale = 1;
		while (true) {
			while (currScale > minScale) {
				currScale = Mathf.Max (minScale, currScale - Time.deltaTime * pulseSpeed);
				foreach (RectTransform button in buttons) {
					button.localScale = new Vector3 (currScale, currScale, currScale);
				}
				yield return null;
			}
			while (currScale < maxScale) {
				currScale = Mathf.Min (maxScale, currScale + Time.deltaTime * pulseSpeed);
				foreach (RectTransform button in buttons) {
					button.localScale = new Vector3 (currScale, currScale, currScale);
				}
				yield return null;
			}
		}
	}
	
	public void RemoveLife(int lifeNum)
	{
		StartCoroutine (_RemoveLife (lifeNum));
	}
	
	private IEnumerator _RemoveLife(int lifeNum)
	{
		Transform life = livesParent.GetChild (lifeNum);
		float currScale = 1;
		while (currScale > 0) {
			currScale = Mathf.Max (0, currScale - dissolveSpeed * Time.deltaTime);
			life.localScale = new Vector3 (currScale, currScale, currScale);
			yield return null;
		}
		yield break;
	}
}
