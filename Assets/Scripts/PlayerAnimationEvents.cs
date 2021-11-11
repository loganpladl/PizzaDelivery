using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Character character;

    public void FootstepSound()
    {
        character.FootstepSound();
    }
}
