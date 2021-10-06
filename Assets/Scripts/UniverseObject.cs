using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UniverseObject : MonoBehaviour
{
    protected MeshRenderer meshRenderer;

    static List<UniverseObject> objects;
    // Start is called before the first frame update
    void Awake()
    {
        if (objects == null)
        {
            objects = new List<UniverseObject>();
        }
        objects.Add(this);

        meshRenderer = GetComponent<MeshRenderer>();
    }

    public abstract void SetBlueUniverseMaterial();
    public abstract void SetRedUniverseMaterial();
    public abstract void SetGreenUniverseMaterial();

    public static void SetBlueUniverse()
    {
        if (objects == null)
        {
            return;
        }

        foreach (UniverseObject obj in objects)
        {
            if (obj.meshRenderer != null)
            {
                obj.SetBlueUniverseMaterial();
            }
        }
    }

    public static void SetRedUniverse()
    {
        if (objects == null)
        {
            return;
        }

        foreach (UniverseObject obj in objects)
        {
            if (obj.meshRenderer != null)
            {
                obj.SetRedUniverseMaterial();
            }
        }
    }

    public static void SetGreenUniverse()
    {
        if (objects == null)
        {
            return;
        }

        foreach (UniverseObject obj in objects)
        {
            if (obj.meshRenderer != null)
            {
                obj.SetGreenUniverseMaterial();
            }
        }
    }
}
