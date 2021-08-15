using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePoint
{
    public Vector3 position;
    public Quaternion rotation;
    public float cameraVerticalRotation;

    public TimePoint(Vector3 position, Quaternion rotation, float cameraVerticalRotation)
    {
        this.position = position;
        this.rotation = rotation;
        this.cameraVerticalRotation = cameraVerticalRotation;
    }
}
