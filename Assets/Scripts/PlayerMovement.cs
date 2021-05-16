using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float maxSpeed = 50.0f;

    [SerializeField]
    float maxAcceleration = 10.0f;

    [SerializeField]
    float maxAirAcceleration = 2.5f;

    [SerializeField]
    float gravity = 10.0f;

    [SerializeField]
    float jumpSpeed = 15.0f;

    [SerializeField]
    Animator animator;

    // Cached references
    Rigidbody rigidBody;

    // For input
    bool tryJump = false;


    bool grounded = false;

    Vector3 velocity, targetVelocity;

    float horizontalAnimationBlend = 0f;
    float verticalAnimationBlend = 0f;

    [SerializeField]
    float animationBlendDelta = .1f;

    bool enable = true;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {

    }

    // Update movement to be done in next fixed update according to horizontal/vertical input
    public void UpdateMove(float horizontal, float vertical)
    {
        if (enable)
        {
            horizontalAnimationBlend = Mathf.MoveTowards(horizontalAnimationBlend, horizontal, animationBlendDelta * Time.deltaTime);
            verticalAnimationBlend = Mathf.MoveTowards(verticalAnimationBlend, vertical, animationBlendDelta * Time.deltaTime);

            animator.SetFloat("Vertical", verticalAnimationBlend);
            animator.SetFloat("Horizontal", horizontalAnimationBlend);
            animator.SetBool("Grounded", grounded);

            Vector3 moveDirection = (transform.right * horizontal + transform.forward * vertical);

            moveDirection.Normalize();
            targetVelocity = moveDirection * maxSpeed;
        }
    }

    void FixedUpdate()
    {
        if (enable)
        {
            Move();
        }
    }

    void Move()
    {
        Vector3 newVelocity = rigidBody.velocity;
        float acceleration = grounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedDelta = acceleration * Time.deltaTime;

        newVelocity.x = Mathf.MoveTowards(newVelocity.x, targetVelocity.x, maxSpeedDelta);
        newVelocity.z = Mathf.MoveTowards(newVelocity.z, targetVelocity.z, maxSpeedDelta);
        rigidBody.velocity = newVelocity;

        if (grounded && tryJump)
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpSpeed, rigidBody.velocity.z);
            animator.SetTrigger("Jumped");
        }

        // Manual gravity for more control
        Vector3 gravityVector = new Vector3(0, -gravity * rigidBody.mass, 0);
        rigidBody.AddForce(gravityVector * Time.deltaTime, ForceMode.VelocityChange);

        grounded = false;
        tryJump = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        grounded = true;
    }

    public void Enable()
    {
        enable = true;
    }

    public void Disable()
    {
        enable = false;
    }

    public void TryJump()
    {
        tryJump = true;
    }
}
