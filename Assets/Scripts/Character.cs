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

    //[SerializeField]
    //GameObject backpackRoot;
    Transform backpackInitialParent;
    bool wearingBackpack = true;
    Vector3 backpackInitialRelativePosition;
    Quaternion backpackInitialRelativeRotation;

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

    [SerializeField]
    AudioSource backpackAudioSource;

    [SerializeField]
    AudioClip backpackDropClip;

    [SerializeField]
    AudioClip backpackPickupClip;

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

    [SerializeField] Backpack initialBackpack;
    Backpack currentBackpack;
    MeshRenderer currentBackpackMesh;

    [SerializeField] float hangingTimeLimit = .25f;
    float hangingTimer;

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

        backpackInitialParent = initialBackpack.transform.parent;
        backpackInitialRelativePosition = initialBackpack.transform.localPosition;
        backpackInitialRelativeRotation = initialBackpack.transform.localRotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject levelStateObject = GameObject.FindWithTag("LevelState");
        if (levelStateObject)
        {
            levelState = levelStateObject.GetComponent<LevelState>();
        }

        inAirTimer = secondsInAirForLandSound;

        mouseLook.tryBackpackPickup = TryBackpackPickup;

        currentBackpack = initialBackpack;

        hangingTimer = hangingTimeLimit;
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
        if (movementComponent.IsHanging() && wearingBackpack)
        {
            hangingTimer -= Time.fixedDeltaTime;

            // Disallow movement while hanging
            if (movementComponent.CanMove())
            {
                movementComponent.SetCannotMove();
            }

            if (mouseLook.CanLook())
            {
                mouseLook.SetCannotLook();
            }

            // Only drop the backpack after hanging for hangingTimeLimit seconds. Provides some leniency to prevent unwanted drops.
            if (hangingTimer <= 0)
            {
                DropBackpack();
            }
        }
        else {
            hangingTimer = hangingTimeLimit;

            // Reallow movement now that the character no longer hanging
            if (!movementComponent.CanMove())
            {
                movementComponent.SetCanMove();
            }

            if (!mouseLook.CanLook())
            {
                mouseLook.SetCanLook();
            }
        }
    }

    private void DropBackpack()
    {
        if (!wearingBackpack)
        {
            return;
        }


        wearingBackpack = false;
        currentBackpack.transform.SetParent(transform.root);

        currentBackpack.Drop();
        movementComponent.DroppedBackpack();

        if (isActiveCharacter)
        {
            // Show pizza dropped message only if this is the active character
            levelState.PizzaDropped();

            currentBackpack.SetNotWornByActiveCharacter();
        }

        mouseLook.SetNotWearingBackpack();

        PlayBackpackDropSound();
    }

    private void PickupBackpack(Backpack backpack)
    {
        currentBackpack = backpack;

        wearingBackpack = true;

        currentBackpack.transform.SetParent(backpackInitialParent);
        currentBackpack.transform.localPosition = backpackInitialRelativePosition;
        currentBackpack.transform.localRotation = backpackInitialRelativeRotation;

        currentBackpack.Pickup();
        movementComponent.PickedUpBackpack(currentBackpack);
        
        if (isActiveCharacter)
        {
            currentBackpack.SetWornByActiveCharacter();
        }

        mouseLook.SetWearingBackpack();
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

        /*
        if (!wearingBackpack)
        {
            PickupBackpack(initialBackpack);
        }
        */

        //PickupBackpack(initialBackpack);
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
        initialBackpack.EnableRewindTarget();

        // Trying to reset rigidbody position and rotation at the start of each loop for determinism
        rigidbodyComponent.position = initialRigidbodyPosition;
        rigidbodyComponent.rotation = initialRigidbodyRotation;
        rigidbodyComponent.velocity = Vector3.zero;

        PickupBackpack(initialBackpack);
    }

    // Disabled at the end of each loop
    public void Disable()
    {
        enable = false;
        movementComponent.Disable();
        mouseLook.Disable();
        animatorComponent.StopRecording();

        rewindTarget.Disable();
        initialBackpack.DisableRewindTarget();
    }

    public void SetActiveCharacter()
    {
        isActiveCharacter = true;

        characterMesh.enabled = false;

        initialBackpack.SetWornByActiveCharacter();

        mouseLook.SetCameraActive();
        //mouseLook.EnableRaycasting();
    }

    public void SetNotActiveCharacter()
    {
        isActiveCharacter = false;

        characterMesh.enabled = true;

        initialBackpack.SetNotWornByActiveCharacter();

        mouseLook.SetCameraInactive();
        //mouseLook.DisableRaycasting();
    }

    public void TryJump()
    {
        movementComponent.TryJump();
    }

    public void TryInteract()
    {
        mouseLook.TryInteract();
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

    public void TryBackpackPickup(Backpack backpack)
    {
        PickupBackpack(backpack);
        PlayBackpackPickupSound(); // Play sound here instead of PickupBackpack() for now since I'm calling PickupBackpack on round start and don't want the sound to play then
    }

    public void PlayBackpackDropSound()
    {
        backpackAudioSource.clip = backpackDropClip;
        backpackAudioSource.volume = .7f;
        backpackAudioSource.Play();
    }

    public void PlayBackpackPickupSound()
    {
        backpackAudioSource.clip = backpackPickupClip;
        backpackAudioSource.volume = .7f;
        backpackAudioSource.Play();
    }
}
