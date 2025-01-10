using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

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
	[FormerlySerializedAs("popUpPanel")] public PopUp popUp;
	public CharacterSelectPanel characterSelectPanel;

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
			{ "CharacterSelect", characterSelectPanel.gameObject }
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
}