using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    bool rewinding;

    TimePoint[] timePoints;

    PlayerMovement movementComponent;
    Rigidbody rigidbodyComponent;

    [SerializeField]
    MouseLook mouseLook;

    // Set by LevelState. Used to rewind correctly.
    float rewindDuration;
    int totalSteps;
    int currentStep = 0;

    float rewindTimer = 0;

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
        else
        {
            RecordStep();
        }
    }

    public void StartRewind()
    {
        movementComponent.Disable();
        rigidbodyComponent.isKinematic = true;
        mouseLook.Disable();
        rewinding = true;

        rewindTimer = 0;
    }

    public void StopRewind()
    {
        movementComponent.Enable();
        rigidbodyComponent.isKinematic = false;
        mouseLook.Enable();
        rewinding = false;

        currentStep = 0;
    }

    private void RewindStep()
    {
        float frac = 1 - (rewindTimer / rewindDuration);
        currentStep = (int)((totalSteps - 1) * frac);

        TimePoint timePoint = timePoints[currentStep];

        transform.position = timePoint.position;
        transform.rotation = timePoint.rotation;
        mouseLook.SetVerticalRotation(timePoint.cameraVerticalRotation);
    }

    private void RecordStep()
    {
        timePoints[currentStep] = new TimePoint(transform.position, transform.rotation, mouseLook.GetVerticalRotation());
        currentStep++;
    }

    public void SetRewindParameters(int totalSteps, float rewindDuration)
    {
        this.totalSteps = totalSteps;
        this.rewindDuration = rewindDuration;

        timePoints = new TimePoint[totalSteps];
    }
}
