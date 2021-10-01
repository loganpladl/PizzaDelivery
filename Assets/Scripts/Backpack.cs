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

    //int initialLayer;

    // TODO: Make dropped backpacks that are on top of another player's backpack preserve momentum of that player, just like players do 

    private void Awake()
    {
        rewindTarget.CreateNewTimePoint += CreateNewTimePoint;
        rewindTarget.Rewinding += RewindStep;
        //rb = GetComponent<Rigidbody>();



        //initialLayer = this.gameObject.layer;
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
        rb.mass = 10.0f;
        /*
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ
            | RigidbodyConstraints.FreezeRotation;
        */
    }

    void DestroyRigidbody()
    {
        Destroy(rb);
    }
}
