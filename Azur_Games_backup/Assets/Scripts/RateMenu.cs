using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RateState
{
    Like, Rate,
}

public class RateMenu : MonoBehaviour
{
    public static RateMenu instance;
    public Text titleText;
    public Text yesText;
    public Text noText;

    public GameObject[] stars;
    int starsCount;

    RateState state;

    private void Awake()
    {
        instance = this;
        Hide();
    }

    public void Yes()
    {
        {
            if (starsCount > 0)
            {
                PlayerPrefs.SetInt("rate", 1);
            }
            if (starsCount > 4)
            {
#if UNITY_ANDROID
                Application.OpenURL("market://details?id=" + Application.identifier);
#elif UNITY_IPHONE
                        Application.OpenURL("itms-apps://itunes.apple.com/app/id1462267627");
#endif
            }
            Hide();
        }
    }

    public void No()
    {
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        //yesText.transform.parent.gameObject.SetActive(false);
        //state = RateState.Like;
        UpdateData();
    }

    void UpdateData()
    {
     
    }

    public void SetStars(int count)
    {
        for(int i = 0;i < 5; i++)
        {
            stars[i].SetActive(i < count);
        }
        starsCount = count;
        //yesText.transform.parent.gameObject.SetActive(true);
        if (count == 5) Yes();
    }

}
