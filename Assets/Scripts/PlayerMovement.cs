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

    //Vector3 attachedWorldPosition;
    //Transform belowPlayerTransform;
    //Vector3 positionRelativeToBelowPlayer;

    Rigidbody connectedBody, previousConnectedBody;

    Vector3 connectionWorldPosition, connectionLocalPosition;

    Character character;

    // Coyote time
    [SerializeField] float extraJumpWindow = .1f;
    float extraJumpTimer;
    bool jumped = false;

    [SerializeField] Collider playerCollider;

    [SerializeField] Collider initialBackpackCollider;

    Collider currentBackpackCollider;

    bool backpackGrounded = false;

    // Flag used to avoid double jump glitch
    bool justJumped = false;

    bool wearingBackpack = true;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        character = GetComponent<Character>();

        currentBackpackCollider = initialBackpackCollider;
    }

    // Start is called before the first frame update
    void Start()
    {
        extraJumpTimer = extraJumpWindow;
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

        previousConnectedBody = connectedBody;
        connectedBody = null;
    }

    void Move()
    {
        // Adjusted target velocity is used to add the velocity of a player we're standing on
        Vector3 adjustedTargetVelocity = targetVelocity;
        if (onPlayerBackpack)
        {
            //rigidBody.velocity = new Vector3(belowPlayersVelocity.x, rigidBody.velocity.y, belowPlayersVelocity.z);
            adjustedTargetVelocity += belowPlayersVelocity;

            // Subtract position relative to below player from our transform when we last had a collision update, and add it to the adjusted target velocity.
            //Vector3 connectionMovement = belowPlayerTransform.TransformPoint(positionRelativeToBelowPlayer) - attachedWorldPosition;

            //adjustedTargetVelocity += connectionMovement;
        }

        Vector3 newVelocity = rigidBody.velocity;
        float acceleration = grounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedDelta = acceleration * Time.deltaTime;

        newVelocity.x = Mathf.MoveTowards(newVelocity.x, adjustedTargetVelocity.x, maxSpeedDelta);
        newVelocity.z = Mathf.MoveTowards(newVelocity.z, adjustedTargetVelocity.z, maxSpeedDelta);
        rigidBody.velocity = newVelocity;


        if (grounded)
        {
            extraJumpTimer = extraJumpWindow;
            jumped = false;
        }
        else
        {
            extraJumpTimer -= Time.fixedDeltaTime;
        }

        // Reset flag
        justJumped = false;
        
        if (CanJump() && tryJump)
        {
            jumped = true;
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpSpeed, rigidBody.velocity.z);
            character.JumpSound();
            animator.SetTrigger("Jumped");
            
            // Flag that prevents the player from being marked as grounded on the same timestep as a jump
            justJumped = true;
        }

        // Manual gravity for more control
        Vector3 gravityVector = new Vector3(0, -gravity * rigidBody.mass, 0);
        rigidBody.AddForce(gravityVector * Time.deltaTime, ForceMode.VelocityChange);

        grounded = false;
        backpackGrounded = false;
        tryJump = false;
    }

    private bool CanJump()
    {
        return (grounded || (extraJumpTimer >= 0)) && !jumped;
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
            // Avoid null reference if collided backpack is dropped
            if (collision.rigidbody == null)
            {
                return;
            }

            // TODO: This is triggering when players just bump into each other for some reason.
            // I'll try fixing it by checking the normal, but this shouldn't be necessary.
            // Also should maybe switch these to triggers and just use the backpack for collision
            Vector3 normal = collision.GetContact(0).normal;
            if (normal.y >= 0.9f)
            {
                onPlayerBackpack = true;
                //attachedWorldPosition = rigidBody.position;
                //belowPlayerTransform = collision.transform;
                belowPlayersVelocity = collision.rigidbody.velocity;
                //positionRelativeToBelowPlayer = collision.transform.InverseTransformPoint(transform.position);

                connectedBody = collision.rigidbody;
                if (connectedBody == previousConnectedBody)
                {
                    Vector3 connectionMovement =
                        connectedBody.transform.TransformPoint(connectionLocalPosition) -
                        connectionWorldPosition;
                    belowPlayersVelocity = connectionMovement / Time.deltaTime;
                }
                connectionWorldPosition = rigidBody.position;
                connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
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
            // Avoid null reference if collided backpack is dropped
            if (collision.rigidbody == null)
            {
                return;
            }

            Vector3 normal = collision.GetContact(0).normal;

            if (normal.y >= 0.9f)
            {
                //attachedWorldPosition = rigidBody.position;
                //belowPlayerTransform = collision.transform;
                belowPlayersVelocity = collision.rigidbody.velocity;
                //positionRelativeToBelowPlayer = collision.transform.InverseTransformPoint(attachedWorldPosition);
                connectedBody = collision.rigidbody;
                if (connectedBody == previousConnectedBody)
                {
                    Vector3 connectionMovement =
                        connectedBody.transform.TransformPoint(connectionLocalPosition) -
                        connectionWorldPosition;
                    belowPlayersVelocity = connectionMovement / Time.deltaTime;
                }
                connectionWorldPosition = rigidBody.position;
                connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
            }
        }
    }

    private void EvaluateGroundCollision(Collision collision)
    {
        // Delay checking for ground collision if this is the same timestep that the player jumped.
        // This avoids a double jump glitch.
        if (justJumped == true)
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            Collider collider = collision.GetContact(i).thisCollider;

            // If the player's main collider is touching the ground
            if (collider == playerCollider)
            {
                Vector3 normal = collision.GetContact(i).normal;
                grounded |= normal.y >= 0.9f;
            }

            // If the player's backpack is touching the ground
            if (collider == currentBackpackCollider)
            {
                
                Vector3 normal = collision.GetContact(i).normal;
                backpackGrounded |= normal.y >= 0.9f;
            }
        }
    }

    public void Enable()
    {
        enable = true;

        // Reset for deterministic replays
        targetVelocity = Vector3.zero;
        rigidBody.velocity = Vector3.zero;
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

    public bool IsHanging()
    {
        return !grounded && backpackGrounded && wearingBackpack;
    }

    public void DroppedBackpack()
    {
        wearingBackpack = false;
    }

    public void PickedUpBackpack(Backpack backpack)
    {
        wearingBackpack = true;

        currentBackpackCollider = backpack.gameObject.GetComponentInChildren<Collider>();
    }
}
