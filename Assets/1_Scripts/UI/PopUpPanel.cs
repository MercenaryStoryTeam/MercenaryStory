using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpPanel : MonoBehaviour
{
	public Text dialogText;
	private Action callback;
	public Button closeButton;

	private void Awake()
	{
		closeButton.onClick.AddListener(CloseButtonClick);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			callback?.Invoke();
		}
	}

	public void CloseButtonClick()
	{
		callback?.Invoke();
	}

	public void DialogOpen(string dialog, Action callback)
	{
		gameObject.SetActive(true);
		this.dialogText.text = dialog;
		this.callback = callback;
	}

	public void DialogClose()
	{
		gameObject.SetActive(false);
	}
}