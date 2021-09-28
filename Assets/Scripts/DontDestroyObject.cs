using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyObject : MonoBehaviour
{
    // Tag to check for duplicates
    [SerializeField] string tagToCheck;
    private void Awake()
    {
        // Destroy this gameobject if another with this tag already exists.
        GameObject[] total = GameObject.FindGameObjectsWithTag(tagToCheck);
        if (total.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            // Persist this gameobject through scene changes
            DontDestroyOnLoad(gameObject);
        }
    }
}
