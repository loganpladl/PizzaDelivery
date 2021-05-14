using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBeat : MonoBehaviour
{
    // BPM of background music. Used to calculate beat frequency.
    [SerializeField][Range(0,300)] float beatsPerMinute;

    // How often a beat occurs
    float secondsPerBeat;

    float beatTimer;

    // How far forward to zoom the camera each beat
    [SerializeField] float beatDistance;

    Vector3 basePosition;
    Vector3 targetPosition;

    // Used by smoothdamp
    Vector3 velocity = Vector3.zero;

    [SerializeField]
    bool enable = false;

    bool zooming = false;

    // How long to zoom in for
    [SerializeField]
    float zoomDuration = .2f;
    float zoomTimer;

    // Start is called before the first frame update
    void Start()
    {
        secondsPerBeat = 1 / (beatsPerMinute / 60);
        beatTimer = secondsPerBeat;
        basePosition = transform.position;
        targetPosition = basePosition + transform.forward * beatDistance;
    }

    // Update is called once per frame
    void Update()
    {
        if (enable)
        {
            beatTimer -= Time.deltaTime;

            if (beatTimer <= 0)
            {
                beatTimer = secondsPerBeat;
                zooming = true;
                zoomTimer = zoomDuration;
            }

            if (zooming)
            {
                zoomTimer -= Time.deltaTime;

                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, zoomDuration);

                if (zoomTimer <= 0)
                {
                    zooming = false;
                    velocity = Vector3.zero;
                }
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, basePosition, ref velocity, secondsPerBeat - zoomDuration);
            }
        }
    }

    public void Enable()
    {
        enable = true;
    }
}
