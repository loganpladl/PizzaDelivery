using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    bool rewinding = false;

    TimePoint[] timePoints;

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

    // Set by LevelState. Used to rewind correctly.
    float maxRewindDuration;
    int totalSteps;
    int currentStep = 0;

    float rewindTimer = 0;

    bool enable = false;

    bool isActiveCharacter = false;

    LevelState levelState;

    // Always revert to initial point at the end to ensure consistency
    TimePoint initialPoint;

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

    private void Awake()
    {
        movementComponent = GetComponent<PlayerMovement>();
        rigidbodyComponent = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        levelState = GameObject.FindGameObjectsWithTag("LevelState")[0].GetComponent<LevelState>();

        initialPoint = new TimePoint(transform.position, transform.rotation, mouseLook.GetVerticalRotation());

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
        if (rewinding)
        {
            RewindStep();
            rewindTimer += Time.deltaTime;
        }
        if (enable)
        {
            RecordStep();
        }
    }

    public void UpdateMove(float horizontal, float vertical)
    {
        movementComponent.UpdateMove(horizontal, vertical);
    }

    public void UpdateLook(float mouseX, float mouseY)
    {
        mouseLook.UpdateLook(mouseX, mouseY);
    }

    public void StartRewind(float rewindDuration)
    {
        //movementComponent.Disable();
        rigidbodyComponent.isKinematic = true;
        mouseLook.Disable();
        rewinding = true;

        rewindTimer = maxRewindDuration - rewindDuration;

        animatorComponent.StartPlayback();
    }

    public void StopRewind()
    {
        // Return to initial time point for consistency
        transform.position = initialPoint.position;
        transform.rotation = initialPoint.rotation;
        mouseLook.SetVerticalRotation(initialPoint.cameraVerticalRotation);


        //movementComponent.Enable();
        rigidbodyComponent.isKinematic = false;
        mouseLook.Enable();
        rewinding = false;

        currentStep = 0;

        animatorComponent.StopPlayback();
    }

    private void RewindStep()
    {
        float frac = 1 - (rewindTimer / maxRewindDuration);
        // Safety clamp for frac that shouldnt be necessary but just in case
        frac = Mathf.Clamp(frac, 0, 1);

        // TODO: Subtracting 1 to fix off by one error when rewinding. Should look for a better solution.
        currentStep = (int)((totalSteps) * frac) - 1;

        // TODO: is there a better way to avoid these index out of bounds errors?
        if (currentStep >= totalSteps) currentStep = totalSteps - 1;
        if (currentStep < 0) currentStep = 0;

        
        animatorComponent.playbackTime = frac * (animatorComponent.recorderStopTime - animatorComponent.recorderStartTime);

        TimePoint timePoint = timePoints[currentStep];

        transform.position = timePoint.position;
        transform.rotation = timePoint.rotation;
        mouseLook.SetVerticalRotation(timePoint.cameraVerticalRotation);
    }

    private void RecordStep()
    {
        if (currentStep >= totalSteps)
        {
            Debug.Log("Reached current step equal to totalSteps when recording, shouldn't happen");
        }
        else
        {
            timePoints[currentStep] = new TimePoint(transform.position, transform.rotation, mouseLook.GetVerticalRotation());
            currentStep++;
        }
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

    public void SetRewindParameters(int totalSteps, float rewindDuration)
    {
        this.totalSteps = totalSteps;
        this.maxRewindDuration = rewindDuration;

        timePoints = new TimePoint[totalSteps];
    }

    // Enabled at the beginning of each loop
    public void Enable()
    {
        enable = true;
        movementComponent.Enable();
        mouseLook.Enable();
        animatorComponent.StartRecording(0);
    }

    // Disabled at the end of each loop
    public void Disable()
    {
        enable = false;
        movementComponent.Disable();
        mouseLook.Disable();
        animatorComponent.StopRecording();
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
            if (isActiveCharacter)
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
