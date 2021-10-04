using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetPlatform : MonoBehaviour
{
    public const float MaxForce = 50.0f;
    public const float MaxDistance = 30.0f;
    // Force starts falling off after this distance
    public const float AttenuationDistance = 5.0f;
    
    // TODO: Currently only works with ground magnet platforms. Sideways wall magnet platforms could be cool too.
    // Distance calculations are also currently done in PlayerMovement.cs and Backpack.cs, not ideal for extension.

    // Calculates force given a distance from the magnet platform
    public static float GetForceFromDistance(float distance)
    {
        distance = Mathf.Abs(distance);

        // If the player is within the attenuation range
        if (distance <= 5.0f)
        {
            return MaxForce;
        }
        else
        {
            float distanceRange = MaxDistance - AttenuationDistance;
            float normalizedDistance = distance - AttenuationDistance;

            float distanceFrac = normalizedDistance / distanceRange;
            if (distanceFrac >= 1)
            {
                // 0 force applied if out of range
                return 0;
            }

            float forceFrac = 1 - distanceFrac;


            // Geometric falloff
            return forceFrac * forceFrac * MaxForce;
        }
    }
}
