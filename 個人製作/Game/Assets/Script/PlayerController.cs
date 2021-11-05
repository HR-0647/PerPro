using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private Rigidbody rig;
	private Vector3 velocity;
	private Vector3 yVelo;
	[SerializeField]
	private float walkSpeed;
	private Animator animator;
	Quaternion targetRotation;
	private bool isGround = false;

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
		yVelo = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical"));

		if (velocity.magnitude > 0.4f)
		{
			animator.SetFloat("speed", velocity.magnitude);
			transform.LookAt(transform.position + velocity);
			targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
		}
		else
		{
			animator.SetFloat("speed", 0f);
		}

		velocity.y = Physics.gravity.y * Time.deltaTime;
		rig.AddForce(velocity * walkSpeed, ForceMode.Force);
		rig.AddForce(yVelo * walkSpeed, ForceMode.Force);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

		if (yVelo.y > 0.5)
        {
			isGround = false;
			animator.SetTrigger("jump");
		}
	}

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
			isGround = true;
        }
    }
}
