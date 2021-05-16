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

    // TODO: replace these mesh renderers with just one for the final character model
    [SerializeField]
    SkinnedMeshRenderer[] characterMesh;

    // Set by LevelState. Used to rewind correctly.
    float rewindDuration;
    int totalSteps;
    int currentStep = 0;

    float rewindTimer = 0;

    bool enable = false;

    private void Awake()
    {
        movementComponent = GetComponent<PlayerMovement>();
        rigidbodyComponent = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {

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
        if (currentStep < 0 || currentStep > 499)
        {
            Debug.Log(currentStep);
        }
        if (timePoints == null)
        {
            Debug.Log("Null");
        }
        timePoints[currentStep] = new TimePoint(transform.position, transform.rotation, mouseLook.GetVerticalRotation());
        currentStep++;
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
        foreach (SkinnedMeshRenderer mesh in characterMesh)
        {
            //mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            mesh.enabled = false;
        }
        mouseLook.SetCameraActive();
    }

    public void SetNotActiveCharacter()
    {
        foreach (SkinnedMeshRenderer mesh in characterMesh)
        {
            //mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mesh.enabled = true;
        }
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
}
