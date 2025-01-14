using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
	public Button closeButton;
	public InputField messageInput;
	public Button sendButton;
	public RectTransform messageContent;
	public Text messageEntryPrefab;
	public Image notification;

	private void Awake()
	{
		closeButton.onClick.AddListener(OnCloseButtonClick);
		sendButton.onClick.AddListener(OnSendButtonClick);
	}

	private void OnEnable()
	{
		notification.gameObject.SetActive(false);
	}

	private void OnCloseButtonClick()
	{
		gameObject.SetActive(false);
	}

	private void OnSendButtonClick()
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