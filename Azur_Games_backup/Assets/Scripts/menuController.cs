using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class menuController : MonoBehaviour
{

	public GameObject menuPanel;
	public Button openMenuButton;
	public Button closeMenuButton;
	public Canvas mainCanvas;

	// Start is called before the first frame update
	void Start()
    {
		menuPanel.SetActive(false);

		openMenuButton.onClick.AddListener(OpenMenu);
		closeMenuButton.onClick.AddListener(CloseMenu);
	}

	public void OpenMenu()
	{
		menuPanel.SetActive(true);
		menuPanel.GetComponent<RectTransform>().DOScale(1f, 0.5f);
		mainCanvas.gameObject.SetActive(false);
		openMenuButton.gameObject.SetActive(false);
	}


	public void CloseMenu()
	{
		menuPanel.SetActive(false);
		openMenuButton.gameObject.SetActive(true);
		mainCanvas.gameObject.SetActive(true);
	}

	/*void LoadLevel1()
	{
		//int count = 1;
		int[] levels = new int[1];
		//AddElement(ref levels, ref count, 1);

		foreach (int level in levels) 
		{
			PlayerPrefs.SetInt("level", level);
		}
		
		SceneManager.LoadScene(0);
		PlayerPrefs.SetInt("level", 100);
		
	}*/

	/*public static void AddElement(ref int[] array, ref int count, int value)
	{
		if (count >= array.Length)
		{
			Array.Resize(ref array, array.Length + 1);
		}

		// Добавляем новый элемент в массив
		array[count++] = value;
	}*/

	// Update is called once per frame
	void Update()
    {
        
    }
}
