using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
	This is the script for a bullet.
*/
public class Bullet : MonoBehaviour
{
	public float speed;
	public ParticleSystem explosion;
	public AudioManager audioManager;
	
	public void Fire()
	{
		gameObject.SetActive (true);
		GetComponent<Rigidbody> ().velocity = new Vector3 (0, speed, 0);
	}
	
	public void EndFire()
	{
		audioManager.fire.Stop ();
		audioManager.explosion.Play ();
		gameObject.SetActive (false);
		explosion.transform.position = transform.position;
		explosion.gameObject.SetActive (false);
		explosion.gameObject.SetActive (true);
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.CompareTag("Ceiling"))
			EndFire ();
	}
}
