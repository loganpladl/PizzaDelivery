using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindTarget : MonoBehaviour
{
    bool rewinding = false;

    TimePoint[] timePoints;

    // Set by LevelState. Used to rewind correctly.
    float maxRewindDuration;
    int totalSteps;
    int currentStep = 0;

    float rewindTimer = 0;

    float rewindStartFrac = 0;
    float currentRewindDuration;

    // Always revert to initial point at the end to ensure consistency
    TimePoint initialPoint;

    bool enable = false;

    public delegate void RewindStarted();
    public event RewindStarted OnRewindStart;

    public delegate void RewindEnded(TimePoint initialTimePoint);
    public event RewindEnded OnRewindEnded;

    public delegate TimePoint CreateTimePoint();
    public event CreateTimePoint CreateNewTimePoint;

    public delegate void RewindingStep(float progress, TimePoint currentTimePoint);
    public event RewindingStep Rewinding;

    // Start is called before the first frame update
    void Start()
    {
        //initialPoint = new TimePoint(transform.position, transform.rotation, mouseLook.GetVerticalRotation());
        initialPoint = CreateNewTimePoint();
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

    public void Enable()
    {
        enable = true;
    }

    public void Disable()
    {
        enable = false;
    }

    public void SetRewindParameters(int totalSteps, float rewindDuration)
    {
        this.totalSteps = totalSteps;
        // TODO: not using maxRewindDuration
        this.maxRewindDuration = rewindDuration;

        timePoints = new TimePoint[totalSteps];
    }

    public void StartRewind(float rewindDuration, float rewindStartFrac)
    {
        
        rewinding = true;

        rewindTimer = 0;
        currentRewindDuration = rewindDuration;

        this.rewindStartFrac = rewindStartFrac;
        
        if (OnRewindStart != null)
        {
            OnRewindStart();
        }
    }

    public void StopRewind()
    {
        // Return to initial time point for consistency
        transform.position = initialPoint.position;
        transform.rotation = initialPoint.rotation;

        // Call event with intiail time point to allow for for custom reset behavior
        if (OnRewindEnded != null)
        {
            OnRewindEnded(initialPoint);
        }

        rewinding = false;

        currentStep = 0;

        
    }

    private void RewindStep()
    {
        float frac = 1 - (rewindTimer / currentRewindDuration);
        // Safety clamp for frac that shouldnt be necessary but just in case
        frac = Mathf.Clamp(frac, 0, 1);

        // TODO: Subtracting 1 to fix off by one error when rewinding. Should look for a better solution.
        //currentStep = (int)(totalSteps * frac) - 1;
        currentStep = (int) (totalSteps * rewindStartFrac * frac) - 1;

        // TODO: is there a better way to avoid these index out of bounds errors?
        if (currentStep >= totalSteps) currentStep = totalSteps - 1;
        if (currentStep < 0) currentStep = 0;

        TimePoint timePoint = timePoints[currentStep];
        if (timePoint == null)
        {
            return;
        }

        transform.position = timePoint.position;
        transform.rotation = timePoint.rotation;
        

        if (Rewinding != null)
        {
            Rewinding(frac, timePoint);
        }
    }

    private void RecordStep()
    {
        if (currentStep >= totalSteps)
        {
            Debug.Log("Reached current step equal to totalSteps when recording, shouldn't happen");
        }
        else
        {
            //timePoints[currentStep] = new TimePoint(transform.position, transform.rotation, mouseLook.GetVerticalRotation());
            timePoints[currentStep] = CreateNewTimePoint();
            currentStep++;
        }
    }
}
