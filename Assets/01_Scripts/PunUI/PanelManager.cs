using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PanelManager : SingletonManager<PanelManager>
{
	private Dictionary<string, GameObject> panels;
	public SignUpPanel signUpPanel;
	public SignInPanel signInPanel;
	public PopUp popUp;
	public CharacterSelectPanel characterSelectPanel;
	public ServerSelectPanel serverSelectPanel;

	protected override void Awake()
	{
		base.Awake();
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
}