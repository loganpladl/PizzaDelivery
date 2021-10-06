using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedUniverseObject : UniverseObject
{
    [SerializeField]
    Material InRedUniverseMaterial;

    [SerializeField]
    Material NotInRedUniverseMaterial;

    public override void SetBlueUniverseMaterial()
    {
        meshRenderer.material = NotInRedUniverseMaterial;
    }
    public override void SetRedUniverseMaterial()
    {
        meshRenderer.material = InRedUniverseMaterial;
    }
    public override void SetGreenUniverseMaterial()
    {
        meshRenderer.material = NotInRedUniverseMaterial;
    }
}
