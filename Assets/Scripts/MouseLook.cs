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

    float prevMouseXVelocity;
    float prevMouseYVelocity;

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

    private Vector2 rotation; // Current rotation in degrees
    Vector2 velocity;

    float CameraVerticalRotation = 0.0f;

    // Accumulate horizontal rotation from input manager. Reset when updating rigidbody rotation.
    float CameraHorizontalRotation = 0.0f;

    bool enable = true;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        levelState = GameObject.FindGameObjectsWithTag("LevelState")[0].GetComponent<LevelState>();

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
        //Look();

        if (enable)
        {
            CameraVerticalRotation = Mathf.Clamp(CameraVerticalRotation, -90.0f, 90.0f);
            Vector3 eulerVertical = new Vector3(CameraVerticalRotation, 0.0f, 0.0f);
            Vector3 newEuler = gameObject.transform.rotation.eulerAngles + eulerVertical;
            gameObject.transform.localRotation = Quaternion.Euler(newEuler);

            Vector3 eulerHorizontal = new Vector3(0.0f, CameraHorizontalRotation, 0.0f);
            CameraHorizontalRotation = 0;
            PlayerRigidbody.MoveRotation(PlayerRigidbody.rotation * Quaternion.Euler(eulerHorizontal));

            Raycast();
        }
        // Just adjust according to camera vertical rotation if disabled
        else
        {
            Vector3 eulerVertical = new Vector3(CameraVerticalRotation, 0.0f, 0.0f);
            Vector3 newEuler = gameObject.transform.rotation.eulerAngles + eulerVertical;
            gameObject.transform.localRotation = Quaternion.Euler(newEuler);

            levelState.HideKnockPrompt();
        }
    }

    public void UpdateLook(float xVelocity, float yVelocity)
    {
        velocity.x = xVelocity;
        velocity.y = yVelocity;

        prevMouseXVelocity = xVelocity;
        prevMouseYVelocity = yVelocity;

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

        if (Physics.Raycast(transform.position, transform.forward, out hit, rayCastLength))
        {
            if (hit.collider.CompareTag("Door"))
            {
                levelState.DisplayKnockPrompt();

                if (Input.GetButtonDown("Interact"))
                {
                    levelState.Victory();
                }
            }
            else
            {
                levelState.HideKnockPrompt();
            }
        }
        else
        {
            levelState.HideKnockPrompt();
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

    public void GetPrevMouseVelocities(out float prevMouseXVelocity, out float prevMouseYVelocity)
    {
        prevMouseXVelocity = this.prevMouseXVelocity;
        prevMouseYVelocity = this.prevMouseYVelocity;
    }
}
