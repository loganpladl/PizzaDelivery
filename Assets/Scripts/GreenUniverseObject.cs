using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenUniverseObject : UniverseObject
{
    [SerializeField]
    Material InGreenUniverseMaterial;

    [SerializeField]
    Material NotInGreenUniverseMaterial;

    public override void SetBlueUniverseMaterial()
    {
        meshRenderer.material = NotInGreenUniverseMaterial;
    }
    public override void SetRedUniverseMaterial()
    {
        meshRenderer.material = NotInGreenUniverseMaterial;
    }
    public override void SetGreenUniverseMaterial()
    {
        meshRenderer.material = InGreenUniverseMaterial;
    }
}
