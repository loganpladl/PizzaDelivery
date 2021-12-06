using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MouseLook : MonoBehaviour
{
    [SerializeField]
    Transform targetTransform;

    [SerializeField]
    Rigidbody PlayerRigidbody;

    [SerializeField]
    float rayCastLength;

    [SerializeField]
    LayerMask rayCastLayer;

    LevelState levelState;

    [SerializeField]
    CinemachineVirtualCamera vcam;

    float CameraVerticalRotation = 0.0f;

    bool enableRaycasting = true;

    // Accumulate horizontal rotation from input manager. Reset when updating rigidbody rotation.
    float CameraHorizontalRotation = 0.0f;

    bool enable = true;

    public delegate void TryBackpackPickup(Backpack backpack);
    public TryBackpackPickup tryBackpackPickup;
    bool wearingBackpack = true;

    bool tryInteract = false;

    // Cannot look while hanging. Set by Character component.
    bool canLook = true;

    Quaternion startHangingLocalRotation;

    // How much to divide the view angle by when hanging
    [SerializeField] float hangingViewAngleDivisor = 1.5f;

    float hangingHorizontalCameraRotation = 0;

    // Apply extra smoothing when correcting after hanging
    [SerializeField] float hangingSmoothDuration = .5f;
    float hangingSmoothTimer = 0;

    [SerializeField] float hangingSmoothDivisor = 5.0f;

    [SerializeField] float rotationSmoothing = 50f;

    Vector3 initialLocalPosition;
    Quaternion initialLocalRotation;

    float latestFixedUpdateTime;
    float secondLatestFixedUpdateTime;

    Quaternion previousCameraRotation;
    Quaternion nextCameraRotation;
    Vector3 previousCameraPosition;
    Vector3 nextCameraPosition;

    float vcamHorizontal = 0;
    float vcamVertical = 0;

    [SerializeField] GameObject baseRealtimeCamera;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameObject levelStateObject = GameObject.FindWithTag("LevelState");
        if (levelStateObject)
        {
            levelState = levelStateObject.GetComponent<LevelState>();
        }

        vcam.transform.position = transform.position;

        startHangingLocalRotation = gameObject.transform.localRotation;

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        latestFixedUpdateTime = Time.time;
        secondLatestFixedUpdateTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // Hacky way to avoid doing anything if the game is paused
        if (Time.timeScale == 0)
        {
            return;
        }

        // Only smooth if enabled since otherwise we're rewinding and no smoothing is preferred
        if (enable)
        {
            InterpolateVcamPosition();


            //temp rotation
            if (canLook)
            {
                vcamVertical = Mathf.Clamp(vcamVertical, -90.0f, 90.0f);
                Vector3 euler = new Vector3(vcamVertical, vcamHorizontal, 0.0f);
                baseRealtimeCamera.transform.localRotation = Quaternion.Euler(euler);

                vcam.transform.rotation = baseRealtimeCamera.transform.rotation;
            }
            else
            {
                Vector3 euler = new Vector3(vcamVertical, vcamHorizontal, 0.0f);
                baseRealtimeCamera.transform.localRotation = Quaternion.Euler(euler);

                // Apply extra smoothing while hanging and briefly after, to mask camera snapping
                vcam.transform.rotation = Quaternion.Slerp(vcam.transform.rotation, baseRealtimeCamera.transform.rotation, rotationSmoothing / hangingSmoothDivisor * Time.deltaTime);
                hangingSmoothTimer -= Time.deltaTime;
            }
        }
        else
        {
            vcam.transform.position = transform.position;
            vcam.transform.rotation = transform.rotation;
        }
    }

    private void InterpolateVcamPosition()
    {
        float t = (Time.time - latestFixedUpdateTime) / (latestFixedUpdateTime - secondLatestFixedUpdateTime);

        if ((latestFixedUpdateTime - secondLatestFixedUpdateTime) == 0)
        {
            // fixes divide by zero TODO: Clean up
            t = 1;
        }

        vcam.transform.position = Vector3.Lerp(previousCameraPosition, nextCameraPosition, t);
    }

    private void FixedUpdate()
    {
        secondLatestFixedUpdateTime = latestFixedUpdateTime;
        latestFixedUpdateTime = Time.time;

        if (canLook)
        {
            transform.position = targetTransform.transform.position;
            transform.rotation = targetTransform.transform.rotation;

            CameraVerticalRotation = Mathf.Clamp(CameraVerticalRotation, -90.0f, 90.0f);
            Vector3 eulerVertical = new Vector3(CameraVerticalRotation, 0.0f, 0.0f);

            Vector3 newEuler = gameObject.transform.localEulerAngles + eulerVertical;
            gameObject.transform.localRotation = Quaternion.Euler(newEuler);
        }

        if (enable)
        {
            Vector3 eulerHorizontal = new Vector3(0.0f, CameraHorizontalRotation, 0.0f);
            CameraHorizontalRotation = 0;

            if (canLook)
            {
                PlayerRigidbody.MoveRotation(PlayerRigidbody.rotation * Quaternion.Euler(eulerHorizontal));
            }

            if (enableRaycasting)
            {
                Raycast();
            }
        }

        tryInteract = false;

        previousCameraRotation = nextCameraRotation;
        nextCameraRotation = transform.rotation;
        previousCameraPosition = nextCameraPosition;
        nextCameraPosition = transform.position;
    }

    public void UpdateLook(float xVelocity, float yVelocity)
    {
        if (canLook)
        {
            // Negate vertical input since positive movement is normally downward
            CameraVerticalRotation -= yVelocity;
            CameraHorizontalRotation += xVelocity;
        }
    }

    public void UpdateLookRealtime(float xVelocity, float yVelocity)
    {
        if (canLook)
        {
            vcamVertical -= yVelocity;
            vcamHorizontal += xVelocity;
        }
    }

    public float GetVerticalRotation()
    {
        return CameraVerticalRotation;
    }

    public void SetVerticalRotation(float cameraVerticalRotation)
    {
        CameraVerticalRotation = cameraVerticalRotation;
    }

    public void Enable()
    {
        enable = true;

        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
    }

    public void Disable()
    {
        enable = false;

        levelState.HideKnockPrompt();
        levelState.HidePickupPrompt();
        levelState.HideCantDeliverPrompt();
    }

    public void Raycast()
    {
        RaycastHit hit;

        // Only show prompts if active TODO: Could clean up, lots of IsCameraActive if statements
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayCastLength, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Door"))
            {
                // Can only knock and complete level if wearing a backpack
                if (wearingBackpack)
                {
                    if (IsCameraActive())
                    {
                        levelState.DisplayKnockPrompt();
                    }

                    if (Interacting())
                    {
                        levelState.Victory();
                    }
                }
                else
                {
                    if (IsCameraActive())
                    {
                        levelState.DisplayCantDeliverPrompt();
                    }
                }
                

                return; // avoid checking for backpack collision if we already found door collision
            }
            else
            {
                if (IsCameraActive())
                {
                    levelState.HideKnockPrompt();
                    levelState.HideCantDeliverPrompt();
                }
            }

            if (hit.collider.CompareTag("Backpack") || hit.collider.CompareTag("BackpackFloor"))
            {
                if (!wearingBackpack)
                {
                    if (IsCameraActive())
                    {
                        levelState.DisplayPickupPrompt();
                    }

                    if (Interacting())
                    {
                        tryBackpackPickup(hit.collider.gameObject.GetComponentInParent<Backpack>());
                    }
                }
            }

            else
            {
                if (IsCameraActive())
                {
                    levelState.HidePickupPrompt();
                }
            }
        }
        else
        {
            if (IsCameraActive())
            {
                levelState.HideKnockPrompt();
                levelState.HidePickupPrompt();
                levelState.HideCantDeliverPrompt();
            }
        }
    }

    public void SetCameraActive()
    {
        vcam.Priority = 1;
    }

    public void SetCameraInactive()
    {
        vcam.Priority = -1;
    }

    public void EnableRaycasting()
    {
        enableRaycasting = true;
    }

    public void DisableRaycasting()
    {
        enableRaycasting = false;
    }

    bool IsCameraActive()
    {
        return vcam.Priority == 1;
    }

    public void SetWearingBackpack()
    {
        wearingBackpack = true;
    }

    public void SetNotWearingBackpack()
    {
        wearingBackpack = false;
    }

    public void TryInteract()
    {
        tryInteract = true;
    }

    bool Interacting()
    {
        return tryInteract;
    }

    public void SetCanLook()
    {
        //hangingSmoothTimer = hangingSmoothDuration;

        //transform.localRotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, startHangingLocalRotation.y, 0));
        //baseRealtimeCamera.transform.localRotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, startHangingLocalRotation.y, 0));
        canLook = true;

        //hangingHorizontalCameraRotation = 0;
    }

    public void SetCannotLook()
    {
        transform.position = targetTransform.transform.position;
        transform.rotation = targetTransform.transform.rotation;

        startHangingLocalRotation = transform.localRotation;
        //baseRealtimeCamera.transform.localRotation = startHangingLocalRotation;
        CameraHorizontalRotation = 0;
        vcamVertical = 0;
        CameraVerticalRotation = 0;
        vcamHorizontal = transform.localRotation.eulerAngles.y;

        canLook = false;
    }

    public bool CanLook()
    {
        return canLook;
    }
}
