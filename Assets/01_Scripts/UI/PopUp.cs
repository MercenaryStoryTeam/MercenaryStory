using System;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
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

	private void CloseButtonClick()
	{
		callback?.Invoke();
	}

	public void PopUpOpen(string dialog, Action callback = null)
	{
		gameObject.SetActive(true);
		closeButton.gameObject.SetActive(true);
		this.dialogText.text = dialog;
		this.callback = callback;
	}

	public void WaitPopUpOpen(string dialog, Action callback = null)
	{
		gameObject.SetActive(true);
		closeButton.gameObject.SetActive(false);
		this.dialogText.text = dialog;
		this.callback = callback;
	}

	public void PopUpClose()
	{
		gameObject.SetActive(false);
	}
}