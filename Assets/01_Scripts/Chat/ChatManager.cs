using System.Collections;
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
	public ChatState state = 0;
	public string[] subscribedChannel;

	public ChatPanel chatPanel;
	private bool _isReconnecting = false;

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

	#region Connect

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

	private IEnumerator ReconnectCoroutine()
	{
		_isReconnecting = true;
		while (_client == null || !_client.CanChat)
		{
			Debug.Log("Attempting to reconnect to Photon Chat...");
			_client.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings
				.AppSettings.GetChatSettings());
			yield return new WaitForSeconds(5); // 5초 간격으로 재시도
		}

		// 구독 다시 하기
		ChatStart(FirebaseManager.Instance.CurrentUserData.user_CurrentServer);
		Debug.Log("Reconnected to Photon Chat successfully!");
		_isReconnecting = false;
	}

	public void OnDisconnected()
	{
		if (!_isReconnecting)
		{
			StartCoroutine(ReconnectCoroutine());
		}
	}

	#endregion

	#region Channel Subscribe

	public void ChatStart(string roomName)
	{
		_client.Unsubscribe(subscribedChannel);
		_client.Subscribe(new string[] { roomName });
	}

	public void OnChatStateChange(ChatState state)
	{
		if (this.state != state)
		{
			print($"Chat state changed: {state}");
			this.state = state;
		}
	}

	public void OnSubscribed(string[] channels, bool[] results)
	{
		subscribedChannel = channels;
		print($"채팅 시작. current channel: {subscribedChannel}");
	}

	#endregion

	#region Message Send & Get

	public void SendChatMessage(string message)
	{
		_client.PublishMessage(subscribedChannel[0], message);
	}

	public void OnGetMessages(string channelName, string[] senders,
		object[] messages)
	{
		chatPanel.NotificationOn();
		for (int i = 0; i < senders.Length; i++)
		{
			chatPanel.ReceiveChatMessage(senders[i], messages[i].ToString());
		}
	}

	#endregion

	public void DebugReturn(DebugLevel level, string message)
	{
	}

	public void OnConnected()
	{
	}

	public void OnPrivateMessage(string sender, object message,
		string channelName)
	{
	}

	public void OnUnsubscribed(string[] channels)
	{
	}

	public void OnStatusUpdate(string user, int status, bool gotMessage,
		object message)
	{
	}

	public void OnUserSubscribed(string channel, string user)
	{
	}

	public void OnUserUnsubscribed(string channel, string user)
	{
	}
}