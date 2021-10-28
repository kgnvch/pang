using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
	This script handles operations specific for each level.
*/
public abstract class LevelScript : MonoBehaviour
{	
	public Ball[] balls;
	public int ballsCount { get; set; }
	public FadeText levelPassed;
	public float nextBallSpawnWait;
	
	public abstract void StartWithParams(float frustumWidth, float frustumHeight);
	
	public void EndLevelSuccess()
	{
		levelPassed.ShowAndHideCanvas ();
	}
}
