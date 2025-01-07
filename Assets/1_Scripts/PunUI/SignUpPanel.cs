using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SignUpPanel : MonoBehaviour
{
	public InputField emailInput;
	public InputField pwInput1;
	public InputField pwInput2;
	public Button emailCheckButton;
	public Button signUpButton;

	private void Awake()
	{
		emailCheckButton.onClick.AddListener(IdCheckButtonClick);
		signUpButton.onClick.AddListener(SignUpButtonClick);
	}

	private void IdCheckButtonClick()
	{
		FirebaseManager.Instance.CheckEmail(emailInput.text);
	}

	private void SignUpButtonClick()
	{
		if (FirebaseManager.Instance.state == FirebaseManager.State.EmailNotChecked)
		{
			PanelManager.Instance.dialogPanel.DialogOpen("email 중복 체크가 완료되지 않았습니다.",
				() => PanelManager.Instance.dialogPanel.DialogClose());
		}
		else if (pwInput1.text != pwInput2.text)
		{
			PanelManager.Instance.dialogPanel.DialogOpen("비밀번호가 일치하지 않습니다.",
				() => PanelManager.Instance.dialogPanel.DialogClose());
		}
		else if (pwInput1.text.Length < 8 || pwInput1.text.Length > 12)
		{
			PanelManager.Instance.dialogPanel.DialogOpen("비밀번호는 8글자 이상 12글자 이하로 설정해야 합니다.",
				() => PanelManager.Instance.dialogPanel.DialogClose());
		}
		else if (!IsValidPassword(pwInput1.text))
		{
			PanelManager.Instance.dialogPanel.DialogOpen("비밀번호는 문자, 숫자, 특수 기호 중 2가지 이상의 종류를 사용해야 합니다.",
				() => PanelManager.Instance.dialogPanel.DialogClose());
		}
		else
		{
			FirebaseManager.Instance.SignUp(emailInput.text, pwInput1.text);
		}
	}

	private bool IsValidPassword(string password)
	{
		if (string.IsNullOrEmpty(password)) return false;
		bool hasLetter = password.Any(char.IsLetter);
		bool hasDigit = password.Any(char.IsDigit);
		bool hasSpecialChar = Regex.IsMatch(password, @"[\W_]");
		int validCategories = Convert.ToInt32(hasLetter) + Convert.ToInt32(hasDigit) + Convert.ToInt32(hasSpecialChar);
		return validCategories >= 2;
	}
}