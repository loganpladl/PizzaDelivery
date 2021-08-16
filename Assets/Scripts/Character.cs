using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    PlayerMovement movementComponent;
    Rigidbody rigidbodyComponent;

    [SerializeField]
    Animator animatorComponent;

    [SerializeField]
    MouseLook mouseLook;

    [SerializeField]
    SkinnedMeshRenderer characterMesh;

    [SerializeField]
    MeshRenderer characterBackpackMesh;

    [SerializeField]
    AudioClip[] footstepSounds;

    [SerializeField]
    AudioSource footstepAudioSource;

    [SerializeField]
    AudioSource mouthAudioSource;

    [SerializeField]
    AudioClip jumpSound;

    [SerializeField]
    AudioClip landSound;

    bool enable = false;

    bool isActiveCharacter = false;

    LevelState levelState;

    [SerializeField] RewindTarget rewindTarget;

    // Prevent duplicate footsteps
    float footstepSoundTimer = 0;
    [SerializeField]
    float footstepSoundIncrement = .05f;

    // grounded status on last update
    bool wasGrounded = true;

    // Only play landing sound if you were in the air for some number of seconds
    float inAirTimer;
    [SerializeField]
    float secondsInAirForLandSound = .25f;

    Vector3 initialRigidbodyPosition;
    Quaternion initialRigidbodyRotation;

    private void Awake()
    {
        movementComponent = GetComponent<PlayerMovement>();
        rigidbodyComponent = GetComponent<Rigidbody>();

        initialRigidbodyPosition = rigidbodyComponent.position;
        initialRigidbodyRotation = rigidbodyComponent.rotation;

        rewindTarget.OnRewindStart += OnRewindStarted;
        rewindTarget.OnRewindEnded += OnRewindEnded;
        rewindTarget.CreateNewTimePoint += CreateNewTimePoint;
        rewindTarget.Rewinding += RewindStep;
    }

    // Start is called before the first frame update
    void Start()
    {
        levelState = GameObject.FindGameObjectsWithTag("LevelState")[0].GetComponent<LevelState>();

        

        inAirTimer = secondsInAirForLandSound;
    }

    // Update is called once per frame
    void Update()
    {
        if (enable)
        {
            footstepSoundTimer -= Time.deltaTime;

            if (wasGrounded)
            {
                inAirTimer = secondsInAirForLandSound;
            }
            else
            {
                inAirTimer -= Time.deltaTime;
                if (movementComponent.IsGrounded() && inAirTimer <= 0)
                {
                    footstepAudioSource.PlayOneShot(landSound, .1f);
                    inAirTimer = secondsInAirForLandSound;
                }
            }

            wasGrounded = movementComponent.IsGrounded();
        }
    }

    private void FixedUpdate()
    {
        
    }

    TimePoint CreateNewTimePoint()
    {
        return new TimePoint(transform.position, transform.rotation, mouseLook.GetVerticalRotation());
    }

    void OnRewindStarted()
    {
        //movementComponent.Disable();
        rigidbodyComponent.isKinematic = true;
        mouseLook.Disable();
        animatorComponent.StartPlayback();
    }

    void OnRewindEnded(TimePoint initialTimePoint)
    {
        mouseLook.SetVerticalRotation(initialTimePoint.cameraVerticalRotation);
        //movementComponent.Enable();
        rigidbodyComponent.isKinematic = false;
        mouseLook.Enable();
        animatorComponent.StopPlayback();
    }

    void RewindStep(float progress, TimePoint timePoint)
    {
        animatorComponent.playbackTime = progress * (animatorComponent.recorderStopTime - animatorComponent.recorderStartTime);
        mouseLook.SetVerticalRotation(timePoint.cameraVerticalRotation);
    }

    public void UpdateMove(float horizontal, float vertical)
    {
        movementComponent.UpdateMove(horizontal, vertical);
    }

    public void UpdateLook(float mouseX, float mouseY)
    {
        mouseLook.UpdateLook(mouseX, mouseY);
    }

    

    // Used by input manager when it runs out of commands to prevent continued movement/animations
    public void StopMovementAndAnimations()
    {
        movementComponent.StopMovementAndAnimations();
    }

    // Used by level state when player knocks
    public void CancelVelocity()
    {
        movementComponent.CancelVelocity();
    }

    

    // Enabled at the beginning of each loop
    public void Enable()
    {
        enable = true;
        movementComponent.Enable();
        mouseLook.Enable();
        animatorComponent.StartRecording(0);

        rewindTarget.Enable();

        // Trying to reset rigidbody position and rotation at the start of each loop for determinism
        rigidbodyComponent.position = initialRigidbodyPosition;
        rigidbodyComponent.rotation = initialRigidbodyRotation;
        rigidbodyComponent.velocity = Vector3.zero;
    }

    // Disabled at the end of each loop
    public void Disable()
    {
        enable = false;
        movementComponent.Disable();
        mouseLook.Disable();
        animatorComponent.StopRecording();

        rewindTarget.Disable();
    }

    public void SetActiveCharacter()
    {
        isActiveCharacter = true;
        characterMesh.enabled = false;
        characterBackpackMesh.enabled = false;
        mouseLook.SetCameraActive();
        mouseLook.EnableRaycasting();
    }

    public void SetNotActiveCharacter()
    {
        isActiveCharacter = false;
        characterMesh.enabled = true;
        characterBackpackMesh.enabled = true;
        mouseLook.SetCameraInactive();
        mouseLook.DisableRaycasting();
    }

    public void TryJump()
    {
        movementComponent.TryJump();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OutOfBounds"))
        {
            if (isActiveCharacter && enable)
            {
                levelState.EarlyRewind();
            }
        }
    }

    public void FootstepSound()
    {
        if (enable && movementComponent.IsGrounded() && footstepSoundTimer <= 0)
        {
            int index = Random.Range(0, footstepSounds.Length);

            footstepAudioSource.PlayOneShot(footstepSounds[index], .6f);
            footstepSoundTimer = footstepSoundIncrement;
        }
    }

    public void JumpSound()
    {

        mouthAudioSource.clip = jumpSound;
        mouthAudioSource.volume = .7f;
        mouthAudioSource.Play();
    }

    public void LandSound()
    {
        //footstepAudioSource.PlayOneShot(landSound);
    }
}
