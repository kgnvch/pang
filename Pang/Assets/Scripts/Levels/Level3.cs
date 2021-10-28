using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
	This is the script for level 2.
*/
public class Level3 : LevelScript
{	
	public override void StartWithParams(float frustumWidth, float frustumHeight)
	{
		ballsCount = balls.Length;
		StartCoroutine (_SetBalls (frustumWidth, frustumHeight));
	}
	
	private IEnumerator _SetBalls(float frustumWidth, float frustumHeight)
	{
		System.Random rand = new System.Random();
		int dir = rand.Next (2) == 0 ? 1 : -1;
		balls [0].StartWithParams (frustumWidth, frustumHeight, dir);
		yield return new WaitForSeconds (nextBallSpawnWait);
		balls [1].StartWithParams (frustumWidth, frustumHeight, -dir);
		yield return new WaitForSeconds (nextBallSpawnWait);
		balls [2].StartWithParams (frustumWidth, frustumHeight, dir);
		yield break;
	}
}
