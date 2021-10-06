using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueUniverseObject : UniverseObject
{
    [SerializeField]
    Material InBlueUniverseMaterial;

    [SerializeField]
    Material NotInBlueUniverseMaterial;

    public override void SetBlueUniverseMaterial()
    {
        meshRenderer.material = InBlueUniverseMaterial;
    }
    public override void SetRedUniverseMaterial()
    {
        meshRenderer.material = NotInBlueUniverseMaterial;
    }
    public override void SetGreenUniverseMaterial()
    {
        meshRenderer.material = NotInBlueUniverseMaterial;
    }
}
