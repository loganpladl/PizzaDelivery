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

    Vector2 velocity;

    float CameraVerticalRotation = 0.0f;

    [SerializeField]
    float positionSmoothing = 1;

    [SerializeField]
    float rotationSmoothing = 1;

    bool enableRaycasting = true;

    // Accumulate horizontal rotation from input manager. Reset when updating rigidbody rotation.
    float CameraHorizontalRotation = 0.0f;

    bool enable = true;

    public delegate void TryBackpackPickup();
    public TryBackpackPickup tryBackpackPickup;
    bool wearingBackpack = true;

    bool tryInteract = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        levelState = GameObject.FindGameObjectsWithTag("LevelState")[0].GetComponent<LevelState>();

        vcam.transform.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Hacky way to avoid doing anything if the game is paused
        if (Time.timeScale == 0)
        {
            return;
        }

        transform.position = targetTransform.transform.position;
        transform.rotation = targetTransform.transform.rotation;


        CameraVerticalRotation = Mathf.Clamp(CameraVerticalRotation, -90.0f, 90.0f);
        Vector3 eulerVertical = new Vector3(CameraVerticalRotation, 0.0f, 0.0f);
        Vector3 newEuler = gameObject.transform.rotation.eulerAngles + eulerVertical;
        gameObject.transform.localRotation = Quaternion.Euler(newEuler);

        /*
        if (enable && enableRaycasting)
        {
            Raycast();
        }
        */

        vcam.transform.position = Vector3.MoveTowards(vcam.transform.position, transform.position, positionSmoothing * Time.deltaTime);
        vcam.transform.rotation = Quaternion.Slerp(vcam.transform.rotation, transform.rotation, rotationSmoothing * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (enable)
        {
            Vector3 eulerHorizontal = new Vector3(0.0f, CameraHorizontalRotation, 0.0f);
            CameraHorizontalRotation = 0;
            PlayerRigidbody.MoveRotation(PlayerRigidbody.rotation * Quaternion.Euler(eulerHorizontal));

            if (enableRaycasting)
            {
                Raycast();
            }
        }

        tryInteract = false;
    }

    public void UpdateLook(float xVelocity, float yVelocity)
    {
        velocity.x = xVelocity;
        velocity.y = yVelocity;

        // Negate vertical input since positive movement is normally downward
        CameraVerticalRotation -= velocity.y;
        CameraHorizontalRotation += velocity.x;
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
    }

    public void Disable()
    {
        enable = false;
    }

    public void Raycast()
    {
        RaycastHit hit;

        Debug.DrawRay(transform.position, transform.forward);

        // Only show prompts if active TODO: Could clean up, lots of IsCameraActive if statements
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayCastLength))
        {
            if (hit.collider.CompareTag("Door"))
            {
                if (IsCameraActive())
                {
                    levelState.DisplayKnockPrompt();
                }

                if (Interacting())
                {
                    levelState.Victory();
                }

                return; // avoid checking for backpack collision if we already found door collision
            }
            else
            {
                if (IsCameraActive())
                {
                    levelState.HideKnockPrompt();
                }
            }

            if (hit.collider.CompareTag("Backpack"))
            {
                if (!wearingBackpack)
                {
                    if (IsCameraActive())
                    {
                        levelState.DisplayPickupPrompt();
                    }

                    if (Interacting())
                    {
                        tryBackpackPickup();
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
}
