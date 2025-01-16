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
	public string currentChannel;

	public ChatPanel chatPanel;

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

	#endregion

	#region Channel Subscribe

	public void ChatStart(string roomName)
	{
		_client.Subscribe(new string[] { roomName });
	}

	public void ChatFinish(string roomName)
	{
		_client.Unsubscribe(new string[] { roomName });
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
		currentChannel = channels[0];
		print($"채팅 시작. current channel: {currentChannel}");
	}

	#endregion

	#region Message Send & Get

	public void SendChatMessage(string message)
	{
		_client.PublishMessage(currentChannel, message);
	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages)
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

	public void OnDisconnected()
	{
	}

	public void OnConnected()
	{
	}

	public void OnPrivateMessage(string sender, object message, string channelName)
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