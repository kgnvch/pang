using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
	This is the script for level 1.
*/
public class Level1 : LevelScript
{	
	public override void StartWithParams(float frustumWidth, float frustumHeight)
	{
		ballsCount = balls.Length;
		
		//Set ball 1 at a random side
		System.Random rand = new System.Random();
		int dir = rand.Next (2) == 0 ? 1 : -1;
		balls [0].StartWithParams (frustumWidth, frustumHeight, dir);
	}
}
