using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PanelManager : MonoBehaviourPunCallbacks
{
	// Singleton pattern
	private static PanelManager _instance;

	public static PanelManager Instance
	{
		get { return _instance; }
	}

	private Dictionary<string, GameObject> panels;
	public SignUpPanel signUpPanel;
	public SignInPanel signInPanel;
	public DialogPanel dialogPanel;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			DestroyImmediate(gameObject);
		}

		panels = new Dictionary<string, GameObject>
		{
			{ "SignUp", signUpPanel.gameObject },
			{ "SignIn", signInPanel.gameObject },
			{ "Dialog", dialogPanel.gameObject }
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

	public void DialogOpen(string dialog)
	{
		dialogPanel.SetDialog(dialog);
		dialogPanel.gameObject.SetActive(true);
	}
}