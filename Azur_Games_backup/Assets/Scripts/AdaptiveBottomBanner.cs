using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdaptiveBottomBanner : MonoBehaviour
{
    public CanvasScaler canvasScaler;
    // Start is called before the first frame update
    void Start()
    {
        var delta = Vector2.up * (Screen.height - Screen.safeArea.height - Screen.safeArea.y);
        GetComponent<LayoutElement>().preferredHeight += delta.y * canvasScaler.referenceResolution.y / Screen.height;
    }

}
