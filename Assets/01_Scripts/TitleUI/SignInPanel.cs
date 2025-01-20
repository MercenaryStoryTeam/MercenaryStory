using UnityEngine;
using UnityEngine.UI;

public class SignInPanel : MonoBehaviour
{
	public InputField emailInput;
	public InputField pwInput;
	public Button signInButton;
	public Button signUpButton;

	private void Awake()
	{
		signInButton.onClick.AddListener(SignInButtonClick);
		signUpButton.onClick.AddListener(SignUpButtonClick);

		// Input Navigation Helper setup
		var inputNav = gameObject.GetComponent<InputNavigationHelper>();
		inputNav.navigationElements = new Selectable[]
			{ emailInput, pwInput, signInButton, signUpButton };
	}

	private void SignInButtonClick()
	{
		FirebaseManager.Instance.SignIn(emailInput.text, pwInput.text);
	}

	private void SignUpButtonClick()
	{
		TitleUI.Instance.PanelOpen("SignUp");
	}
}