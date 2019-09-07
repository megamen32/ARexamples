﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


public class BulletScript : MonoBehaviour 
{

	public GameObject playerOwner;
	Vector3 startSize;
	public float MaxDistToTravel;

	

	void Start()
	{
		startSize = transform.localScale;
		
	}

	void Update()
	{
		if (playerOwner != null)
		{
			transform.localScale = Vector3.Lerp(Vector3.one * 1f / 5f, startSize + startSize.z * Vector3.forward,
				Vector3.Distance(playerOwner.transform.position, transform.position) / MaxDistToTravel );
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		var hitObject = collision.gameObject;
		if (playerOwner != null && hitObject == playerOwner) 
		{
			Debug.Log("Colision with the same object: " + hitObject.name);
			return;
		}
		
		var health = hitObject.GetComponent<PlayerHealth>();

		if (health  != null)
		{
			Debug.Log(hitObject.name + " takes damage");
			health.TakeDamage(10);
		} 

		Destroy(gameObject);
		
	}
}
