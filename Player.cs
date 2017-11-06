using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float speed;
	public float laneSpeed;

	private Rigidbody rb;
	private int currentLane = 1;
	private Vector3 verticalTargetPosition;

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody>();

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			ChangeLane(-1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			ChangeLane(1);
		}

		Vector3 targetPosition = new Vector3(verticalTargetPosition.x, verticalTargetPosition.y, transform.position.z);
		transform.position = Vector3.MoveTowards(transform.position, targetPosition, laneSpeed * Time.deltaTime);

	}

	private void FixedUpdate()
	{
		rb.velocity = Vector3.forward * speed;
	}

	void ChangeLane(int direction)
	{
		int targetLane = currentLane + direction;
		if (targetLane < 0 || targetLane > 2)
			return;
		currentLane = targetLane;
		verticalTargetPosition = new Vector3((currentLane - 1), 0, 0);
	}
}
