using System.Collections.Generic;
using UnityEngine;

public class TitleUI : MonoBehaviour
{
	// Singleton pattern
	private static TitleUI _instance;
	private Dictionary<string, GameObject> panels;
	public SignUpPanel signUpPanel;
	public SignInPanel signInPanel;
	public PopUp popUp;
	public CharacterSelectPanel characterSelectPanel;
	public ServerSelectPanel serverSelectPanel;

	public static TitleUI Instance
	{
		get { return _instance; }
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}

		panels = new Dictionary<string, GameObject>
		{
			{ "SignUp", signUpPanel.gameObject },
			{ "SignIn", signInPanel.gameObject },
			{ "CharacterSelect", characterSelectPanel.gameObject },
			{ "ServerSelect", serverSelectPanel.gameObject }
		};

		PanelOpen("SignIn");
	}

	public void PanelOpen(string panelName)
	{
		foreach (var row in panels)
		{
			row.Value.SetActive(row.Key == panelName);
		}
	}

	public void PanelCloseAll()
	{
		foreach (GameObject panel in panels.Values)
		{
			panel.gameObject.SetActive(false);
		}
	}
}