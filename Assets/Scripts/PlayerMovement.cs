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

    bool onPlayerBackpack = false;
    Vector3 belowPlayersVelocity;

    Character character;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        character = GetComponent<Character>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        animator.SetBool("Grounded", grounded);
    }

    // Update movement to be done in next fixed update according to horizontal/vertical input
    public void UpdateMove(float horizontal, float vertical)
    {
        if (enable)
        {
            horizontalAnimationBlend = Mathf.MoveTowards(horizontalAnimationBlend, horizontal, animationBlendDelta * Time.deltaTime);
            verticalAnimationBlend = Mathf.MoveTowards(verticalAnimationBlend, vertical, animationBlendDelta * Time.deltaTime);

            // TODO: do i need to do this in here or can I move it to Update()
            animator.SetFloat("Vertical", verticalAnimationBlend);
            animator.SetFloat("Horizontal", horizontalAnimationBlend);

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
        // Adjusted target velocity is used to add the velocity of a player we're standing on
        Vector3 adjustedTargetVelocity = targetVelocity;
        if (onPlayerBackpack)
        {
            //rigidBody.velocity = new Vector3(belowPlayersVelocity.x, rigidBody.velocity.y, belowPlayersVelocity.z);
            adjustedTargetVelocity += belowPlayersVelocity;
        }

        Vector3 newVelocity = rigidBody.velocity;
        float acceleration = grounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedDelta = acceleration * Time.deltaTime;

        newVelocity.x = Mathf.MoveTowards(newVelocity.x, adjustedTargetVelocity.x, maxSpeedDelta);
        newVelocity.z = Mathf.MoveTowards(newVelocity.z, adjustedTargetVelocity.z, maxSpeedDelta);
        rigidBody.velocity = newVelocity;

        

        if (grounded && tryJump)
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpSpeed, rigidBody.velocity.z);
            character.JumpSound();
            animator.SetTrigger("Jumped");
        }

        // Manual gravity for more control
        Vector3 gravityVector = new Vector3(0, -gravity * rigidBody.mass, 0);
        rigidBody.AddForce(gravityVector * Time.deltaTime, ForceMode.VelocityChange);

        grounded = false;
        tryJump = false;
    }

    // Used by input manager when it runs out of commands to prevent continued movement/animations
    public void StopMovementAndAnimations()
    {
        targetVelocity = Vector3.zero;

        animator.SetFloat("Vertical", 0);
        animator.SetFloat("Horizontal", 0);
        animator.SetBool("Grounded", grounded);
    }

    // Cancel rigidbody velocity. Used when the player knocks on the door.
    public void CancelVelocity()
    {
        rigidBody.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateGroundCollision(collision);

        if (collision.collider.CompareTag("BackpackFloor"))
        {
            // TODO: This is triggering when players just bump into each other for some reason.
            // I'll try fixing it by checking the normal, but this shouldn't be necessary.
            // Also should maybe switch these to triggers and just use the backpack for collision
            Vector3 normal = collision.GetContact(0).normal;
            if (normal.y >= 0.9f)
            {
                onPlayerBackpack = true;
                belowPlayersVelocity = collision.rigidbody.velocity;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("BackpackFloor"))
        {
            onPlayerBackpack = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateGroundCollision(collision);

        // TODO: This is triggering when players just bump into each other for some reason.
        // I'll try fixing it by checking the normal, but this shouldn't be necessary.
        if (collision.collider.CompareTag("BackpackFloor"))
        {
            Vector3 normal = collision.GetContact(0).normal;
            if (normal.y >= 0.9f)
            {
                belowPlayersVelocity = collision.rigidbody.velocity;
            }
        }
    }

    private void EvaluateGroundCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            grounded |= normal.y >= 0.9f;
        }
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

    public bool IsGrounded()
    {
        return grounded;
    }
}
