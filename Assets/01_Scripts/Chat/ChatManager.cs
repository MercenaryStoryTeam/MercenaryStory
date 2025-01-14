using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Chat.Demo;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ChatAuthValues = Photon.Chat.AuthenticationValues;

public class ChatManager : MonoBehaviour, IChatClientListener
{
	private static ChatManager _instance;
	private ChatClient _client;

	public static ChatManager Instance
	{
		get { return _instance; }
	}

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
	}

	private void Start()
	{
		_client = new ChatClient(this);
	}

	private void Update()
	{
		_client.Service();
	}

	public void SetNickName(string nickName)
	{
		_client.AuthValues = new ChatAuthValues(nickName);
	}

	public void ConnectUsingSettings()
	{
		AppSettings appSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
		ChatAppSettings chatSettings = appSettings.GetChatSettings();
		_client.ConnectUsingSettings(chatSettings);
	}

	public void DebugReturn(DebugLevel level, string message)
	{
	}

	public void OnDisconnected()
	{
	}

	public void OnConnected()
	{
	}

	public void OnChatStateChange(ChatState state)
	{
	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages)
	{
	}

	public void OnPrivateMessage(string sender, object message, string channelName)
	{
	}

	public void OnSubscribed(string[] channels, bool[] results)
	{
	}

	public void OnUnsubscribed(string[] channels)
	{
	}

	public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{
	}

	public void OnUserSubscribed(string channel, string user)
	{
	}

	public void OnUserUnsubscribed(string channel, string user)
	{
	}
}