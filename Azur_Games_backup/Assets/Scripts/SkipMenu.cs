using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
    }

    

    public void Yes()
    {
        AdsAnaliticsManager.instance.ShowRewarded((bool result)=> {
            if (result)
            {
                PlayerPrefs.SetInt("SkipAds", 1);
                FinishMenu.instance.Next();
            }
        });
    }
}
