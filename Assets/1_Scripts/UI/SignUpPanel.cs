using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SignUpPanel : MonoBehaviour
{
	public InputField idInput;
	public InputField pwInput1;
	public InputField pwInput2;
	public Button idCheckButton;
	public Button signUpButton;

	private void Awake()
	{
		idCheckButton.onClick.AddListener(IdCheckButtonClick);
		signUpButton.onClick.AddListener(SignUpButtonClick);
	}

	private void IdCheckButtonClick()
	{
		PanelManager.Instance.DialogOpen("이미 사용중인 ID입니다.");
	}

	private void SignUpButtonClick()
	{
		PanelManager.Instance.DialogOpen("회원가입이 완료되었습니다.");
	}
}