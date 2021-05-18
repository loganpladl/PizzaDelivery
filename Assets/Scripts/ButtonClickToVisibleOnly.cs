using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickToVisibleOnly : MonoBehaviour
{
    [SerializeField]
    float alphaThreshold = 0.1f;

    /*
    * IMAGE MUST HAVE IN SETTINGS
    *          TEXTURE TYPE - SPRITE (2D AND UI)
    *          READ/WRITE ENABLED
    */

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }
}
