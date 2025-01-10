using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class PlayerManager
{
	public static void ConnectLobby()
	{
		PhotonNetwork.NickName = FirebaseManager.Instance.CurrentUserData.user_Name;
		PhotonNetwork.ConnectUsingSettings();
	}
}