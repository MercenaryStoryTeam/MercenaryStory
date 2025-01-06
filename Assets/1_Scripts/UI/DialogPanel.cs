using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{
	public Text dialogText;

	public void SetDialog(string dialog)
	{
		dialogText.text = dialog;
	}
}