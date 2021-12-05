using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MouseLook : MonoBehaviour
{
    [SerializeField]
    float mouseSensitivity;
    [SerializeField]
    float acceleration;

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

    [SerializeField]
    float positionSmoothing = 1;

    [SerializeField]
    float rotationSmoothing = 1;

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

        previousCameraRotation = transform.rotation;
        nextCameraRotation = transform.rotation;
        previousCameraPosition = transform.position;
        nextCameraPosition = transform.position;


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
            
            //vcam.transform.position = Vector3.MoveTowards(vcam.transform.position, transform.position, positionSmoothing * Time.deltaTime);

            float t = (Time.time - latestFixedUpdateTime) / (latestFixedUpdateTime - secondLatestFixedUpdateTime);
            
            if ((latestFixedUpdateTime - secondLatestFixedUpdateTime) == 0)
            {
                // fixes divide by zero TODO: Clean up
                t = 1;
            }

            //vcam.transform.position = transform.position * .2f + vcam.transform.position * .8f;

            

            vcam.transform.position = Vector3.Lerp(previousCameraPosition, nextCameraPosition, t);


            //temp rotation
            if (canLook)
            {
                //vcam.transform.position = targetTransform.transform.position;
                //vcam.transform.rotation = targetTransform.transform.rotation;

                vcamVertical = Mathf.Clamp(vcamVertical, -90.0f, 90.0f);
                Vector3 euler = new Vector3(vcamVertical, vcamHorizontal, 0.0f);
                //vcamHorizontal = 0;
                //Vector3 newEuler = gameObject.transform.rotation.eulerAngles + eulerVertical;
                //Vector3 newEuler = gameObject.transform.localEulerAngles + euler;
                baseRealtimeCamera.transform.localRotation = Quaternion.Euler(euler);

                vcam.transform.rotation = baseRealtimeCamera.transform.rotation;
            }
            else
            {
                // TODO: Adapt hanging code from parts of FixedUpdate
                Debug.Log(vcam.transform.rotation);
                Debug.Log(baseRealtimeCamera.transform.rotation);
                // Apply extra smoothing while hanging and briefly after, to mask camera snapping
                vcam.transform.rotation = Quaternion.Slerp(vcam.transform.rotation, baseRealtimeCamera.transform.rotation, rotationSmoothing / hangingSmoothDivisor * Time.deltaTime);
                hangingSmoothTimer -= Time.deltaTime;

                // Reset 
                vcamVertical = CameraVerticalRotation;
                vcamHorizontal = transform.localRotation.eulerAngles.y;
            }

            //vcam.transform.rotation = Quaternion.Slerp(previousCameraRotation, nextCameraRotation, t);
            

            if (hangingSmoothTimer <= 0)
            {
                
            }
            /*
            else
            {
                // Apply extra smoothing while hanging and briefly after, to mask camera snapping
                vcam.transform.rotation = Quaternion.Slerp(vcam.transform.rotation, baseRealtimeCamera.transform.rotation, rotationSmoothing / hangingSmoothDivisor  * Time.deltaTime);
                hangingSmoothTimer -= Time.deltaTime;

                // Reset 
                vcamVertical = CameraVerticalRotation;
                vcamHorizontal = transform.localRotation.eulerAngles.y;
            }
            */
        }
        else
        {
            vcam.transform.position = transform.position;
            vcam.transform.rotation = transform.rotation;
        }
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
            //Vector3 newEuler = gameObject.transform.rotation.eulerAngles + eulerVertical;
            Vector3 newEuler = gameObject.transform.localEulerAngles + eulerVertical;
            gameObject.transform.localRotation = Quaternion.Euler(newEuler);
        }

        // TODO: Get rid of copy paste
        // Tighter vertical view angle if hanging
        else
        {
            transform.position = targetTransform.transform.position;
            transform.rotation = targetTransform.transform.rotation;

            CameraVerticalRotation = Mathf.Clamp(CameraVerticalRotation, -90.0f, 90.0f);
            CameraVerticalRotation /= hangingViewAngleDivisor;

            Vector3 eulerVertical = new Vector3(CameraVerticalRotation, 0.0f, 0.0f);
            //Vector3 newEuler = gameObject.transform.rotation.eulerAngles + eulerVertical;
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
            else
            {
                
                hangingHorizontalCameraRotation += eulerHorizontal.y;
                hangingHorizontalCameraRotation /= hangingViewAngleDivisor;
                eulerHorizontal = new Vector3(0.0f, hangingHorizontalCameraRotation, 0.0f);

                Vector3 newEuler = gameObject.transform.localEulerAngles + eulerHorizontal;
                gameObject.transform.localRotation = Quaternion.Euler(newEuler);
                
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
        // Negate vertical input since positive movement is normally downward
        CameraVerticalRotation -= yVelocity;
        CameraHorizontalRotation += xVelocity;
    }

    public void UpdateLookRealtime(float xVelocity, float yVelocity)
    {
        vcamVertical -= yVelocity;
        vcamHorizontal += xVelocity;
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
        hangingSmoothTimer = hangingSmoothDuration;

        transform.localRotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, startHangingLocalRotation.y, 0));
        baseRealtimeCamera.transform.localRotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, startHangingLocalRotation.y, 0));
        canLook = true;

        hangingHorizontalCameraRotation = 0;
    }

    public void SetCannotLook()
    {
        startHangingLocalRotation = transform.localRotation;
        baseRealtimeCamera.transform.localRotation = startHangingLocalRotation;

        canLook = false;
    }

    public bool CanLook()
    {
        return canLook;
    }
}
