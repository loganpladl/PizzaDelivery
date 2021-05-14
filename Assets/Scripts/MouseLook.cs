using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Vector2 rotation; // Current rotation in degrees
    Vector2 velocity;

    float CameraVerticalRotation = 0.0f;

    bool enable = true;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = targetTransform.transform.position;
        transform.rotation = targetTransform.transform.rotation;
        Look();
    }
    private void Look()
    {
        // TODO: Encapsulate input
        if (enable)
        {
            float MouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float MouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            velocity.x = Mathf.MoveTowards(velocity.x, MouseX, acceleration * Time.deltaTime);
            velocity.y = Mathf.MoveTowards(velocity.y, MouseY, acceleration * Time.deltaTime);

            // Negate vertical input since positive movement is normally downward
            CameraVerticalRotation -= velocity.y;

            CameraVerticalRotation = Mathf.Clamp(CameraVerticalRotation, -90.0f, 90.0f);
            Vector3 eulerVertical = new Vector3(CameraVerticalRotation, 0.0f, 0.0f);
            Vector3 newEuler = gameObject.transform.rotation.eulerAngles + eulerVertical;
            gameObject.transform.localRotation = Quaternion.Euler(newEuler);

            Vector3 eulerHorizontal = new Vector3(0.0f, velocity.x, 0.0f);
            PlayerRigidbody.MoveRotation(PlayerRigidbody.rotation * Quaternion.Euler(eulerHorizontal));
        }
        // Just adjust according to camera vertical rotation if disabled
        else
        {
            Vector3 eulerVertical = new Vector3(CameraVerticalRotation, 0.0f, 0.0f);
            Vector3 newEuler = gameObject.transform.rotation.eulerAngles + eulerVertical;
            gameObject.transform.localRotation = Quaternion.Euler(newEuler);
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
    }

    public void Disable()
    {
        enable = false;
    }
}
