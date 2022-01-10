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
	public bool isAir = false;
	public float Multiplier;
	public float AirSpeed;
	private Vector3 AirVelo;

	void Start(){
		rig = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();

		targetRotation = transform.rotation;
		AirVelo = Vector3.zero;
	}

	void Update()
	{
		var rotationSpeed = 600 * Time.deltaTime;
		var horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
		var horizontalAirRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);

		velocity = horizontalRotation * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
		airVelocity = horizontalAirRotation * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
		yVelocity = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical"));

		// 地面+アニメーション
		if (isGround)
		{
			isAir = false;
			animator.SetBool("Ground", true);
			animator.SetBool("inAir", false);
			AirVelo = Vector3.zero;
		}
		else
		{
			isAir = true;
			animator.SetBool("Ground", false);
			animator.SetBool("inAir", true);
			animator.SetFloat("speed", 0f);
		}

		if (isGround == true)
		{
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
		}

		if (isAir == true)
		{
			if (airVelocity.magnitude > 0.8f)
			{
				animator.SetFloat("AirSpeed", airVelocity.magnitude);
				transform.LookAt(transform.position + airVelocity);
				targetRotation = Quaternion.LookRotation(airVelocity, Vector3.up);
				transform.Translate(Vector3.forward * Time.deltaTime * 10);
			}
		}

		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

		// ジャンプ
		if (isGround)
		{
			if (yVelocity.y > 0.5)
			{
				isGround = false;
				rig.AddForce(0, 250, 0, ForceMode.VelocityChange);
				animator.SetBool("Ground", false);
				animator.SetBool("jump", true);
			}
			else if (yVelocity.y == 0)
			{
				animator.SetBool("jump", false);
			}
		}

		if (isGround == true)
		{
			Multiplier = 25;
		}
	}

	private void FixedUpdate()
	{
		// 重力
		rig.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);

		isGround = groundCheck.IsGround();
	}
}
