using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class SignUpPanel : MonoBehaviour
{
	public InputField emailInput;
	public Button emailCheckButton;
	public InputField pwInput1;
	public InputField pwInput2;
	public InputField nameInput;
	public Button signUpButton;
	public Button signInButton;

	private void Awake()
	{
		emailCheckButton.onClick.AddListener(IdCheckButtonClick);
		signUpButton.onClick.AddListener(SignUpButtonClick);
		signInButton.onClick.AddListener(SignInButtonClick);

		// Input Navigation Helper setup
		var inputNav = gameObject.GetComponent<InputNavigationHelper>();
		inputNav.navigationElements = new Selectable[]
			{ emailInput, emailCheckButton, pwInput1, pwInput2, nameInput, signUpButton };
	}

	private void IdCheckButtonClick()
	{
		FirebaseManager.Instance.CheckEmail(emailInput.text);
	}

	private void SignUpButtonClick()
	{
		if (ValidateInputs())
		{
			FirebaseManager.Instance.SignUp(emailInput.text, pwInput1.text, nameInput.text);
		}
	}

	private void SignInButtonClick()
	{
		TitleUI.Instance.PanelOpen("SignIn");
	}

	private bool ValidateInputs()
	{
		if (FirebaseManager.Instance.state == FirebaseManager.State.EmailNotChecked)
		{
			ShowDialog("Email 중복 체크가 \n완료되지 않았습니다.");
			return false;
		}
		else if (pwInput1.text != pwInput2.text)
		{
			ShowDialog("비밀번호가 일치하지 않습니다.");
			return false;
		}
		else if (pwInput1.text.Length < 8 || pwInput1.text.Length > 12)
		{
			ShowDialog("비밀번호는 8글자 이상 \n12글자 이하로 설정해야 합니다.");
			return false;
		}
		else if (!IsValidPassword(pwInput1.text))
		{
			ShowDialog("비밀번호는 문자, 숫자, 특수문자\n 등 2가지 이상의 종류를 \n사용해야 합니다.");
			return false;
		}

		return true;
	}

	private void ShowDialog(string message)
	{
		TitleUI.Instance.popUp.PopUpOpen(message, () => TitleUI.Instance.popUp.PopUpClose());
	}

	private bool IsValidPassword(string password)
	{
		if (string.IsNullOrEmpty(password)) return false;
		bool hasLetter = password.Any(char.IsLetter);
		bool hasDigit = password.Any(char.IsDigit);
		bool hasSpecialChar = Regex.IsMatch(password, @"[\W_]");
		int validCategories = Convert.ToInt32(hasLetter) + Convert.ToInt32(hasDigit) +
		                      Convert.ToInt32(hasSpecialChar);
		return validCategories >= 2;
	}
}