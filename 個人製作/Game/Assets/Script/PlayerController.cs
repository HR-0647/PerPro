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
	public bool isGround = false;
	public float Multiplier = 1f;

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


			if (velocity.magnitude > 0.9f) 
			{
				animator.SetFloat("speed", velocity.magnitude);
				animator.SetBool("Ground", true);
				transform.LookAt(transform.position + velocity);
				targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
			}
			else
			{
				animator.SetFloat("speed", 0f);
				animator.SetBool("Ground", true);
			}
		
		
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

		//Debug.Log(isGround);
		if (yVelo.y > 0.5) 
		{
			//isGround = false;
			Multiplier = -20;
			animator.SetTrigger("jump");
			animator.SetBool("inAir", true);
		}
		else
		{
			//isGround = true;
			animator.ResetTrigger("jump");
			Multiplier = 25;
		}
		
		if(isGround == false)
        {
			Multiplier = 300;
        }
        else
        {
			Multiplier = 25;
        }
	}

    private void FixedUpdate()
    {
		rig.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {
		string name = collision.gameObject.name;
        if(name=="Plane")
        {
			isGround = true;
			animator.SetBool("Ground", true);
        }
  //      else
  //      {
		//	isGround = false;
		//	animator.SetBool("Ground", false);
		//	animator.SetBool("inAir", true);
		//}
		//Debug.Log(name);
	}
}
