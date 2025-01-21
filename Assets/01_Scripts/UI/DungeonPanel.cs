using UnityEngine;
using UnityEngine.UI;

public class DungeonPanel : MonoBehaviour
{
	public Button cancelButton;
	public Button enterButton;

	private void Awake()
	{
		cancelButton.onClick.AddListener(OnCloseButtonClick);
		enterButton.onClick.AddListener(OnEnterButtonClick);
	}

	private void OnCloseButtonClick()
	{
		gameObject.SetActive(false);
	}

	private void OnEnterButtonClick()
	{
		// 던전 들어가기
		ServerManager.LoadScene("LJW_1-1");
	}
}