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
    float rewindDuration;
    int totalSteps;
    int currentStep = 0;

    float rewindTimer = 0;

    bool enable = false;

    bool isActiveCharacter = false;

    LevelState levelState;

    private void Awake()
    {
        movementComponent = GetComponent<PlayerMovement>();
        rigidbodyComponent = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        levelState = GameObject.FindGameObjectsWithTag("LevelState")[0].GetComponent<LevelState>();
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

    public void StartRewind()
    {
        //movementComponent.Disable();
        rigidbodyComponent.isKinematic = true;
        mouseLook.Disable();
        rewinding = true;

        rewindTimer = 0;

        animatorComponent.StartPlayback();
    }

    public void StopRewind()
    {
        //movementComponent.Enable();
        rigidbodyComponent.isKinematic = false;
        mouseLook.Enable();
        rewinding = false;

        currentStep = 0;

        animatorComponent.StopPlayback();
    }

    private void RewindStep()
    {
        float frac = 1 - (rewindTimer / rewindDuration);
        currentStep = (int)((totalSteps - 1) * frac);

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

    public void SetRewindParameters(int totalSteps, float rewindDuration)
    {
        this.totalSteps = totalSteps;
        this.rewindDuration = rewindDuration;

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

    public void SetRewindDuration()
    {
        this.rewindDuration = rewindDuration;
    }
}
