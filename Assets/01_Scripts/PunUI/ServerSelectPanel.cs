using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
		connectButton.onClick.AddListener(OnConnectButtonClick);
	}

	private void Start()
	{
		serverNum = 0;
	}

	private void OnConnectButtonClick()
	{
		if (serverNum == 0)
		{
			PanelManager.Instance.popUp.PopUpOpen("서버를 선택해 주세요.");
		}
		else
		{
			PlayerManager.ConnectLobby();
		}
	}
}