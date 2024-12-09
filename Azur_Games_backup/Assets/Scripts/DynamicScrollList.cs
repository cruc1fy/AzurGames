using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DynamicScrollList : MonoBehaviour
{
	public GameObject buttonPrefab;
	public Transform contentParent;
	public int totalButtons = 30;
	public int visibleCount = 10;
	private int topIndex = 0;
	private int bottomIndex = 0;
	private List<GameObject> activeButtons = new List<GameObject>();
	private RectTransform contentRect;
	private Level instance;


	// Start is called before the first frame update
	void Start()
    {
		contentRect = contentParent.GetComponent<RectTransform>();

		for (int i = 1; i < visibleCount; i++)
		{
			CreateButton(i);
		}
		bottomIndex = visibleCount - 1;
	}

	void CreateButton(int index)
	{
		var buttonHeight = buttonPrefab.GetComponent<RectTransform>().rect.height;

		GameObject newButton = Instantiate(buttonPrefab, contentParent);
		newButton.GetComponentInChildren<Text>().text = "Level " + index;

		RectTransform rectTransform = newButton.GetComponent<RectTransform>();
		rectTransform.anchoredPosition = new Vector2(0, -index * buttonHeight);

		activeButtons.Add(newButton);

		UnityEngine.UI.Button localButton = newButton.GetComponent<UnityEngine.UI.Button>();
		localButton.onClick.AddListener(() => OnButtonClick(index));
		totalButtons++;
	}

	private void OnButtonClick(int index)
	{
		for (int i = 0; i < index; i++)
		{
			SceneManager.LoadScene(0);
			PlayerPrefs.SetInt("level", index - 1);
			
		}
	}

	void UpdateButtons()
	{
		float scrollPosition = contentParent.GetComponent<RectTransform>().anchoredPosition.y;

		if (scrollPosition > (visibleCount - 1) * 100)
		{
			int newIndex = activeButtons.Count;
			CreateButton(newIndex);
			//activeButtons.RemoveAt(0);
			//Destroy(activeButtons[0]);
		}
	}

	// Update is called once per frame
	void Update()
	{
		var buttonHeight = buttonPrefab.GetComponent<RectTransform>().rect.height;

		float scrollPos = contentParent.GetComponent<RectTransform>().anchoredPosition.y;
		int newTopIndex = Mathf.FloorToInt(scrollPos / buttonHeight);

		if (newTopIndex > topIndex)
		{
			for (int i = 0; i < newTopIndex - topIndex; i++)
			{
				if (topIndex + visibleCount < totalButtons)
				{
					
					topIndex++;
					bottomIndex++;
					CreateButton(bottomIndex);
				}
			}
		}
	}
}
