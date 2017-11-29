using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	private Transform player;
	private Vector3 offset;

	// Use this for initialization
	void Start () {

		player = GameObject.FindGameObjectWithTag("Player").transform;
		offset = transform.position - player.position;

	}
	
	// Update is called once per frame
	void LateUpdate () {

		Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, player.position.z + offset.z);
		transform.position = newPosition;

	}
}
