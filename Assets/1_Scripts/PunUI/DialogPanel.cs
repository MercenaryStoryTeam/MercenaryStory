using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{
	public Text dialogText;
	private Action callback;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			callback?.Invoke();
		}
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