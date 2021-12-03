using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private Rigidbody rig;
	private Vector3 velocity;
	private Vector3 yVelocity;
	[SerializeField]
	private float walkSpeed;
	private Animator animator;
	Quaternion targetRotation;
	[SerializeField]
	private GroundCheck groundCheck;
	public bool isGround = true;
	public float Multiplier;

	void Start()
	{
		rig = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();

		targetRotation = transform.rotation;
	}

	void Update()
	{
		var rotationSpeed = 600 * Time.deltaTime;
		var horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);

		velocity = horizontalRotation * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
		yVelocity = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical"));

		if (isGround)
		{
			animator.SetBool("Ground", true);
			animator.SetBool("inAir", false);
		}
        else
        {
			animator.SetBool("inAir", true);
			animator.SetBool("Ground", false);
			animator.SetFloat("speed", 0f);
			Debug.Log("a");
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

			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

		if (isGround)
		{
			if (yVelocity.y > 0.5)
			{
				isGround = false;
				animator.SetBool("Ground", false);
				Multiplier = -20;
				animator.SetBool("jump",true);
			}
			else
			{
				Multiplier = 25;
				animator.SetBool("jump", false);
			}

			if (isGround == false)
			{
				Multiplier = 10 * Time.deltaTime;
			}
		}
	}

    private void FixedUpdate()
    {
		rig.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
		
		isGround = groundCheck.IsGround();
    }
}
