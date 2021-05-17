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
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }

    public void SetNotActiveCharacter()
    {
        isActiveCharacter = false;
        characterMesh.enabled = true;
        characterBackpackMesh.enabled = true;
        mouseLook.SetCameraInactive();
    }

    public void GetPrevMouseVelocities(out float prevMouseXVelocity, out float prevMouseYVelocity)
    {
        mouseLook.GetPrevMouseVelocities(out prevMouseXVelocity, out prevMouseYVelocity);
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
}
