using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
	This is the player controller.
*/
public class Player : MonoBehaviour
{
	public Gameplay gameplay;
	public float runSpeed;
	public float rotateSpeed;
	private int runDir;
	private float startX;
	
	/*
		Animator transition names and meanings:
		r: Run
		f: Fire
		d: Die
		e: End (perform a short dancing animation at the completion of a level)
	*/
	private Animator anim;
	
	private Rigidbody rb;
	private Quaternion forwardRot;
	private System.Random rand;
	
	public Transform firePos;
	public Bullet bullet;
	
	public float exitWait;
	
	public GameObject sideWalls;
	
	void Start()
	{
		forwardRot = Quaternion.Euler (0, 180, 0);
		anim = GetComponent<Animator> ();
	}
	
	void Update()
	{
		//Stop fire animation immediately.
		if (anim.GetBool ("f") && anim.GetCurrentAnimatorStateInfo (0).IsName ("Fire") && anim.GetAnimatorTransitionInfo(0).normalizedTime == 0) {
			anim.SetBool ("f", false);
		}
		
		if (Input.GetKeyDown("right")) {
			MoveRight ();
		} else if (Input.GetKeyDown("left")) {
			MoveLeft ();
		} else if (Input.GetKeyUp("right") || Input.GetKeyUp("left")) {
			StopRun ();
		} else if (Input.GetButtonDown ("Jump")) {
			Fire ();
		}
	}
	
    public void StartWithParams(Camera mainCam)
    {	
		gameObject.SetActive (true);
		sideWalls.SetActive (false);
		GetComponent<Collider> ().enabled = true;
		
		//Place the player off camera in a random direction, but very close to the camera edge.
		rand = new System.Random();
		float dist = transform.position.z - mainCam.transform.position.z;
        float frustumHeight = 2.0f * dist * Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
		float frustumWidth = frustumHeight * mainCam.aspect;
		runDir = rand.Next (2) == 0 ? 1 : -1;
		startX = -runDir * (frustumWidth / 2  + 0.5f);
		transform.position = new Vector3 (startX, transform.position.y, transform.position.z);
		
		//Rotate the player towards the middle of the screen and make him run
		anim = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody> ();
		rb.isKinematic = false;
		StartRun ();
		anim.Play("Run", -1, 0f);
		StartCoroutine (_CheckReachCenter ());
    }
	
	private IEnumerator _CheckReachCenter()
	{
		while (true) {
			if ((runDir > 0 && transform.position.x >= 0) || (runDir < 0 && transform.position.x <= 0)) {
				transform.position = new Vector3 (0, transform.position.y, transform.position.z);
				StopRun ();
				sideWalls.SetActive (true);
				gameplay.StartPlaying ();
				yield break;
			}
			else
				yield return null;
		}
	}
	
	private IEnumerator _RotateForward()
	{
		while (runDir == 0) {
			transform.rotation = Quaternion.RotateTowards(transform.rotation, forwardRot, rotateSpeed * Time.deltaTime);
			if (transform.rotation == forwardRot)
				yield break;
			else
				yield return null;
		}
		yield break;
	}
	
	public void Die()
	{
		rb.isKinematic = true;
		GetComponent<Collider> ().enabled = false;
		int animNum = rand.Next (4);
		anim.SetBool("d" + animNum, true);
		StartCoroutine (_ResetDie (animNum));
	}
	
	private IEnumerator _ResetDie(int animNum)
	{
		yield return new WaitForSeconds (1); //Wait for 1 seconds before restarting the death trigger, to ensure it started playing.
		anim.SetBool("d" + animNum, false);
		yield break;
	}
	
	public void MoveRight()
	{
		if (!gameplay.isPlaying)
			return;
		runDir = 1;
		StartRun ();
	}
	
	public void MoveLeft()
	{
		if (!gameplay.isPlaying)
			return;
		runDir = -1;
		StartRun ();
	}
	
	private void StartRun()
	{
		float playerRot = runDir == 1 ? 90 : -90;
		transform.eulerAngles = new Vector3 (0, playerRot, 0);
		anim.SetBool ("r", true);
		rb.velocity = new Vector3 (runDir * runSpeed, 0, 0);
	}
	
	public void StopRun()
	{
		runDir = 0;
		anim.SetBool ("r", false);
		StartCoroutine (_RotateForward ());
		rb.velocity = Vector3.zero;
	}
	
	public void Fire()
	{
		if (!gameplay.isPlaying || bullet.gameObject.activeSelf)
			return;
		StopRun ();
		anim.SetBool ("f", true);
		bullet.transform.position = new Vector3 (firePos.position.x, firePos.position.y, 0);
		bullet.Fire ();
		gameplay.audioManager.fire.Play ();
	}
	
	public void EndLevelSuccess()
	{
		StopRun ();
		sideWalls.SetActive (false);
		StartCoroutine (_ExitWait ());
	}
	
	private IEnumerator _ExitWait()
	{
		int danceAnimNum = rand.Next (4);
		anim.SetBool ("e" + danceAnimNum, true);
		yield return new WaitForSeconds (exitWait);
		anim.SetBool ("e" + danceAnimNum, false);
		
		if (startX > transform.position.x)
			runDir = 1;
		else
			runDir = -1;
		StartRun ();
		while (runDir == 1 ? startX > transform.position.x : startX < transform.position.x)
			yield return null;
		
		StopRun ();
		gameObject.SetActive (false);
		gameplay.LoadNextLevel ();
		yield break;
	}

}
