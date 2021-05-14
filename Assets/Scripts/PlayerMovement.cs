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
    float Vertical = 0;
    float Horizontal = 0;
    bool tryJump = false;


    bool grounded = false;

    Vector3 velocity, targetVelocity;

    float horizontalAnimationBlend = 0f;
    float verticalAnimationBlend = 0f;

    [SerializeField]
    float animationBlendDelta = .1f;

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
        Vertical = Input.GetAxisRaw("Vertical");

        Horizontal = Input.GetAxisRaw("Horizontal");

        horizontalAnimationBlend = Mathf.MoveTowards(horizontalAnimationBlend, Horizontal, animationBlendDelta * Time.deltaTime);
        verticalAnimationBlend = Mathf.MoveTowards(verticalAnimationBlend, Vertical, animationBlendDelta * Time.deltaTime);

        animator.SetFloat("Vertical", verticalAnimationBlend);
        animator.SetFloat("Horizontal", horizontalAnimationBlend);

        if (Input.GetButtonDown("Jump"))
        {
            tryJump = true;
        }

        Vector3 moveDirection = (transform.right * Horizontal + transform.forward * Vertical);

        moveDirection.Normalize();
        targetVelocity = moveDirection * maxSpeed;

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
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
}
