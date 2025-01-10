using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerSelectPanel : MonoBehaviour
{
	public Button server1Button;
	public Button server2Button;
	public Button connectButton;

	private int serverNum = 0;

	private void Awake()
	{
		server1Button.onClick.AddListener(() => serverNum = 1);
		server2Button.onClick.AddListener(() => serverNum = 2);
		// connectButton.onClick.AddListener(()=>PanelManager.Instance.);
	}

	private void Start()
	{
		serverNum = 0;
	}
}