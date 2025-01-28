using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
	[Header("채팅 재전송 가능 시간")] public float chatCool = 1f;
	public Button closeButton;
	public InputField messageInput;
	public Button sendButton;
	public RectTransform messageContent;
	public Text messageEntryPrefab;
	public Image notification;
	public Button chatButton;

	private void Awake()
	{
		closeButton.onClick.AddListener(OnCloseButtonClick);
		sendButton.onClick.AddListener(Send);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Send();
		}
	}

	private void OnEnable()
	{
		notification.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		chatButton.gameObject.SetActive(true);
	}

	private void OnCloseButtonClick()
	{
		UIManager.Instance.CloseChatPanel();
	}

	private void Send()
	{
		string message = messageInput.text;
		if (string.IsNullOrEmpty(message)) return;
		ChatManager.Instance.SendChatMessage(message);
		messageInput.text = "";
		messageInput.ActivateInputField();
	}

	public void ReceiveChatMessage(string nickName, string message)
	{
		var entry = Instantiate(messageEntryPrefab, messageContent);
		entry.text = $"{nickName}: {message}";
	}

	public void NotificationOn()
	{
		if (!gameObject.activeSelf)
		{
			notification.gameObject.SetActive(true);
		}
	}
}