using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private Rigidbody rig;
	private Vector3 velocity;
	private Vector3 yVelocity;
	private Vector3 airVelocity;
	private Animator animator;
	Quaternion targetRotation;
	[SerializeField]
	private GroundCheck groundCheck;
	public bool isGround = false;
	private bool inAir = false;
	private bool rightHook;
	private bool leftHook;
	public float AirTime;
	private float inTime = 0;
	public float Multiplier;
	public float AirSpeed;
	private Vector3 AirVelo;
	public RightGrapplingHand rightHand;
	public LeftGrapplingHand leftHand;

	private void Awake(){
		isGround = true;
    }

    void Start(){
		rig = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();

		targetRotation = transform.rotation;
		AirVelo = Vector3.zero;
	}

	void Update()
	{
		if (isGround == false)
		{
			inTime += Time.deltaTime;
		}

		var rotationSpeed = 600 * Time.deltaTime;
		var horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
		var horizontalAirRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);


		velocity = horizontalRotation * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
		airVelocity = horizontalAirRotation * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
		yVelocity = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical"));

		if (isGround)
		{
			animator.SetBool("Ground", true);
			rightHook = false;
			leftHook = false;
			AirVelo = Vector3.zero;
		}
		else
		{
			animator.SetBool("Ground", false);
			animator.SetFloat("speed", 0f);
			inAir = true;
		}

		if (velocity.magnitude > 0.9f)
		{
			animator.SetFloat("speed", velocity.magnitude);
			transform.LookAt(transform.position + velocity);
			targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
		}
		else
		{
			animator.SetFloat("speed", 0f);
		}

		if (inAir)
		{
			if (airVelocity.magnitude > 0.9f)
			{
				animator.SetFloat("AirSpeed", airVelocity.magnitude);
				transform.LookAt(transform.position + airVelocity);
				targetRotation = Quaternion.LookRotation(airVelocity, Vector3.up);
				airVelocity += AirVelo.normalized * AirSpeed;
			}

			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

			if (isGround)
			{
				if (yVelocity.y > 0.5)
				{
					isGround = false;
					animator.SetBool("Ground", false);
					animator.SetBool("jump", true);
				}
				else if (yVelocity.y == 0)
				{
					animator.SetBool("jump", false);
				}
			}
			Debug.Log(rightHand);
			Debug.Log(leftHand);
			if (rightHook == true || leftHook == true && inTime >= AirTime)
			{
				inTime = AirTime;
				Multiplier = 50 * 5;
			}
			else if (isGround == true)
			{
				Multiplier = 25;
				inTime = 0;
			}
		}
	}

	private void FixedUpdate()
	{
		rig.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
		isGround = groundCheck.IsGround();

		rightHook = rightHand.DetachCheck();
		leftHook = leftHand.DetachCheck();
	}
}
