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

	private bool isCooldown = false;

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
		gameObject.SetActive(false);
		UIManager.Instance.currentPanel = null;
	}

	private void Send()
	{
		if (isCooldown)
		{
			UIManager.Instance.popUp.PopUpOpen("아직 메시지를 전송할 수 없습니다.",
				() => UIManager.Instance.popUp.PopUpClose());
			return;
		}

		string message = messageInput.text;
		if (string.IsNullOrEmpty(message)) return;
		ChatManager.Instance.SendChatMessage(message);
		messageInput.text = "";
		// messageInput.ActivateInputField();
		StartCoroutine(SendCooldownCoroutine());
	}

	private IEnumerator SendCooldownCoroutine()
	{
		isCooldown = true;
		yield return new WaitForSeconds(chatCool);
		isCooldown = false;
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