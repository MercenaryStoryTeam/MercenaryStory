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
		TitleUI.Instance.PanelOpen("SignUp");
	}

	private void SignInButtonClick()
	{
		FirebaseManager.Instance.SignIn(emailInput.text, pwInput.text);
	}
}