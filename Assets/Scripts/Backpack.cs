using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    [SerializeField] RewindTarget rewindTarget;
    bool beingWorn = true;

    // Needs to know whether this backpack belongs to the active character to know whether to disable mesh renderer when rewinding
    bool beingWornByActiveCharacter;

    Rigidbody rb;

    [SerializeField]
    MeshRenderer meshRenderer;

    // Magnet force to apply next fixed update
    float magnetForce = 0;

    //int initialLayer;

    // TODO: Make dropped backpacks that are on top of another player's backpack preserve momentum of that player, just like players do 

    private void Awake()
    {
        rewindTarget.CreateNewTimePoint += CreateNewTimePoint;
        rewindTarget.Rewinding += RewindStep;
        rewindTarget.OnRewindStart += StartRewind;
        //rb = GetComponent<Rigidbody>();



        //initialLayer = this.gameObject.layer;
    }

    private void FixedUpdate()
    {
        if (!beingWorn && rb != null)
        {
            // Add magnet force
            Vector3 magnetVector = new Vector3(0, -magnetForce * rb.mass, 0);
            rb.AddForce(magnetVector * Time.fixedDeltaTime, ForceMode.VelocityChange);

            magnetForce = 0;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Magnet")
        {
            // Magnet only affects characters wearing backpack
            if (!beingWorn)
            {
                // Applies force in the direction of the magnet, proportional to the distance to the magnet
                float distance = transform.position.y - other.transform.position.y;

                magnetForce = MagnetPlatform.GetForceFromDistance(distance);
            }
        }
    }

    TimePoint CreateNewTimePoint()
    {
        // Store whether the active character is wearing this backpack as 3rd TimePoint parameter
        float wearingFlag;
        if (beingWornByActiveCharacter)
        {
            wearingFlag = 1;
        }
        else
        {
            wearingFlag = 0;
        }
        
        return new TimePoint(transform.position, transform.rotation, wearingFlag); // TODO: Third TimePoint parameter is still named as camera vertical rotation, but I'm using it for different contextual purposes.
    }

    public void EnableRewindTarget()
    {
        rewindTarget.Enable();
    }

    public void DisableRewindTarget()
    {
        rewindTarget.Disable();
    }

    public void Drop()
    {
        beingWorn = false;
        //rb.isKinematic = false;
        //rb.detectCollisions = true;

        //this.gameObject.layer = LayerMask.NameToLayer("Default");
        CreateRigidbody();
    }

    public void Pickup()
    {
        beingWorn = true;
        //rb.isKinematic = true;
        //rb.detectCollisions = false;

        //this.gameObject.layer = initialLayer;
        DestroyRigidbody();
    }

    public void SetWornByActiveCharacter()
    {
        beingWornByActiveCharacter = true;

        meshRenderer.enabled = false;
    }

    public void SetNotWornByActiveCharacter()
    {
        beingWornByActiveCharacter = false;

        meshRenderer.enabled = true;
    }

    void RewindStep(float progress, TimePoint timePoint)
    {
        /*
        if (beingWornByActiveCharacter)
        {
            // TODO: Again, this parameter is still named cameraVerticalRotation, but I should rename to a context-dependent float
            if (timePoint.cameraVerticalRotation == 0)
            {
                backpackMeshRenderer.enabled = true;
            }
            else
            {
                // if the active character was wearing this backpack at this timepoint, disable the mesh renderer
                backpackMeshRenderer.enabled = false;
            }
        }
        */

        // Disable the mesh renderer while rewinding only if the active character was wearing this backpack at this timestep
        if (timePoint.cameraVerticalRotation == 0)
        {
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }

    }

    void CreateRigidbody()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 2.5f;
        /*
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ
            | RigidbodyConstraints.FreezeRotation;
        */
    }

    void DestroyRigidbody()
    {
        Destroy(rb);
    }

    void StartRewind()
    {
        if (rb)
        {
            Destroy(rb);
        }
    }
}
