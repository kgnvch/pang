using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/**
	This script handles all the common actions of the game (excluding actions specific for each level).
*/
public class Gameplay : MonoBehaviour
{
	public Camera mainCam;
	public Transform backQuad;
	public Player player;
	public GameObject map;
	public Transform[] screenBounds;
	
	private float frustumHeight, frustumWidth;
	
	public bool isPlaying { get; private set; }
	
	public Canvas gameCanvas;
	public CanvasGroup gameButtons;
	public float gameCanvasShowSpeed;
	
	public CanvasGroup mainCanvas;
	public float mainCanvasShowSpeed;
	
	public CanvasGroup menuCanvas, quitCanvas;
	public float menuShiftAmount, menuShiftSpeed;
	
	private int currLevel;
	
	public int numOfLives;
	private int currNumOfLives;
	public GameUIElemsAnims uiAnims;
	public float dieReloadWait;
	public FadeText gameOver;
	
	private bool canClick = true;
	
	public AudioManager audioManager;
	
    void Start()
    {
		float dist;
		
		//Stretch the background quad to match the screen size.
		dist = backQuad.position.z - mainCam.transform.position.z;
        frustumHeight = 2.0f * dist * Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
		frustumWidth = frustumHeight * mainCam.aspect;
		backQuad.localScale = new Vector3 (frustumWidth, frustumHeight, 1);
		
		//Set the screen side cubes at the screen edges
		dist = player.transform.position.z - mainCam.transform.position.z;
        frustumHeight = 2.0f * dist * Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
		frustumWidth = frustumHeight * mainCam.aspect;
		screenBounds [0].transform.position = new Vector3 (frustumWidth / 2, 0, 0);
		screenBounds [1].transform.position = -screenBounds [0].transform.position;
		screenBounds [2].transform.position = new Vector3 (0, frustumHeight / 2, 0);
		
		ResetGameParams ();
    }
	
	private void ResetGameParams()
	{
		currLevel = 1;
		currNumOfLives = numOfLives;
	}
	
	public void StartGame()
	{
		if (!canClick)
			return;
		canClick = false;
		audioManager.click.Play ();
		uiAnims.ResetLives ();
		StartCoroutine (_StartGame ());
	}

    private IEnumerator _StartGame ()
	{
		while (mainCanvas.alpha > 0) {
			mainCanvas.alpha -= Time.deltaTime * mainCanvasShowSpeed;
			yield return null;
		}
		mainCanvas.gameObject.SetActive (false);
		
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (currLevel, LoadSceneMode.Additive);
		
		while (!asyncLoad.isDone)
            yield return null;
		
		gameCanvas.gameObject.SetActive (true);
		player.StartWithParams (mainCam);
		yield break;		
	}
	
	public void LoadNextLevel()
	{
		SceneManager.UnloadSceneAsync (currLevel);
		currLevel = Math.Min (3, currLevel + 1);
		StartCoroutine (_StartGame ());		
	}
	
	public void ReloadLevel()
	{
		SceneManager.UnloadSceneAsync (currLevel);
		StartCoroutine (_StartGame ());		
	}
	
	public void StartPlaying()
	{
		isPlaying = true;
		GameObject.FindGameObjectWithTag ("LevelScript").GetComponent<LevelScript> ().StartWithParams(frustumWidth, frustumHeight);
		StartCoroutine (_ShowCanvas (gameButtons, gameCanvasShowSpeed));
	}
	
	private IEnumerator _ShowCanvas(CanvasGroup canvas, float showSpeed)
	{
		canvas.gameObject.SetActive (true);
		canvas.alpha = 0;
		while (canvas.alpha < 1) {
			canvas.alpha += Time.deltaTime * showSpeed;
			yield return null;
		}
		canClick = true;
		yield break;
	}
	
	private IEnumerator _HideCanvas(CanvasGroup canvas, float showSpeed)
	{
		while (canvas.alpha > 0) {
			canvas.alpha -= Time.deltaTime * showSpeed;
			yield return null;
		}
		canvas.gameObject.SetActive (false);
		yield break;
	}
	
	public void EndGame()
	{
		isPlaying = false;
		player.Die ();
		currNumOfLives--;
		uiAnims.RemoveLife(currNumOfLives);
		audioManager.explosion.Play ();
		audioManager.fail.Play ();
		if (currNumOfLives == 0) {
			StartCoroutine (_GameOverWait ());
		} else {
			StartCoroutine (_ReloadWait ());
		}
	}
	
	public void ShowMainMenu()
	{
		SceneManager.UnloadSceneAsync (currLevel);
		player.gameObject.SetActive (false);
		gameCanvas.gameObject.SetActive (false);
		gameButtons.gameObject.SetActive (false);
		ResetGameParams ();
		StartCoroutine (_ShowCanvas (mainCanvas, mainCanvasShowSpeed));
	}
	
	public void ClickX()
	{
		if (!canClick)
			return;
		canClick = false;
		audioManager.click.Play ();
		ShowMainMenu ();
	}
	
	public void EndLevelSuccess()
	{
		isPlaying = false;
		audioManager.win.Play ();
		StartCoroutine (_HideCanvas (gameButtons, gameCanvasShowSpeed));
		player.EndLevelSuccess ();
	}
	
	public void QuitConfirm()
	{
		if (!canClick)
			return;
		canClick = false;
		audioManager.click.Play ();
		StartCoroutine (_SwitchRects (menuCanvas, quitCanvas, -1));
	}
	
	public void Quit()
	{
		if (!canClick)
			return;
		canClick = false;
		audioManager.click.Play ();
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
	}
	
	public void CancelQuit()
	{
		if (!canClick)
			return;
		canClick = false;
		audioManager.click.Play ();
		StartCoroutine (_SwitchRects (quitCanvas, menuCanvas, 1));
	}
	
	private IEnumerator _SwitchRects(CanvasGroup src, CanvasGroup dst, int moveDir)
	{
		canClick = false;
		if (moveDir == 1) {
			dst.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (-menuShiftAmount, 0, 0);
		} else {
			dst.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (menuShiftAmount, 0, 0);
		}
		float currShift = 0;
		dst.alpha = 0;
		dst.gameObject.SetActive (true);
		while (currShift < 1) {
			currShift = Mathf.Min (1, currShift + menuShiftSpeed * Time.deltaTime);
			src.alpha = 1 - currShift;
			dst.alpha = currShift;
			src.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (menuShiftAmount * currShift * moveDir, 0, 0);
			dst.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (-menuShiftAmount * (1 - currShift) * moveDir, 0, 0);
			yield return null;
		}
		src.gameObject.SetActive (false);
		canClick = true;
		yield break;
	}
	
	private IEnumerator _ReloadWait()
	{
		StartCoroutine (_HideCanvas (gameButtons, gameCanvasShowSpeed));
		yield return new WaitForSeconds (dieReloadWait);
		ReloadLevel ();
		yield break;
	}
	
	private IEnumerator _GameOverWait()
	{
		StartCoroutine (_HideCanvas (gameButtons, gameCanvasShowSpeed));
		yield return new WaitForSeconds (dieReloadWait);
		gameOver.ShowAndHideCanvas (ShowMainMenu);
		yield break;
	}
	
}
