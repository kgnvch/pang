using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
	This is the script for a ball.
*/
public class Ball : MonoBehaviour
{
	public float maxHeight;
	public float moveSpeed;
	private float moveDirX, moveDirY;
	private float initHeight;
	
	private float frustomWidth, frustumHeight;
	private Rigidbody rb;
	private Collider coll;
	
	private GameObject[] screenSides;
	private bool isIgnoringScreenSides = false;
	
	public LevelScript levelScript;
	private Gameplay gameplay;
	
	void Start()
	{
		rb = GetComponent<Rigidbody> ();
		coll = GetComponent<Collider> ();
		gameplay = GameObject.FindWithTag ("GameController").GetComponent<Gameplay> ();
			
		transform.rotation = Random.rotation;
		gameplay.audioManager.newBall.Play ();
	}
	
	public void StartWithParams(float frustomWidth, float frustumHeight, int startDir, bool isFirstBall=true)
	{
		this.frustomWidth = frustomWidth;
		this.frustumHeight = frustumHeight;
		moveDirX = -startDir;
		moveDirY = -1;
		
		//Initially set the ball at the screen side at max height, outside the screen bounds but close to it
		if (isFirstBall) {
			transform.position = new Vector3(
				startDir * (frustomWidth / 2 + transform.lossyScale.x),
				frustumHeight * maxHeight - frustumHeight / 2,
				0
			);
			
			//Because ball starts off screen, ignore the collision with the screen edges at the start.
			screenSides = GameObject.FindGameObjectsWithTag ("ScreenSide");
			foreach (GameObject sideWall in screenSides)
				Physics.IgnoreCollision (GetComponent<Collider> (), sideWall.GetComponent<Collider> (), true);
			isIgnoringScreenSides = true;
		}
		initHeight = transform.position.y;
		
		gameObject.SetActive (true);
	}
	
	void OnCollisionEnter(Collision collision)
	{
		//On first collision enable the side walls.
		if (isIgnoringScreenSides) {
			isIgnoringScreenSides = false;
			foreach (GameObject sideWall in screenSides)
				Physics.IgnoreCollision (coll, sideWall.GetComponent<Collider> (), false);
		}
			
		if (collision.collider.CompareTag ("ScreenSide")) {
			gameplay.audioManager.bounce.Play ();
			moveDirX = -moveDirX;
		} else if (collision.collider.CompareTag ("Ground")) {
			gameplay.audioManager.bounce.Play ();
			moveDirY = 1;
			//On ground collision, move up and the collision relative speed to the return to the original height.
			rb.velocity = collision.relativeVelocity;
		} else if (collision.collider.CompareTag ("Player")) {
			gameplay.audioManager.newBall.Play ();
			rb.velocity = collision.relativeVelocity;
			gameplay.EndGame ();
		} else if (collision.collider.CompareTag ("Bullet") && collision.collider.gameObject.activeSelf) {
			EndBall (collision.collider.GetComponent<Bullet> ());
		} 
	}
	
	void FixedUpdate()
	{
		rb.velocity = new Vector3 (moveDirX * Time.fixedDeltaTime * moveSpeed, rb.velocity.y, 0);
		
		//When the ball reaches its maxium point, adjust its height to the initial height to prevent displacement issues over time.
		//Difference after one iteration will not be noticible.
		if (gameplay.isPlaying && moveDirY == 1 && rb.velocity.y <= 0) {
			moveDirY = -1;
			transform.position = new Vector3 (transform.position.x, initHeight, 0);
		}
	}
	
	void EndBall(Bullet bullet)
	{
		gameObject.SetActive (false);
		bullet.EndFire ();
		levelScript.ballsCount--;
		
		if (transform.childCount > 0)
			gameplay.audioManager.newBall.Play ();
		
		for (int i = transform.childCount - 1; i >= 0; i--) {
			Ball ball = transform.GetChild (i).GetComponent<Ball> ();
			ball.transform.parent = null;
			int balldir = i == 0 ? 1 : -1;
			ball.StartWithParams (frustomWidth, frustumHeight, balldir, false);
			levelScript.ballsCount++;
		}
		
		if (levelScript.ballsCount == 0) {
			levelScript.EndLevelSuccess ();
			gameplay.EndLevelSuccess ();
		}
	}
}
