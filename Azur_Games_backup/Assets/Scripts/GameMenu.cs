using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameMenu : MonoBehaviour
{
    public static GameMenu instance;
    public PlayerState playerState;
    public bool settingsExpand;

    public RectTransform[] settingsButtons;
    public RectTransform[] levelMenuButtons;

    public Image vibrateImage;
    public Sprite vibrateOnIcon;
    public Sprite vibrateOffIcon;

    public Image soundImage;
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;

    public Image musicImage;
    public Sprite musicOnIcon;
    public Sprite musicOffIcon;

    public Vector2 vibrateStartPosition;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        foreach (var button in settingsButtons)
        {
            button.anchoredPosition = new Vector2(184f, button.anchoredPosition.y);
        }

		foreach (var button in levelMenuButtons)
		{
			button.anchoredPosition = new Vector2(10f, button.anchoredPosition.y);
		}
    }

    public void SwitchSettings()
    {
        settingsExpand = !settingsExpand;
        foreach(var button in settingsButtons)
        {
            DOTween.To(()=>button.anchoredPosition, x => button.anchoredPosition = x, settingsExpand ? new Vector2(-184f, button.anchoredPosition.y) : new Vector2(184f, button.anchoredPosition.y), 0.3f);
        }

        UpdateVibrate();
        UpdateSound();
    }

    public void SwitchVibration()
    {
        playerState.vibrate = !playerState.vibrate;
        playerState.Save();

        

        UpdateVibrate();

    }

    public void SwitchSound()
    {
        var v = PlayerPrefs.GetInt("Sound", 1);
        v = (v + 1) % 2;
        PlayerPrefs.SetInt("Sound", v);

        playerState.ChangeSound(v == 1);

        UpdateSound();
    }

    public void SwitchMusic()
    {
        var v = PlayerPrefs.GetInt("Music", 1);
        v = (v + 1) % 2;
        PlayerPrefs.SetInt("Music", v);

        playerState.ChangeMusic(v == 1);
    }

    void UpdateVibrate()
    {
        vibrateImage.sprite = playerState.vibrate ? vibrateOnIcon : vibrateOffIcon;
    }

    void UpdateSound()
    {
        soundImage.sprite = PlayerPrefs.GetInt("Sound", 1) == 1 ? soundOnIcon : soundOffIcon;
    }

    void UpdateMusic()
    {
        musicImage.sprite = PlayerPrefs.GetInt("Music", 1) == 1 ? musicOnIcon : musicOffIcon;
    }
}
