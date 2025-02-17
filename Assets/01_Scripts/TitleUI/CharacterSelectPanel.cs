using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPanel : MonoBehaviour
{
	public Button button1;
	public Button button2;
	public Button button3;

	private void Awake()
	{
		button1.onClick.AddListener(() => OnButtonClick(1));
		button2.onClick.AddListener(() => OnButtonClick(2));
		button3.onClick.AddListener(() => OnButtonClick(3));
	}

	private void OnButtonClick(int appearance)
	{
		FirebaseManager.Instance.CurrentUserData.UpdateUserData(appearance: appearance);
		// server에 userdata 업데이트 해야함
		FirebaseManager.Instance.UploadCurrentUserData("user_Appearance",
			FirebaseManager.Instance.CurrentUserData.user_Appearance);
		TitleUI.Instance.PanelOpen("ServerSelect");
	}
}