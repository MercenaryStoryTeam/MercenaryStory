using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignInPanel : MonoBehaviour
{
	public InputField emailInput;
	public InputField pwInput;
	public Button signUpButton;
	public Button signInButton;

	private void Awake()
	{
		signUpButton.onClick.AddListener(SignUpButtonClick);
		signInButton.onClick.AddListener(SignInButtonClick);
	}

	private void SignUpButtonClick()
	{
		PanelManager.Instance.PanelOpen("SignUp");
	}

	private void SignInButtonClick()
	{
		// PanelManager.Instance.dialogPanel.DialogOpen("로그인 중입니다.");
	}
}