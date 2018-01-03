using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float speed;
	public float laneSpeed;
	public float jumpLength;
	public float jumpHeight;
	public float slideLength;
	public int maxLife = 3;
	public float minSpeed = 10f;
	public float maxSpeed = 30f;
	public float invincibleTime;
	public GameObject model;

	private Animator anim;
	private Rigidbody rb;
	private BoxCollider boxCollider;
	private int currentLane = 1;
	private Vector3 verticalTargetPosition;
	private bool jumping = false;
	private float jumpStart;
	private bool sliding = false;
	private float slideStart;
	private Vector3 boxColliderSize;
	private bool isSwipping = false;
	private Vector2 startingTouch;
	private int currentLife;
	private bool invincible = false;
	static int blinkingValue;
	private UIManager uiManager;
	[HideInInspector]
	public int coins;
	[HideInInspector]
	public float score;

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody>();
		anim = GetComponentInChildren<Animator>();
		boxCollider = GetComponent<BoxCollider>();
		boxColliderSize = boxCollider.size;
		anim.Play("runStart");
		currentLife = maxLife;
		speed = minSpeed;
		blinkingValue = Shader.PropertyToID("_BlinkingValue");
		uiManager = FindObjectOfType<UIManager>();
		GameManager.gm.StartMissions();
	}
	
	// Update is called once per frame
	void Update () {

		score += Time.deltaTime * speed;
		uiManager.UpdateScore((int)score);

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			ChangeLane(-1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			ChangeLane(1);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			Jump();
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			Slide();
		}

		if(Input.touchCount == 1)
		{
			if (isSwipping)
			{
				Vector2 diff = Input.GetTouch(0).position - startingTouch;
				diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);
				if(diff.magnitude > 0.01f)
				{
					if(Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
					{
						if(diff.y < 0)
						{
							Slide();
						}
						else
						{
							Jump();
						}
					}
					else
					{
						if(diff.x < 0)
						{
							ChangeLane(-1);
						}
						else
						{
							ChangeLane(1);
						}
					}

					isSwipping = false;
				}
			}

			if (Input.GetTouch(0).phase == TouchPhase.Began)
			{
				startingTouch = Input.GetTouch(0).position;
				isSwipping = true;
			}
			else if (Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				isSwipping = false;
			}
		}

		

		if (jumping)
		{
			float ratio = (transform.position.z - jumpStart) / jumpLength;
			if(ratio >= 1f)
			{
				jumping = false;
				anim.SetBool("Jumping", false);
			}
			else
			{
				verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
			}
		}
		else
		{
			verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, 5 * Time.deltaTime);
		}

		if (sliding)
		{
			float ratio = (transform.position.z - slideStart) / slideLength;
			if(ratio >= 1f)
			{
				sliding = false;
				anim.SetBool("Sliding", false);
				boxCollider.size = boxColliderSize;
			}
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

	void Jump()
	{
		if (!jumping)
		{
			jumpStart = transform.position.z;
			anim.SetFloat("JumpSpeed", speed / jumpLength);
			anim.SetBool("Jumping", true);
			jumping = true;
		}
	}

	void Slide()
	{
		if(!jumping && !sliding)
		{
			slideStart = transform.position.z;
			anim.SetFloat("JumpSpeed", speed / slideLength);
			anim.SetBool("Sliding", true);
			Vector3 newSize = boxCollider.size;
			newSize.y = newSize.y / 2;
			boxCollider.size = newSize;
			sliding = true;

		}
	}

	private void OnTriggerEnter(Collider other)
	{

		if (other.CompareTag("Coin"))
		{
			coins++;
			uiManager.UpdateCoins(coins);
			other.transform.parent.gameObject.SetActive(false);
		}

		if (invincible)
			return;

		if (other.CompareTag("Obstacle"))
		{
			currentLife--;
			uiManager.UpdateLives(currentLife);
			anim.SetTrigger("Hit");
			speed = 0;
			if(currentLife <= 0)
			{
				speed = 0;
				anim.SetBool("Dead", true);
				uiManager.gameOverPanel.SetActive(true);
				Invoke("CallMenu", 2f);
			}
			else
			{
				StartCoroutine(Blinking(invincibleTime));
			}
		}
	}

	IEnumerator Blinking(float time)
	{
		invincible = true;
		float timer = 0;
		float currentBlink = 1f;
		float lastBlink = 0;
		float blinkPeriod = 0.1f;
		bool enabled = false;
		yield return new WaitForSeconds(1f);
		speed = minSpeed;
		while(timer < time && invincible)
		{
			model.SetActive(enabled);
			//Shader.SetGlobalFloat(blinkingValue, currentBlink);
			yield return null;
			timer += Time.deltaTime;
			lastBlink += Time.deltaTime;
			if(blinkPeriod < lastBlink)
			{
				lastBlink = 0;
				currentBlink = 1f - currentBlink;
				enabled = !enabled;
			}
		}
		model.SetActive(true);
		//Shader.SetGlobalFloat(blinkingValue, 0);
		invincible = false;
	}

	void CallMenu()
	{
		GameManager.gm.coins += coins;
		GameManager.gm.EndRun();
	}

	public void IncreaseSpeed()
	{
		speed *= 1.15f;
		if (speed >= maxSpeed)
			speed = maxSpeed;
	}
}
