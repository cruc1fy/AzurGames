using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SuckMenu : MonoBehaviour
{
    public static SuckMenu instance;

    public Button yesButton;
    public Button noButton;

    System.Action yesAction;
    System.Action noAction;

    public Text bonusText;

    public PlayerState playerState;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Yes()
    {
        yesAction();
        gameObject.SetActive(false);
    }

    public void No()
    {
        noAction();
        gameObject.SetActive(false);
    }
        

    public void Show(System.Action yes, System.Action no)
    {
        gameObject.SetActive(true);
        yesAction = yes;
        noAction = no;
        bonusText.text = "+" + (int)(playerState.totalBalls * RemoteSettings.GetFloat("ExtraBallsPercent", 0.33f));

        noButton.gameObject.SetActive(false);
        DOTween.Sequence().AppendInterval(RemoteSettings.GetFloat("noThankYouDelay", 2f)).AppendCallback(()=> {
            noButton.gameObject.SetActive(true);
        });
    }
}
