using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    [SerializeField] RewindTarget rewindTarget;
    bool wearingBackpack = true;

    [SerializeField] MeshRenderer backpackMeshRenderer;

    // Needs to know whether this backpack belongs to the active character to know whether to disable mesh renderer when rewinding
    bool activeCharacter;

    Rigidbody rb;

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
        // Store wearing backpack is being worn in 3rd timepoint parameter
        float wearingFlag;
        if (wearingBackpack)
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
        wearingBackpack = false;
        //rb.isKinematic = false;
        //rb.detectCollisions = true;

        //this.gameObject.layer = LayerMask.NameToLayer("Default");
        CreateRigidbody();
    }

    public void Pickup()
    {
        wearingBackpack = true;
        //rb.isKinematic = true;
        //rb.detectCollisions = false;

        //this.gameObject.layer = initialLayer;
        DestroyRigidbody();
    }

    public void SetActiveCharacter()
    {
        activeCharacter = true;
    }

    public void SetNotActiveCharacter()
    {
        activeCharacter = false;
    }

    void RewindStep(float progress, TimePoint timePoint)
    {
        if (activeCharacter)
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
